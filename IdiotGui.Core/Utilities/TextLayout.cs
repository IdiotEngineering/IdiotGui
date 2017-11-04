using System;
using System.Collections.Generic;
using System.Text;
using IdiotGui.Core.BasicTypes;
using SkiaSharp;

namespace IdiotGui.Core.Utilities
{
  public enum TextHorizontalPosition
  {
    Left,
    Middle,
    Right
  }

  public enum TextVerticalPosition
  {
    Top,
    Middle,
    Bottom
  }

  public class TextLineLayout
  {
    #region Fields / Properties

    public string LineText;
    public Rectangle RelativeBounds;

    #endregion

    public TextLineLayout(IEnumerable<TextToken> tokens, float lineHeight)
    {
      var builder = new StringBuilder();
      var width = 0.0f;
      foreach (var token in tokens)
      {
        builder.Append(token.Text);
        width += token.MeasuredWidth;
      }
      LineText = builder.ToString();
      RelativeBounds = new Rectangle(0, 0, width, lineHeight);
    }
  }

  /// <summary>
  ///   A simple text layout utility class.
  /// </summary>
  public class TextLayout
  {
    #region Fields / Properties

    public string Text;
    public float LineSpacing = 1.5f;
    public SKPaint Paint;
    public string TruncateMarker = "…";
    public TextHorizontalPosition HorizontalPosition = TextHorizontalPosition.Left;
    public TextVerticalPosition VerticalPosition = TextVerticalPosition.Middle;
    public bool WrapLines = true;
    public IEnumerable<TextLineLayout> LineLayouts;
    public TextTokenizer Tokenizer;
    private TokenStream _tokenStream;

    private TextToken _truncateMarkerToken;

    #endregion

    public TextLayout(SKPaint paint)
    {
      Paint = paint;
      Tokenizer = new TextTokenizer(paint);
    }

    /// <summary>
    ///   Re tokenizes the string. This only needs to be done with the Text, text size or typefaces change. It does not need to
    ///   be (and should not) called when just the layout changes.
    /// </summary>
    public void ReTokenize()
    {
      Tokenizer.Text = Text;
      Tokenizer.Paint = Paint;
      Tokenizer.Tokenize();
      _tokenStream = Tokenizer.TokenStream;
      _truncateMarkerToken = new TextToken(TruncateMarker, Paint.MeasureText(TruncateMarker));
    }

    /// <summary>
    ///   Recomputes the text layout given the Text and Paint, in the workArea provided.
    /// </summary>
    public void ComputeLineLayouts(Size workArea)
    {
      var lineLayouts = new List<TextLineLayout>();
      // Must have more than one token, and a greater than 0 work area.
      if (_tokenStream.Count == 0 || workArea.Width <= 0)
      {
        LineLayouts = lineLayouts;
        return;
      }
      var lineHeight = Paint.TextSize * LineSpacing;
      _tokenStream.RewindBeginning();
      while (_tokenStream.CanReadForward)
      {
        lineLayouts.Add(new TextLineLayout(GetTokenLine(workArea.Width), lineHeight));
        // Only read a single line for non-WrapLines
        if (!WrapLines) break;
      }
      //else
      //  lineLayouts.Add(new TextLineLayout(GetSingleLineTokens(workArea.Width), lineHeight));
      // Layout all lines horizontally
      switch (HorizontalPosition)
      {
        case TextHorizontalPosition.Left:
          break;
        case TextHorizontalPosition.Middle:
          foreach (var line in lineLayouts)
            line.RelativeBounds.Left = Math.Max(0, (workArea.Width - line.RelativeBounds.Width) / 2.0f);
          break;
        case TextHorizontalPosition.Right:
          foreach (var line in lineLayouts)
            line.RelativeBounds.Left = Math.Max(0, workArea.Width - line.RelativeBounds.Width);
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
      // Offset all lines vertically
      var totalHeight = lineLayouts.Count * lineHeight;
      var startOffset = 0.0f;
      switch (VerticalPosition)
      {
        case TextVerticalPosition.Top:
          startOffset = Paint.TextSize;
          break;
        case TextVerticalPosition.Middle:
          startOffset = Math.Max(Paint.TextSize, (workArea.Height - totalHeight) / 2.0f + Paint.TextSize);
          break;
        case TextVerticalPosition.Bottom:
          startOffset = Math.Max(Paint.TextSize, workArea.Height - totalHeight + Paint.TextSize);
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
      var verticalOffset = startOffset;
      foreach (var line in lineLayouts)
      {
        line.RelativeBounds.Top = verticalOffset;
        verticalOffset += lineHeight;
      }
      LineLayouts = lineLayouts;
    }

    /// <summary>
    ///   Returns a single line of tokens that fit within the given width. If a \n is found the line will be ended if WrapText
    ///   is true, or ignored otherwise. Whitespace at the beginning of line will be removed. This function was a fucking
    ///   nightmare to write... off-by-one error galore. Finally had to make the TokenStream class for sanity.
    /// </summary>
    private IEnumerable<TextToken> GetTokenLine(float maxWidth)
    {
      // Count the tokens we can fit in maxWidth
      var width = 0.0f;
      // Skip spaces at the beginning of stream if we aren't at the start of stream and the last token wasn't a \n
      while (_tokenStream.CanReadBackward &&
             !_tokenStream.PeakBackward().IsNewLine &&
             _tokenStream.CanReadForward &&
             _tokenStream.PeakForward().IsWhiteSpace)
        _tokenStream.ReadForward();
      // Make sure we didn't just skip the entire stream
      if (!_tokenStream.CanReadForward) yield break;
      // Read at least one non-whitespace token regardless of if it fits or not.
      var firstToken = _tokenStream.ReadForward();
      width += firstToken.MeasuredWidth;
      yield return firstToken;
      // Read the rest of the tokens that fit.
      while (_tokenStream.CanReadForward && _tokenStream.PeakForward().MeasuredWidth + width <= maxWidth)
      {
        var token = _tokenStream.ReadForward();
        // Slightly different things happen at a newline for WrapText and non-WrapText
        if (token.IsNewLine)
        {
          // If we are wrapping, then we are just done iterating this line.
          if (WrapLines) yield break;
          // If we aren't wrapping, then just ignore the \n altogether.
          continue;
        }
        width += token.MeasuredWidth;
        yield return token;
      }
    }
  }
}