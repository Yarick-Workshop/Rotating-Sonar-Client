namespace Rotating.Sonar.Client.Visualizer;

using Silk.NET.OpenGL.Legacy;

#pragma warning disable CS0618
public static class OpenGL_1_1_Extensions
{
    private const int CircleSegments = 64;

    private static readonly (float Cos, float Sin)[] UnitCirclePoints = CreateUnitCirclePoints();

    public static void DrawCircle(this GL gl, float centerX, float centerY, float radius)
    {
        gl.Begin(GLEnum.LineLoop);
        foreach (var (cos, sin) in UnitCirclePoints)
        {
            float x = centerX + (radius * cos);
            float y = centerY + (radius * sin);
            gl.Vertex2(x, y);
        }
        gl.End();
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
}
#pragma warning restore CS0618
