using SkiaSharp;

namespace IdiotGui.Core.BasicTypes
{
  /// <summary>
  ///   A size in Width, Height
  /// </summary>
  public struct Size
  {
    #region Fields / Properties

    public float Width;
    public float Height;

    #endregion

    public Size(float width, float height)
    {
      Width = width;
      Height = height;
    }

    public Size(float both) : this(both, both)
    {
    }

    public override string ToString() => $"{{Width={Width}, Height={Height}";

    // Skia
    public static implicit operator SKSize(Size size) => new SKSize(size.Width, size.Height);

    public static implicit operator Size(SKSize size) => new Size(size.Width, size.Height);

    // System.Drawing
    public static implicit operator System.Drawing.Size(Size size) =>
      new System.Drawing.Size((int) size.Width, (int) size.Height);

    public static implicit operator Size(System.Drawing.Size size) => new Size(size.Width, size.Height);

    public bool Equals(Size other) => Width.Equals(other.Width) && Height.Equals(other.Height);

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      return obj is Size size && Equals(size);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return (Width.GetHashCode() * 397) ^ Height.GetHashCode();
      }
    }

    public static bool operator ==(Size left, Size right) => left.Equals(right);

    public static bool operator !=(Size left, Size right) => !left.Equals(right);
  }
}