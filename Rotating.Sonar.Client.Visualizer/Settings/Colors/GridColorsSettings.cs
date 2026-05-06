namespace Rotating.Sonar.Client.Visualizer;

public sealed class GridColorsSettings
{
    public FloatColor4 LineColor { get; set; } = new(0.302f, 0.302f, 0.302f, 0.502f);
    public FloatColor4 TickLabelColor { get; set; } = new(0.6f, 0.6f, 0.6f, 0.502f);
    public FloatColor4 LabelColor { get; set; } = new(0.78f, 0.78f, 0.78f, 0.502f);
}
