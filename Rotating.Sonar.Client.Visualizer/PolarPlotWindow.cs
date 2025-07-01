namespace Rotating.Sonar.Client.Visualizer;

using Silk.NET.OpenGL.Legacy;
using Silk.NET.Windowing;

internal class PolarPlotWindow : IDisposable
{
    private readonly PolarPlotData plotData;
    private IWindow? window;
    private GL? gl;
    private int width, height;
    private float cx, cy, radius;
    private int maxDistance = 100;
    private bool isDisposed = false;

    public PolarPlotWindow(PolarPlotData plotData)
    {
        this.plotData = plotData;
    }

    public void Run(int width, int height, string title)
    {
        var options = WindowOptions.Default;
        options.Size = new Silk.NET.Maths.Vector2D<int>(width, height);
        options.Title = title;
        options.API = new GraphicsAPI(ContextAPI.OpenGL, new APIVersion(2, 1));
        window = Window.Create(options);
        window.Load += OnLoad;
        window.Render += OnRender;
        window.Run();
    }

    private void OnLoad()
    {
        gl = GL.GetApi(window!);
        width = window!.Size.X;
        height = window!.Size.Y;
        cx = width / 2f;
        cy = height / 2f;
        radius = MathF.Min(cx, cy) - 40;
        gl!.ClearColor(0f, 0f, 0f, 1f);
        gl.Disable(GLEnum.DepthTest);
    }

    private void OnRender(double delta)
    {
        if (gl == null) return;
        gl.Viewport(0, 0, (uint)window!.Size.X, (uint)window!.Size.Y);
        gl.Clear(ClearBufferMask.ColorBufferBit);
        gl.MatrixMode(GLEnum.Projection);
        gl.LoadIdentity();
        // 2D orthographic projection
        gl.Ortho(0, width, 0, height, -1, 1);
        gl.MatrixMode(GLEnum.Modelview);
        gl.LoadIdentity();
        DrawPolarGrid();
        DrawPoints();
    }

    private void DrawPolarGrid()
    {
        // Draw circles
        gl!.Color3(0.3f, 0.3f, 0.3f);
        for (int r = 1; r <= 4; r++)
        {
            DrawCircle(cx, cy, radius * r / 4);
        }
        // Draw radial lines
        for (int a = 0; a < 360; a += 30)
        {
            double rad = a * Math.PI / 180.0;
            float x = cx + (float)(radius * Math.Cos(rad));
            float y = cy + (float)(radius * Math.Sin(rad));
            gl.Begin(GLEnum.Lines);
            gl.Vertex2(cx, cy);
            gl.Vertex2(x, y);
            gl.End();
        }
    }

    private void DrawPoints()
    {
        var points = plotData.GetPoints();
        gl!.PointSize(6f);
        gl.Begin(GLEnum.Points);
        foreach (var (angle, distance) in points)
        {
            double rad = angle * Math.PI / 180.0;
            float r = (float)distance / maxDistance * radius;
            float x = cx + (float)(r * Math.Cos(rad));
            float y = cy + (float)(r * Math.Sin(rad));
            // Color: green (close) to red (far)
            float t = Math.Clamp((float)distance / maxDistance, 0f, 1f);
            float red = t;
            float green = 1f - t;
            gl.Color3(red, green, 0f);
            gl.Vertex2(x, y);
        }
        // Draw a white point at the center
        gl.Color3(1.0f, 1.0f, 1.0f);
        gl.Vertex2(cx, cy);
        gl.End();
    }

    private void DrawCircle(float cx, float cy, float r)
    {
        gl!.Begin(GLEnum.LineLoop);
        for (int i = 0; i < 64; i++)
        {
            double theta = 2.0 * Math.PI * i / 64;
            float x = cx + (float)(r * Math.Cos(theta));
            float y = cy + (float)(r * Math.Sin(theta));
            gl.Vertex2(x, y);
        }
        gl.End();
    }

    public void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;
        window.Dispose();
    }
}
