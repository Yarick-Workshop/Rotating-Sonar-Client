namespace Rotating.Sonar.Client.Visualizer;

using Silk.NET.OpenGL.Legacy;

#pragma warning disable CS0618
public class OpenGL_1_1_Primitives
{
    private const int CircleSegments = 64;
    private const int MarkerCircleSegments = 16;

    private readonly GL gl;
    private readonly float pointSize;
    private static readonly (float X, float Y)[] UnitCirclePoints = CreateUnitCirclePoints(CircleSegments);
    private readonly (float X, float Y)[] markerCirclePoints;
    private readonly (float X, float Y)[] markerSquareCorners;

    public OpenGL_1_1_Primitives(GL gl, VisualizerSettings settings)
    {
        this.gl = gl;
        this.pointSize = Math.Max(1f, settings.Points.PointSize);

        float radiusPx = this.pointSize / 2f;
        this.markerCirclePoints = CreateScaledCirclePoints(MarkerCircleSegments, radiusPx);
        this.markerSquareCorners =
        [
            (-radiusPx, -radiusPx),
            (radiusPx, -radiusPx),
            (radiusPx, radiusPx),
            (-radiusPx, radiusPx),
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

    public void DrawPointMarker(float x, float y, PointRenderStyle pointRenderStyle)
    {
        ((float X, float Y)[] unitPoints, bool filled) = pointRenderStyle switch
        {
            PointRenderStyle.OutlineSquare => (this.markerSquareCorners, false),
            PointRenderStyle.SolidSquare => (this.markerSquareCorners, true),
            PointRenderStyle.SolidCircle => (this.markerCirclePoints, true),
            PointRenderStyle.OutlineCircle => (this.markerCirclePoints, false),
            _ => throw new ArgumentOutOfRangeException(nameof(pointRenderStyle), pointRenderStyle, "Unknown point render style value."),
        };

        this.DrawMarkerPolygon(x, y, unitPoints, filled);
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

    private void DrawMarkerPolygon(float x, float y, (float X, float Y)[] points, bool filled)
    {
        this.gl.Begin(filled ? GLEnum.TriangleFan : GLEnum.LineLoop);
        if (filled)
        {
            this.gl.Vertex2(x, y);
        }

        for (int i = 0; i <= points.Length; i++)
        {
            var (pointX, pointY) = points[i % points.Length];
            this.gl.Vertex2(x + pointX, y + pointY);
        }

        this.gl.End();
    }

    private static (float X, float Y)[] CreateScaledCirclePoints(int segments, float radius)
    {
        var points = new (float X, float Y)[segments];
        for (int i = 0; i < segments; i++)
        {
            double theta = 2d * Math.PI * i / segments;
            points[i] = ((float)Math.Cos(theta) * radius, (float)Math.Sin(theta) * radius);
        }

        return points;
    }
}
#pragma warning restore CS0618
