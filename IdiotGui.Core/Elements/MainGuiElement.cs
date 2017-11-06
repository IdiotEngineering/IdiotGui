using System;
using IdiotGui.Core.BasicTypes;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace IdiotGui.Core.Elements
{
  /// <summary>
  ///   A specialized subclass of Element for managing the main element (that is spanned across the entire screen)
  /// </summary>
  public class MainElement : Element
  {
    #region Fields / Properties

    public Element FocusedElement { get; private set; }
    private readonly Window _window;
    private Element _lastMouseOver;

    #endregion

    public MainElement(Window window)
    {
      _window = window;
      ChildAlignment = ChildAlignments.Horizontal;
      // Register NativeWindow events
      window.NativeWindow.KeyDown += (sender, args) => FocusedElement?.OnKeyDown(args);
      window.NativeWindow.KeyUp += (sender, args) => FocusedElement?.OnKeyUp(args);
      //window.NativeWindow.Resized += (sender, args) => _glTexture.Resized(_window.UnscaledSize.Width, _window.UnscaledSize.Height);
      window.NativeWindow.MouseDown += (sender, args) =>
      {
        var clickedElement = GetTopmostElementAtPoint(args.Position);
        if (clickedElement == FocusedElement) return;
        // De-Focus last focused element
        FocusedElement?.OnLostFocus();
        // Focus the new one
        FocusedElement = clickedElement;
        FocusedElement.OnFocus();
      };
      window.NativeWindow.MouseUp += (sender, args) =>
      {
        var clickedElement = GetTopmostElementAtPoint(args.Position);
        // If the mouse is still within the control's bounds fire click event
        if (clickedElement == FocusedElement) FocusedElement?.OnClicked(args);
        // TODO: Fire the drag release event here
        clickedElement?.OnMouseUp(args);
      };
      window.NativeWindow.MouseMove += (sender, args) =>
      {
        var mouseOverElement = GetTopmostElementAtPoint(args.Position);
        if (mouseOverElement != _lastMouseOver) _lastMouseOver?.OnMouseLeave();
        _lastMouseOver = mouseOverElement;
        mouseOverElement?.OnMouseEnter();
      };
      window.NativeWindow.MouseWheel += (sender, args) => FocusedElement?.OnMouseWheel(args);
      window.NativeWindow.FocusedChanged += (sender, args) =>
      {
        if (window.NativeWindow.Focused)
        {
          // Refocused
          FocusedElement?.OnFocus();
        }
        else
        {
          // Unfocused
          FocusedElement?.OnLostFocus();
          _lastMouseOver?.OnMouseLeave();
        }
      };
    }
  }
}