namespace Rotating.Sonar.Client.Visualizer;

public sealed class GridColorsSettings
{
    public FloatColor4 LineColor { get; set; } = ChainColorParser.Instance.ParseOrThrow("#4D4D4D80", nameof(LineColor));
    public FloatColor4 TickLabelColor { get; set; } = ChainColorParser.Instance.ParseOrThrow("#99999980", nameof(TickLabelColor));
    public FloatColor4 LabelColor { get; set; } = ChainColorParser.Instance.ParseOrThrow("#C7C7C780", nameof(LabelColor));
}
