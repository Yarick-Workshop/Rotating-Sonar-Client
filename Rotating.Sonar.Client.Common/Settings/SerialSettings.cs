namespace Rotating.Sonar.Client.Common.Settings;

using System.ComponentModel.DataAnnotations;

public sealed class SerialSettings
{
    [Range(1, int.MaxValue, ErrorMessage = "DefaultBaudRate must be at least 1.")]
    public int DefaultBaudRate { get; set; } = 9600;

    [Range(0, int.MaxValue, ErrorMessage = "ReadTimeoutMilliseconds must be non-negative.")]
    public int ReadTimeoutMilliseconds { get; set; } = 250;

    public FakeSerialDataSettings FakeData { get; set; } = new();
}
