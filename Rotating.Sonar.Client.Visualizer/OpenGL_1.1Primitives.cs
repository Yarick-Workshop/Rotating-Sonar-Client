namespace Rotating.Sonar.Client.Visualizer;

using Silk.NET.OpenGL.Legacy;

#pragma warning disable CS0618
public class OpenGL_1_1_Primitives
{
    private const int CircleSegments = 64;
    private const int MarkerCircleSegments = 16;

    private readonly GL gl;
    private static readonly (float X, float Y)[] UnitCirclePoints = CreateUnitCirclePoints(CircleSegments);
    private static readonly (float X, float Y)[] UnitMarkerCirclePoints = CreateUnitCirclePoints(MarkerCircleSegments);
    private static readonly (float X, float Y)[] UnitSquareCorners =
    [
        (-1f, -1f),
        (1f, -1f),
        (1f, 1f),
        (-1f, 1f),
    ];

    public OpenGL_1_1_Primitives(GL gl)
    {
        this.gl = gl;
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

    public void DrawPointMarker(float x, float y, PointRenderStyle pointRenderStyle, float pointSize)
    {
        ((float X, float Y)[] unitPoints, bool filled) = pointRenderStyle switch
        {
            PointRenderStyle.OutlineSquare => (UnitSquareCorners, false),
            PointRenderStyle.SolidSquare => (UnitSquareCorners, true),
            PointRenderStyle.SolidCircle => (UnitMarkerCirclePoints, true),
            PointRenderStyle.OutlineCircle => (UnitMarkerCirclePoints, false),
            _ => throw new ArgumentOutOfRangeException(nameof(pointRenderStyle), pointRenderStyle, "Unknown point render style value."),
        };

        this.DrawMarkerPolygon(x, y, pointSize, unitPoints, filled);
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

    private void DrawMarkerPolygon(float x, float y, float size, (float X, float Y)[] unitPoints, bool filled)
    {
        float radiusPx = size / 2f;
        this.gl.Begin(filled ? GLEnum.TriangleFan : GLEnum.LineLoop);
        if (filled)
        {
            this.gl.Vertex2(x, y);
        }

        for (int i = 0; i <= unitPoints.Length; i++)
        {
            var (unitX, unitY) = unitPoints[i % unitPoints.Length];
            this.gl.Vertex2(x + radiusPx * unitX, y + radiusPx * unitY);
        }

        this.gl.End();
    }
}
#pragma warning restore CS0618
