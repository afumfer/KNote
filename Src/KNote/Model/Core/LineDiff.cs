using System;
using System.Collections.Generic;

namespace KNote.Model.Core;

/// <summary>
/// The differences between two texts, line by line (H12): Myers' algorithm after taking out the lines both texts begin and end
/// with. Lines compare as NoteChangeRules.SameText compares texts: CR LF and LF are the same and the spaces at the end of a line
/// don't count, nor the blank lines at the end of the text. Above MaxEdits changes it doesn't look for the shortest script (its
/// time and memory grow with the square of them): the different middle counts as removed and added (Replaced).
/// </summary>
public static class LineDiff
{
    public const int MaxEdits = 1000;

    public enum Kind
    {
        Equal,
        Removed,
        Added
    }

    /// <summary>A line of the comparison: its index in the text before (-1 if added) and after (-1 if removed).</summary>
    public readonly record struct Line(Kind Kind, int BeforeIndex, int AfterIndex);

    public sealed class Comparison
    {
        public IReadOnlyList<string> Before { get; init; }
        public IReadOnlyList<string> After { get; init; }
        public IReadOnlyList<Line> Lines { get; init; }
        public bool Replaced { get; init; }
        public int Added { get; init; }
        public int Removed { get; init; }
    }

    /// <summary>The lines of a text, without the spaces at the end of each one nor the blank lines at the end of the text.</summary>
    public static string[] SplitLines(string text)
    {
        if (string.IsNullOrEmpty(text))
            return Array.Empty<string>();

        var normalized = text.Replace("\r\n", "\n").Replace('\r', '\n').TrimEnd();
        if (normalized.Length == 0)
            return Array.Empty<string>();

        var lines = normalized.Split('\n');
        for (int i = 0; i < lines.Length; i++)
            lines[i] = lines[i].TrimEnd();
        return lines;
    }

    public static Comparison Compare(string before, string after, int maxEdits = MaxEdits) =>
        Compare(SplitLines(before), SplitLines(after), maxEdits);

    public static Comparison Compare(IReadOnlyList<string> before, IReadOnlyList<string> after, int maxEdits = MaxEdits)
    {
        before ??= Array.Empty<string>();
        after ??= Array.Empty<string>();

        int prefix = 0;
        while (prefix < before.Count && prefix < after.Count && before[prefix] == after[prefix])
            prefix++;

        int suffix = 0;
        while (suffix < before.Count - prefix && suffix < after.Count - prefix
            && before[before.Count - 1 - suffix] == after[after.Count - 1 - suffix])
            suffix++;

        int n = before.Count - prefix - suffix;
        int m = after.Count - prefix - suffix;

        var lines = new List<Line>(before.Count + after.Count);
        for (int i = 0; i < prefix; i++)
            lines.Add(new Line(Kind.Equal, i, i));

        var middle = n == 0 || m == 0 ? null : ShortestScript(before, after, prefix, n, m, maxEdits);
        var replaced = middle == null && n > 0 && m > 0;
        if (middle != null)
            lines.AddRange(middle);
        else
        {
            for (int i = 0; i < n; i++)
                lines.Add(new Line(Kind.Removed, prefix + i, -1));
            for (int j = 0; j < m; j++)
                lines.Add(new Line(Kind.Added, -1, prefix + j));
        }

        for (int i = 0; i < suffix; i++)
            lines.Add(new Line(Kind.Equal, before.Count - suffix + i, after.Count - suffix + i));

        int added = 0, removed = 0;
        foreach (var line in lines)
        {
            if (line.Kind == Kind.Added) added++;
            else if (line.Kind == Kind.Removed) removed++;
        }

        return new Comparison { Before = before, After = after, Lines = lines, Replaced = replaced, Added = added, Removed = removed };
    }

    // Myers (1986), forward, keeping the frontier of each step to go back along the path found. a and b are the middles
    // (offset, n and m lines). Null if it needs more than maxEdits steps.
    private static List<Line> ShortestScript(IReadOnlyList<string> a, IReadOnlyList<string> b, int offset, int n, int m, int maxEdits)
    {
        int limit = Math.Min(n + m, Math.Max(0, maxEdits));
        int vOffset = limit + 1;
        var v = new int[2 * limit + 3];
        var trace = new List<int[]>();

        int found = -1;
        for (int d = 0; d <= limit && found < 0; d++)
        {
            // The frontier before this step: diagonals -d-1 to d+1.
            var copy = new int[2 * d + 3];
            Array.Copy(v, vOffset - d - 1, copy, 0, copy.Length);
            trace.Add(copy);

            for (int k = -d; k <= d; k += 2)
            {
                int x = k == -d || (k != d && v[vOffset + k - 1] < v[vOffset + k + 1])
                    ? v[vOffset + k + 1]
                    : v[vOffset + k - 1] + 1;
                int y = x - k;
                while (x < n && y < m && a[offset + x] == b[offset + y])
                {
                    x++;
                    y++;
                }
                v[vOffset + k] = x;
                if (x >= n && y >= m)
                {
                    found = d;
                    break;
                }
            }
        }

        if (found < 0)
            return null;

        var reversed = new List<Line>();
        int cx = n, cy = m;
        for (int d = found; d > 0; d--)
        {
            var previous = trace[d];
            int k = cx - cy;
            bool down = k == -d || (k != d && previous[k - 1 + d + 1] < previous[k + 1 + d + 1]);
            int prevK = down ? k + 1 : k - 1;
            int prevX = previous[prevK + d + 1];
            int prevY = prevX - prevK;
            int startX = down ? prevX : prevX + 1;
            int startY = down ? prevY + 1 : prevY;

            while (cx > startX && cy > startY)
            {
                reversed.Add(new Line(Kind.Equal, offset + cx - 1, offset + cy - 1));
                cx--;
                cy--;
            }
            if (down)
                reversed.Add(new Line(Kind.Added, -1, offset + prevY));
            else
                reversed.Add(new Line(Kind.Removed, offset + prevX, -1));
            cx = prevX;
            cy = prevY;
        }
        while (cx > 0 && cy > 0)
        {
            reversed.Add(new Line(Kind.Equal, offset + cx - 1, offset + cy - 1));
            cx--;
            cy--;
        }

        reversed.Reverse();
        return reversed;
    }
}
