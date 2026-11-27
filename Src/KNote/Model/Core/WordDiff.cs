using System;
using System.Collections.Generic;
using System.Text;

namespace KNote.Model.Core;

/// <summary>
/// The words that changed between a line removed and the line added in its place («Compare», H12): presentation only, the counts
/// stay by lines. The lines are cut into words (letters and digits), runs of spaces and each other character, and compared with
/// LineDiff (any list of strings). Null when they are too different to be told by words (less than MinSimilarity of their
/// characters in common, or too long): then the whole lines are marked.
/// </summary>
public static class WordDiff
{
    public const int MaxTokens = 2000;
    public const int MaxEdits = 300;
    public const double MinSimilarity = 0.4;

    /// <summary>A piece of a line: changed or as it was.</summary>
    public readonly record struct Segment(string Text, bool Changed);

    public sealed record Comparison(IReadOnlyList<Segment> Before, IReadOnlyList<Segment> After);

    public static Comparison Compare(string before, string after)
    {
        var a = Tokens(before);
        var b = Tokens(after);
        if (a.Count == 0 || b.Count == 0 || a.Count > MaxTokens || b.Count > MaxTokens)
            return null;

        var comparison = LineDiff.Compare(a, b, MaxEdits);
        if (comparison.Replaced)
            return null;

        int common = 0;
        var beforeSegments = new List<Segment>();
        var afterSegments = new List<Segment>();
        foreach (var token in comparison.Lines)
        {
            switch (token.Kind)
            {
                case LineDiff.Kind.Equal:
                    var text = a[token.BeforeIndex];
                    if (!string.IsNullOrWhiteSpace(text))
                        common += text.Length;
                    beforeSegments.Add(new Segment(text, false));
                    afterSegments.Add(new Segment(text, false));
                    break;
                case LineDiff.Kind.Removed:
                    beforeSegments.Add(new Segment(a[token.BeforeIndex], true));
                    break;
                case LineDiff.Kind.Added:
                    afterSegments.Add(new Segment(b[token.AfterIndex], true));
                    break;
            }
        }

        var significant = Math.Max(SignificantLength(a), SignificantLength(b));
        if (significant == 0 || common < MinSimilarity * significant)
            return null;

        return new Comparison(Coalesce(beforeSegments), Coalesce(afterSegments));
    }

    /// <summary>The words, runs of spaces and other characters of a line, in order.</summary>
    public static List<string> Tokens(string line)
    {
        var tokens = new List<string>();
        if (string.IsNullOrEmpty(line))
            return tokens;

        int i = 0;
        while (i < line.Length)
        {
            int start = i;
            if (IsWordChar(line[i]))
                while (i < line.Length && IsWordChar(line[i])) i++;
            else if (char.IsWhiteSpace(line[i]))
                while (i < line.Length && char.IsWhiteSpace(line[i])) i++;
            else
                i++;
            tokens.Add(line[start..i]);
        }
        return tokens;
    }

    private static bool IsWordChar(char c) => char.IsLetterOrDigit(c) || c == '_';

    private static int SignificantLength(List<string> tokens)
    {
        int length = 0;
        foreach (var token in tokens)
            if (!string.IsNullOrWhiteSpace(token))
                length += token.Length;
        return length;
    }

    // Pieces of the same state together; the spaces between two changes, changed too (one mark, not several).
    private static List<Segment> Coalesce(List<Segment> segments)
    {
        for (int i = 1; i < segments.Count - 1; i++)
            if (!segments[i].Changed && string.IsNullOrWhiteSpace(segments[i].Text) && segments[i - 1].Changed && segments[i + 1].Changed)
                segments[i] = segments[i] with { Changed = true };

        var merged = new List<Segment>();
        var text = new StringBuilder();
        bool? state = null;
        foreach (var segment in segments)
        {
            if (state != null && state != segment.Changed)
            {
                merged.Add(new Segment(text.ToString(), state.Value));
                text.Clear();
            }
            state = segment.Changed;
            text.Append(segment.Text);
        }
        if (state != null && text.Length > 0)
            merged.Add(new Segment(text.ToString(), state.Value));
        return merged;
    }
}
