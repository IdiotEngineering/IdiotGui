namespace IdiotGui.Core.Utilities
{
  public static class Extensions
  {
    /// <summary>
    ///   Returns true only if the string is made up of whitespace.
    /// </summary>
    public static bool IsWhitespace(this string str) => string.IsNullOrWhiteSpace(str);

    /// <summary>
    ///   Returns true if the string is \n or \r\n
    /// </summary>
    public static bool IsNewLine(this string str) => str == "\n" || str == "\r\n";
  }
}