namespace KNote.Model;

/// <summary>
/// The first content of the notes of the context of KNote Atlas (chapters 19–21), in English as the interface; people
/// write in their own language. Their help are HTML comments: seen when editing, never sent to the AI (AtlasMarkdown.Clean).
/// Written here with LF; a note is created with CR+LF (NoteText).
/// </summary>
public static class AtlasTemplates
{
    public const string FoldersSection = "Folders";
    public const string NoteTypesSection = "Note types";

    /// <summary>The Markdown of a note KNote creates from a template: with CR+LF, as the notes written in the editors of Windows.</summary>
    public static string NoteText(string markdown) => (markdown ?? "").ReplaceLineEndings("\r\n");

    public const string Profile =
        "<!-- The profile of the repository (KNote Atlas): the AI receives it in all its tasks. Write it in plain words, " +
        "in your own language. The sections left empty are not sent, nor these comments. Up to about 2,000 words. -->\n" +
        "\n" +
        "## Purpose\n" +
        "\n" +
        "<!-- What this repository is for: a project, a subject, a collection, an album... -->\n" +
        "\n" +
        "## Who uses it\n" +
        "\n" +
        "<!-- A team (and the role of each one) or one person. -->\n" +
        "\n" +
        "## " + FoldersSection + "\n" +
        "\n" +
        "<!-- What there is in each folder. «Fill in folders and types» adds the folders missing here. -->\n" +
        "\n" +
        "## " + NoteTypesSection + "\n" +
        "\n" +
        "<!-- What each note type is for. «Fill in folders and types» adds the note types missing here. -->\n" +
        "\n" +
        "## Conventions\n" +
        "\n" +
        "<!-- How the notes are named, what each tag means, what is considered important. -->\n" +
        "\n" +
        "## What matters in summaries\n" +
        "\n" +
        "<!-- What should stand out in the summaries of a period. -->\n" +
        "\n" +
        "## Knowledge base classes\n" +
        "\n" +
        "<!-- The kinds of entries the knowledge base should have (for example: computers, manufacturers, programs). -->\n";

    /// <summary>
    /// The first content of the note that customizes a task (Mode Add): empty to write only one's own instructions, and
    /// KNote's default ones in a comment, as a starting point for Replace.
    /// </summary>
    public static string TaskPrompt(AiTask task) =>
        $"<!-- Your instructions for the task «{task.Name}» (KNote Atlas). With the mode Add, they are added to KNote's; " +
        "with Replace, they take their place (KNote's fixed rules always stay). Write them below, in your own language. " +
        "These comments are not sent to the AI. -->\n" +
        "\n" +
        "\n" +
        "\n" +
        $"<!-- KNote's instructions for this task, to start from with the mode Replace:\n\n{task.DefaultInstructions?.Replace("-->", "- ->")}\n-->\n";
}
