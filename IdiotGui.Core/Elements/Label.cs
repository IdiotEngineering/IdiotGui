using IdiotGui.Core.BasicTypes;
using IdiotGui.Core.Utilities;
using SkiaSharp;

namespace IdiotGui.Core.Elements
{
  public class Label : Element
  {
    #region Fields / Properties

    public string Text;
    public bool MultiLine;
    public HorizontalTextAlignment HorizontalTextAlignment = HorizontalTextAlignment.Left;
    public Color Textcolor = Color.White;

    private readonly SKPaint _paint;
    private readonly TextLayout _textLayout;

    #endregion

    public Label()
    {
      _paint = new SKPaint
      {
        TextSize = 12.0f,
        IsAntialias = true
      };
      _textLayout = new TextLayout(_paint);
    }

    public override void Draw(SKCanvas canvas)
    {
      base.Draw(canvas);
      _paint.Color = Textcolor;
      _textLayout.Text = Text;
      _textLayout.HorizontalAlignment = HorizontalTextAlignment;
      _textLayout.MultiLine = MultiLine;
      _textLayout.TruncateOverflow = !MultiLine;
      var results = _textLayout.Layout(ContentArea.Size);
      foreach (var line in results.TextLines)
        canvas.DrawText(line.Text, ContentArea.Left + line.Bounds.Left, ContentArea.Top + line.Bounds.Top, _paint);
    }
  }
}