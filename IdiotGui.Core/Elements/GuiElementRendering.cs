using IdiotGui.Core.BasicTypes;
using SkiaSharp;

namespace IdiotGui.Core.Elements
{
  /// <summary>
  ///   Handles drawing an element. This is only for the background and border but can handle border radii. Simply change
  ///   colors to change the look of the control.
  /// </summary>
  public partial class Element
  {
    #region Fields / Properties

    // Note that border is defined in Layout as it's needed for layout calculations.

    /// <summary>
    ///   The background color of the element.
    /// </summary>
    public Color Background = Color.Transparent;

    #endregion

    /// <summary>
    ///   Draws the control onto the given canvas.
    /// </summary>
    /// <param name="canvas"></param>
    public virtual void Draw(SKCanvas canvas)
    {
      // Test a few short-circuits real fast (true for most elements)
      var hasBoder = !Border.Color.IsTransparent && Border.Size != 0;
      if (Background.IsTransparent && !hasBoder)
      {
        foreach (var child in Children) child.Draw(canvas);
        return;
      }
      // Background
      if (!Background.IsTransparent)
        if (Border.Radius > float.Epsilon)
          canvas.DrawRoundRect(ContentArea + Padding + Border.Size,
            Border.Radius, Border.Radius,
            new SKPaint
            {
              IsAntialias = true,
              Color = Background,
              Style = SKPaintStyle.Fill
            });
        else
          canvas.DrawRect(ContentArea + Padding + Border.Size,
            new SKPaint
            {
              IsAntialias = true,
              Color = Background,
              Style = SKPaintStyle.Fill
            });
      // Border
      if (hasBoder)
        if (Border.Radius > float.Epsilon)
          canvas.DrawRoundRect(ContentArea + Padding + Border.Size.Left / 2.0f,
            Border.Radius, Border.Radius,
            new SKPaint
            {
              IsAntialias = true,
              Color = Border.Color,
              Style = SKPaintStyle.Stroke
              // StrokeWidth = Border.WindowClientSize.Left
            });
        else
          canvas.DrawRect(ContentArea + Padding + Border.Size.Left / 2.0f,
            new SKPaint
            {
              IsAntialias = true,
              Color = Border.Color,
              Style = SKPaintStyle.Stroke
              //StrokeWidth = Border.WindowClientSize.Left
            });
      // Debug
      if (false)
      {
        canvas.DrawRect(BoxArea, new SKPaint {Color = Color.Red, Style = SKPaintStyle.Stroke, IsAntialias = true});
        canvas.DrawRect(BoxArea - ComputedMargin,
          new SKPaint {Color = Color.Green, Style = SKPaintStyle.Stroke, IsAntialias = true});
        canvas.DrawRect(BoxArea - ComputedMargin - Border.Size,
          new SKPaint {Color = Color.Blue, Style = SKPaintStyle.Stroke, IsAntialias = true});
      }
      foreach (var child in Children) child.Draw(canvas);
    }
  }
}