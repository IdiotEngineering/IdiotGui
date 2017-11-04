using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Policy;
using SkiaSharp;

namespace IdiotGui.Core.Utilities
{
  public struct TextToken
  {
    #region Fields / Properties

    /// <summary>
    ///   True if the token is a newline.
    /// </summary>
    public bool IsNewLine => Text == "\n";

    /// <summary>
    ///   True if the token is all whitespace (excluding newlines).
    /// </summary>
    public bool IsWhiteSpace => string.IsNullOrWhiteSpace(Text);

    /// <summary>
    ///   The text of this token.
    /// </summary>
    public string Text;

    /// <summary>
    ///   The Measured width of this token.
    /// </summary>
    public float MeasuredWidth;

    #endregion

    public TextToken(string text, float measuredWidth)
    {
      Text = text;
      MeasuredWidth = measuredWidth;
    }
  }

  /// <summary>
  ///   This class was made almost entirely for my own sanity, because the layout code in TextLayout was starting to drive me
  ///   totally insane. This is a cleaner abstraction for parsing tokens than handling the indexing manually.
  /// </summary>
  public class TokenStream
  {
    #region Fields / Properties

    public List<TextToken> Tokens = new List<TextToken>();

    private int _readIndex;

    #endregion

    public int Count => Tokens.Count;

    public bool CanReadForward => _readIndex < Tokens.Count;

    public bool CanReadBackward => _readIndex > 0;

    public TextToken PeakForward()
    {
      Debug.Assert(CanReadForward);
      return Tokens[_readIndex];
    }

    public TextToken ReadForward()
    {
      Debug.Assert(CanReadForward);
      return Tokens[_readIndex++];
    }

    public TextToken PeakBackward()
    {
      Debug.Assert(_readIndex > 0);
      return Tokens[_readIndex - 1];
    }

    public void RewindBeginning()
    {
      _readIndex = 0;
    }

    public void RewindOne()
    {
      _readIndex--;
      Debug.Assert(_readIndex > 0);
    }
  }

  /// <summary>
  ///   Tokenizes and measures each token against the given SKPaint in a single pass. This class is slightly optimized and is
  ///   thus below normal beauty standards.
  /// </summary>
  public class TextTokenizer
  {
    #region Fields / Properties

    /// <summary>
    ///   The newline character that will be used in all tokens (\r\n will be transformed into \n)
    /// </summary>
    public static string NewLine = "\n";

    /// <summary>
    ///   The number of spaces tabs will be transformed into.
    /// </summary>
    public int TabToSpaceWidth = 4;

    /// <summary>
    ///   The text that is to be / was tokenized.
    /// </summary>
    public string Text;

    /// <summary>
    ///   The Paint that is to be / was used to measure each token.
    /// </summary>
    public SKPaint Paint;

    /// <summary>
    ///   The string tokens. See Tokenize()
    /// </summary>
    public TokenStream TokenStream = new TokenStream();

    #endregion

    public TextTokenizer(SKPaint paint) => Paint = paint;

    /// <summary>
    ///   Generates or re-generates the Tokens field, producing a list of tokens. Any continuous length of whitespace
    ///   (excluding newlines) are always grouped together into a single token. This needs to be called explicitly after either
    ///   Text is changed, or the TextSize / TypeFace of the paint is changed.
    /// </summary>
    public void Tokenize()
    {
      TokenStream.RewindBeginning();
      TokenStream.Tokens.Clear();
      var tabString = new string(' ', TabToSpaceWidth);
      for (var startIndex = 0; startIndex < Text.Length; /* No Op */)
      {
        var length = 1;
        var ch = Text[startIndex];
        var tabReplaceNeeded = false;
        // If \n or \r\n, create a newline token
        switch (ch)
        {
          case '\n':
          case '\r':
            TokenStream.Tokens.Add(new TextToken(NewLine, 0));
            startIndex += Text[startIndex] == '\r' ? 2 : 1;
            continue;
          case ' ':
          case '\t':
            while (startIndex + length < Text.Length && Text[startIndex + length] == ' ' ||
                   Text[startIndex + length] == '\t') length++;
            tabReplaceNeeded = true;
            break;
          default:
            while (startIndex + length < Text.Length && !char.IsWhiteSpace(Text[startIndex + length])) length++;
            break;
        }
        // Create the token from the scan (whitespace or non-whitespace)
        var tokenText = Text.Substring(startIndex, length);
        if (tabReplaceNeeded) tokenText = tokenText.Replace("\t", tabString);
        TokenStream.Tokens.Add(new TextToken(tokenText, Paint.MeasureText(tokenText)));
        startIndex += length;
      }
    }
  }
}