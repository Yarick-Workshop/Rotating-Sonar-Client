namespace Rotating.Sonar.Client.Visualizer;
using System.Collections.Concurrent;

internal class PolarPlotData
{
    private readonly ConcurrentDictionary<int, int> points = new();

    public void Update(int angle, int distance)
    {
        points[angle] = distance;

        Console.WriteLine($"Updated with {angle}° {distance}cm");
    }

    public List<(int angle, int distance)> GetPoints()
    {
        return points.Select(kv => (kv.Key, kv.Value)).ToList();
    }
}
