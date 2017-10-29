using System;
using OpenTK.Input;

namespace IdiotGui.Core.Elements
{
  public partial class Element
  {
    #region Fields / Properties

    public Action<Element, KeyboardKeyEventArgs> KeyDown;
    public Action<Element, KeyboardKeyEventArgs> KeyUp;
    public Action<Element, MouseButtonEventArgs> MouseDown;
    public Action<Element, MouseButtonEventArgs> MouseUp;
    public Action<Element, MouseWheelEventArgs> MouseWheel;
    public Action<Element, MouseButtonEventArgs> Clicked;
    public Action<Element> Focus;
    public Action<Element> LostFocus;
    public Action<Element> MouseEnter;
    public Action<Element> MouseLeave;

    public bool IsKeyDown;
    public bool IsFocused;
    public bool IsMouseOver;
    public bool IsMouseDown;

    #endregion

    internal virtual void OnKeyDown(KeyboardKeyEventArgs e)
    {
      IsKeyDown = true;
      KeyDown?.Invoke(this, e);
    }

    internal virtual void OnKeyUp(KeyboardKeyEventArgs e)
    {
      IsKeyDown = false;
      KeyUp?.Invoke(this, e);
    }

    internal virtual void OnMouseDown(MouseButtonEventArgs e)
    {
      IsMouseDown = true;
      MouseDown?.Invoke(this, e);
    }

    internal virtual void OnMouseUp(MouseButtonEventArgs e)
    {
      IsMouseDown = false;
      MouseUp?.Invoke(this, e);
    }

    internal virtual void OnMouseWheel(MouseWheelEventArgs e) => MouseWheel?.Invoke(this, e);
    internal virtual void OnClicked(MouseButtonEventArgs e) => Clicked?.Invoke(this, e);

    internal virtual void OnFocus()
    {
      IsFocused = true;
      Focus?.Invoke(this);
    }

    internal virtual void OnLostFocus()
    {
      IsFocused = false;
      LostFocus?.Invoke(this);
    }

    internal virtual void OnMouseEnter()
    {
      IsMouseOver = true;
      MouseEnter?.Invoke(this);
    }

    internal virtual void OnMouseLeave()
    {
      IsMouseOver = false;
      MouseLeave?.Invoke(this);
    }
  }
}