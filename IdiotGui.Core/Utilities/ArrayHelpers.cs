using System;

namespace IdiotGui.Core.Utilities
{
  public class ArrayHelpers
  {
    public static T[][] Init2D<T>(int xCount, int yCount, Func<T> generator)
    {
      // Ugly, but fast (ish)
      var data = new T[xCount][];
      for (var x = 0; x < xCount; x++)
      {
        data[x] = new T[yCount];
        for (var y = 0; y < yCount; y++)
        {
          data[x][y] = generator();
        }
      }
      return data;
    }
  }
}