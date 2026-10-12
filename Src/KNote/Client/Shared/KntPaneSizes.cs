using System.Globalization;
using System.Text.Json;
using Microsoft.JSInterop;
using Radzen;

namespace KNote.Client.Shared;

/// <summary>
/// Sizes (percentages) of the panes of a RadzenSplitter, as the user leaves them, kept in the browser (localStorage,
/// under Key) for the next time. The page gives each pane its size (Size="@paneSizes[i]") and the splitter's resizes
/// to Resized (Resize="@(args => paneSizes.Resized(args))").
/// </summary>
public class KntPaneSizes
{
    private readonly IJSInProcessRuntime js;
    private readonly string key;
    private readonly string[] sizes;

    /// <summary>The sizes saved under key, or the default ones (also when the saved ones don't fit the panes).</summary>
    public KntPaneSizes(IJSRuntime js, string key, params string[] defaultSizes)
    {
        // Synchronous (WebAssembly): the panes start with their sizes, without moving after the first render.
        this.js = (IJSInProcessRuntime)js;
        this.key = key;
        sizes = Read(defaultSizes.Length) ?? defaultSizes;
    }

    public string this[int pane] => sizes[pane];

    /// <summary>A pane was resized with its bar: it and the next one share the space they had.</summary>
    public void Resized(RadzenSplitterResizeEventArgs args)
    {
        var pane = args.PaneIndex;
        if (pane < 0 || pane >= sizes.Length - 1)
            return;

        var both = Percent(sizes[pane]) + Percent(sizes[pane + 1]);
        sizes[pane] = Format(args.NewSize);
        sizes[pane + 1] = Format(both - args.NewSize);
        js.InvokeVoid("localStorage.setItem", key, JsonSerializer.Serialize(sizes));
    }

    private string[]? Read(int panes)
    {
        var saved = js.Invoke<string?>("localStorage.getItem", key);
        if (string.IsNullOrEmpty(saved))
            return null;

        try
        {
            var read = JsonSerializer.Deserialize<string[]>(saved);
            return read != null && read.Length == panes && read.All(s => Percent(s) > 0) ? read : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static double Percent(string size) =>
        double.TryParse(size.TrimEnd('%'), NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : 0;

    private static string Format(double percent) =>
        percent.ToString("0.##", CultureInfo.InvariantCulture) + "%";
}
