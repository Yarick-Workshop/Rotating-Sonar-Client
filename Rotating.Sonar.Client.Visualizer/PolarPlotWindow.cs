namespace Rotating.Sonar.Client.Visualizer;

using System;
using Silk.NET.OpenGL.Legacy;
using Silk.NET.Windowing;
using Serilog;

internal class PolarPlotWindow : IDisposable
{
    private readonly PolarPlotData plotData;
    private IWindow window;
    private RenderOpenGL_1_1? render;
    
    private bool isDisposed = false;

    public PolarPlotWindow(PolarPlotData plotData, int width, int height, string title)
    {
        var options = WindowOptions.Default;
        options.Size = new Silk.NET.Maths.Vector2D<int>(width, height);
        options.Title = title;
        options.API = new GraphicsAPI(ContextAPI.OpenGL, new APIVersion(1, 1));

        window = Window.Create(options);

        window.Load += OnLoad;
        window.Render += OnRender;

        this.plotData = plotData;
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

        render = new RenderOpenGL_1_1(gl, plotData, this.window.Size.X, this.window.Size.Y, 200f);
        /* 
        Log.Information("OpenGL version: {Version}", gl.GetString(StringName.Version));
        Log.Information("OpenGL vendor: {Vendor}", gl.GetString(StringName.Vendor));
        Log.Information("OpenGL renderer: {Renderer}", gl.GetString(StringName.Renderer));
        Log.Information("OpenGL shading language version: {ShadingLanguageVersion}", gl.GetString(StringName.ShadingLanguageVersion));
        Log.Information("OpenGL extensions: {Extensions}", gl.GetString(StringName.Extensions));*/
    }

    private void OnRender(double delta)
    {
        this.render!.Render();
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
