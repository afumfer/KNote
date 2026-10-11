using Radzen;

namespace KNote.Client.Shared;

/// <summary>A Radzen theme in its light and dark versions.</summary>
public record KntThemeFamily(string Text, string Light, string Dark);

/// <summary>
/// The Radzen themes the app offers: the free ones, which ship with the Radzen.Blazor package. Used by the theme
/// selector (KntThemeSelector) and to validate the theme saved in the browser (App.razor).
/// </summary>
public static class KntThemes
{
    public const string Default = "software";

    public static readonly KntThemeFamily[] Families =
    [
        new("Software", "software", "software-dark"),
        new("Standard", "standard", "standard-dark"),
        new("Material", "material", "material-dark"),
        new("Humanistic", "humanistic", "humanistic-dark"),
        new("Default", "default", "dark")
    ];

    public static bool IsKnown(string? theme) => Families.Any(f => f.Light == theme || f.Dark == theme);

    public static bool IsDark(string? theme) => Families.Any(f => f.Dark == theme);

    public static KntThemeFamily FamilyOf(string? theme) =>
        Families.FirstOrDefault(f => f.Light == theme || f.Dark == theme) ?? Families[0];

    /// <summary>Colors of a theme (primary, secondary, base, content...), as Radzen describes it.</summary>
    public static Theme Colors(string theme) => Themes.Free.First(t => t.Value == theme);
}
