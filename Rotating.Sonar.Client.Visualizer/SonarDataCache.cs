namespace Rotating.Sonar.Client.Visualizer;

using System.Collections.Concurrent;
using Serilog;

public class SonarDataCache
{
    public const int UninitializedLatestAngle = int.MinValue;

    private readonly ConcurrentDictionary<int, int> sonarPoints = new();
    private volatile int latestAngle = UninitializedLatestAngle;

    public int LatestAngle => this.latestAngle;

    public void Update(int angleDeg, int distanceCm)
    {
        this.sonarPoints[angleDeg] = distanceCm;
        this.latestAngle = angleDeg;

        Log.Debug("Updated with {Angle}° {Distance}cm", angleDeg, distanceCm);
    }

    public List<(int angle, int distance)> GetPoints()
    {
        return this.sonarPoints.Select(kv => (kv.Key, kv.Value)).ToList();
    }

    public bool TryGetLatestPoint(out (int angle, int distance) latestPoint)
    {
        int angleDeg = this.latestAngle;
        if (angleDeg == UninitializedLatestAngle)
        {
            latestPoint = default;
            return false;
        }

        if (!this.sonarPoints.TryGetValue(angleDeg, out int latestDistance))
        {
            latestPoint = default;
            return false;
        }

        latestPoint = (angleDeg, latestDistance);
        return true;
    }
}
