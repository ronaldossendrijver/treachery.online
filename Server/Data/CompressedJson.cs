using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;

namespace Treachery.Server;

public static class CompressedJson
{
    private static readonly Encoding Utf8 = new UTF8Encoding(false, true);

    /// <summary>
    /// When false, game JSON is written as uncompressed UTF-8 and legacy TEXT rows are left as-is at startup.
    /// Reading always accepts gzip-compressed, uncompressed UTF-8 BLOB and legacy TEXT values.
    /// </summary>
    public static bool CompressGameJson { get; set; } = false;

    public static byte[] Encode(string json) => CompressGameJson ? Compress(json) : Utf8.GetBytes(json);

    // Valid JSON never starts with the gzip magic bytes (0x1f is a control character, 0x8b a UTF-8 continuation byte).
    public static string Decode(byte[] data) => IsCompressed(data) ? Decompress(data) : Utf8.GetString(data);

    public static bool IsCompressed(byte[] data) => data.Length >= 2 && data[0] == 0x1f && data[1] == 0x8b;

    public static byte[] Compress(string json)
    {
        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionLevel.Optimal, leaveOpen: true))
        {
            gzip.Write(Utf8.GetBytes(json));
        }

        return output.ToArray();
    }

    public static string Decompress(byte[] data)
    {
        // GZipStream writes no bytes for empty input.
        if (data.Length == 0)
            return string.Empty;
        if (data.Length < 18 || data[0] != 0x1f || data[1] != 0x8b || data[2] != 8)
            throw new InvalidDataException("Invalid compressed game JSON header.");

        using var input = new MemoryStream(data);
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        gzip.CopyTo(output);
        if (output.Length != BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(data.Length - 4)))
            throw new InvalidDataException("Compressed game JSON is truncated or has an invalid length.");

        return Utf8.GetString(output.ToArray());
    }
}
