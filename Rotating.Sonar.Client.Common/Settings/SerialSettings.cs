namespace Rotating.Sonar.Client.Common.Settings;

public sealed class SerialSettings
{
    public int DefaultBaudRate { get; set; } = 9600;

    public int ReadTimeoutMilliseconds { get; set; } = 250;

    public FakeSerialDataSettings FakeData { get; set; } = new();
}
