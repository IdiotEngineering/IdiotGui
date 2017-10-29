using SkiaSharp;

namespace IdiotGui.Core.BasicTypes
{
  /// <summary>
  ///   A point in space in Left, Top, or X, Y coordinates.
  /// </summary>
  public struct Point
  {
    #region Fields / Properties

    public float X { get; set; }
    public float Y { get; set; }

    public float Left
    {
      get => X;
      set => X = value;
    }

    public float Top
    {
      get => Y;
      set => Y = value;
    }

    #endregion

    public Point(float x, float y)
    {
      X = x;
      Y = y;
    }

    public override string ToString() => $"{{X={X}, Y={Y}}}";

    // Skia
    public static implicit operator SKPoint(Point point) => new SKPoint(point.X, point.Y);

    public static implicit operator Point(SKPoint point) => new Point(point.X, point.Y);

    // System.Drawing
    public static implicit operator System.Drawing.Point(Point point) =>
      new System.Drawing.Point((int) point.X, (int) point.Y);

    public static implicit operator Point(System.Drawing.Point point) => new Point(point.X, point.Y);

    public bool Equals(Point other) => X.Equals(other.X) && Y.Equals(other.Y);

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      return obj is Point point && Equals(point);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return (X.GetHashCode() * 397) ^ Y.GetHashCode();
      }
    }

    public static bool operator ==(Point left, Point right) => left.Equals(right);

    public static bool operator !=(Point left, Point right) => !left.Equals(right);
  }
}