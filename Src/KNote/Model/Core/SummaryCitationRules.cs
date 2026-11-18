using System;
using System.Collections.Generic;
using System.Linq;

namespace KNote.Model.Core;

/// <summary>
/// The citations of a summary (KNote Atlas, stage 2, 2.6, D16): a citation #123 of a note that was not read is not a source. It is left
/// as text without a link: an invisible character (a word joiner) between # and the number keeps it as it looks, and the viewers don't
/// make it a link (NoteCitationLinker). The ones read, in the order they are first cited, are the order of the relations of the summary
/// with its notes (D17). The measure counts the ones not recognized with the same rule.
/// </summary>
public static class SummaryCitationRules
{
    public const char Unlinked = '⁠';

    public sealed record Checked(string Text, List<int> NotRecognized);

    /// <summary>The text with the citations of notes not read without a link, and their numbers, each once, in the order they appear.</summary>
    public static Checked Check(string text, ISet<int> read)
    {
        var notRecognized = new List<int>();
        var result = NoteCitationLinker.ReplaceCitations(text ?? "", (number, citation) =>
        {
            if (read.Contains(number))
                return citation;
            if (!notRecognized.Contains(number))
                notRecognized.Add(number);
            return $"#{Unlinked}{citation[1..]}";
        });
        return new Checked(result, notRecognized);
    }

    /// <summary>The notes cited in these texts, each once, in the order they are first cited.</summary>
    public static List<int> CitedInOrder(IEnumerable<string> texts)
    {
        var cited = new List<int>();
        foreach (var text in texts ?? Enumerable.Empty<string>())
            NoteCitationLinker.ReplaceCitations(text ?? "", (number, citation) =>
            {
                if (!cited.Contains(number))
                    cited.Add(number);
                return citation;
            });
        return cited;
    }

    /// <summary>The line of the data of the summary with the citations not recognized: «2 citations not recognized: #77, #912».</summary>
    public static string NotRecognizedText(IReadOnlyCollection<int> numbers) =>
        numbers == null || numbers.Count == 0 ? null
            : $"{(numbers.Count == 1 ? "1 citation" : $"{numbers.Count} citations")} not recognized: {string.Join(", ", numbers.Select(n => $"#{Unlinked}{n}"))}";
}
