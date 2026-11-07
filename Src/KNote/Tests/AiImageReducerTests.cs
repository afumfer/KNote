using KNote.Ai;
using SkiaSharp;

namespace KNote.Tests;

[TestClass]
public class AiImageReducerTests
{
    private static byte[] Image(int width, int height, SKEncodedImageFormat format = SKEncodedImageFormat.Png, SKColor? color = null)
    {
        using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        bitmap.Erase(color ?? SKColors.Red);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(format, 90);
        return data.ToArray();
    }

    private static SKBitmap Decode(byte[] content) => SKBitmap.Decode(content);

    // A JPEG with an EXIF block saying how to turn it, as a phone saves a photo: 6 = turn it 90 degrees.
    private static byte[] WithOrientation(byte[] jpeg, ushort orientation)
    {
        byte[] tiff =
        [
            0x4D, 0x4D, 0x00, 0x2A, 0x00, 0x00, 0x00, 0x08, 0x00, 0x01,
            0x01, 0x12, 0x00, 0x03, 0x00, 0x00, 0x00, 0x01, (byte)(orientation >> 8), (byte)orientation, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00
        ];
        byte[] exif = [.. "Exif"u8, 0, 0, .. tiff];
        var length = exif.Length + 2;
        return [jpeg[0], jpeg[1], 0xFF, 0xE1, (byte)(length >> 8), (byte)length, .. exif, .. jpeg[2..]];
    }

    [TestMethod]
    public void ASmallImage_IsSentAsItIs()
    {
        var content = Image(800, 600);

        var reduced = AiImageReducer.Reduce(content, "image/png");

        Assert.AreSame(content, reduced!.Content);
        Assert.AreEqual("image/png", reduced.ContentType);
    }

    [TestMethod]
    public void ABigImage_IsReducedToItsLongestSide_KeepingItsShape()
    {
        var reduced = AiImageReducer.Reduce(Image(3200, 1600), "image/png");

        using var bitmap = Decode(reduced!.Content);
        Assert.AreEqual("image/jpeg", reduced.ContentType);
        Assert.AreEqual(AiImageReducer.ModelMaxSide, bitmap.Width);
        Assert.AreEqual(800, bitmap.Height);
    }

    [TestMethod]
    public void ATallImage_IsReducedByItsHeight()
    {
        var reduced = AiImageReducer.Reduce(Image(1000, 4000), "image/png");

        using var bitmap = Decode(reduced!.Content);
        Assert.AreEqual(400, bitmap.Width);
        Assert.AreEqual(AiImageReducer.ModelMaxSide, bitmap.Height);
    }

    [TestMethod]
    public void ATransparentImage_ComesOutOverWhite_NotOverBlack()
    {
        var reduced = AiImageReducer.Reduce(Image(2000, 2000, color: SKColors.Transparent), "image/png");

        using var bitmap = Decode(reduced!.Content);
        var pixel = bitmap.GetPixel(10, 10);
        Assert.IsTrue(pixel.Red > 250 && pixel.Green > 250 && pixel.Blue > 250, pixel.ToString());
    }

    [TestMethod]
    public void APhotoTurnedByItsExif_ComesOutTheRightWayUp()
    {
        var photo = WithOrientation(Image(400, 200, SKEncodedImageFormat.Jpeg), 6);

        var reduced = AiImageReducer.Reduce(photo, "image/jpeg");

        using var bitmap = Decode(reduced!.Content);
        Assert.AreEqual(200, bitmap.Width);
        Assert.AreEqual(400, bitmap.Height);
    }

    [TestMethod]
    public void TheBytesOfSomethingElse_AreNotAnImage()
    {
        Assert.IsNull(AiImageReducer.Reduce("This is not an image"u8.ToArray(), "image/png"));
        Assert.IsNull(AiImageReducer.Reduce(Array.Empty<byte>(), "image/png"));
        Assert.IsNull(AiImageReducer.Reduce(null!, "image/png"));
    }

    [TestMethod]
    public void AnImageWithoutAKnownType_IsConverted_SoThatTheModelGetsOneItAccepts()
    {
        var reduced = AiImageReducer.Reduce(Image(100, 100), "application/octet-stream");

        Assert.AreEqual("image/jpeg", reduced!.ContentType);
    }

    [TestMethod]
    public void Preview_IsSmall_AndReadyForAPage()
    {
        var preview = AiImageReducer.Preview(Image(2000, 1000), "image/png");

        using var bitmap = Decode(Convert.FromBase64String(preview!.ContentBase64));
        Assert.AreEqual(AiImageReducer.PreviewMaxSide, bitmap.Width);
        StringAssert.StartsWith(preview.DataUrl, "data:image/jpeg;base64,");
    }

    [TestMethod]
    public void Preview_OfSomethingElse_IsNull() =>
        Assert.IsNull(AiImageReducer.Preview("Text"u8.ToArray(), "image/png"));
}
