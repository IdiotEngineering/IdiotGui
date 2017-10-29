using IdiotGui.Core.BasicTypes;
using OpenTK.Input;

namespace IdiotGui.Core.Elements
{
  public class Button : Element
  {
    public Button()
    {
      Height = (SFixed) 24;
      Background = Theme.Colors.DefaultFill;
      Border = new BorderStyle(1, Theme.Colors.DefaultBorder);
    }

    internal override void OnMouseDown(MouseButtonEventArgs e)
    {
      Border.Color = Theme.Colors.HighlightBorder;
      base.OnMouseDown(e);
    }

    internal override void OnMouseUp(MouseButtonEventArgs e)
    {
      Border.Color = Theme.Colors.DefaultBorder;
      base.OnMouseUp(e);
    }

    internal override void OnMouseEnter()
    {
      Background = Theme.Colors.MouseOverFill;
      base.OnMouseEnter();
    }

    internal override void OnMouseLeave()
    {
      Background = Theme.Colors.DefaultFill;
      base.OnMouseLeave();
    }
  }
}