namespace KNote.ClientWin.Utils;

// The files in the clipboard, for the windows where they can be pasted: a picture (a screenshot, as PNG) or files copied in the
// file explorer. Empty with text, so that Ctrl+V pastes it as always.
public static class KntClipboardFiles
{
    public static List<(string Name, Func<byte[]> Content)> Read(Func<string, bool> accepts = null)
    {
        var files = new List<(string, Func<byte[]>)>();
        if (Clipboard.ContainsImage())
        {
            using var picture = Clipboard.GetImage();
            if (picture != null)
            {
                using var png = new MemoryStream();
                picture.Save(png, System.Drawing.Imaging.ImageFormat.Png);
                var content = png.ToArray();
                files.Add(($"image-{DateTime.Now:yyyyMMdd-HHmmss}.png", () => content));
            }
        }
        else if (Clipboard.ContainsFileDropList())
        {
            foreach (var file in Clipboard.GetFileDropList().Cast<string>().Where(f => accepts == null || accepts(f)))
                files.Add((Path.GetFileName(file), () => File.ReadAllBytes(file)));
        }
        return files;
    }
}
