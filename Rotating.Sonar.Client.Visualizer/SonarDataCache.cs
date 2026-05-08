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
        if (this.latestAngle == UninitializedLatestAngle)
        {
            latestPoint = default;
            return false;
        }

        if (!this.sonarPoints.TryGetValue(this.latestAngle, out int latestDistance))
        {
            latestPoint = default;
            return false;
        }

        latestPoint = (this.latestAngle, latestDistance);
        return true;
    }
}
