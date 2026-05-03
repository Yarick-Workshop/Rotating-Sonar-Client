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
    private float zoomScale = 1f;
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

    private const float MinZoomScale = 0.25f;
    private const float MaxZoomScale = 4f;
    private const float ZoomFactorPerStep = 1.12f;
    private const float MaxWheelZoomExponent = 5f;
    private const float OverflowArrowHeadLengthPx = 8f;
    private const float OverflowArrowHalfWidthPx = 5f;
    private const float OverflowArrowRimInsetPx = 8f;
    private const float OverflowArrowMinTailRadiusPx = 26f;
    private const float InsideRingEpsilon = 1e-4f;
    private const float RangeRingStepCm = 100f;

    public void ZoomIn()
    {
        zoomScale = Math.Min(MaxZoomScale, zoomScale * ZoomFactorPerStep);
    }

    public void ZoomOut()
    {
        zoomScale = Math.Max(MinZoomScale, zoomScale / ZoomFactorPerStep);
    }

    public void ResetZoom()
    {
        zoomScale = 1f;
    }

    public void ZoomWheel(float deltaY)
    {
        if (deltaY == 0f)
            return;

        float signedMag = Math.Sign(deltaY) * Math.Min(Math.Abs(deltaY), MaxWheelZoomExponent);
        zoomScale *= MathF.Pow(ZoomFactorPerStep, signedMag);
        zoomScale = Math.Clamp(zoomScale, MinZoomScale, MaxZoomScale);
    }

    public void Render(double fps, bool showFps)
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

        if (showFps)
        {
            // Draw FPS in top-left corner
            string fpsText = $"{fps:F0}FPS";
            float textX = 10;
            float textY = height - 40;
            textRenderer.DrawText(fpsText, textX, textY);
        }

        string zoomText = $"Zoom {zoomScale * 100f:F0}%";
        float zoomMargin = 10f;
        textRenderer.DrawText(zoomText, width - zoomMargin, zoomMargin, HorizontalAlignment.Right);
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
            float cos = (float)Math.Cos(rad);
            float sin = (float)Math.Sin(rad);
            float rEcho = ScaledEchoRadius(distance);
            if (rEcho <= radius + InsideRingEpsilon)
            {
                gl.Vertex2(cx + rEcho * cos, cy + rEcho * sin);
            }
        }
        gl.End();

        foreach (var (angle, distance) in points)
        {
            double rad = -angle * Math.PI / 180.0;
            float cos = (float)Math.Cos(rad);
            float sin = (float)Math.Sin(rad);
            float rEcho = ScaledEchoRadius(distance);
            if (rEcho > radius + InsideRingEpsilon)
                DrawOverflowArrow(cx, cy, cos, sin, rEcho);
        }

        gl.PopMatrix();

        // Draw a white point at the center
        gl.Color3(1.0f, 1.0f, 1.0f);
        gl.Begin(GLEnum.Points);
        gl.Vertex2(cx, cy);
        gl.End();
    }

    private void DrawPolarGrid()
    {
        // Every 100 cm ring that fits inside the rim (same scale as echoes). Fills disc: farthest
        // ring at d = maxDistance/zoomScale coincides with the plot edge when zoom ≠ 1.
        gl.Color3(0.3f, 0.3f, 0.3f);
        if (maxDistance > 0f && zoomScale > 0f)
        {
            float dMax = maxDistance / zoomScale;
            for (float d = RangeRingStepCm; d <= dMax + InsideRingEpsilon; d += RangeRingStepCm)
            {
                float ringR = ScaledEchoRadius(d);
                if (ringR <= radius + InsideRingEpsilon)
                    DrawCircle(cx, cy, ringR);
            }
        }

        if (maxDistance > 0f)
            DrawCircle(cx, cy, radius);
        // Draw radial lines
        for (int a = 0; a < 360; a += 30)
        {
            gl.PushMatrix();

            gl.Translate(cx, cy, 0f); // Move to center
            gl.Rotate(a, 0f, 0f, 1f); // Rotate to angle
            
            gl.Begin(GLEnum.Lines);
                gl.Vertex2(0, 0); // Start at center
                gl.Vertex2(radius, 0); // End at outer ring along X-axis
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

    private float ScaledEchoRadius(float distanceCm)
    {
        return distanceCm / maxDistance * radius * zoomScale;
    }

    private void DrawOverflowArrow(float centerX, float centerY, float cos, float sin, float rEcho)
    {
        float overflow = rEcho - radius;
        if (overflow <= InsideRingEpsilon)
            return;

        float rTip = radius - OverflowArrowRimInsetPx;
        float maxHeadBack = rTip - OverflowArrowMinTailRadiusPx;
        if (maxHeadBack <= InsideRingEpsilon)
            return;

        float stretch = Math.Min(overflow * 0.08f, 12f);
        float headBack = Math.Min(OverflowArrowHeadLengthPx + stretch * 0.25f, maxHeadBack);
        headBack = Math.Max(headBack, 2.5f);

        float rHeadBase = rTip - headBack;

        float tipX = centerX + rTip * cos;
        float tipY = centerY + rTip * sin;
        float baseMidX = centerX + rHeadBase * cos;
        float baseMidY = centerY + rHeadBase * sin;
        float px = -sin;
        float py = cos;

        float w = OverflowArrowHalfWidthPx;
        float b0x = baseMidX + w * px;
        float b0y = baseMidY + w * py;
        float b1x = baseMidX - w * px;
        float b1y = baseMidY - w * py;

        gl.Color3(0.95f, 0.15f, 0.12f);
        gl.Begin(GLEnum.Triangles);
        gl.Vertex2(tipX, tipY);
        gl.Vertex2(b0x, b0y);
        gl.Vertex2(b1x, b1y);
        gl.End();
    }
}

#pragma warning restore CS0618
