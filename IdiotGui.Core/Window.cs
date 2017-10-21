using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using IdiotGui.Core.BasicTypes;

namespace IdiotGui.Core
{
  /// <summary>
  ///   An OS window managed and drawn by IdiotGui
  /// </summary>
  public class Window
  {
    #region Fields / Properties

    public readonly float DpiScale;
    public readonly NativeWindow NativeWindow;
    public Size UnscaledSize => new Size(NativeWindow.Width, NativeWindow.Height);
    public event EventHandler<EventArgs> Resize;
    public event EventHandler<EventArgs> Closing;

    /// <summary>
    ///   The size of the window as it was requested, before any DPI scaling. For example, if the window was requested at
    ///   1680x1050, that will be that.
    /// </summary>
    public Size Size
    {
      get { return _requestedSize; }
      set { NativeWindow.Size = (_requestedSize = value).ToSysSize(); }
    }

    public Point Location
    {
      get { return new Point(NativeWindow.Location); }
      set { NativeWindow.Location = value.ToSysPoint(); }
    }

    private Size _requestedSize;

    #endregion

    public Window(string title, int width, int height)
    {
      _requestedSize = new Size(width, height);
      // Native window and Render Context initialize
      NativeWindow = new NativeWindow(width, height, title, GameWindowFlags.Default, GraphicsMode.Default,
        DisplayDevice.Default)
      {Visible = true, Location = new Point(50, 50).ToSysPoint(), Title = title};
      DpiScale = NativeWindow.Width/(float) width;
      RegisterEventHandlers();
    }

    public void Update()
    {
      NativeWindow.ProcessEvents();
    }

    internal void Render()
    {
      // Enqueues draw calls for the world and main gui bound to the RenderTarget
    }

    private void RegisterEventHandlers()
    {
      // Resize
      NativeWindow.Resize += (sender, args) =>
      {
        // Note that GUI stays in scaled coordinates
        //GuiCamera.Viewport = new Rectangle(0, 0, UnscaledSize.Width, UnscaledSize.Height);
        //GuiCamera.OrthographicBounds = new Rectangle(0, 0, NativeWindow.Width, NativeWindow.Height);
        //MainGuiElement.Width = (SFixed) NativeWindow.Width;
        //MainGuiElement.Height = (SFixed) NativeWindow.Height;
        //MainGuiElement.LayoutChildren(new Rectangle(0, 0, NativeWindow.Width, NativeWindow.Height));
        Resize?.Invoke(this, args);
      };
      // Closing
      NativeWindow.Closing += (sender, args) => Closing?.Invoke(this, args);
    }
  }
}