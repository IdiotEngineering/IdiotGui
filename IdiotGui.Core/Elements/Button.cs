using IdiotGui.Core.BasicTypes;
using OpenTK.Input;
using SkiaSharp;

namespace IdiotGui.Core.Elements
{
  public class Button : Element
  {
    public string Text = "Button";

    public Button()
    {
      Height = (SFixed) 24;
      Width = new SFill();
      Background = Theme.Colors.DefaultFill;
      Border = new BorderStyle(1, Theme.Colors.DefaultBorder);
    }

    public override void Draw(SKCanvas canvas)
    {
      base.Draw(canvas);
      canvas.DrawText(Text, ContentArea.Center.X, ContentArea.Top + (ContentArea.Height / 2.0f) + 5.0f, new SKPaint
      {
        TextAlign = SKTextAlign.Center,
        IsAntialias = true,
        TextSize = 12.0f,
        Style = SKPaintStyle.Fill,
        Color = Color.White
      });
    }

    internal override void OnMouseDown(MouseButtonEventArgs e)
    {
      Border.Color = Theme.Colors.HighlightBorder;
      base.OnMouseDown(e);
    }

    internal override void OnMouseUp(MouseButtonEventArgs e)
    {
      Border.Color = Theme.Colors.DefaultBorder;
      base.OnMouseUp(e);
    }

    internal override void OnMouseEnter()
    {
      Background = Theme.Colors.MouseOverFill;
      base.OnMouseEnter();
    }

    internal override void OnMouseLeave()
    {
      Background = Theme.Colors.DefaultFill;
      base.OnMouseLeave();
    }
  }
}