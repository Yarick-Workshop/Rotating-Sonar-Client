namespace Rotating.Sonar.Client.Visualizer;

using Rotating.Sonar.Client.Common.Settings;
using Rotating.Sonar.Client.Common.Zoom;
using System;
using System.Runtime.InteropServices;
using Silk.NET.Maths;
using Silk.NET.OpenGL.Legacy;
using Silk.NET.Windowing;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using Silk.NET.Input;

internal class ScanDisplayWindow : IDisposable
{
    private const int FpsWindowSize = 60;// TODO to config
    private const float MaxDistanceCm = 200f;

    private readonly ScanPointBuffer scanPointBuffer;
    private readonly AppSettings appSettings;
    private IWindow window;
    private IInputContext? inputContext;
    private OpenGlScanDisplayRenderer? render;
    private RenderZoomControl? zoomControl;
    private OpenGlTextRenderer? textRenderer;
    private double latestFps = 0;
    private readonly Queue<double> fpsHistory = new Queue<double>(FpsWindowSize);

    private Vector2D<int> previousSize;
    
    private bool _disposed = false;

    public ScanDisplayWindow(
        ScanPointBuffer scanPointBuffer,
        AppSettings appSettings,
        int windowWidthPx,
        int windowHeightPx,
        string title)
    {
        var options = WindowOptions.Default;
        options.Size = new Vector2D<int>(windowWidthPx, windowHeightPx);
        options.Title = title;
        options.API = new GraphicsAPI(ContextAPI.OpenGL, new APIVersion(1, 1));

        this.window = Window.Create(options);

        this.window.Load += this.OnLoad;
        this.window.Render += this.OnRender;
        this.window.Resize += this.OnResize;

        this.scanPointBuffer = scanPointBuffer;
        this.appSettings = appSettings;
    }

    public void Run()
    {        
        this.window!.Run();
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

    public void Dispose()
    {
        if (this._disposed)
        {
            return;
        }

        this._disposed = true;
        this.ReleaseInput();
        this.window?.Dispose();
    }

    private void OnLoad()
    {
        var gl = GL.GetApi(this.window);
        var width = this.window.Size.X;
        var height = this.window.Size.Y;

        this.previousSize = this.window.Size;

        Log.Information("OpenGL scan display initialized with size {Width}x{Height}", width, height);

        this.inputContext = this.window.CreateInput();
        for (int i = 0; i < this.inputContext.Keyboards.Count; i++)
        {
            this.inputContext.Keyboards[i].KeyDown += this.OnKeyDown;
        }

        for (int i = 0; i < this.inputContext.Mice.Count; i++)
        {
            this.inputContext.Mice[i].Scroll += this.OnMouseScroll;
        }

        this.textRenderer = new OpenGlTextRenderer(gl, "°");
        this.render = new OpenGlScanDisplayRenderer(gl, this.scanPointBuffer, this.appSettings, this.window.Size.X, this.window.Size.Y, MaxDistanceCm, this.textRenderer);
        this.zoomControl = new RenderZoomControl(this.render);

        LogOpenGlDriverInfo(gl);
    }

    private void OnRender(double deltaSeconds)
    {
        if (deltaSeconds > 0)
        {
            // TODO, optimize
            double fps = 1.0 / deltaSeconds;
            if (this.fpsHistory.Count == FpsWindowSize)
            {
                this.fpsHistory.Dequeue();
            }

            this.fpsHistory.Enqueue(fps);
            this.latestFps = this.fpsHistory.Average();
        }
        this.render!.Render(this.latestFps);
    }

    private void OnResize(Vector2D<int> newWindowSize)
    {
        Log.Information(
            "Window resized: {OldWidth}x{OldHeight} -> {NewWidth}x{NewHeight}", 
            this.previousSize.X,
            this.previousSize.Y,
            newWindowSize.X,
            newWindowSize.Y);

        this.previousSize = newWindowSize;

        this.render?.UpdateViewport((float)newWindowSize.X, (float)newWindowSize.Y);
    }

    private void OnKeyDown(IKeyboard keyboard, Key key, int scancode)
    {
        // Ctrl shortcuts use only this event's keyboard. Wheel zoom uses IsCtrlPressed(), which scans all keyboards
        // so Ctrl held on another physical device still qualifies.
        bool ctrl = keyboard.IsKeyPressed(Key.ControlLeft) || keyboard.IsKeyPressed(Key.ControlRight);

        switch (key)
        {
            case Key.Equal:
            case Key.KeypadAdd:
                if (ctrl)
                {
                    this.zoomControl?.ZoomIn();
                }

                break;
            case Key.Minus:
            case Key.KeypadSubtract:
                if (ctrl)
                {
                    this.zoomControl?.ZoomOut();
                }

                break;
            case Key.D0:
            case Key.Keypad0:
                if (ctrl)
                {
                    this.zoomControl?.ResetZoom();
                }

                break;
            case Key.P:
                ScanPointRenderStyle pointStyle = this.render!.ToggleScanPointRenderStyle();
                Log.Information("Point style toggled: {PointStyle}", pointStyle);
                break;
            case Key.F:
            case Key.F3:
                bool showFps = this.render!.ToggleFpsDisplay();
                Log.Information("FPS display toggled: {ShowFps}", showFps);
                break;
            case Key.Z:
                bool showZoom = this.render!.ToggleZoomDisplay();
                Log.Information("Zoom display toggled: {ShowZoom}", showZoom);
                break;
            case Key.S:
                bool showSweep = this.render!.ToggleSweepDisplay();
                Log.Information("Sweep display toggled: {ShowSweep}", showSweep);
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
            {
                return true;
            }
        }

        return false;
    }

    private void OnMouseScroll(IMouse mouse, ScrollWheel scrollWheel)
    {
        if (!this.IsCtrlPressed())
        {
            return;
        }

        this.zoomControl?.ZoomWheel(scrollWheel.Y);
    }

    private void ReleaseInput()
    {
        if (this.inputContext == null)
        {
            return;
        }

        foreach (IKeyboard keyboard in this.inputContext.Keyboards)
        {
            keyboard.KeyDown -= this.OnKeyDown;
        }

        foreach (IMouse mouse in this.inputContext.Mice)
        {
            mouse.Scroll -= this.OnMouseScroll;
        }

        if (this.inputContext is IDisposable disposable)
        {
            disposable.Dispose();
        }

        this.inputContext = null;
    }

    private static void LogOpenGlDriverInfo(GL gl)
    {
        Log.Information(
            "OpenGL version: {Version}; OpenGL vendor: {Vendor}; OpenGL renderer: {Renderer}",
            GlString(gl, StringName.Version),
            GlString(gl, StringName.Vendor),
            GlString(gl, StringName.Renderer));
    }

    private static unsafe string GlString(GL gl, StringName name)
    {
        byte* p = gl.GetString(name);
        if (p == null)
        {
            return "(not available)";
        }

        return Marshal.PtrToStringUTF8((nint)p) ?? "(not available)";
    }
}
