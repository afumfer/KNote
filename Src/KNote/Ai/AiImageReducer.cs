using KNote.Model;
using SkiaSharp;

namespace KNote.Ai;

// Photos at a size a model can take and a conversation can show (KNote Atlas, D12). The original stays intact in its resource.
// What can be sent as it is, is: a small image keeps its format and its quality.
public static class AiImageReducer
{
    public const int ModelMaxSide = 1600;
    public const int PreviewMaxSide = 480;

    private const int JpegQuality = 85;

    // Anthropic takes 5 MB of base64 per image: 3.5 MB of file, with some margin.
    private const int MaxUnchangedLength = 3_500_000;

    public sealed record ReducedImage(byte[] Content, string ContentType);

    // Null if it is not an image that can be read, or if this system has not got the native library of SkiaSharp: the model is
    // then told that it does not see it, and the conversation shows its name.
    public static ReducedImage Reduce(byte[] content, string contentType, int maxSide = ModelMaxSide)
    {
        if (content == null || content.Length == 0)
            return null;

        try
        {
            return Decode(content, contentType, maxSide);
        }
        catch (Exception ex) when (ex is DllNotFoundException or TypeInitializationException)
        {
            return null;
        }
    }

    private static ReducedImage Decode(byte[] content, string contentType, int maxSide)
    {
        using var data = SKData.CreateCopy(content);
        using var codec = SKCodec.Create(data);
        if (codec == null)
            return null;

        using var bitmap = SKBitmap.Decode(codec);
        if (bitmap == null)
            return null;

        // Phones save the photo as the sensor saw it, and say how to turn it.
        var degrees = codec.EncodedOrigin switch
        {
            SKEncodedOrigin.RightTop => 90,
            SKEncodedOrigin.BottomRight => 180,
            SKEncodedOrigin.LeftBottom => 270,
            _ => 0
        };
        var (width, height) = degrees is 90 or 270 ? (bitmap.Height, bitmap.Width) : (bitmap.Width, bitmap.Height);

        var scale = Math.Min(1.0, (double)maxSide / Math.Max(width, height));
        if (scale == 1.0 && degrees == 0 && content.Length <= MaxUnchangedLength && AiAttachmentRules.IsImage(contentType))
            return new ReducedImage(content, contentType);

        var targetWidth = Math.Max(1, (int)Math.Round(width * scale));
        var targetHeight = Math.Max(1, (int)Math.Round(height * scale));
        using var surface = SKSurface.Create(new SKImageInfo(targetWidth, targetHeight));
        if (surface == null)
            return null;

        // White under what is transparent: JPEG has no transparency, and it would come out black.
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);
        canvas.Translate(targetWidth / 2f, targetHeight / 2f);
        canvas.RotateDegrees(degrees);
        var halfWidth = (float)(bitmap.Width * scale / 2);
        var halfHeight = (float)(bitmap.Height * scale / 2);
        using var image = SKImage.FromBitmap(bitmap);
        canvas.DrawImage(image, new SKRect(-halfWidth, -halfHeight, halfWidth, halfHeight), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));

        using var snapshot = surface.Snapshot();
        using var encoded = snapshot.Encode(SKEncodedImageFormat.Jpeg, JpegQuality);
        return encoded == null ? null : new ReducedImage(encoded.ToArray(), "image/jpeg");
    }

    // To show it in the conversation, as a data: url.
    public static AiChatImagePreview Preview(byte[] content, string contentType)
    {
        var reduced = Reduce(content, contentType, PreviewMaxSide);
        return reduced == null ? null : new AiChatImagePreview(reduced.ContentType, Convert.ToBase64String(reduced.Content));
    }
}

public sealed record AiChatImagePreview(string ContentType, string ContentBase64)
{
    public string DataUrl => $"data:{ContentType};base64,{ContentBase64}";
}
