#pragma warning disable CS0618
namespace Rotating.Sonar.Client.Visualizer;

using Silk.NET.OpenGL.Legacy;
using Silk.NET.Windowing;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;

record GlyphInfo(float U1, float V1, float U2, float V2, int Width, int Height);

public enum HorizontalAlignment
{
    None,
    Left,
    Right,
    Center
}

public class TextRenderOpenGL_1_1
{
    private readonly GL gl;
    private uint atlasTexture;//TODO, temp
    private readonly Dictionary<char, GlyphInfo> glyphs;

    const int TileSize = 32;
    const int Columns = 16;

    public TextRenderOpenGL_1_1(GL gl, List<char> charTable)
    {
        this.gl = gl;

        this.glyphs = this.GenerateFontAtlas(charTable, TileSize, Columns);

        gl.Enable(GLEnum.Texture2D);// TODO, to think of it, where to initialize?
        gl.Enable(GLEnum.Blend);
        gl.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);
    }

    public TextRenderOpenGL_1_1(GL gl)
        : this(gl, GetASCIITable())
    {
    }

    public TextRenderOpenGL_1_1(GL gl, string additionalChars)
        : this(gl, GetASCIITable().Union(additionalChars).ToList())
    {
    }

    public void DrawText(string text, float x, float y)
    {
        DrawText(text, x, y, HorizontalAlignment.Left);
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
                startX = x - CalculateTextWidth(text);
                break;
            case HorizontalAlignment.Center:
                startX = x - CalculateTextWidth(text) / 2f;
                break;
            case HorizontalAlignment.None:
                // Use the original logic - no adjustment needed
                break;
            default:
                throw new ArgumentException($"Unsupported horizontal alignment: {hAlign}");
        }

        gl.BindTexture(TextureTarget.Texture2D, atlasTexture);

        float cursorX = startX;
        foreach (var c in text)
        {
            if (!glyphs.TryGetValue(c, out var g))
            {
                throw new ArgumentException($"Glyph not found for character: {c}");
            }

            float w = g.Width, h = g.Height;

            gl.Begin(GLEnum.Quads);
                gl.TexCoord2(g.U1, g.V1);
                gl.Vertex2(cursorX, y + h);

                gl.TexCoord2(g.U2, g.V1);
                gl.Vertex2(cursorX + w, y + h);

                gl.TexCoord2(g.U2, g.V2);
                gl.Vertex2(cursorX + w, y);

                gl.TexCoord2(g.U1, g.V2);
                gl.Vertex2(cursorX, y);
            gl.End();

            cursorX += w * 0.75f; // advance
        }

        gl.BindTexture(TextureTarget.Texture2D, 0);
    }

    public float CalculateTextWidth(string text)
    {
        float totalWidth = 0f;
        foreach (var c in text)
        {
            if (!glyphs.TryGetValue(c, out var g))
            {
                throw new ArgumentException($"Glyph not found for character: {c}");
            }
            totalWidth += g.Width * 0.75f; // advance
        }
        return totalWidth;
    }

    private Dictionary<char, GlyphInfo> GenerateFontAtlas(List<char> charTable, int tileSize, int columns)
    {
        var count = charTable.Count;
        int rows = (int)Math.Ceiling(count / (float)columns);
        int atlasWidth = columns * tileSize;
        int atlasHeight = rows * tileSize;

        using var bitmap = new SKBitmap(atlasWidth, atlasHeight);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Transparent);

        using var paint = new SKPaint
        {
            TextSize = tileSize * 0.75f,
            Typeface = SKTypeface.Default,
            IsAntialias = true,
            Color = SKColors.White,
            TextAlign = SKTextAlign.Center
        };

        var glyphMap = new Dictionary<char, GlyphInfo>();

        for (var i = 0; i < charTable.Count; i++)
        {
            var c = charTable[i];
            int col = i % columns;
            int row = i / columns;
            float x = col * tileSize;
            float y = row * tileSize;

            canvas.DrawText(c.ToString(), x + tileSize / 2, y + tileSize * 0.75f, paint);

            float u1 = x / (float)atlasWidth;
            float v1 = y / (float)atlasHeight;
            float u2 = (x + tileSize) / (float)atlasWidth;
            float v2 = (y + tileSize) / (float)atlasHeight;

            glyphMap[c] = new GlyphInfo(u1, v1, u2, v2, tileSize, tileSize);
        }

        this.atlasTexture = this.UploadToGL(bitmap);

        return glyphMap;
    }

    unsafe uint UploadToGL(SKBitmap bitmap)
    {
        uint tex = this.gl.GenTexture();
        this.gl.BindTexture(TextureTarget.Texture2D, tex);

        var data = bitmap.Bytes;
        fixed (void* ptr = data)
        {
            this.gl.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgba,
                (uint)bitmap.Width, (uint)bitmap.Height, 0,
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
