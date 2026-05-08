namespace Rotating.Sonar.Client.Visualizer;

using System.Collections.Concurrent;
using Serilog;

public class SonarDataCache
{
    public const int UninitializedLatestAngle = int.MinValue;

    // TODO, refactor
    private readonly ConcurrentDictionary<int, int> sonarPoints = new();
    private int latestAngle = UninitializedLatestAngle;

    public int LatestAngle => this.latestAngle;

    public void Update(int angle, int distance)
    {
        this.sonarPoints[angle] = distance;
        this.latestAngle = angle;

        Log.Debug("Updated with {Angle}° {Distance}cm", angle, distance);
    }

    public List<(int angle, int distance)> GetPoints()
    {
        return this.sonarPoints.Select(kv => (kv.Key, kv.Value)).ToList();
    }

    public bool TryGetLatestPoint(out (int angle, int distance) point)
    {
        if (this.latestAngle == UninitializedLatestAngle)
        {
            point = default;
            return false;
        }

        if (!this.sonarPoints.TryGetValue(this.latestAngle, out int latestDistance))
        {
            point = default;
            return false;
        }

        point = (this.latestAngle, latestDistance);
        return true;
    }
}
