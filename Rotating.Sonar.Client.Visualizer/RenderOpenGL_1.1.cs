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
    private readonly OpenGL_1_1_Primitives glPrimitives;

    private readonly SonarDataCache sonarDataCache;
    private float cx;
    private float cy;
    private float radius;
    private float zoomScale = 1f;
    private readonly float maxDistanceCm;
    private float width;
    private float height;
    private readonly TextRenderOpenGL_1_1 textRenderer;
    private readonly VisualizerSettings visualizerSettings;
    private bool showFps = true;
    private bool showZoom = true;
    private bool showRay = false;
    private PointRenderStyle pointRenderStyle = PointRenderStyle.SolidSquare;

    public RenderOpenGL_1_1(GL gl, SonarDataCache sonarDataCache, AppSettings appSettings, float width, float height, float maxDistanceCm, TextRenderOpenGL_1_1 textRenderer)
    {
        this.gl = gl;
        this.glPrimitives = new OpenGL_1_1_Primitives(gl, appSettings.Visualizer);
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
        this.visualizerSettings = appSettings.Visualizer;

        var bg = this.visualizerSettings.BackgroundColor;
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

    public bool ToggleRayDisplay()
    {
        this.showRay = !this.showRay;
        return this.showRay;
    }

    public PointRenderStyle TogglePointRenderStyle()
    {
        this.pointRenderStyle = this.pointRenderStyle switch
        {
            PointRenderStyle.OutlineSquare => PointRenderStyle.SolidSquare,
            PointRenderStyle.SolidSquare => PointRenderStyle.SolidCircle,
            PointRenderStyle.SolidCircle => PointRenderStyle.OutlineCircle,
            PointRenderStyle.OutlineCircle => PointRenderStyle.Line,
            PointRenderStyle.Line => PointRenderStyle.OutlineSquare,
            _ => throw new NotImplementedException($"Point render style '{this.pointRenderStyle}' is not implemented."),
        };

        return this.pointRenderStyle;
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

        var uiText = this.visualizerSettings.UiTextColor;
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
        
        // TODO refactor
        float distanceScale = this.radius * this.zoomScale / this.maxDistanceCm;
        float tipRadius = this.radius - OverflowArrowRimInsetPx;
        float maxHeadBack = tipRadius - OverflowArrowMinTailRadiusPx;
        float arrowHeadBack = maxHeadBack > InsideRingEpsilon
            ? Math.Clamp(OverflowArrowHeadLengthPx, 2.5f, maxHeadBack)
            : 0f;
        var pointItems = new List<(float Angle, float Radius)>(points.Count);
        var arrowAngles = new List<float>(points.Count);
        foreach (var (angle, distance) in points)
        {
            float pointRadius = distance * distanceScale;
            float overflow = pointRadius - this.radius;
            bool isArrow = overflow > InsideRingEpsilon && maxHeadBack > InsideRingEpsilon;
            if (!isArrow)
            {
                pointItems.Add((angle, pointRadius));
                continue;
            }

            arrowAngles.Add(angle);
        }

        RaySettings raySettings = this.visualizerSettings.Ray;
        (int angle, int distance) latestPoint = default;
        bool shouldDrawLatestRay = this.showRay && this.sonarDataCache.TryGetLatestPoint(out latestPoint);
        float latestRayRadius = shouldDrawLatestRay
            ? this.radius * (raySettings.LengthPercent / 100f)
            : 0f;

        this.gl.PushMatrix();
        this.gl.Translate(this.cx, this.cy, 0f);
        this.gl.Rotate(90f, 0f, 0f, 1f); // Keep the same world orientation as before.
        this.gl.Translate(-this.cx, -this.cy, 0f);

        var echoPoint = this.visualizerSettings.Points.EchoColor;
        this.gl.Color4(echoPoint.R, echoPoint.G, echoPoint.B, echoPoint.A);
        this.DrawPolarPointsOnly(pointItems);

        if (shouldDrawLatestRay)
        {
            this.DrawLatestRay(latestPoint.angle, latestRayRadius, raySettings);
        }

        var overflowArrow = this.visualizerSettings.Points.OverflowArrowColor;
        this.gl.Color4(overflowArrow.R, overflowArrow.G, overflowArrow.B, overflowArrow.A);
        this.DrawPolarArrowsOnly(arrowAngles, tipRadius, arrowHeadBack);
        this.gl.PopMatrix();

        // Draw a white point at the center
        var centerPoint = this.visualizerSettings.Points.CenterColor;
        this.gl.Color4(centerPoint.R, centerPoint.G, centerPoint.B, centerPoint.A);
        this.gl.PushMatrix();
        this.gl.Translate(this.cx, this.cy, 0f);
        this.glPrimitives.DrawPointPrimitive(this.pointRenderStyle);
        this.gl.PopMatrix();
    }

    private void DrawPolarPointsOnly(IReadOnlyList<(float Angle, float Radius)> pointItems)
    {
        foreach (var (angle, radius) in pointItems)
        {
            this.gl.PushMatrix();
            this.gl.Translate(this.cx, this.cy, 0f);
            this.gl.Rotate(-angle, 0f, 0f, 1f);
            this.gl.Translate(radius, 0f, 0f);
            this.glPrimitives.DrawPointPrimitive(this.pointRenderStyle);
            this.gl.PopMatrix();
        }
    }

    // TODO, optimize!
    private void DrawLatestRay(float angle, float radius, RaySettings raySettings)
    {
        FloatColor4 color = raySettings.Color;
        float sweepHalfAngleDeg = Math.Max(0f, raySettings.SweepAngleDeg) / 2f;
        int sweepSegments = Math.Max(1, raySettings.SweepSegments);
        float coneCenterAlpha = Math.Clamp(raySettings.ConeCenterAlpha, 0f, 1f);
        float coneEdgeAlpha = Math.Clamp(raySettings.ConeEdgeAlpha, 0f, 1f);
        float lineAlpha = Math.Clamp(raySettings.LineAlpha, 0f, 1f);
        float lineWidth = Math.Max(0.1f, raySettings.LineWidth);

        this.gl.PushMatrix();
        this.gl.Translate(this.cx, this.cy, 0f);
        this.gl.Rotate(-angle, 0f, 0f, 1f);

        float centerAlpha = Math.Clamp(color.A * coneCenterAlpha, 0f, 1f);
        float edgeAlpha = Math.Clamp(color.A * coneEdgeAlpha, 0f, 1f);

        // Draw a soft radar-like sweep cone.
        this.gl.Begin(GLEnum.TriangleFan);
        this.gl.Color4(color.R, color.G, color.B, centerAlpha);
        this.gl.Vertex2(0f, 0f);
        for (int i = 0; i <= sweepSegments; i++)
        {
            float t = i / (float)sweepSegments;
            float beamAngleDeg = -sweepHalfAngleDeg + (t * sweepHalfAngleDeg * 2f);
            float beamAngleRad = beamAngleDeg * (MathF.PI / 180f);
            float x = MathF.Cos(beamAngleRad) * radius;
            float y = MathF.Sin(beamAngleRad) * radius;

            this.gl.Color4(color.R, color.G, color.B, edgeAlpha);
            this.gl.Vertex2(x, y);
        }
        this.gl.End();

        Span<float> previousWidth = stackalloc float[1];
        this.gl.GetFloat(GLEnum.LineWidth, previousWidth);
        this.gl.LineWidth(lineWidth);
        this.gl.Begin(GLEnum.Lines);
        this.gl.Color4(color.R, color.G, color.B, Math.Clamp(color.A * lineAlpha, 0f, 1f));
        this.gl.Vertex2(0f, 0f);
        this.gl.Vertex2(radius, 0f);
        this.gl.End();
        this.gl.LineWidth(previousWidth[0]);

        this.gl.PopMatrix();
    }

    private void DrawPolarArrowsOnly(IReadOnlyList<float> arrowAngles, float tipRadius, float arrowHeadBack)
    {
        if (arrowHeadBack <= InsideRingEpsilon)
        {
            return;
        }

        foreach (var angle in arrowAngles)
        {
            this.gl.PushMatrix();
            this.gl.Translate(this.cx, this.cy, 0f);
            this.gl.Rotate(-angle, 0f, 0f, 1f);
            this.gl.Translate(tipRadius, 0f, 0f);
            this.glPrimitives.DrawArrowPrimitive(arrowHeadBack, OverflowArrowHalfWidthPx);
            this.gl.PopMatrix();
        }
    }

    private void DrawPolarGrid()
    {
        this.gl.Enable(GLEnum.Blend);
        this.gl.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);

        // Every 100 cm ring that fits inside the rim (same scale as echoes). Fills disc: farthest
        // ring at d = maxDistanceCm/zoomScale coincides with the plot edge when zoom ≠ 1.
        var gridLine = this.visualizerSettings.Grid.LineColor;
        this.gl.Color4(gridLine.R, gridLine.G, gridLine.B, gridLine.A);
        if (this.maxDistanceCm > 0f && this.zoomScale > 0f)
        {
            float dMax = this.maxDistanceCm / this.zoomScale;
            for (int i = 1; i * RangeRingStepCm <= dMax + InsideRingEpsilon; i++)
            {
                float ringR = this.ScaledEchoRadius(i * RangeRingStepCm);
                if (ringR <= this.radius + InsideRingEpsilon)
                {
                    this.glPrimitives.DrawCircle(this.cx, this.cy, ringR);
                }
            }
        }

        if (this.maxDistanceCm > 0f)
        {
            this.glPrimitives.DrawCircle(this.cx, this.cy, this.radius, lineWidth: 2f);
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
        var gridTickLabel = this.visualizerSettings.Grid.TickLabelColor;
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

        var gridLabel = this.visualizerSettings.Grid.LabelColor;
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

}

#pragma warning restore CS0618
