namespace Rotating.Sonar.Client.Visualizer;
#pragma warning disable CS0618

public class OpenGlScanDisplayRenderer : IZoomable
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
    private readonly OpenGlPrimitives glPrimitives;

    private readonly ScanPointBuffer scanPointBuffer;
    private float cx;
    private float cy;
    private float radius;
    private float zoomScale = 1f;
    private readonly float maxDistanceCm;
    private float width;
    private float height;
    private readonly OpenGlTextRenderer textRenderer;
    private readonly VisualizerSettings visualizerSettings;
    private bool showFps = true;
    private bool showZoom = true;
    private bool showSweep = false;
    private ScanPointRenderStyle pointRenderStyle = ScanPointRenderStyle.SolidSquare;

    public float ZoomScale
    {
        get
        {
            return this.zoomScale;
        }
    }

    public OpenGlScanDisplayRenderer(
        GL gl,
        ScanPointBuffer scanPointBuffer,
        AppSettings appSettings,
        float viewportWidthPx,
        float viewportHeightPx,
        float maxDistanceCm,
        OpenGlTextRenderer textRenderer)
    {
        this.gl = gl;
        this.glPrimitives = new OpenGlPrimitives(gl, appSettings.Visualizer);
        this.scanPointBuffer = scanPointBuffer;

        // TODO, investigate why option this.UpdateViewport(width, height); does not work
        this.width = viewportWidthPx;
        this.height = viewportHeightPx;
        
        this.cx = viewportWidthPx / 2f;
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

    public bool ToggleSweepDisplay()
    {
        this.showSweep = !this.showSweep;
        return this.showSweep;
    }

    public ScanPointRenderStyle ToggleScanPointRenderStyle()
    {
        this.pointRenderStyle = this.pointRenderStyle switch
        {
            ScanPointRenderStyle.OutlineSquare => ScanPointRenderStyle.SolidSquare,
            ScanPointRenderStyle.SolidSquare => ScanPointRenderStyle.SolidCircle,
            ScanPointRenderStyle.SolidCircle => ScanPointRenderStyle.OutlineCircle,
            ScanPointRenderStyle.OutlineCircle => ScanPointRenderStyle.Line,
            ScanPointRenderStyle.Line => ScanPointRenderStyle.OutlineSquare,
            _ => throw new NotImplementedException($"Point render style '{this.pointRenderStyle}' is not implemented."),
        };

        return this.pointRenderStyle;
    }

    public void Render(double framesPerSecond)
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

        var uiText = this.visualizerSettings.OverlayTextColor;
        this.gl.Color4(uiText.R, uiText.G, uiText.B, uiText.A);

        if (this.showFps)
        {
            // Draw FPS in top-left corner
            string fpsText = $"{framesPerSecond:F0}FPS";
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

    public void UpdateViewport(float viewportWidthPx, float viewportHeightPx)
    {
        this.width = viewportWidthPx;
        this.height = viewportHeightPx;

        // Update center and radius based on new dimensions
        this.cx = this.width / 2f;
        this.cy = this.height / 2f;
        this.radius = MathF.Min(this.cx, this.cy) - 40;
    }

    private void DrawPoints()
    {
        var points = this.scanPointBuffer.GetPoints();

        if (points.Count == 0)
        {
            Log.Warning("No scan points to draw.");
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

        SweepSettings sweepSettings = this.visualizerSettings.Sweep;
        (int angle, int distance) latestPoint = default;
        bool shouldDrawLatestSweep = this.showSweep && this.scanPointBuffer.TryGetLatestPoint(out latestPoint);
        float latestSweepRadius = shouldDrawLatestSweep
            ? this.radius * (sweepSettings.LengthPercentOfRadius / 100f)
            : 0f;

        this.gl.PushMatrix();
        this.gl.Translate(this.cx, this.cy, 0f);
        this.gl.Rotate(90f, 0f, 0f, 1f); // Keep the same world orientation as before.
        this.gl.Translate(-this.cx, -this.cy, 0f);

        var scanPoint = this.visualizerSettings.ScanPoints.ScanPointColor;
        this.gl.Color4(scanPoint.R, scanPoint.G, scanPoint.B, scanPoint.A);
        this.DrawPolarPointsOnly(pointItems);

        if (shouldDrawLatestSweep)
        {
            this.DrawLatestSweep(latestPoint.angle, latestSweepRadius, sweepSettings);
        }

        var overflowArrow = this.visualizerSettings.ScanPoints.OffScaleIndicatorColor;
        this.gl.Color4(overflowArrow.R, overflowArrow.G, overflowArrow.B, overflowArrow.A);
        this.DrawPolarArrowsOnly(arrowAngles, tipRadius, arrowHeadBack);
        this.gl.PopMatrix();

        // Draw a white point at the center
        var centerPoint = this.visualizerSettings.ScanPoints.OriginMarkerColor;
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

            // TODO, add option to enable/disable this?
            if (this.pointRenderStyle == ScanPointRenderStyle.SolidSquare || this.pointRenderStyle == ScanPointRenderStyle.OutlineSquare)
            {
                this.gl.Rotate(angle, 0f, 0f, 1f);
            }

            this.glPrimitives.DrawPointPrimitive(this.pointRenderStyle);
            this.gl.PopMatrix();
        }
    }

    // TODO, optimize!
    private void DrawLatestSweep(float sweepAngleDeg, float sweepRadiusPx, SweepSettings sweepSettings)
    {
        FloatColor4 color = sweepSettings.SweepColor;
        float sweepHalfAngleDeg = Math.Max(0f, sweepSettings.SweepAngleDeg) / 2f;
        int sweepSegments = Math.Max(1, sweepSettings.SweepSegmentCount);
        float coneCenterAlpha = Math.Clamp(sweepSettings.ConeCenterAlpha, 0f, 1f);
        float coneEdgeAlpha = Math.Clamp(sweepSettings.ConeEdgeAlpha, 0f, 1f);
        float lineAlpha = Math.Clamp(sweepSettings.LineAlpha, 0f, 1f);
        float lineWidth = Math.Max(0.1f, sweepSettings.LineWidthPx);

        this.gl.PushMatrix();
        this.gl.Translate(this.cx, this.cy, 0f);
        this.gl.Rotate(-sweepAngleDeg, 0f, 0f, 1f);

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
            float x = MathF.Cos(beamAngleRad) * sweepRadiusPx;
            float y = MathF.Sin(beamAngleRad) * sweepRadiusPx;

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
        this.gl.Vertex2(sweepRadiusPx, 0f);
        this.gl.End();
        this.gl.LineWidth(previousWidth[0]);

        this.gl.PopMatrix();
    }

    private void DrawPolarArrowsOnly(
        IReadOnlyList<float> arrowAnglesDeg,
        float arrowTipRadiusPx,
        float arrowHeadBackPx)
    {
        if (arrowHeadBackPx <= InsideRingEpsilon)
        {
            return;
        }

        foreach (var angle in arrowAnglesDeg)
        {
            this.gl.PushMatrix();
            this.gl.Translate(this.cx, this.cy, 0f);
            this.gl.Rotate(-angle, 0f, 0f, 1f);
            this.gl.Translate(arrowTipRadiusPx, 0f, 0f);
            this.glPrimitives.DrawArrowPrimitive(arrowHeadBackPx, OverflowArrowHalfWidthPx);
            this.gl.PopMatrix();
        }
    }

    private void DrawPolarGrid()
    {
        this.gl.Enable(GLEnum.Blend);
        this.gl.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);

        // Every 100 cm ring that fits inside the rim (same scale as scan points). Fills disc: farthest
        // ring at d = maxDistanceCm/zoomScale coincides with the plot edge when zoom ≠ 1.
        var gridLine = this.visualizerSettings.RangeGrid.LineColor;
        this.gl.Color4(gridLine.R, gridLine.G, gridLine.B, gridLine.A);
        if (this.maxDistanceCm > 0f && this.zoomScale > 0f)
        {
            float dMax = this.maxDistanceCm / this.zoomScale;
            for (int i = 1; i * RangeRingStepCm <= dMax + InsideRingEpsilon; i++)
            {
                float ringR = this.ScaledScanPointRadius(i * RangeRingStepCm);
                if (ringR <= this.radius + InsideRingEpsilon)
                {
                    this.glPrimitives.DrawCircle(this.cx, this.cy, ringR);
                }
            }
        }

        if (this.maxDistanceCm > 0f)
        {
            this.glPrimitives.DrawCircle(this.cx, this.cy, this.radius, lineWidthPx: 2f);
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
        var gridTickLabel = this.visualizerSettings.RangeGrid.RangeLabelColor;
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

        var gridLabel = this.visualizerSettings.RangeGrid.BearingLabelColor;
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

    private float ScaledScanPointRadius(float distanceCm)
    {
        return distanceCm / this.maxDistanceCm * this.radius * this.zoomScale;
    }

}

#pragma warning restore CS0618
