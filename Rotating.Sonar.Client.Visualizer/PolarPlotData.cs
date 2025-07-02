namespace Rotating.Sonar.Client.Visualizer;
using System.Collections.Concurrent;
using Serilog;

internal class PolarPlotData
{
    private readonly ConcurrentDictionary<int, int> points = new();

    public void Update(int angle, int distance)
    {
        points[angle] = distance;

        Log.Debug($"Updated with {angle}° {distance}cm");
    }

    public List<(int angle, int distance)> GetPoints()
    {
        return points.Select(kv => (kv.Key, kv.Value)).ToList();
    }
}
