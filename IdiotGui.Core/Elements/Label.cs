using IdiotGui.Core.BasicTypes;
using IdiotGui.Core.Utilities;
using SkiaSharp;

namespace IdiotGui.Core.Elements
{
  public class Label : Element
  {
    #region Fields / Properties

    public Color Textcolor = Color.White;

    public string Text
    {
      get => _layout.Text;
      set
      {
        _layout.Text = value;
        _reTokenize = true;
      }
    }

    public TextVerticalPosition VerticalTextPostion
    {
      get => _layout.VerticalPosition;
      set
      {
        _layout.VerticalPosition = value;
        _dirty = true;
      }
    }

    public TextHorizontalPosition HorizontalPosition
    {
      get => _layout.HorizontalPosition;
      set
      {
        _layout.HorizontalPosition = value;
        _dirty = true;
      }
    }

    public bool MultiLine
    {
      get => _layout.WrapLines;
      set
      {
        _layout.WrapLines = value;
        _dirty = true;
      }
    }

    public float LineSpacing
    {
      get => _layout.LineSpacing;
      set
      {
        _layout.LineSpacing = value;
        _dirty = true;
      }
    }

    public float TextSize
    {
      get => _paint.TextSize;
      set
      {
        _paint.TextSize = value;
        _dirty = true;
      }
    }

    private readonly SKPaint _paint;

    private readonly TextLayout _layout;
    private bool _reTokenize = true;
    private Size _lastSize;
    private bool _dirty = true;

    #endregion

    public Label()
    {
      _paint = new SKPaint
      {
        TextSize = 14.0f,
        IsAntialias = true
      };
      _layout = new TextLayout(_paint);
    }

    public override void Draw(SKCanvas canvas)
    {
      base.Draw(canvas);
      _paint.Color = Textcolor;
      // Re-tokenize if needed
      if (_reTokenize)
      {
        _layout.ReTokenize();
        _reTokenize = false;
      }
      // Re-layout if needed
      if (_dirty || ContentArea.Size != _lastSize)
      {
        _layout.ComputeLineLayouts(ContentArea.Size);
        _lastSize = ContentArea.Size;
        _dirty = false;
      }
      foreach (var line in _layout.LineLayouts)
        canvas.DrawText(line.LineText, ContentArea.Left + line.RelativeBounds.Left,
          ContentArea.Top + line.RelativeBounds.Top, _paint);
    }

    protected override void ComputeSizes()
    {
      base.ComputeSizes();
    }
  }
}