namespace IdiotGui.Core.BasicTypes
{
  /// <summary>
  ///   A size in Width, Height
  /// </summary>
  public struct Size
  {
    #region Fields / Properties

    public int Width;
    public int Height;

    #endregion

    public Size(int width, int height)
    {
      Width = width;
      Height = height;
    }

    public Size(int both) : this(both, both)
    {
    }

    public Size(System.Drawing.Size size) : this(size.Width, size.Height)
    {
    }

    public System.Drawing.Size ToSysSize() => new System.Drawing.Size(Width, Height);
  }
}