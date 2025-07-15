namespace Rotating.Sonar.Client.Visualizer;

using System;
using Silk.NET.Maths;
using Silk.NET.OpenGL.Legacy;
using Silk.NET.Windowing;
using Serilog;
using System.Collections.Generic;
using System.Linq;

internal class PolarPlotWindow : IDisposable
{
    private readonly SonarDataCache sonarDataCache;
    private IWindow window;
    private RenderOpenGL_1_1? render;
    private TextRenderOpenGL_1_1? textRenderer;
    private double latestFps = 0;
    private readonly Queue<double> fpsHistory = new Queue<double>(7);
    
    private bool isDisposed = false;

    public PolarPlotWindow(SonarDataCache sonarDataCache, int width, int height, string title)
    {
        var options = WindowOptions.Default;
        options.Size = new Silk.NET.Maths.Vector2D<int>(width, height);
        options.Title = title;
        options.API = new GraphicsAPI(ContextAPI.OpenGL, new APIVersion(1, 1));

        window = Window.Create(options);

        window.Load += OnLoad;
        window.Render += OnRender;
        window.Resize += OnResize;

        this.sonarDataCache = sonarDataCache;
    }

    public void Run()
    {        
        window!.Run();
    }

    private void OnLoad()
    {
        var gl = GL.GetApi(window);
        var width = window.Size.X;
        var height = window.Size.Y;

        Log.Information("OpenGL Polar Plot Visualizer initialized with size {Width}x{Height}", width, height);

        textRenderer = new TextRenderOpenGL_1_1(gl, "°");
        render = new RenderOpenGL_1_1(gl, this.sonarDataCache, this.window.Size.X, this.window.Size.Y, 200f, textRenderer);
        /* 
        Log.Information("OpenGL version: {Version}", gl.GetString(StringName.Version));
        Log.Information("OpenGL vendor: {Vendor}", gl.GetString(StringName.Vendor));
        Log.Information("OpenGL renderer: {Renderer}", gl.GetString(StringName.Renderer));
        Log.Information("OpenGL shading language version: {ShadingLanguageVersion}", gl.GetString(StringName.ShadingLanguageVersion));
        Log.Information("OpenGL extensions: {Extensions}", gl.GetString(StringName.Extensions));*/
    }

    private void OnRender(double delta)
    {
        if (delta > 0)
        {// TODO, optimize
            double fps = 1.0 / delta;
            if (fpsHistory.Count == 120)
                fpsHistory.Dequeue();
            fpsHistory.Enqueue(fps);
            latestFps = fpsHistory.Average();
        }
        this.render!.Render(latestFps);
    }

    private void OnResize(Vector2D<int> newSize)
    {
        render?.UpdateViewport((float)newSize.X, (float)newSize.Y);
    }

    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }

        isDisposed = true;
        window?.Dispose();
    }
}
