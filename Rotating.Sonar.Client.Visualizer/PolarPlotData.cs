namespace Rotating.Sonar.Client.Visualizer;

internal class PolarPlotData
{
    private const int MaxPoints = 360;
    private readonly List<(int angle, int distance)> points = new();
    private readonly object lockObj = new();
    public void Update(int angle, int distance)
    {
        lock (lockObj)
        {
            points.Add((angle, distance));
            if (points.Count > MaxPoints)
                points.RemoveAt(0);
        }
    }
    public List<(int angle, int distance)> GetPoints()
    {
        lock (lockObj)
        {
            return new List<(int angle, int distance)>(points);
        }
    }
}
