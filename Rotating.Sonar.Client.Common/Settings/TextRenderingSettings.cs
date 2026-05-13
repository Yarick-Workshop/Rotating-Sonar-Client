namespace Rotating.Sonar.Client.Common.Settings;

using System.ComponentModel.DataAnnotations;

public sealed class TextRenderingSettings
{
    [Range(1, int.MaxValue, ErrorMessage = "GlyphTileSizePx must be at least 1.")]
    public int GlyphTileSizePx { get; set; } = 32;

    [Range(1, int.MaxValue, ErrorMessage = "AtlasColumns must be at least 1.")]
    public int AtlasColumns { get; set; } = 16;

    [Range(0, 100, ErrorMessage = "TextSizePercentOfTile must be between 0 and 100.")]
    public float TextSizePercentOfTile { get; set; } = 75f;

    public string ExtraGlyphs { get; set; } = "°";
}
