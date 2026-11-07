using KNote.Ai;
using Microsoft.Extensions.AI;
using SkiaSharp;

namespace KNote.ClientWin.Tests.Helpers;

// KNote Atlas 1.5: an image with a number, reduced as the assistant does, read back by a real model: the check of the
// RequiresRealAiProvider suites that DataContent still reaches each provider.
internal static class AiImageSmokeCheck
{
    public static async Task AssertTheModelReadsAnImageAsync(IChatClient client)
    {
        var image = AiImageReducer.Reduce(Picture("7351"), "image/png");

        var response = await client.GetResponseAsync([new ChatMessage(ChatRole.User,
            [new TextContent("What number is written in this image? Reply with only the number."), new DataContent(image.Content, image.ContentType)])]);

        StringAssert.Contains(response.Text, "7351");
    }

    private static byte[] Picture(string text)
    {
        using var surface = SKSurface.Create(new SKImageInfo(600, 200));
        surface.Canvas.Clear(SKColors.White);
        using var font = new SKFont(SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold), 90);
        using var paint = new SKPaint { Color = SKColors.Black };
        surface.Canvas.DrawText(text, 120, 130, font, paint);
        using var snapshot = surface.Snapshot();
        using var data = snapshot.Encode(SKEncodedImageFormat.Png, 100);
        return data.ToArray();
    }
}
