using IdiotGui.Core.Elements;
using SkiaSharp;

namespace IdiotGui.Core.BasicTypes
{
  public struct Rectangle
  {
    #region Fields / Properties

    public float Bottom => Top + Height;
    public float Right => Left + Width;

    public float X => Left;
    public float Y => Top;
    public float Left;
    public float Top;
    public float Width;
    public float Height;

    public Point Location
    {
      get => new Point(Left, Top);
      set
      {
        Left = value.Left;
        Top = value.Top;
      }
    }

    public Size Size
    {
      get => new Size(Width, Height);
      set
      {
        Width = value.Width;
        Height = value.Height;
      }
    }

    public Point Center
    {
      get => new Point(Left + Width / 2.0f, Top + Height / 2.0f);
      set
      {
        Left = value.Left - Width / 2.0f;
        Top = value.Top - Height / 2.0f;
      }
    }

    #endregion

    public Rectangle(float left, float top, float width, float height)
    {
      Left = left;
      Top = top;
      Width = width;
      Height = height;
    }

    public Rectangle(Point location, Size size) : this(location.Left, location.Top, size.Width, size.Height)
    {
    }

    public override string ToString() => $"{{Left={Left}, Top={Top}, Width={Width}, Height={Height}}}";

    // Skia
    public static implicit operator Rectangle(SKRect rect) =>
      new Rectangle(rect.Left, rect.Top, rect.Width, rect.Height);

    public static implicit operator SKRect(Rectangle rect) => new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);

    // System.Drawing
    public static implicit operator Rectangle(System.Drawing.Rectangle rect) =>
      new Rectangle(rect.Left, rect.Top, rect.Width, rect.Height);

    public static implicit operator System.Drawing.Rectangle(Rectangle rect) =>
      new System.Drawing.Rectangle((int) rect.Left, (int) rect.Top, (int) rect.Right, (int) rect.Bottom);

    public bool Equals(Rectangle other) => Left.Equals(other.Left) && Top.Equals(other.Top) &&
                                           Width.Equals(other.Width) && Height.Equals(other.Height);

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      return obj is Rectangle && Equals((Rectangle) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = Left.GetHashCode();
        hashCode = (hashCode * 397) ^ Top.GetHashCode();
        hashCode = (hashCode * 397) ^ Width.GetHashCode();
        hashCode = (hashCode * 397) ^ Height.GetHashCode();
        return hashCode;
      }
    }

    public static bool operator ==(Rectangle left, Rectangle right) => left.Equals(right);

    public static bool operator !=(Rectangle left, Rectangle right) => !left.Equals(right);

    public static Rectangle operator +(Rectangle rect, BorderSize border) => new Rectangle(rect.Left - border.Left,
      rect.Top - border.Top, rect.Width + border.Left + border.Right, rect.Height + border.Top + border.Bottom);

    public static Rectangle operator -(Rectangle rect, BorderSize border) => new Rectangle(rect.Left + border.Left,
      rect.Top + border.Top, rect.Width - (border.Left + border.Right), rect.Height - (border.Top + border.Bottom));
  }
}