namespace Rotating.Sonar.Client.Visualizer;

using Serilog;
using Silk.NET.OpenGL.Legacy;

#pragma warning disable CS0618

public class RenderOpenGL_1_1 : IZoomable
{
    private const float OverflowArrowHeadLengthPx = 8f;
    private const float OverflowArrowHalfWidthPx = 5f;
    private const float OverflowArrowRimInsetPx = 8f;
    private const float OverflowArrowMinTailRadiusPx = 26f;
    private const float InsideRingEpsilon = 1e-4f;
    private const float RangeRingStepCm = 100f;
    private const float OuterTickLength10DegPx = 12f;
    private const float OuterTickLength5DegPx = 7f;

    private readonly GL gl;

    private readonly SonarDataCache sonarDataCache;
    private float cx;
    private float cy;
    private float radius;
    private float zoomScale = 1f;
    private readonly float maxDistanceCm;
    private float width;
    private float height;
    private readonly TextRenderOpenGL_1_1 textRenderer;
    private readonly VisualizerSettings visualizerColors;
    private bool showFps = true;
    private bool showZoom = true;

    public RenderOpenGL_1_1(GL gl, SonarDataCache sonarDataCache, AppSettings appSettings, float width, float height, float maxDistanceCm, TextRenderOpenGL_1_1 textRenderer)
    {
        this.gl = gl;
        this.sonarDataCache = sonarDataCache;

        // TODO, investigate why option this.UpdateViewport(width, height); does not work
        this.width = width;
        this.height = height;
        
        this.cx = width / 2f;
        this.cy = this.height / 2f;
        this.radius = MathF.Min(this.cx, this.cy) - 40;

        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(maxDistanceCm, 0f);
        this.maxDistanceCm = maxDistanceCm;
        this.textRenderer = textRenderer;
        this.visualizerColors = appSettings.Visualizer;

        var bg = this.visualizerColors.BackgroundColor;
        this.gl.ClearColor(bg.R, bg.G, bg.B, bg.A);
        this.gl.Disable(GLEnum.DepthTest);
        this.gl.Disable(GLEnum.CullFace);
    }

    public float ZoomScale
    {
        get
        {
            return this.zoomScale;
        }
    }

    public void SetZoom(float zoomScale)
    {
        this.zoomScale = Math.Clamp(zoomScale, RenderZoomControl.MinScale, RenderZoomControl.MaxScale);
    }

    public bool ToggleFpsDisplay()
    {
        this.showFps = !this.showFps;
        return this.showFps;
    }

    public bool ToggleZoomDisplay()
    {
        this.showZoom = !this.showZoom;
        return this.showZoom;
    }

    public void Render(double fps)
    {
        this.gl.Viewport(0, 0, (uint)this.width, (uint)this.height);
        this.gl.Clear(ClearBufferMask.ColorBufferBit);
        this.gl.MatrixMode(GLEnum.Projection);
        this.gl.LoadIdentity();
        // 2D orthographic projection
        this.gl.Ortho(0, this.width, 0, this.height, -1, 1);
        this.gl.MatrixMode(GLEnum.Modelview);
        this.gl.LoadIdentity();

        this.DrawPoints();
        this.DrawPolarGrid();

        var uiText = this.visualizerColors.UiTextColor;
        this.gl.Color4(uiText.R, uiText.G, uiText.B, uiText.A);

        if (this.showFps)
        {
            // Draw FPS in top-left corner
            string fpsText = $"{fps:F0}FPS";
            float textX = 10;
            float textY = this.height - 40;
            this.textRenderer.DrawText(fpsText, textX, textY);
        }

        if (this.showZoom)
        {
            string zoomText = $"Zoom {this.zoomScale * 100f:F0}%";
            float zoomMargin = 10f;
            this.textRenderer.DrawText(zoomText, this.width - zoomMargin, zoomMargin, HorizontalAlignment.Right);
        }
    }

    public void UpdateViewport(float newWidth, float newHeight)
    {
        this.width = newWidth;
        this.height = newHeight;

        // Update center and radius based on new dimensions
        this.cx = this.width / 2f;
        this.cy = this.height / 2f;
        this.radius = MathF.Min(this.cx, this.cy) - 40;
    }

    private void DrawPoints()
    {
        var points = this.sonarDataCache.GetPoints();

        if (points.Count == 0)
        {
            Log.Warning("No points to draw in polar plot.");
            return;
        }

        this.gl.PushMatrix();
        this.gl.Translate(this.cx, this.cy, 0f);
        this.gl.Rotate(90f, 0f, 0f, 1f); // 90 degrees CCW around Z
        this.gl.Translate(-this.cx, -this.cy, 0f);
        this.gl.PointSize(12f);
        var echoPoint = this.visualizerColors.Points.EchoColor;
        this.gl.Color4(echoPoint.R, echoPoint.G, echoPoint.B, echoPoint.A);
        this.gl.Begin(GLEnum.Points);

        var overflowArrows = new List<(float Cos, float Sin, float Radius)>();
        foreach (var (angle, distance) in points)
        {
            double rad = -angle * Math.PI / 180.0;
            float cos = (float)Math.Cos(rad);
            float sin = (float)Math.Sin(rad);   
            float rEcho = this.ScaledEchoRadius(distance);
            if (rEcho <= this.radius + InsideRingEpsilon)
            {
                this.gl.Vertex2(this.cx + rEcho * cos, this.cy + rEcho * sin);
            }
            else
            {
                overflowArrows.Add((cos, sin, rEcho));
            }
        }
        this.gl.End();

        this.DrawOverflowArrows(overflowArrows);

        this.gl.PopMatrix();

        // Draw a white point at the center
        var centerPoint = this.visualizerColors.Points.CenterColor;
        this.gl.Color4(centerPoint.R, centerPoint.G, centerPoint.B, centerPoint.A);
        this.gl.Begin(GLEnum.Points);
        this.gl.Vertex2(this.cx, this.cy);
        this.gl.End();
    }

    private void DrawPolarGrid()
    {
        this.gl.Enable(GLEnum.Blend);
        this.gl.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);

        // Every 100 cm ring that fits inside the rim (same scale as echoes). Fills disc: farthest
        // ring at d = maxDistanceCm/zoomScale coincides with the plot edge when zoom ≠ 1.
        var gridLine = this.visualizerColors.Grid.LineColor;
        this.gl.Color4(gridLine.R, gridLine.G, gridLine.B, gridLine.A);
        if (this.maxDistanceCm > 0f && this.zoomScale > 0f)
        {
            float dMax = this.maxDistanceCm / this.zoomScale;
            for (int i = 1; i * RangeRingStepCm <= dMax + InsideRingEpsilon; i++)
            {
                float ringR = this.ScaledEchoRadius(i * RangeRingStepCm);
                if (ringR <= this.radius + InsideRingEpsilon)
                {
                    this.gl.DrawCircle(this.cx, this.cy, ringR);
                }
            }
        }

        if (this.maxDistanceCm > 0f)
        {
            this.gl.DrawCircle(this.cx, this.cy, this.radius, lineWidth: 2f);
        }

        // Draw radial lines
        for (int a = 0; a < 360; a += 30)
        {
            this.gl.PushMatrix();

            this.gl.Translate(this.cx, this.cy, 0f); // Move to center
            this.gl.Rotate(a, 0f, 0f, 1f); // Rotate to angle
            
            this.gl.Begin(GLEnum.Lines);
                this.gl.Vertex2(0, 0); // Start at center
                this.gl.Vertex2(this.radius, 0); // End at outer ring along X-axis
            this.gl.End();

            this.gl.PopMatrix();
        }

        // Draw short rim ticks every 5 degrees; longer marks every 10 degrees.
        var gridTickLabel = this.visualizerColors.Grid.TickLabelColor;
        this.gl.Color4(gridTickLabel.R, gridTickLabel.G, gridTickLabel.B, gridTickLabel.A);
        this.gl.Begin(GLEnum.Lines);
        for (int a = 0; a < 360; a += 5)
        {
            float tickLength = a % 10 == 0 ? OuterTickLength10DegPx : OuterTickLength5DegPx;
            double radians = a * Math.PI / 180.0;
            float cos = (float)Math.Cos(radians);
            float sin = (float)Math.Sin(radians);

            float innerX = this.cx + this.radius * cos;
            float innerY = this.cy + this.radius * sin;
            float outerX = this.cx + (this.radius + tickLength) * cos;
            float outerY = this.cy + (this.radius + tickLength) * sin;

            this.gl.Vertex2(innerX, innerY);
            this.gl.Vertex2(outerX, outerY);
        }
        this.gl.End();

        var gridLabel = this.visualizerColors.Grid.LabelColor;
        this.gl.Color4(gridLabel.R, gridLabel.G, gridLabel.B, gridLabel.A);

        // Draw compass-like degree labels around the largest circle
        for (int a = 0; a < 360; a += 30)
        {
            string angleText = $"{a}°";
            
            this.gl.PushMatrix();

            this.gl.Translate(this.cx, this.cy, 0f); // Move to center
            this.gl.Rotate(-a + 90, 0f, 0f, 1f); // Rotate to angle (CW direction)
            this.gl.Translate(this.radius, 0, 0f); // Move to label position
            this.gl.Rotate(-90, 0f, 0f, 1f);

            this.textRenderer.DrawText(angleText, 0, 0, HorizontalAlignment.Center);

            this.gl.PopMatrix();
        }
    }

    private float ScaledEchoRadius(float distanceCm)
    {
        return distanceCm / this.maxDistanceCm * this.radius * this.zoomScale;
    }

    private void DrawOverflowArrows(List<(float Cos, float Sin, float Radius)> arrows)
    {
        if (arrows.Count == 0)
        {
            return;
        }

        var overflowArrow = this.visualizerColors.Points.OverflowArrowColor;
        this.gl.Color4(overflowArrow.R, overflowArrow.G, overflowArrow.B, overflowArrow.A);
        foreach (var (cos, sin, rEcho) in arrows)
        {
            this.DrawOverflowArrow(this.cx, this.cy, cos, sin, rEcho);
        }
    }

    private void DrawOverflowArrow(float centerX, float centerY, float cos, float sin, float rEcho)
    {
        float overflow = rEcho - this.radius;
        if (overflow <= InsideRingEpsilon)
        {
            return;
        }

        float rTip = this.radius - OverflowArrowRimInsetPx;
        float maxHeadBack = rTip - OverflowArrowMinTailRadiusPx;
        if (maxHeadBack <= InsideRingEpsilon)
        {
            return;
        }

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

        this.gl.Begin(GLEnum.Triangles);
        this.gl.Vertex2(tipX, tipY);
        this.gl.Vertex2(b0x, b0y);
        this.gl.Vertex2(b1x, b1y);
        this.gl.End();
    }
}

#pragma warning restore CS0618
