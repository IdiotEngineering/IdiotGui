using System.Diagnostics;
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

    /// <summary>
    ///   If set to true, the BoxArea, BoxArea - ComputedMargin and BoxArea - ComputedMargin - Border.Size will be drawn.
    /// </summary>
    public bool DrawDebugBorders = false;

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
        DrawDebug(canvas);
        foreach (var child in Children) child.Draw(canvas);
        return;
      }
      DrawBackground(canvas);
      if (hasBoder) DrawBorder(canvas);
      DrawDebug(canvas);
      foreach (var child in Children) child.Draw(canvas);
    }

    private void DrawBackground(SKCanvas canvas)
    {
      if (Background.IsTransparent) return;
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
    }

    private void DrawBorder(SKCanvas canvas)
    {
      Debug.Assert(Border.Size.IsUniform, "Non-uniform border size is not yet supported");
      if (Border.Radius > float.Epsilon)
        canvas.DrawRoundRect(ContentArea + Padding + Border.Size.Left / 2.0f,
          Border.Radius, Border.Radius,
          new SKPaint
          {
            IsAntialias = true,
            Color = Border.Color,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = Border.Size.Left
          });
      else
        canvas.DrawRect(ContentArea + Padding + Border.Size.Left / 2.0f,
          new SKPaint
          {
            IsAntialias = true,
            Color = Border.Color,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = Border.Size.Left
          });
    }

    private void DrawDebug(SKCanvas canvas)
    {
      if (!DrawDebugBorders) return;
      canvas.DrawRect(BoxArea, new SKPaint {Color = Color.Red, Style = SKPaintStyle.Stroke, IsAntialias = true});
      canvas.DrawRect(BoxArea - ComputedMargin,
        new SKPaint {Color = Color.Green, Style = SKPaintStyle.Stroke, IsAntialias = true});
      canvas.DrawRect(BoxArea - ComputedMargin - Border.Size,
        new SKPaint {Color = Color.Blue, Style = SKPaintStyle.Stroke, IsAntialias = true});
    }
  }
}