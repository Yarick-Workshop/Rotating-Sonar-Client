namespace Rotating.Sonar.Client.Visualizer;

using System.Collections.Concurrent;
using Serilog;

public class SonarDataCache
{
    // TODO, refactor
    private readonly ConcurrentDictionary<int, int> sonarPoints = new();

    public void Update(int angle, int distance)
    {
        this.sonarPoints[angle] = distance;

        Log.Debug("Updated with {Angle}° {Distance}cm", angle, distance);
    }

    public List<(int angle, int distance)> GetPoints()
    {
        return this.sonarPoints.Select(kv => (kv.Key, kv.Value)).ToList();
    }
}
