namespace Rotating.Sonar.Client.Visualizer;

using Rotating.Sonar.Client.Common.Settings;
using Rotating.Sonar.Client.Common.Settings.Sections;
using Silk.NET.OpenGL.Legacy;

#pragma warning disable CS0618
public class OpenGlPrimitives
{
    private readonly GL gl;
    private readonly (float X, float Y)[] unitCirclePoints;
    private readonly (float X, float Y)[] pointCirclePoints;
    private readonly (float X, float Y)[] pointSquareCorners;
    private readonly float pointRadius;
    private readonly float pointLineWidthPx;

    public OpenGlPrimitives(GL gl, VisualizerSettings visualizerSettings)
    {
        this.gl = gl;

        float pointSize = visualizerSettings.ScanPoints.PointSizePx;
        this.pointRadius = pointSize / 2f;
        this.pointLineWidthPx = visualizerSettings.ScanPoints.LineWidthPx;
        // TODO: generalize circle segment settings instead of keeping this grid-specific.
        this.unitCirclePoints = CreateUnitCirclePoints(visualizerSettings.RangeGrid.GridCircleSegments);
        this.pointCirclePoints = CreateScaledCirclePoints(visualizerSettings.ScanPoints.CircleSegments, this.pointRadius);
        this.pointSquareCorners =
        [
            (-this.pointRadius, -this.pointRadius),
            (this.pointRadius, -this.pointRadius),
            (this.pointRadius, this.pointRadius),
            (-this.pointRadius, this.pointRadius),
        ];
    }

    public void DrawCircle(float centerXPx, float centerYPx, float radiusPx, float lineWidthPx = 1f)
    {
        Span<float> previousWidth = stackalloc float[1];
        this.gl.GetFloat(GLEnum.LineWidth, previousWidth);
        this.gl.LineWidth(lineWidthPx);

        this.gl.Begin(GLEnum.LineLoop);
        foreach (var (unitX, unitY) in this.unitCirclePoints)
        {
            this.gl.Vertex2(centerXPx + (radiusPx * unitX), centerYPx + (radiusPx * unitY));
        }

        this.gl.End();
        this.gl.LineWidth(previousWidth[0]);
    }

    public void DrawPointPrimitive(ScanPointRenderStyle pointRenderStyle)
    {
        if (pointRenderStyle == ScanPointRenderStyle.Line)
        {
            this.DrawPointLinePrimitive();
            return;
        }

        ((float X, float Y)[] unitPoints, bool filled) = pointRenderStyle switch
        {
            ScanPointRenderStyle.OutlineSquare => (this.pointSquareCorners, false),
            ScanPointRenderStyle.SolidSquare => (this.pointSquareCorners, true),
            ScanPointRenderStyle.SolidCircle => (this.pointCirclePoints, true),
            ScanPointRenderStyle.OutlineCircle => (this.pointCirclePoints, false),
            _ => throw new NotImplementedException($"Point render style '{pointRenderStyle}' is not implemented."),
        };

        this.DrawPointPolygon(unitPoints, filled);
    }

    public void DrawArrowPrimitive(float arrowHeadBackPx, float arrowHalfWidthPx)
    {
        this.gl.Begin(GLEnum.Triangles);
        this.gl.Vertex2(0f, 0f);
        this.gl.Vertex2(-arrowHeadBackPx, arrowHalfWidthPx);
        this.gl.Vertex2(-arrowHeadBackPx, -arrowHalfWidthPx);
        this.gl.End();
    }

    private void DrawPointLinePrimitive()
    {
        Span<float> previousWidth = stackalloc float[1];
        this.gl.GetFloat(GLEnum.LineWidth, previousWidth);
        this.gl.LineWidth(this.pointLineWidthPx);

        this.gl.Begin(GLEnum.Lines);
        this.gl.Vertex2(0f, -this.pointRadius);
        this.gl.Vertex2(0f, this.pointRadius);
        this.gl.End();

        this.gl.LineWidth(previousWidth[0]);
    }

    private void DrawPointPolygon((float X, float Y)[] points, bool filled)
    {
        this.gl.Begin(filled ? GLEnum.TriangleFan : GLEnum.LineLoop);
        if (filled)
        {
            this.gl.Vertex2(0f, 0f);
        }

        for (int i = 0; i <= points.Length; i++)
        {
            var (pointX, pointY) = points[i % points.Length];
            this.gl.Vertex2(pointX, pointY);
        }

        this.gl.End();
    }

    private static (float X, float Y)[] CreateUnitCirclePoints(int segmentCount)
    {
        var points = new (float X, float Y)[segmentCount];
        for (int i = 0; i < segmentCount; i++)
        {
            double theta = 2d * Math.PI * i / segmentCount;
            points[i] = ((float)Math.Cos(theta), (float)Math.Sin(theta));
        }

        return points;
    }

    private static (float X, float Y)[] CreateScaledCirclePoints(int segments, float pointRadius)
    {
        var points = new (float X, float Y)[segments];
        for (int i = 0; i < segments; i++)
        {
            double theta = 2d * Math.PI * i / segments;
            points[i] = ((float)Math.Cos(theta) * pointRadius, (float)Math.Sin(theta) * pointRadius);
        }

        return points;
    }
}
#pragma warning restore CS0618
