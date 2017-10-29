using System;
using IdiotGui.Core.BasicTypes;

namespace IdiotGui.Core.Elements
{
  public struct BorderSize
  {
    #region Fields / Properties

    public int Top;
    public int Right;
    public int Bottom;
    public int Left;

    /// <summary>
    /// Evaluates to true only if all values are the same (even 0).
    /// </summary>
    public bool IsUniform => Left == Top && Left == Right && Left == Bottom;

    #endregion

    public BorderSize(int top, int right, int bottom, int left)
    {
      Top = top;
      Right = right;
      Bottom = bottom;
      Left = left;
    }

    public static implicit operator BorderSize(int value) => new BorderSize(value, value, value, value);

    public static implicit operator BorderSize(int[] values)
    {
      switch (values.Length)
      {
        case 1: return values[0];
        case 2: return new BorderSize(values[0], values[1], values[0], values[1]);
        case 4: return new BorderSize(values[0], values[1], values[2], values[3]);
      }
      throw new Exception("BorderSize from int array must be 1, 2, or 4 values.");
    }

    public bool Equals(BorderSize other) => Top == other.Top && Right == other.Right && Bottom == other.Bottom && Left == other.Left;

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      return obj is BorderSize && Equals((BorderSize) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = Top;
        hashCode = (hashCode * 397) ^ Right;
        hashCode = (hashCode * 397) ^ Bottom;
        hashCode = (hashCode * 397) ^ Left;
        return hashCode;
      }
    }

    public static bool operator ==(BorderSize left, BorderSize right) => left.Equals(right);

    public static bool operator !=(BorderSize left, BorderSize right) => !left.Equals(right);

    public static BorderSize operator +(BorderSize l, BorderSize r) =>
      new BorderSize(l.Left + r.Left, l.Top + r.Top, l.Right + r.Right, l.Bottom + r.Bottom);

    public static BorderSize operator -(BorderSize l, BorderSize r) =>
      l + new BorderSize(-r.Left, -r.Top, -r.Right, -r.Bottom);
  }

  public struct BorderStyle
  {
    #region Fields / Properties

    public BorderSize Size;
    public Color Color;
    public BorderSize Radius;

    #endregion

    public BorderStyle(BorderSize size, Color color, BorderSize radius)
    {
      Size = size;
      Color = color;
      Radius = radius;
    }

    public BorderStyle(BorderSize size, Color color) : this(size, color, 0)
    {
    }
  }

  public abstract class SSizing
  {
    #region Fields / Properties

    public SizingTypes Type;

    #endregion

    protected SSizing(SizingTypes type) => Type = type;
  }

  public class SFixed : SSizing
  {
    #region Fields / Properties

    public int Size;

    #endregion

    public SFixed(int size) : base(SizingTypes.Fixed) => Size = size;

    public static explicit operator SFixed(int size) => new SFixed(size);
  }

  public class SWeighted : SSizing
  {
    #region Fields / Properties

    public float Weight;

    #endregion

    public SWeighted(float weight = 1.0f) : base(SizingTypes.Weighted) => Weight = weight;
  }

  public class SFill : SWeighted
  {
    public SFill() : base(1.0f)
    {
    }
  }

  public class SFitChildren : SSizing
  {
    public SFitChildren() : base(SizingTypes.Auto)
    {
    }
  }

  public enum ChildAlignments
  {
    Horizontal,
    Vertical
  }

  public enum SizingTypes
  {
    Auto,
    Fill,
    Weighted,
    Fixed
  }
}