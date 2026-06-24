using System.IO.Compression;

namespace PdfTemplateApi.Services;

public static class PngWriter
{
    private static readonly byte[] PngSignature =
    [
        137, 80, 78, 71, 13, 10, 26, 10
    ];

    public static byte[] WriteRgba(
        byte[] rgba,
        int width,
        int height)
    {
        if (rgba.Length != width * height * 4)
            throw new ArgumentException("RGBA buffer size does not match width/height.");

        using var output = new MemoryStream();

        output.Write(PngSignature);

        WriteChunk(output, "IHDR", BuildIhdr(width, height));
        WriteChunk(output, "IDAT", BuildIdat(rgba, width, height));
        WriteChunk(output, "IEND", []);

        return output.ToArray();
    }

    private static byte[] BuildIhdr(int width, int height)
    {
        using var stream = new MemoryStream();

        WriteUInt32BigEndian(stream, (uint)width);
        WriteUInt32BigEndian(stream, (uint)height);

        stream.WriteByte(8); // Bit depth
        stream.WriteByte(6); // Colour type: RGBA
        stream.WriteByte(0); // Compression method
        stream.WriteByte(0); // Filter method
        stream.WriteByte(0); // Interlace method

        return stream.ToArray();
    }

    private static byte[] BuildIdat(
        byte[] rgba,
        int width,
        int height)
    {
        using var raw = new MemoryStream();

        var rowLength = width * 4;

        for (var y = 0; y < height; y++)
        {
            raw.WriteByte(0); // PNG filter type: none

            raw.Write(
                rgba,
                y * rowLength,
                rowLength);
        }

        using var compressed = new MemoryStream();

        using (var zlib = new ZLibStream(
            compressed,
            CompressionLevel.Fastest,
            leaveOpen: true))
        {
            raw.Position = 0;
            raw.CopyTo(zlib);
        }

        return compressed.ToArray();
    }

    private static void WriteChunk(
        Stream output,
        string chunkType,
        byte[] data)
    {
        var typeBytes = System.Text.Encoding.ASCII.GetBytes(chunkType);

        WriteUInt32BigEndian(output, (uint)data.Length);

        output.Write(typeBytes);
        output.Write(data);

        var crcInput = new byte[typeBytes.Length + data.Length];

        Buffer.BlockCopy(typeBytes, 0, crcInput, 0, typeBytes.Length);
        Buffer.BlockCopy(data, 0, crcInput, typeBytes.Length, data.Length);

        var crc = Crc32.Compute(crcInput);

        WriteUInt32BigEndian(output, crc);
    }

    private static void WriteUInt32BigEndian(
        Stream stream,
        uint value)
    {
        stream.WriteByte((byte)(value >> 24));
        stream.WriteByte((byte)(value >> 16));
        stream.WriteByte((byte)(value >> 8));
        stream.WriteByte((byte)value);
    }

    private static class Crc32
    {
        private const uint Polynomial = 0xEDB88320u;

        private static readonly uint[] Table = BuildTable();

        public static uint Compute(byte[] bytes)
        {
            var crc = 0xFFFFFFFFu;

            foreach (var b in bytes)
            {
                var index = (crc ^ b) & 0xFF;
                crc = (crc >> 8) ^ Table[index];
            }

            return crc ^ 0xFFFFFFFFu;
        }

        private static uint[] BuildTable()
        {
            var table = new uint[256];

            for (uint i = 0; i < table.Length; i++)
            {
                var value = i;

                for (var bit = 0; bit < 8; bit++)
                {
                    value = (value & 1) == 1
                        ? (value >> 1) ^ Polynomial
                        : value >> 1;
                }

                table[i] = value;
            }

            return table;
        }
    }
}
