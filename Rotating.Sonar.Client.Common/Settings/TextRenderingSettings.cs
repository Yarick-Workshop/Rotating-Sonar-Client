namespace Rotating.Sonar.Client.Common.Settings;

public sealed class TextRenderingSettings
{
    public int GlyphTileSizePx { get; set; } = 32;

    public int AtlasColumns { get; set; } = 16;

    public float TextSizePercentOfTile { get; set; } = 75f;

    public string ExtraGlyphs { get; set; } = "°";
}
