namespace Rotating.Sonar.Client.Common.Settings;

public sealed class FakeSerialDataSettings
{
    public int MinAngleDeg { get; set; } = -90;

    public int MaxAngleDeg { get; set; } = 90;

    public int AngleStepDeg { get; set; } = 15;

    public int BaseDistanceCm { get; set; } = 120;

    public int DistanceJitterCm { get; set; } = 10;

    public int IntervalMilliseconds { get; set; } = 100;
}
