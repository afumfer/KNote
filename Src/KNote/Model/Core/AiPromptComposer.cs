using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using KNote.Model.Dto;

namespace KNote.Model;

/// <summary>
/// What the AI receives for a task, in layers, always in this order (KNote Atlas, chapter 21; D5):
/// <list type="number">
/// <item>KNote's fixed rules of the task, with the repository, the user, the role and today's date: never replaced.</item>
/// <item>The instructions of the task: KNote's default ones, plus (Add) or instead of (Replace) the repository's active
/// note of the task; or, in a conversation that replaces them (InstructionsOverride, D6), only those.</item>
/// <item>The repository's profile, cut to MaxProfileWords.</item>
/// <item>The glossary terms that appear in the conversation (GlossaryTable.Find, D7).</item>
/// </list>
/// The notes go without their comments nor their empty sections (AtlasMarkdown.Clean); a layer with nothing is left out.
/// The data (the conversation, the notes read) are not part of it.
/// </summary>
public static class AiPromptComposer
{
    public const int MaxProfileWords = 2000;

    public const string RulesLayer = "KNote rules";
    public const string InstructionsLayer = "Instructions";
    public const string ProfileLayer = "Repository profile";
    public const string GlossaryLayer = "Glossary";
    private const string KNoteSource = "KNote";

    public sealed class Parts
    {
        public AiTask Task { get; init; } = AiTaskCatalog.Assistant;
        public string RepositoryName { get; init; }
        public string UserName { get; init; }
        public EnumRoles? Role { get; init; }
        public DateTime Today { get; init; } = DateTime.Today;
        public string InstructionsOverride { get; init; }
        // The active note of the task (TaskPromptReplaces: its Mode is Replace).
        public string TaskPromptText { get; init; }
        public bool TaskPromptReplaces { get; init; }
        public int? TaskPromptNoteNumber { get; init; }
        public string ProfileText { get; init; }
        public int? ProfileNoteNumber { get; init; }
        public IReadOnlyList<GlossaryTermDto> GlossaryTerms { get; init; } = Array.Empty<GlossaryTermDto>();
        public int? GlossaryNoteNumber { get; init; }
    }

    public static AiComposedPromptDto Compose(Parts parts)
    {
        var task = parts.Task ?? AiTaskCatalog.Assistant;
        var composed = new AiComposedPromptDto();

        composed.Layers.Add(new AiPromptLayerDto { Name = RulesLayer, Source = KNoteSource, Text = Rules(task, parts) });

        var instructions = Instructions(task, parts, out var instructionsSource);
        if (instructions.Length > 0)
            composed.Layers.Add(new AiPromptLayerDto { Name = InstructionsLayer, Source = instructionsSource, Text = instructions });

        var (profile, cut) = Profile(parts.ProfileText);
        composed.ProfileCut = cut;
        if (profile.Length > 0)
            composed.Layers.Add(new AiPromptLayerDto { Name = ProfileLayer, Source = NoteSource(parts.ProfileNoteNumber), Text = profile });

        var terms = parts.GlossaryTerms ?? Array.Empty<GlossaryTermDto>();
        if (terms.Count > 0)
        {
            composed.GlossaryTerms = terms.ToList();
            composed.Layers.Add(new AiPromptLayerDto { Name = GlossaryLayer, Source = NoteSource(parts.GlossaryNoteNumber), Text = Glossary(terms) });
        }

        composed.Text = string.Join("\n\n", composed.Layers.Select(l => $"# {l.Name}\n\n{l.Text}"));
        composed.Words = AtlasMarkdown.CountWords(composed.Text);
        composed.ApproxTokens = (composed.Text.Length + 3) / 4;
        return composed;
    }

    /// <summary>One line with the size and the layers of what the AI receives, for «What the AI receives».</summary>
    public static string Summary(AiComposedPromptDto composed) =>
        $"About {composed.Words.ToString("N0", CultureInfo.InvariantCulture)} words " +
        $"(~{composed.ApproxTokens.ToString("N0", CultureInfo.InvariantCulture)} tokens). Layers: " +
        string.Join(" · ", composed.Layers.Select(l => l.Name == GlossaryLayer
            ? $"{l.Name} ({l.Source}: {composed.GlossaryTerms.Count} terms)"
            : $"{l.Name} ({l.Source})")) +
        (composed.ProfileCut ? $". The profile was cut to {MaxProfileWords} words." : ".");

    /// <summary>The profile as the AI receives it: clean and cut to MaxProfileWords; true if it was cut.</summary>
    public static (string Text, bool Cut) Profile(string profileText)
    {
        var clean = AtlasMarkdown.Clean(profileText);
        var (text, cut) = AtlasMarkdown.TrimToWords(clean, MaxProfileWords);
        return cut ? (text + $"\n\n(The profile was cut: it is longer than {MaxProfileWords} words.)", true) : (text, false);
    }

    /// <summary>The fixed rules of a task with its placeholders filled in.</summary>
    public static string Rules(AiTask task, Parts parts) =>
        (task.FixedRules ?? "")
            .Replace("{repository}", string.IsNullOrWhiteSpace(parts.RepositoryName) ? "(no name)" : parts.RepositoryName)
            .Replace("{user}", string.IsNullOrWhiteSpace(parts.UserName) ? "(unknown)" : parts.UserName)
            .Replace("{role}", parts.Role?.ToString() ?? "(unknown)")
            .Replace("{date}", parts.Today.ToString("dddd, yyyy-MM-dd", CultureInfo.InvariantCulture));

    private static string Instructions(AiTask task, Parts parts, out string source)
    {
        var defaults = task.DefaultInstructions ?? "";
        if (!string.IsNullOrWhiteSpace(parts.InstructionsOverride))
        {
            source = "This conversation";
            return parts.InstructionsOverride.Trim();
        }

        var note = AtlasMarkdown.Clean(parts.TaskPromptText);
        if (note.Length == 0)
        {
            source = KNoteSource;
            return defaults;
        }
        source = $"{KNoteSource} + {NoteSource(parts.TaskPromptNoteNumber)}";
        if (parts.TaskPromptReplaces)
        {
            source = NoteSource(parts.TaskPromptNoteNumber);
            return note;
        }
        return defaults.Length == 0 ? note : $"{defaults}\n\n{note}";
    }

    private static string Glossary(IEnumerable<GlossaryTermDto> terms)
    {
        var text = new StringBuilder("Terms of this repository that appear in the conversation:\n");
        foreach (var term in terms)
        {
            text.Append($"\n- **{term.Term}**");
            if (term.AlsoWrittenAs?.Count > 0)
                text.Append($" (also written as {string.Join(", ", term.AlsoWrittenAs)})");
            text.Append($": {term.Meaning}");
        }
        return text.ToString();
    }

    private static string NoteSource(int? noteNumber) => noteNumber is int number ? $"#{number}" : "a note";
}
