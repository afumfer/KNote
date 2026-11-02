using System;
using System.Buffers.Binary;

namespace KNote.Model;

/// <summary>
/// How a vector of the semantic index is kept in the NoteChunks.Vector column: its floats one after the other, in
/// float32 little-endian (4 bytes each), the same in every machine and database engine.
/// </summary>
public static class EmbeddingVector
{
    public static byte[] ToBytes(float[] vector)
    {
        if (vector == null)
            return null;

        var bytes = new byte[vector.Length * sizeof(float)];
        for (var i = 0; i < vector.Length; i++)
            BinaryPrimitives.WriteSingleLittleEndian(bytes.AsSpan(i * sizeof(float)), vector[i]);
        return bytes;
    }

    public static float[] FromBytes(byte[] bytes)
    {
        if (bytes == null)
            return null;
        if (bytes.Length % sizeof(float) != 0)
            throw new ArgumentException($"A vector of {bytes.Length} bytes is not a list of float32.", nameof(bytes));

        var vector = new float[bytes.Length / sizeof(float)];
        for (var i = 0; i < vector.Length; i++)
            vector[i] = BinaryPrimitives.ReadSingleLittleEndian(bytes.AsSpan(i * sizeof(float)));
        return vector;
    }
}
