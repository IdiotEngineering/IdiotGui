using System;
using System.Collections.Generic;
using System.Text;
using IdiotGui.Core.BasicTypes;
using SkiaSharp;

namespace IdiotGui.Core.Utilities
{
  public enum HorizontalTextAlignment
  {
    Left,
    Middle,
    Right
  }

  public class TextLayoutResults
  {
    #region Types

    public struct TextLine
    {
      #region Fields / Properties

      public string Text;
      public Rectangle Bounds;

      #endregion
    }

    #endregion

    #region Fields / Properties

    public IEnumerable<TextLine> TextLines;

    #endregion
  }

  /// <summary>
  ///   A dead simple text layout utility class. Doesn't use Harfbuzz or anything fancy, just Latin-1 LTR.
  /// </summary>
  public class TextLayout
  {
    #region Types

    private class WordToken
    {
      #region Fields / Properties

      public readonly int StartLocation;
      public readonly bool IsWhitespace;
      public readonly bool IsNewline;
      public int Length;
      public float Width;

      #endregion

      public WordToken(int startLocation, int length, bool isWhitespace, bool isNewLine)
      {
        StartLocation = startLocation;
        Length = length;
        IsWhitespace = isWhitespace;
        IsNewline = isNewLine;
      }
    }

    #endregion

    #region Fields / Properties

    /// <summary>
    ///   The text to layout.
    /// </summary>
    public string Text;

    /// <summary>
    ///   If true, text will wrap around after overflowing horizontally.
    /// </summary>
    public bool MultiLine;

    /// <summary>
    ///   If set to true, any overflow (that doesn't fit in the layout bounds) will be truncated and "..." will be added to the
    ///   end.
    /// </summary>
    public bool TruncateOverflow;

    /// <summary>
    ///   How the text should be aligned horizontally.
    /// </summary>
    public HorizontalTextAlignment HorizontalAlignment = HorizontalTextAlignment.Middle;

    /// <summary>
    ///   The space between lines (relative to the hight of the typeface) in multi-line modes.
    /// </summary>
    public float LineSpacing = 1.5f;

    private readonly SKPaint _paint;
    private readonly float _ellipsisWidth;

    #endregion

    public TextLayout(SKPaint paint)
    {
      _paint = paint;
      _ellipsisWidth = paint.MeasureText("…");
    }

    public TextLayoutResults Layout(Size workArea)
    {
      var textLines = new List<TextLayoutResults.TextLine>();
      var lineWidths = ComputeOverflows(workArea.Width);
      var lineHeight = _paint.TextSize * LineSpacing;
      var topOffset = lineHeight;
      var i = 0;
      // Always keep at least one line
      do
      {
        // Keep the line
        var left = GetLineLeft(workArea.Width, topOffset, lineWidths[i].Item2);
        textLines.Add(new TextLayoutResults.TextLine
        {
          Bounds = new Rectangle(left, topOffset, lineWidths[i].Item2, lineHeight),
          Text = lineWidths[i].Item1
        });
        i++;
        topOffset += lineHeight;
      } while (i < lineWidths.Count && topOffset < workArea.Height);
      return new TextLayoutResults {TextLines = textLines};
    }

    private List<WordToken> TokenizeAndMeasure()
    {
      // Loop over every character and tokenize.
      var tokens = new List<WordToken>();
      for (var i = 0; i < Text.Length; i++)
      {
        // If the current character is a newline character, then create a new token.
        if (Text[i] == '\r' || Text[i] == '\n')
        {
          var isTwoCharacters = Text[i] == '\r' && Text.Length > i + 1 && Text[i + 1] == '\n';
          tokens.Add(new WordToken(i, isTwoCharacters ? 2 : 1, true, true));
          // Skip the next character if it's a \r\n as we added both of them just now.
          if (isTwoCharacters) i++;
          continue;
        }
        var isWhitespace = char.IsWhiteSpace(Text[i]);
        // If there was no tokens before, or the token before had a different IsWhitespace or
        // was a new line, then we need a new token.
        if (tokens.Count == 0 || tokens[tokens.Count - 1].IsNewline ||
            tokens[tokens.Count - 1].IsNewline != isWhitespace)
          tokens.Add(new WordToken(i, 0, isWhitespace, false));
        tokens[tokens.Count - 1].Length++;
      }
      // Measure all tokens
      foreach (var token in tokens)
        token.Width = _paint.MeasureText(Text.Substring(token.StartLocation, token.Length));
      return tokens;
    }

    /// <summary>
    ///   Consumes tokens starting at startIndex, filling a single line. If addTruncationMarker is set, and the line overflows,
    ///   a "..." will be added to the end of line.
    /// </summary>
    private string FillLine(List<WordToken> tokens, int startIndex, float maxWidth,
      bool addTruncationMarker, out int endIndex, out bool didTruncate, out float finalWidth)
    {
      didTruncate = false;
      finalWidth = 0.0f;
      for (endIndex = startIndex; endIndex < tokens.Count; endIndex++)
      {
        var token = tokens[endIndex];
        if (finalWidth + token.Width > maxWidth)
        {
          // We overflowed, see if we need to add the ellipsis
          if (addTruncationMarker)
            while (finalWidth + _ellipsisWidth > maxWidth)
            {
              endIndex--;
              finalWidth -= tokens[endIndex].Width;
            }
          // Set didTruncate regardless of if we added the marker or not.
          didTruncate = true;
          break;
        }
        finalWidth += token.Width;
      }
      var stringBuilder = new StringBuilder();
      for (var i = startIndex; i < endIndex; i++)
        stringBuilder.Append(Text.Substring(tokens[i].StartLocation, tokens[i].Length));
      if (didTruncate && addTruncationMarker) stringBuilder.Append("…");
      if (endIndex != startIndex) return stringBuilder.ToString();
      // If we didn't consume any tokens (because the region is too narrow) then just claim we consumed at least one to avoid an infinite loop
      didTruncate = true;
      endIndex = startIndex + 1;
      return stringBuilder.ToString();
    }

    /// <summary>
    ///   Fills line(s) with tokens until they need to be wrapped or truncated.
    /// </summary>
    private List<Tuple<string, float>> ComputeOverflows(float maxWidth)
    {
      var results = new List<Tuple<string, float>>();
      var tokens = TokenizeAndMeasure();
      if (MultiLine)
      {
        for (var i = 0; i < tokens.Count; i++)
        {
          var lineText = FillLine(tokens, i, maxWidth, false, out i, out var didTruncate, out var finalWidth);
          results.Add(new Tuple<string, float>(lineText, finalWidth));
        }
      }
      else
      {
        var lineText = FillLine(tokens, 0, maxWidth, true, out var _, out var _, out var finalWidth);
        results.Add(new Tuple<string, float>(lineText, finalWidth));
      }
      return results;
    }

    private float GetLineLeft(float maxWidth, float topOffset, float lineWidth)
    {
      switch (HorizontalAlignment)
      {
        case HorizontalTextAlignment.Left: return 0;
        case HorizontalTextAlignment.Middle: return (maxWidth - lineWidth) / 2.0f;
        case HorizontalTextAlignment.Right: return maxWidth - lineWidth;
        default: throw new ArgumentOutOfRangeException();
      }
    }
  }
}