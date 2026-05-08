namespace Rotating.Sonar.Client.Visualizer;

using Silk.NET.OpenGL.Legacy;

#pragma warning disable CS0618
public class OpenGL_1_1_Primitives
{
    private const int CircleSegments = 64;
    private const int PointCircleSegments = 16;

    private readonly GL gl;
    private static readonly (float X, float Y)[] UnitCirclePoints = CreateUnitCirclePoints(CircleSegments);
    private readonly (float X, float Y)[] pointCirclePoints;
    private readonly (float X, float Y)[] pointSquareCorners;
    private readonly float pointRadius;

    public OpenGL_1_1_Primitives(GL gl, VisualizerSettings visualizerSettings)
    {
        this.gl = gl;

        float pointSize = Math.Max(1f, visualizerSettings.Points.PointSize);
        this.pointRadius = pointSize / 2f;
        this.pointCirclePoints = CreateScaledCirclePoints(PointCircleSegments, this.pointRadius);
        this.pointSquareCorners =
        [
            (-this.pointRadius, -this.pointRadius),
            (this.pointRadius, -this.pointRadius),
            (this.pointRadius, this.pointRadius),
            (-this.pointRadius, this.pointRadius),
        ];
    }

    public void DrawCircle(float centerX, float centerY, float radius, float lineWidth = 1f)
    {
        Span<float> previousWidth = stackalloc float[1];
        this.gl.GetFloat(GLEnum.LineWidth, previousWidth);
        this.gl.LineWidth(lineWidth);
        
        this.gl.Begin(GLEnum.LineLoop);
        foreach (var (unitX, unitY) in UnitCirclePoints)
        {
            this.gl.Vertex2(centerX + (radius * unitX), centerY + (radius * unitY));
        }

        this.gl.End();
        this.gl.LineWidth(previousWidth[0]);
    }

    public void DrawPointPrimitive(PointRenderStyle pointRenderStyle)
    {
        if (pointRenderStyle == PointRenderStyle.Line)
        {
            this.DrawPointLinePrimitive();
            return;
        }

        ((float X, float Y)[] unitPoints, bool filled) = pointRenderStyle switch
        {
            PointRenderStyle.OutlineSquare => (this.pointSquareCorners, false),
            PointRenderStyle.SolidSquare => (this.pointSquareCorners, true),
            PointRenderStyle.SolidCircle => (this.pointCirclePoints, true),
            PointRenderStyle.OutlineCircle => (this.pointCirclePoints, false),
            _ => throw new NotImplementedException($"Point render style '{pointRenderStyle}' is not implemented."),
        };

        this.DrawPointPolygon(unitPoints, filled);
    }

    private void DrawPointLinePrimitive()
    {
        // TODO: move line-point width to visualizer configuration.
        Span<float> previousWidth = stackalloc float[1];
        this.gl.GetFloat(GLEnum.LineWidth, previousWidth);
        this.gl.LineWidth(2f);

        this.gl.Begin(GLEnum.Lines);
        this.gl.Vertex2(0f, -this.pointRadius);
        this.gl.Vertex2(0f, this.pointRadius);
        this.gl.End();

        this.gl.LineWidth(previousWidth[0]);
    }

    public void DrawArrowPrimitive(float headBack, float halfWidth)
    {
        this.gl.Begin(GLEnum.Triangles);
        this.gl.Vertex2(0f, 0f);
        this.gl.Vertex2(-headBack, halfWidth);
        this.gl.Vertex2(-headBack, -halfWidth);
        this.gl.End();
    }

    private static (float X, float Y)[] CreateUnitCirclePoints(int segments)
    {
        var points = new (float X, float Y)[segments];
        for (int i = 0; i < segments; i++)
        {
            double theta = 2d * Math.PI * i / segments;
            points[i] = ((float)Math.Cos(theta), (float)Math.Sin(theta));
        }

        return points;
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
