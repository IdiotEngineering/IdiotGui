using System;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using SkiaSharp;

namespace IdiotGui.Core
{
  public class BugReport : GameWindow
  {
    #region Fields / Properties

    private GRContext context;
    private GRBackendRenderTargetDesc renderTarget;

    #endregion

    public BugReport()
      : base(
        800,
        600,
        GraphicsMode.Default,
        "OpenTK Quick Start Sample",
        GameWindowFlags.Default,
        DisplayDevice.Default,
        1,
        0,
        GraphicsContextFlags.Debug)
    {
    }

    public static GRBackendRenderTargetDesc CreateRenderTarget()
    {
      int framebuffer, stencil, samples;
      GL.GetInteger(GetPName.FramebufferBinding, out framebuffer);
      // debug: framebuffer = 0
      GL.GetInteger(GetPName.StencilBits, out stencil);
      // debug: stencil = 0
      GL.GetInteger(GetPName.Samples, out samples);
      // debug: samples = 0

      var bufferWidth = 0;
      var bufferHeight = 0;

      //#if __IOS__ || __TVOS__
      GL.GetRenderbufferParameter(RenderbufferTarget.Renderbuffer, RenderbufferParameterName.RenderbufferWidth,
        out bufferWidth);
      // debug: bufferWidth = 0
      GL.GetRenderbufferParameter(RenderbufferTarget.Renderbuffer, RenderbufferParameterName.RenderbufferHeight,
        out bufferHeight);
      // debug: bufferHeight = 0
      //#endif

      return new GRBackendRenderTargetDesc
      {
        Width = bufferWidth,
        Height = bufferHeight,
        Config = GRPixelConfig.Bgra8888, // Question: Is this the right format and how to do it platform independent?
        Origin = GRSurfaceOrigin.TopLeft,
        SampleCount = samples,
        StencilBits = stencil,
        RenderTargetHandle = (IntPtr) framebuffer
      };
    }

    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);
      var glInterface = GRGlInterface.CreateNativeGlInterface();
      Debug.Assert(glInterface.Validate());

      context = GRContext.Create(GRBackend.OpenGL, glInterface);
      Debug.Assert(context.Handle != IntPtr.Zero);
      renderTarget = CreateRenderTarget();
    }

    protected override void OnUnload(EventArgs e)
    {
      base.OnUnload(e);
      context?.Dispose();
      context = null;
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
      base.OnRenderFrame(e);

      renderTarget.Width = Width;
      renderTarget.Height = Height;

      using (var surface = SKSurface.Create(context, renderTarget))
      {
        Debug.Assert(surface != null);
        Debug.Assert(surface.Handle != IntPtr.Zero);

        var canvas = surface.Canvas;

        canvas.Flush();

        // <this was a event delegate before
        var info = renderTarget;

        //canvas.Clear(SKColors.ForestGreen);

        // removing this block stop the exception and paints the windows green
        using (var paint = new SKPaint
        {
          Style = SKPaintStyle.Stroke,
          Color = SKColors.Red,
          StrokeWidth = 25
        })
        {
          paint.Style = SKPaintStyle.Fill;
          paint.Color = SKColors.Blue;
          canvas.DrawCircle(50, 50, 10, paint);
        }
        // />

        canvas.Flush();
      }
      context.Flush();
      SwapBuffers();
    }
  }

  public static class Thing
  {
    public static void DoThing()
    {
      using (var game2 = new BugReport())
      {
        game2.Run(30.0);
      }
    }
  }
}