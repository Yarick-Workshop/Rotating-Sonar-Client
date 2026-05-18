namespace Rotating.Sonar.Client.Common.Settings.Sections;

using System.ComponentModel.DataAnnotations;

public sealed class WindowSettings
{
    [Range(1, int.MaxValue, ErrorMessage = "WidthPx must be at least 1.")]
    public int WidthPx { get; set; } = 1920;

    [Range(1, int.MaxValue, ErrorMessage = "HeightPx must be at least 1.")]
    public int HeightPx { get; set; } = 1080;

    [Required(ErrorMessage = "Title is required.")]
    public string Title { get; set; } = "OpenGL Scan Display";
}
