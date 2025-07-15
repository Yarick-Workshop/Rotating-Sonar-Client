namespace Rotating.Sonar.Client.Visualizer;

using System;
using Silk.NET.Maths;
using Silk.NET.OpenGL.Legacy;
using Silk.NET.Windowing;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using Silk.NET.Input;

internal class PolarPlotWindow : IDisposable
{
    private readonly SonarDataCache sonarDataCache;
    private IWindow window;
    private RenderOpenGL_1_1? render;
    private TextRenderOpenGL_1_1? textRenderer;
    private double latestFps = 0;
    private readonly Queue<double> fpsHistory = new Queue<double>(7);
    private bool showFps = true;

    private Vector2D<int> previousSize;
    
    private bool isDisposed = false;

    public PolarPlotWindow(SonarDataCache sonarDataCache, int width, int height, string title)
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(width, height);
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

        previousSize = window.Size;

        Log.Information("OpenGL Polar Plot Visualizer initialized with size {Width}x{Height}", width, height);

        IInputContext input = window.CreateInput();
        for (int i = 0; i < input.Keyboards.Count; i++)
        {
            input.Keyboards[i].KeyDown += OnKeyDown;
        }

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
        this.render!.Render(latestFps, showFps);
    }

    private void OnResize(Vector2D<int> newSize)
    {
        Log.Information(
            "Window resized: {OldWidth}x{OldHeight} -> {NewWidth}x{NewHeight}", 
            previousSize.X,
            previousSize.Y,
            newSize.X,
            newSize.Y);

        this.previousSize = newSize;

        this.render?.UpdateViewport((float)newSize.X, (float)newSize.Y);
    }

    private void OnKeyDown(IKeyboard keyboard, Key key, int scancode)
    {
        switch (key)
        {
            case Key.F:
            case Key.F3:
                showFps = !showFps;
                Log.Information("FPS display toggled: {ShowFps}", showFps);
                break;
            case Key.F11:
                this.ToggleFullscreen();
                break;
            case Key.Enter:
                if (keyboard.IsKeyPressed(Key.AltLeft))
                {
                    this.ToggleFullscreen();
                }
                break;
            case Key.Escape:
                Log.Information("Window closing requested via Escape key");
                this.window?.Close();
                break;
        }
    }

    public void ToggleFullscreen()
    {
        var oldState = window.WindowState;

        if (oldState == WindowState.Fullscreen)
        {
            window.WindowState = WindowState.Normal;
        }
        else
        {
            // It is not an useless line.
            // It is a fix of a bug when going back to normal from fullscreen
            // STR: Maximize => Full screen => Try to go back with either F11 or Alt+Enter
            window.WindowState = WindowState.Normal;

            window.WindowState = WindowState.Fullscreen;
        }

        Log.Information(
            "Toggling fullscreen: {OldState} -> {NewState}",
            oldState, 
            window.WindowState);
    }

    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }

        isDisposed = true;
        this.window?.Dispose();
    }
}
