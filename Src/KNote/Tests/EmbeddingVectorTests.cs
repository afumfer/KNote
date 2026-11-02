using KNote.Model;

namespace KNote.Tests;

/// <summary>EmbeddingVector: a vector of the semantic index as the bytes of the NoteChunks.Vector column.</summary>
[TestClass]
public class EmbeddingVectorTests
{
    [TestMethod]
    public void ToBytes_FromBytes_RoundTrip()
    {
        var vector = new[] { 0f, 1f, -1f, 0.123456f, float.Epsilon, float.MaxValue, -3.5e-8f };

        var bytes = EmbeddingVector.ToBytes(vector);

        Assert.AreEqual(vector.Length * 4, bytes.Length);
        CollectionAssert.AreEqual(vector, EmbeddingVector.FromBytes(bytes));
    }

    [TestMethod]
    public void ToBytes_IsFloat32LittleEndian()
    {
        CollectionAssert.AreEqual(new byte[] { 0x00, 0x00, 0x80, 0x3F, 0x00, 0x00, 0x00, 0xC0 },
            EmbeddingVector.ToBytes(new[] { 1f, -2f }));
    }

    [TestMethod]
    public void Null_And_Empty()
    {
        Assert.IsNull(EmbeddingVector.ToBytes(null!));
        Assert.IsNull(EmbeddingVector.FromBytes(null!));
        Assert.AreEqual(0, EmbeddingVector.FromBytes(Array.Empty<byte>()).Length);
    }

    [TestMethod]
    public void FromBytes_NotAListOfFloats_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => EmbeddingVector.FromBytes(new byte[] { 1, 2, 3 }));
    }

    [TestMethod]
    public void Normalize_GivesLengthOne_AndCutsToTheDimensionsAskedFor()
    {
        var normalized = EmbeddingVector.Normalize(new[] { 3f, 4f, 12f });
        var cut = EmbeddingVector.Normalize(new[] { 3f, 4f, 12f }, 2);

        Assert.AreEqual(1f, EmbeddingVector.Dot(normalized, normalized), 1e-6f);
        CollectionAssert.AreEqual(new[] { 3f / 13, 4f / 13, 12f / 13 }, normalized);
        CollectionAssert.AreEqual(new[] { 0.6f, 0.8f }, cut);
    }

    [TestMethod]
    public void Normalize_ZerosStayZeros_AndMoreDimensionsThanTheModelsFail()
    {
        CollectionAssert.AreEqual(new[] { 0f, 0f }, EmbeddingVector.Normalize(new[] { 0f, 0f }));
        Assert.ThrowsExactly<ArgumentException>(() => EmbeddingVector.Normalize(new[] { 1f, 2f }, 3));
    }

    [TestMethod]
    public void Dot_OfVectorsOfDifferentSizes_Fails()
    {
        Assert.AreEqual(11f, EmbeddingVector.Dot(new[] { 1f, 2f }, new[] { 3f, 4f }));
        Assert.ThrowsExactly<ArgumentException>(() => EmbeddingVector.Dot(new[] { 1f }, new[] { 1f, 2f }));
    }
}
