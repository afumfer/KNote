using System;
using System.Buffers.Binary;

namespace KNote.Model;

/// <summary>
/// How a vector of the semantic index is kept in the NoteChunks.Vector column: its floats one after the other, in
/// float32 little-endian (4 bytes each), the same in every machine and database engine. The vectors are kept
/// normalized (Normalize), so the similarity of two of them is their dot product.
/// </summary>
public static class EmbeddingVector
{
    /// <summary>
    /// A vector as the index keeps it: cut to its first dimensions when the repository asks for fewer than the model's
    /// (valid with the models trained for it, MRL) and normalized to length 1. A vector of zeros stays as it is.
    /// </summary>
    public static float[] Normalize(float[] vector, int? dimensions = null)
    {
        ArgumentNullException.ThrowIfNull(vector);
        var size = dimensions ?? vector.Length;
        if (size <= 0 || size > vector.Length)
            throw new ArgumentException($"The model gives vectors of {vector.Length} dimensions: they cannot be cut to {size}.", nameof(dimensions));

        var result = new float[size];
        double sum = 0;
        for (var i = 0; i < size; i++)
            sum += (double)vector[i] * vector[i];

        var length = Math.Sqrt(sum);
        for (var i = 0; i < size; i++)
            result[i] = length > 0 ? (float)(vector[i] / length) : vector[i];
        return result;
    }

    /// <summary>The dot product of two vectors of the same size (their similarity, if they are normalized).</summary>
    public static float Dot(float[] a, float[] b)
    {
        if (a.Length != b.Length)
            throw new ArgumentException($"Vectors of {a.Length} and {b.Length} dimensions.", nameof(b));

        double sum = 0;
        for (var i = 0; i < a.Length; i++)
            sum += (double)a[i] * b[i];
        return (float)sum;
    }

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
