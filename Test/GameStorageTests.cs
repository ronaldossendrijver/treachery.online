using System.IO;
using System.Text;
using Treachery.Server;

namespace Treachery.Test;

[TestClass]
[DoNotParallelize]
public sealed class GameStorageTests
{
    [TestMethod]
    public void CompressionPreservesUnicodeAndEmptyStrings()
    {
        foreach (var json in new[] { "", "{}", "{\"name\":\"\\u00e9\\u6e38\"}", "{\"name\":\"\u00e9\u6e38\"}" })
        {
            var compressed = CompressedJson.Compress(json);
            Assert.AreEqual(json, CompressedJson.Decompress(compressed));
            if (json.Length > 0)
            {
                Assert.AreEqual((byte)0x1f, compressed[0]);
                Assert.AreEqual((byte)0x8b, compressed[1]);
            }
            else
            {
                Assert.IsEmpty(compressed);
            }
        }
    }

    [TestMethod]
    public void RepeatedEventJsonUsesLessStorage()
    {
        var json = "[" + string.Join(",", Enumerable.Repeat(
            "{\"type\":\"GameEvent\",\"player\":\"Test player\",\"turn\":10,\"time\":\"2026-09-29T23:00:00Z\"}", 1000)) + "]";

        var compressed = CompressedJson.Compress(json);

        Assert.IsLessThan(Encoding.UTF8.GetByteCount(json) / 4, compressed.Length);
        Assert.AreEqual(json, CompressedJson.Decompress(compressed));
    }

    [TestMethod]
    public void CorruptCompressionFailsExplicitly()
    {
        Assert.ThrowsExactly<InvalidDataException>(() => CompressedJson.Decompress([0xff, 0xff, 0xff, 0xff]));
        var compressed = CompressedJson.Compress("{\"events\":[]}");
        Assert.ThrowsExactly<InvalidDataException>(() => CompressedJson.Decompress(compressed[..^1]));
        Assert.ThrowsExactly<InvalidDataException>(() => CompressedJson.Decompress(compressed[..^8]));
    }

    [TestCleanup]
    public void RestoreCompressionSetting() => CompressedJson.CompressGameJson = false;

    [TestMethod]
    public void CompressionIsDisabledByDefault()
    {
        Assert.IsFalse(CompressedJson.CompressGameJson);
        Assert.AreEqual("{\"a\":1}", Encoding.UTF8.GetString(CompressedJson.Encode("{\"a\":1}")));
    }

    [TestMethod]
    public void DecodeAcceptsCompressedAndPlainJson()
    {
        foreach (var json in new[] { "", "{}", "{\"name\":\"\u00e9\u6e38\"}" })
        {
            Assert.AreEqual(json, CompressedJson.Decode(CompressedJson.Compress(json)));
            Assert.AreEqual(json, CompressedJson.Decode(Encoding.UTF8.GetBytes(json)));
        }
    }
}
