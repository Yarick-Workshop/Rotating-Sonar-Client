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
    private IInputContext? inputContext;
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

        this.window = Window.Create(options);

        this.window.Load += OnLoad;
        this.window.Render += OnRender;
        this.window.Resize += OnResize;

        this.sonarDataCache = sonarDataCache;
    }

    public void Run()
    {        
        this.window!.Run();
    }

    private void OnLoad()
    {
        var gl = GL.GetApi(this.window);
        var width = this.window.Size.X;
        var height = this.window.Size.Y;

        this.previousSize = this.window.Size;

        Log.Information("OpenGL Polar Plot Visualizer initialized with size {Width}x{Height}", width, height);

        this.inputContext = this.window.CreateInput();
        for (int i = 0; i < this.inputContext.Keyboards.Count; i++)
        {
            this.inputContext.Keyboards[i].KeyDown += OnKeyDown;
        }

        for (int i = 0; i < this.inputContext.Mice.Count; i++)
        {
            this.inputContext.Mice[i].Scroll += OnMouseScroll;
        }

        this.textRenderer = new TextRenderOpenGL_1_1(gl, "°");
        this.render = new RenderOpenGL_1_1(gl, this.sonarDataCache, this.window.Size.X, this.window.Size.Y, 200f, this.textRenderer);
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
            if (this.fpsHistory.Count == 120)
                this.fpsHistory.Dequeue();
            this.fpsHistory.Enqueue(fps);
            this.latestFps = this.fpsHistory.Average();
        }
        this.render!.Render(this.latestFps, this.showFps);
    }

    private void OnResize(Vector2D<int> newSize)
    {
        Log.Information(
            "Window resized: {OldWidth}x{OldHeight} -> {NewWidth}x{NewHeight}", 
            this.previousSize.X,
            this.previousSize.Y,
            newSize.X,
            newSize.Y);

        this.previousSize = newSize;

        this.render?.UpdateViewport((float)newSize.X, (float)newSize.Y);
    }

    private void OnKeyDown(IKeyboard keyboard, Key key, int scancode)
    {
        // Ctrl shortcuts use only this event's keyboard. Wheel zoom uses IsCtrlPressed(), which scans all keyboards
        // so Ctrl held on another physical device still qualifies.
        bool ctrl = keyboard.IsKeyPressed(Key.ControlLeft) || keyboard.IsKeyPressed(Key.ControlRight);

        if (ctrl)
        {
            switch (key)
            {
                case Key.Equal:
                case Key.KeypadAdd:
                    this.render?.ZoomIn();
                    return;
                case Key.Minus:
                case Key.KeypadSubtract:
                    this.render?.ZoomOut();
                    return;
                case Key.D0:
                case Key.Keypad0:
                    this.render?.ResetZoom();
                    return;
            }
        }

        switch (key)
        {
            case Key.F:
            case Key.F3:
                this.showFps = !this.showFps;
                Log.Information("FPS display toggled: {ShowFps}", this.showFps);
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

    private bool IsCtrlPressed()
    {
        // Scroll events carry no keyboard; any keyboard may have held Ctrl (multi-keyboard / mixed hardware).
        foreach (IKeyboard keyboard in this.inputContext!.Keyboards)
        {
            if (keyboard.IsKeyPressed(Key.ControlLeft) || keyboard.IsKeyPressed(Key.ControlRight))
                return true;
        }

        return false;
    }

    private void OnMouseScroll(IMouse mouse, ScrollWheel scroll)
    {
        if (!this.IsCtrlPressed())
            return;

        this.render?.ZoomWheel(scroll.Y);
    }

    public void ToggleFullscreen()
    {
        var oldState = this.window.WindowState;

        if (oldState == WindowState.Fullscreen)
        {
            this.window.WindowState = WindowState.Normal;
        }
        else
        {
            // It is not an useless line.
            // It is a fix of a bug when going back to normal from fullscreen
            // STR: Maximize => Full screen => Try to go back with either F11 or Alt+Enter
            this.window.WindowState = WindowState.Normal;

            this.window.WindowState = WindowState.Fullscreen;
        }

        Log.Information(
            "Toggling fullscreen: {OldState} -> {NewState}",
            oldState, 
            this.window.WindowState);
    }

    private void ReleaseInput()
    {
        if (this.inputContext == null)
            return;

        foreach (IKeyboard keyboard in this.inputContext.Keyboards)
            keyboard.KeyDown -= OnKeyDown;

        foreach (IMouse mouse in this.inputContext.Mice)
            mouse.Scroll -= OnMouseScroll;

        if (this.inputContext is IDisposable disposable)
            disposable.Dispose();

        this.inputContext = null;
    }

    public void Dispose()
    {
        if (this.isDisposed)
        {
            return;
        }

        this.isDisposed = true;
        this.ReleaseInput();
        this.window?.Dispose();
    }
}
