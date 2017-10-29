using SkiaSharp;

namespace IdiotGui.Core.BasicTypes
{
  public class Theme
  {
    #region Types

    public class SkiaPaintSet
    {
      #region Fields / Properties

      public SKPaint DefaultText = new SKPaint
      {
        TextSize = 12.0f,
        TextAlign = SKTextAlign.Center,
        Color = Colors.DefaultText,
        Style = SKPaintStyle.Fill,
        IsAntialias = true
      };
      public SKPaint DefaultFill = new SKPaint {Color = Colors.DefaultFill, Style = SKPaintStyle.Fill};
      public SKPaint DefaultBorder = new SKPaint {Color = Colors.DefaultBorder, Style = SKPaintStyle.Stroke};

      public SKPaint MouseOverText = new SKPaint
      {
        TextSize = 12.0f,
        TextAlign = SKTextAlign.Center,
        Color = Colors.MouseOverText,
        Style = SKPaintStyle.Fill,
        IsAntialias = true
      };
      public SKPaint MouseOverFill = new SKPaint {Color = Colors.MouseOverFill, Style = SKPaintStyle.Fill};
      public SKPaint MouseOverBourder = new SKPaint {Color = Colors.MouseOverBorder, Style = SKPaintStyle.Stroke};

      public SKPaint HighlightText = new SKPaint
      {
        TextSize = 12.0f,
        TextAlign = SKTextAlign.Center,
        Color = Colors.HighlightText,
        Style = SKPaintStyle.Fill,
        IsAntialias = true
      };
      public SKPaint HighlightFill = new SKPaint {Color = Colors.HighlightBackground, Style = SKPaintStyle.Fill};
      public SKPaint HighlightBorder = new SKPaint {Color = Colors.HighlightBorder, Style = SKPaintStyle.Stroke};

      #endregion
    }

    public class ColorPalette
    {
      #region Fields / Properties

      public Color DefaultText = new Color(133);
      public Color DefaultFill = new Color(24);
      public Color DefaultBorder = new Color(86);

      public Color MouseOverText = new Color(255);
      public Color MouseOverFill = new Color(69);
      public Color MouseOverBorder = new Color(133);

      public Color HighlightText = new Color(255);
      public Color HighlightBackground = new Color(0, 170, 222);
      public Color HighlightBorder = Color.White;

      #endregion
    }

    #endregion

    #region Fields / Properties

    public static ColorPalette Colors = new ColorPalette();
    public static SkiaPaintSet PaintSet = new SkiaPaintSet();

    #endregion
  }
}