namespace Rotating.Sonar.Client.Common.Settings;

public sealed class RangeGridSettings
{
    public FloatColor4 LineColor { get; set; } = ChainColorParser.Instance.ParseOrThrow("#4D4D4D80", nameof(LineColor));
    public FloatColor4 RangeLabelColor { get; set; } = ChainColorParser.Instance.ParseOrThrow("#99999980", nameof(RangeLabelColor));
    public FloatColor4 BearingLabelColor { get; set; } = ChainColorParser.Instance.ParseOrThrow("#C7C7C780", nameof(BearingLabelColor));
}
