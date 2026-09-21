namespace VectorVisualizer;

using System.IO.Compression;
using System.Text;

/// <summary>
/// Encodes an 8-bit index raster as a PNG data URI, so a noise field can be shown as an ordinary
/// <c>&lt;img&gt;</c> element with no JavaScript interop and no canvas.
/// </summary>
/// <remarks>
/// <para><b>Why palette PNG rather than truecolour.</b> One byte per pixel keeps the encoded image small
/// enough to regenerate on every slider drag, and it puts the colour ramp in the palette — so changing
/// from a greyscale field to a terrain ramp re-writes 768 bytes rather than re-sampling the noise.</para>
/// <para>The encoder is deliberately minimal: no interlacing, no ancillary chunks, one filter mode (none).
/// It exists to display a field, not to be a general image library.</para>
/// </remarks>
public static class NoiseRaster
{
    private static readonly byte[] Signature = { 137, 80, 78, 71, 13, 10, 26, 10 };

    private static readonly uint[] CrcTable = BuildCrcTable();

    /// <summary>
    /// Encodes <paramref name="indices"/> (one palette index per pixel, row-major from the top) as a
    /// <c>data:image/png;base64,…</c> URI ready to drop into an <c>img src</c>.
    /// </summary>
    /// <param name="indices">Exactly <paramref name="width"/> × <paramref name="height"/> bytes.</param>
    /// <param name="palette">Exactly 768 bytes: 256 entries of R, G, B.</param>
    public static string ToPngDataUri(byte[] indices, int width, int height, byte[] palette)
    {
        if (indices is null) throw new ArgumentNullException(nameof(indices));
        if (palette is null) throw new ArgumentNullException(nameof(palette));
        if (width < 1 || height < 1) throw new ArgumentOutOfRangeException(nameof(width), "The raster must have a positive extent.");
        if (indices.Length != width * height) throw new ArgumentException("One index per pixel is required.", nameof(indices));
        if (palette.Length != 768) throw new ArgumentException("A palette of 256 RGB triples is required.", nameof(palette));

        using var png = new MemoryStream();
        png.Write(Signature, 0, Signature.Length);

        var ihdr = new byte[13];
        WriteBigEndian(ihdr, 0, (uint)width);
        WriteBigEndian(ihdr, 4, (uint)height);
        ihdr[8] = 8;    // bit depth
        ihdr[9] = 3;    // colour type 3: indexed
        ihdr[10] = 0;   // compression: deflate
        ihdr[11] = 0;   // filter method 0
        ihdr[12] = 0;   // no interlacing
        WriteChunk(png, "IHDR", ihdr);

        WriteChunk(png, "PLTE", palette);

        // Each scanline is prefixed with its filter byte; filter 0 (none) keeps the encoder honest and
        // costs little, because a noise field has no row-to-row coherence for a filter to exploit anyway.
        byte[] idat;
        using (var deflated = new MemoryStream())
        {
            using (var zlib = new ZLibStream(deflated, CompressionLevel.Fastest, leaveOpen: true))
            {
                for (int y = 0; y < height; y++)
                {
                    zlib.WriteByte(0);
                    zlib.Write(indices, y * width, width);
                }
            }

            idat = deflated.ToArray();
        }

        WriteChunk(png, "IDAT", idat);
        WriteChunk(png, "IEND", Array.Empty<byte>());

        return "data:image/png;base64," + Convert.ToBase64String(png.ToArray());
    }

    private static void WriteChunk(Stream target, string type, byte[] data)
    {
        Span<byte> length = stackalloc byte[4];
        WriteBigEndian(length, 0, (uint)data.Length);
        target.Write(length);

        // The CRC covers the type and the data but not the length, so they are checksummed together.
        var typeBytes = Encoding.ASCII.GetBytes(type);
        var body = new byte[typeBytes.Length + data.Length];
        Buffer.BlockCopy(typeBytes, 0, body, 0, typeBytes.Length);
        Buffer.BlockCopy(data, 0, body, typeBytes.Length, data.Length);
        target.Write(body, 0, body.Length);

        Span<byte> crc = stackalloc byte[4];
        WriteBigEndian(crc, 0, Crc32(body));
        target.Write(crc);
    }

    private static void WriteBigEndian(Span<byte> target, int offset, uint value)
    {
        target[offset] = (byte)(value >> 24);
        target[offset + 1] = (byte)(value >> 16);
        target[offset + 2] = (byte)(value >> 8);
        target[offset + 3] = (byte)value;
    }

    private static uint Crc32(byte[] buffer)
    {
        uint c = 0xFFFFFFFFu;
        for (int i = 0; i < buffer.Length; i++)
        {
            c = CrcTable[(c ^ buffer[i]) & 0xFF] ^ (c >> 8);
        }

        return c ^ 0xFFFFFFFFu;
    }

    private static uint[] BuildCrcTable()
    {
        var table = new uint[256];
        for (uint n = 0; n < 256; n++)
        {
            uint c = n;
            for (int k = 0; k < 8; k++)
            {
                c = (c & 1) != 0 ? 0xEDB88320u ^ (c >> 1) : c >> 1;
            }

            table[n] = c;
        }

        return table;
    }
}
