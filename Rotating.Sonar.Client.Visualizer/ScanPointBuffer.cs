namespace Rotating.Sonar.Client.Visualizer;

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

public class ScanPointBuffer
{
    public const int UninitializedLatestAngle = int.MinValue;

    private readonly ConcurrentDictionary<int, int> scanPoints = new();
    private readonly ILogger<ScanPointBuffer> logger;
    private volatile int latestAngle = UninitializedLatestAngle;

    public int LatestAngle => this.latestAngle;

    public ScanPointBuffer(ILogger<ScanPointBuffer> logger)
    {
        this.logger = logger;
    }

    public void Update(int angleDeg, int distanceCm)
    {
        this.scanPoints[angleDeg] = distanceCm;
        this.latestAngle = angleDeg;

        this.logger.LogDebug("Updated with {Angle}° {Distance}cm", angleDeg, distanceCm);
    }

    public List<(int angle, int distance)> GetPoints()
    {
        return this.scanPoints.Select(kv => (kv.Key, kv.Value)).ToList();
    }

    public bool TryGetLatestPoint(out (int angle, int distance) latestPoint)
    {
        int angleDeg = this.latestAngle;
        if (angleDeg == UninitializedLatestAngle)
        {
            latestPoint = default;
            return false;
        }

        if (!this.scanPoints.TryGetValue(angleDeg, out int latestDistance))
        {
            latestPoint = default;
            return false;
        }

        latestPoint = (angleDeg, latestDistance);
        return true;
    }
}
