using KNote.Model.Dto;

namespace KNote.ClientWin.Core;

/// <summary>
/// The task of "Add quick resolved task" (the notes list and the post-it's menu): already resolved, from when the note was
/// created until now, with "Resolved task - " and the note's topic (shortened when long) as its Topic/Tags.
/// </summary>
public static class QuickResolvedTask
{
    public const string Title = "Resolved task";

    // The longest topic kept whole in the task's Topic/Tags: a longer one is cut at a word, with "...".
    public const int MaxTopicLength = 30;

    public static NoteTaskDto Create(Guid noteId, Guid userId, string noteTopic, DateTime noteCreation, DateTime now) => new()
    {
        NoteId = noteId,
        UserId = userId,
        Description = noteTopic,
        Tags = TagsFor(noteTopic),
        Priority = 1,
        Resolved = true,
        // A note dated ahead of this clock would make the task end before it starts: it then starts now.
        StartDate = noteCreation <= now ? noteCreation : now,
        EndDate = now
    };

    public static string TagsFor(string noteTopic)
    {
        var topic = ShortTopic(noteTopic);
        return topic.Length == 0 ? Title : $"{Title} - {topic}";
    }

    // In one line, without repeated spaces; cut at the last word that fits (or in a word, if the first half has no space).
    internal static string ShortTopic(string noteTopic)
    {
        var topic = string.Join(" ", (noteTopic ?? "").Split((char[])null, StringSplitOptions.RemoveEmptyEntries));
        if (topic.Length <= MaxTopicLength)
            return topic;

        var cut = topic[..MaxTopicLength];
        var space = cut.LastIndexOf(' ');
        if (space > MaxTopicLength / 2)
            cut = cut[..space];
        return cut.TrimEnd(' ', ',', ';', ':', '.', '-') + "...";
    }
}
