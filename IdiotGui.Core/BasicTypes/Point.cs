using OpenTK;

namespace IdiotGui.Core.BasicTypes
{
  /// <summary>
  ///   A point in space in Left, Top, or X, Y coordinates.
  /// </summary>
  public struct Point
  {
    #region Fields / Properties

    public int X { get; set; }
    public int Y { get; set; }

    public int Left
    {
      get { return X; }
      set { X = value; }
    }

    public int Top
    {
      get { return Y; }
      set { Y = value; }
    }

    #endregion

    public Point(int x, int y)
    {
      X = x;
      Y = y;
    }

    public Point(System.Drawing.Point point) : this(point.X, point.Y)
    {
    }

    public System.Drawing.Point ToSysPoint() => new System.Drawing.Point(X, Y);
  }
}