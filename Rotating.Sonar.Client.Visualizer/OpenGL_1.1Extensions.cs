namespace Rotating.Sonar.Client.Visualizer;

using Silk.NET.OpenGL.Legacy;

#pragma warning disable CS0618
public static class OpenGL_1_1_Extensions
{
    private const int CircleSegments = 64;
    private const int MarkerCircleSegments = 16;

    private static readonly (float Cos, float Sin)[] UnitCirclePoints = CreateUnitCirclePoints();

    public static void DrawCircle(this GL gl, float centerX, float centerY, float radius, float lineWidth = 1f)
    {
        Span<float> previousWidth = stackalloc float[1];
        gl.GetFloat(GLEnum.LineWidth, previousWidth);
        gl.LineWidth(lineWidth);
        
        gl.Begin(GLEnum.LineLoop);
        foreach (var (cos, sin) in UnitCirclePoints)
        {
            float x = centerX + (radius * cos);
            float y = centerY + (radius * sin);
            gl.Vertex2(x, y);
        }

        gl.End();
        gl.LineWidth(previousWidth[0]);
    }

    // TODO, optimize
    public static void DrawPointMarker(this GL gl, float x, float y, float size, PointRenderStyle pointRenderStyle)
    {
        switch (pointRenderStyle)
        {
            case PointRenderStyle.OutlineSquare:
                DrawSquareMarker(gl, x, y, size, filled: false);
                return;
            case PointRenderStyle.SolidSquare:
                DrawSquareMarker(gl, x, y, size, filled: true);
                return;
            case PointRenderStyle.SolidCircle:
                DrawCircleMarker(gl, x, y, size, filled: true);
                return;
            case PointRenderStyle.OutlineCircle:
                DrawCircleMarker(gl, x, y, size, filled: false);
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(pointRenderStyle), pointRenderStyle, "Unknown point render style value.");
        }
    }
    
    private static (float Cos, float Sin)[] CreateUnitCirclePoints()
    {
        var points = new (float Cos, float Sin)[CircleSegments];
        for (int i = 0; i < CircleSegments; i++)
        {
            double theta = 2d * Math.PI * i / CircleSegments;
            points[i] = ((float)Math.Cos(theta), (float)Math.Sin(theta));
        }

        return points;
    }

    private static void DrawSquareMarker(GL gl, float x, float y, float size, bool filled)
    {
        float half = size / 2f;
        gl.Begin(filled ? GLEnum.Quads : GLEnum.LineLoop);
        gl.Vertex2(x - half, y - half);
        gl.Vertex2(x + half, y - half);
        gl.Vertex2(x + half, y + half);
        gl.Vertex2(x - half, y + half);
        gl.End();
    }

    private static void DrawCircleMarker(GL gl, float x, float y, float size, bool filled)
    {
        float radiusPx = size / 2f;
        gl.Begin(filled ? GLEnum.TriangleFan : GLEnum.LineLoop);

        for (int i = 0; i <= MarkerCircleSegments; i++)
        {
            float angle = i * 2f * MathF.PI / MarkerCircleSegments;
            gl.Vertex2(
                x + radiusPx * MathF.Cos(angle),
                y + radiusPx * MathF.Sin(angle));
        }

        gl.End();
    }
}
#pragma warning restore CS0618
