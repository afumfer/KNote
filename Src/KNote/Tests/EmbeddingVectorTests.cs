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
}
