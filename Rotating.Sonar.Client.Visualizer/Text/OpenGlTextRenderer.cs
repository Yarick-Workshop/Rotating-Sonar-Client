#pragma warning disable CS0618
namespace Rotating.Sonar.Client.Visualizer.Text;

using Silk.NET.OpenGL.Legacy;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

record GlyphInfo(float U1, float V1, float U2, float V2, int Width, int Height, float Advance);

// TODO, refactor
public class OpenGlTextRenderer
{
    private const int TileSize = 32;
    private const int Columns = 16;

    private readonly GL gl;
    private uint atlasTexture;//TODO, temp
    private readonly Dictionary<char, GlyphInfo> glyphs;

    public OpenGlTextRenderer(GL gl, List<char> charTable)
    {
        this.gl = gl;

        this.glyphs = this.GenerateFontAtlas(charTable, TileSize, Columns);

        gl.Enable(GLEnum.Texture2D);// TODO, to think of it, where to initialize?
        gl.Enable(GLEnum.Blend);
        gl.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);
    }

    public OpenGlTextRenderer(GL gl)
        : this(gl, GetASCIITable())
    {
    }

    public OpenGlTextRenderer(GL gl, string extraGlyphs)
        : this(gl, GetASCIITable().Union(extraGlyphs).ToList())
    {
    }

    public void DrawText(string text, float x, float y)
    {
        this.DrawText(text, x, y, HorizontalAlignment.Left);
    }

    public void DrawText(string text, float x, float y, HorizontalAlignment hAlign)
    {
        float startX = x;

        switch (hAlign)
        {
            case HorizontalAlignment.Left:
                // Use the original logic - no adjustment needed
                break;
            case HorizontalAlignment.Right:
                startX = x - this.CalculateTextWidth(text);
                break;
            case HorizontalAlignment.Center:
                startX = x - this.CalculateTextWidth(text) / 2f;
                break;
            case HorizontalAlignment.None:
                // Use the original logic - no adjustment needed
                break;
            default:
                throw new ArgumentException($"Unsupported horizontal alignment: {hAlign}");
        }

        this.gl.BindTexture(TextureTarget.Texture2D, this.atlasTexture);

        float cursorX = startX;
        foreach (var c in text)
        {
            if (!this.glyphs.TryGetValue(c, out var g))
            {
                throw new ArgumentException($"Glyph not found for character: {c}");
            }

            float w = g.Width, h = g.Height;

            this.gl.Begin(GLEnum.Quads);
            this.gl.TexCoord2(g.U1, g.V1);
            this.gl.Vertex2(cursorX, y + h);

            this.gl.TexCoord2(g.U2, g.V1);
            this.gl.Vertex2(cursorX + w, y + h);

            this.gl.TexCoord2(g.U2, g.V2);
            this.gl.Vertex2(cursorX + w, y);

            this.gl.TexCoord2(g.U1, g.V2);
            this.gl.Vertex2(cursorX, y);
            this.gl.End();

            cursorX += g.Advance;
        }

        this.gl.BindTexture(TextureTarget.Texture2D, 0);
    }

    public float CalculateTextWidth(string text)
    {
        float totalWidth = 0f;
        foreach (var c in text)
        {
            if (!this.glyphs.TryGetValue(c, out var g))
            {
                throw new ArgumentException($"Glyph not found for character: {c}");
            }
            totalWidth += g.Advance;
        }
        return totalWidth;
    }

    private Dictionary<char, GlyphInfo> GenerateFontAtlas(List<char> glyphChars, int glyphTileSizePx, int columnsPerRow)
    {
        var count = glyphChars.Count;
        int rows = (int)Math.Ceiling(count / (float)columnsPerRow);
        int atlasWidth = columnsPerRow * glyphTileSizePx;
        int atlasHeight = rows * glyphTileSizePx;

        using var bitmap = new SKBitmap(atlasWidth, atlasHeight);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Transparent);

        using var paint = new SKPaint
        {
            TextSize = glyphTileSizePx * 0.75f,
            Typeface = SKTypeface.Default,
            IsAntialias = true,
            Color = SKColors.White,
            TextAlign = SKTextAlign.Left
        };

        var glyphMap = new Dictionary<char, GlyphInfo>();

        for (var i = 0; i < glyphChars.Count; i++)
        {
            var c = glyphChars[i];
            int col = i % columnsPerRow;
            int row = i / columnsPerRow;
            float x = col * glyphTileSizePx;
            float y = row * glyphTileSizePx;

            canvas.DrawText(c.ToString(), x, y + glyphTileSizePx * 0.75f, paint);

            float u1 = x / (float)atlasWidth;
            float v1 = y / (float)atlasHeight;
            float u2 = (x + glyphTileSizePx) / (float)atlasWidth;
            float v2 = (y + glyphTileSizePx) / (float)atlasHeight;

            float advance = paint.MeasureText(c.ToString());
            glyphMap[c] = new GlyphInfo(u1, v1, u2, v2, glyphTileSizePx, glyphTileSizePx, advance);
        }

        this.atlasTexture = this.UploadToGL(bitmap);

        return glyphMap;
    }

    unsafe uint UploadToGL(SKBitmap atlasBitmap)
    {
        uint tex = this.gl.GenTexture();
        this.gl.BindTexture(TextureTarget.Texture2D, tex);

        var data = atlasBitmap.Bytes;
        fixed (void* ptr = data)
        {
            this.gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgba,
                (uint)atlasBitmap.Width, (uint)atlasBitmap.Height, 0,
                PixelFormat.Bgra, PixelType.UnsignedByte, ptr);
        }

        this.gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        this.gl.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);

        return tex;
    }

    private static List<char> GetASCIITable()
    {
        var asciiTable = new List<char>();
        for (int i = 32; i < 127; i++)
        {
            asciiTable.Add((char)i);
        }
        return asciiTable;
    }
}

#pragma warning restore CS0618
