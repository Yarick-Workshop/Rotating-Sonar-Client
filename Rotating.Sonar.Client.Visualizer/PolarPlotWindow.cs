namespace Rotating.Sonar.Client.Visualizer;

using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Graphics.OpenGL;

internal class PolarPlotWindow : GameWindow
{
    private readonly PolarPlotData plotData;
    public PolarPlotWindow(GameWindowSettings gws, NativeWindowSettings nws, PolarPlotData plotData)
        : base(gws, nws)
    {
        this.plotData = plotData;
    }
  
        protected override void OnLoad()
        {
            base.OnLoad();

            // Set clear color
            GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);

            // Disable depth testing
            GL.Disable(EnableCap.DepthTest);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            base.OnRenderFrame(args);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            DrawPolarPlot();
            SwapBuffers();
        }

        private void DrawPolarPlot()
        {
            int w = Size.X;
            int h = Size.Y;
            float cx = w / 2f;
            float cy = h / 2f;
            float radius = Math.Min(cx, cy) - 40;
            int maxDistance = 100;

            // Set up 2D projection
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            //GL.Ortho(0, w, 0, h, -1, 1);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();

            // Draw polar grid
            GL.Color3(0.3f, 0.3f, 0.3f);
            for (int r = 1; r <= 4; r++)
            {
                DrawCircle(cx, cy, radius * r / 4);
            }
            for (int a = 0; a < 360; a += 30)
            {
                double rad = a * Math.PI / 180.0;
                float x = cx + (float)(radius * 
    Math.Cos(rad));
            float y = cy + (float)(radius * Math.Sin(rad));
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex2(cx, cy);
            GL.Vertex2(x, y);
            GL.End();
        }

        GL.Begin(PrimitiveType.Lines);
        GL.Color3(1.0f, 1.0f, 0.0);
        GL.Vertex2(1, 1);
        GL.Vertex2(0, 0);
        GL.End();

        // Draw points
        var points = plotData.GetPoints();
        GL.PointSize(6f);
        GL.Begin(PrimitiveType.Points);
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
            GL.Color3(0.6, 0.9, 0f);
            GL.Vertex2(x, y);
        }
        // Test: Draw a white point at the center
        GL.Color3(1.0, 1.0, 1.0);
        GL.Vertex2(cx, cy);
        GL.End();
    }
    private void DrawCircle(float cx, float cy, float r)
    {
        GL.Begin(PrimitiveType.LineLoop);
        for (int i = 0; i < 64; i++)
        {
            double theta = 2.0 * Math.PI * i / 64;
            float x = cx + (float)(r * Math.Cos(theta));
            float y = cy + (float)(r * Math.Sin(theta));
            GL.Vertex2(x, y);
        }
        GL.End();
    }
}
