using System;
using System.Collections.Generic;
using System.Diagnostics;
using IdiotGui.Core.BasicTypes;
using IdiotGui.Core.Elements;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using SkiaSharp;

namespace IdiotGui.Core
{
  /// <inheritdoc />
  /// <summary>
  ///   An OS window managed and drawn by IdiotGui
  /// </summary>
  public class Window : Element, IDisposable
  {
    #region Fields / Properties

    /// <summary>
    ///   Fired AFTER the window is resized (not during resize)
    /// </summary>
    public event EventHandler<EventArgs> Resized;

    /// <summary>
    ///   Fired when the window is closing.
    /// </summary>
    public event EventHandler<EventArgs> Closing;

    public Element FocusedElement { get; private set; }
    private Element _lastMouseOver;

    /// <summary>
    ///   The location (in screen-space, pixel coordinates) of the window.
    /// </summary>
    public Point WindowLocation
    {
      get => NativeWindow.Location;
      set => NativeWindow.Location = value;
    }

    public Size WindowClientSize
    {
      get => NativeWindow.ClientSize;
      set
      {
        NativeWindow.ClientSize = value;
        UpdateScreenSize();
      }
    }

    internal readonly NativeWindow NativeWindow;
    private GRContext _skiaContext;
    private GraphicsContext _glGraphicsContext;
    private GRBackendRenderTargetDesc _skiaRenderTarget;

    #endregion


    public Window(string title, int width, int height)
    {
      // Native window and Render Context initialize
      NativeWindow = new NativeWindow(width, height, title, GameWindowFlags.Default, GraphicsMode.Default,
          DisplayDevice.Default)
        {Visible = true, Title = title};
      RegisterEventHandlers();
      // GL Context
      _glGraphicsContext =
        new GraphicsContext(GraphicsMode.Default, NativeWindow.WindowInfo, 4, 2, GraphicsContextFlags.Debug);
      _glGraphicsContext.LoadAll();
      _glGraphicsContext.MakeCurrent(NativeWindow.WindowInfo);
      // SkiaSharp Context
      var glInterface = GRGlInterface.CreateNativeGlInterface();
      Debug.Assert(glInterface.Validate());
      _skiaContext = GRContext.Create(GRBackend.OpenGL, glInterface);
      Debug.Assert(_skiaContext.Handle != IntPtr.Zero);
      _skiaRenderTarget = CreateRenderTarget();
      // Main Gui Element setup
      ChildAlignment = ChildAlignments.Horizontal;
      // Register NativeWindow events
      NativeWindow.KeyDown += (sender, args) => FocusedElement?.OnKeyDown(args);
      NativeWindow.KeyUp += (sender, args) => FocusedElement?.OnKeyUp(args);
      //window.NativeWindow.Resized += (sender, args) => _glTexture.Resized(_window.UnscaledSize.Width, _window.UnscaledSize.Height);
      NativeWindow.MouseDown += (sender, args) =>
      {
        var clickedElement = GetTopmostElementAtPoint(args.Position);
        if (clickedElement == FocusedElement) return;
        // De-Focus last focused element
        FocusedElement?.OnLostFocus();
        // Focus the new one
        FocusedElement = clickedElement;
        FocusedElement.OnFocus();
      };
      NativeWindow.MouseUp += (sender, args) =>
      {
        var clickedElement = GetTopmostElementAtPoint(args.Position);
        // If the mouse is still within the control's bounds fire click event
        if (clickedElement == FocusedElement) FocusedElement?.OnClicked(args);
        // TODO: Fire the drag release event here
        clickedElement?.OnMouseUp(args);
      };
      NativeWindow.MouseMove += (sender, args) =>
      {
        NativeWindow.Title = args.Position.ToString();
        var mouseOverElement = GetTopmostElementAtPoint(args.Position);
        if (mouseOverElement != _lastMouseOver) _lastMouseOver?.OnMouseLeave();
        _lastMouseOver = mouseOverElement;
        mouseOverElement?.OnMouseEnter();
      };
      NativeWindow.MouseWheel += (sender, args) => FocusedElement?.OnMouseWheel(args);
      NativeWindow.FocusedChanged += (sender, args) =>
      {
        if (NativeWindow.Focused)
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

    public void Dispose()
    {
      _skiaContext?.AbandonContext();
      _skiaContext = null;
      _glGraphicsContext?.Dispose();
      _glGraphicsContext = null;
    }

    public void Update()
    {
      NativeWindow.ProcessEvents();
      UpdateScreenSize();
    }

    public void Render()
    {
      if (_skiaContext == null || _glGraphicsContext == null) return;
      _glGraphicsContext.MakeCurrent(NativeWindow.WindowInfo);
      // Enqueues draw calls for the world and main GUI bound to the RenderTarget
      _skiaRenderTarget.Width = NativeWindow.Width;
      _skiaRenderTarget.Height = NativeWindow.Height;
      using (var surface = SKSurface.Create(_skiaContext, _skiaRenderTarget))
      {
        Debug.Assert(surface != null);
        Debug.Assert(surface.Handle != IntPtr.Zero);
        var canvas = surface.Canvas;
        // clear the canvas / fill with white
        canvas.Clear(SKColors.White);
        Draw(canvas);
        canvas.Flush();
      }
      _skiaContext.Flush();
      _glGraphicsContext.SwapBuffers();
    }

    private void RegisterEventHandlers()
    {
      // Resized
      NativeWindow.Resize += (sender, args) =>
      {
        // Note that GUI stays in scaled coordinates
        UpdateScreenSize();
        Resized?.Invoke(this, args);
      };
      // Closing
      NativeWindow.Closing += (sender, args) =>
      {
        _skiaContext.AbandonContext();
        _skiaContext = null;
        _glGraphicsContext.Dispose();
        _glGraphicsContext = null;
        Closing?.Invoke(this, args);
      };
    }

    private static GRBackendRenderTargetDesc CreateRenderTarget()
    {
      GL.GetInteger(GetPName.FramebufferBinding, out var framebuffer);
      GL.GetInteger(GetPName.StencilBits, out var stencil);
      GL.GetInteger(GetPName.Samples, out var samples);
      GL.GetRenderbufferParameter(RenderbufferTarget.Renderbuffer, RenderbufferParameterName.RenderbufferWidth,
        out var bufferWidth);
      GL.GetRenderbufferParameter(RenderbufferTarget.Renderbuffer, RenderbufferParameterName.RenderbufferHeight,
        out var bufferHeight);
      return new GRBackendRenderTargetDesc
      {
        Width = bufferWidth,
        Height = bufferHeight,
        Config = GRPixelConfig.Bgra8888,
        Origin = GRSurfaceOrigin.TopLeft,
        SampleCount = samples,
        StencilBits = stencil,
        RenderTargetHandle = (IntPtr) framebuffer
      };
    }
    private void UpdateScreenSize()
    {
      Width = (SFixed) NativeWindow.Width;
      Height = (SFixed) NativeWindow.Height;
      BoxArea = new Rectangle(new Point(), WindowClientSize);
      ComputeMinimumSize();
      LayoutChildren();
    }
  }

}