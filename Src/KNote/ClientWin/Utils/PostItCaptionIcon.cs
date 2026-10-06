namespace KNote.ClientWin.Utils;

/// <summary>
/// Color of the icon on a Post-It's caption: always a yellow, the app's own color. It's the soft yellow when it
/// stands out on the caption (dark or non-yellow captions) and, on light captions such as the default yellow
/// one, a deeper gold, the lightest one with enough contrast against the caption.
/// </summary>
internal static class PostItCaptionIcon
{
    // Contrast ratio (WCAG) the icon needs over the caption to be seen at a glance.
    internal const double MinContrast = 2.0;

    // Lightness range of the yellow tones tried, from the soft yellow (#FFE680) down to a dark gold.
    private const double SoftLightness = 0.75;
    private const double DeepestLightness = 0.30;
    private const double LightnessStep = 0.025;

    internal static Color GetColor(Color captionBackColor)
    {
        double background = RelativeLuminance(captionBackColor);
        Color best = Yellow(SoftLightness);
        double bestContrast = 0;

        for (double lightness = SoftLightness; lightness >= DeepestLightness; lightness -= LightnessStep)
        {
            var color = Yellow(lightness);
            double contrast = ContrastRatio(RelativeLuminance(color), background);
            if (contrast >= MinContrast)
                return color;
            if (contrast > bestContrast)
                (best, bestContrast) = (color, contrast);
        }

        // Mid-tone captions where no yellow reaches the contrast: the one that stands out the most.
        return best;
    }

    // Yellow (hue 48°, full saturation) with the given HSL lightness: black at 0, gold #FFCC00 at 0.5, white at 1.
    private static Color Yellow(double lightness)
    {
        static int Channel(double gold, double lightness) => (int)Math.Round(lightness <= 0.5
            ? gold * lightness * 2
            : gold + (255 - gold) * (lightness * 2 - 1));

        return Color.FromArgb(Channel(255, lightness), Channel(204, lightness), Channel(0, lightness));
    }

    internal static double ContrastRatio(double luminanceA, double luminanceB)
        => (Math.Max(luminanceA, luminanceB) + 0.05) / (Math.Min(luminanceA, luminanceB) + 0.05);

    internal static double RelativeLuminance(Color color)
    {
        static double Channel(byte value)
        {
            double c = value / 255.0;
            return c <= 0.03928 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
        }
        return 0.2126 * Channel(color.R) + 0.7152 * Channel(color.G) + 0.0722 * Channel(color.B);
    }
}
