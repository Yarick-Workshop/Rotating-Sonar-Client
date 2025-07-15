namespace Rotating.Sonar.Client.Visualizer;

using Serilog;
using Silk.NET.OpenGL.Legacy;

#pragma warning disable CS0618

public class RenderOpenGL_1_1
{
    private readonly GL gl;

    private readonly SonarDataCache sonarDataCache;
    private float cx;
    private float cy;
    private float radius;
    private readonly float maxDistance;
    private float width;
    private float height;
    private readonly TextRenderOpenGL_1_1 textRenderer;

    public RenderOpenGL_1_1(GL gl, SonarDataCache sonarDataCache, float width, float heigh, float maxDistance, TextRenderOpenGL_1_1 textRenderer)
    {
        this.gl = gl;
        this.sonarDataCache = sonarDataCache;

        // TODO, investigate why option this.UpdateViewport(width, height); does not work
        this.width = width;
        this.height = heigh;
        
        this.cx = width / 2f;
        this.cy = height / 2f;
        this.radius = MathF.Min(cx, cy) - 40;

        this.maxDistance = maxDistance;
        this.textRenderer = textRenderer;

        gl.ClearColor(0f, 0f, 0f, 1f);
        gl.Disable(GLEnum.DepthTest);
        gl.Disable(GLEnum.CullFace);
    }

    public void Render(double fps)
    {
        gl.Viewport(0, 0, (uint)width, (uint)height);
        gl.Clear(ClearBufferMask.ColorBufferBit);
        gl.MatrixMode(GLEnum.Projection);
        gl.LoadIdentity();
        // 2D orthographic projection
        gl.Ortho(0, width, 0, height, -1, 1);
        gl.MatrixMode(GLEnum.Modelview);
        gl.LoadIdentity();

        this.DrawPolarGrid();
        this.DrawPoints();
        // Draw FPS in top-left corner
        string fpsText = $"{fps:F0}FPS";
        float textX = 10;
        float textY = height - 40;
        textRenderer.DrawText(fpsText, textX, textY);
    }

    public void UpdateViewport(float newWidth, float newHeight)
    {
        this.width = newWidth;
        this.height = newHeight;
        
        // Update center and radius based on new dimensions
        this.cx = width / 2f;
        this.cy = height / 2f;
        this.radius = MathF.Min(cx, cy) - 40;
    }

    private void DrawPoints()
    {
        // TODO: refactor the code
        var points = sonarDataCache.GetPoints();

        if (points.Count == 0)
        {
            Log.Warning("No points to draw in polar plot.");
            return;
        }

        gl.PushMatrix();
        gl.Translate(cx, cy, 0f);
        gl.Rotate(90f, 0f, 0f, 1f); // 90 degrees CCW around Z
        gl.Translate(-cx, -cy, 0f);
        gl.PointSize(12f);
        gl.Color3(1.0f, 0.2f, 0.2f);
        gl.Begin(GLEnum.Points);
        foreach (var (angle, distance) in points)
        {
            double rad = -angle * Math.PI / 180.0;
            float r = (float)distance / maxDistance * radius;
            float x = cx + (float)(r * Math.Cos(rad));
            float y = cy + (float)(r * Math.Sin(rad));
            gl.Vertex2(x, y);
        }
        gl.End();
        gl.PopMatrix();

        // Draw a white point at the center
        gl.Color3(1.0f, 1.0f, 1.0f);
        gl.Begin(GLEnum.Points);
        gl.Vertex2(cx, cy);
        gl.End();
    }

    private void DrawPolarGrid()
    {
        // Draw circles
        gl.Color3(0.3f, 0.3f, 0.3f);
        for (int r = 1; r <= 4; r++)
        {
            DrawCircle(cx, cy, radius * r / 4);
        }
        // Draw radial lines
        for (int a = 0; a < 360; a += 30)
        {
            gl.PushMatrix();

            gl.Translate(cx, cy, 0f); // Move to center
            gl.Rotate(a, 0f, 0f, 1f); // Rotate to angle
            
            gl.Begin(GLEnum.Lines);
                gl.Vertex2(0, 0); // Start at center
                gl.Vertex2(radius, 0); // End at radius distance along X-axis
            gl.End();

            gl.PopMatrix();
        }

        // Draw compass-like degree labels around the largest circle
        for (int a = 0; a < 360; a += 30)
        {
            string angleText = $"{a}°";
            
            gl.PushMatrix();

            gl.Translate(cx, cy, 0f); // Move to center
            gl.Rotate(-a+ 90, 0f, 0f, 1f); // Rotate to angle (CW direction)
            gl.Translate(radius, 0, 0f); // Move to label position
            gl.Rotate(-90, 0f, 0f, 1f);

            textRenderer.DrawText(angleText, 0, 0, HorizontalAlignment.Center);

            gl.PopMatrix();
        }
    }

    private void DrawCircle(float cx, float cy, float r)
    {
        gl.Begin(GLEnum.LineLoop);
        for (int i = 0; i < 64; i++)
        {
            double theta = 2.0 * Math.PI * i / 64;
            float x = cx + (float)(r * Math.Cos(theta));//TODO, optimize
            float y = cy + (float)(r * Math.Sin(theta));
            gl.Vertex2(x, y);
        }
        gl.End();
    }
}

#pragma warning restore CS0618