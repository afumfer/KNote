namespace KNote.ClientWin.Utils;

/// <summary>
/// Replaces the app's previous, inconsistent per-Form column-resize handlers (three different ad-hoc
/// strategies: "stretch the last column via Width=-2", and two bespoke manual-width computations) with
/// one: a single "primary" column - the entity's identifying/descriptive column - grows or shrinks to
/// absorb whatever width the other, fixed-width columns don't use.
/// </summary>
public static class ListViewColumnResizer
{
    /// <summary>
    /// Wires the resize on every ListView.Resize. Also call Resize(...) once explicitly right after the
    /// list is first populated: a panel reparented into a TabPage (RepositoryEditorForm's tabs) does not
    /// reliably raise Resize the first time it becomes visible.
    /// </summary>
    public static void Attach(ListView listView, int primaryColumnIndex, int minPrimaryWidth = 80)
    {
        listView.Resize += (s, e) => Resize(listView, primaryColumnIndex, minPrimaryWidth);
    }

    public static void Resize(ListView listView, int primaryColumnIndex, int minPrimaryWidth = 80)
    {
        if (primaryColumnIndex < 0 || primaryColumnIndex >= listView.Columns.Count)
            return;

        try
        {
            var otherWidths = new List<int>(listView.Columns.Count - 1);
            for (int i = 0; i < listView.Columns.Count; i++)
            {
                if (i != primaryColumnIndex)
                    otherWidths.Add(listView.Columns[i].Width);
            }

            listView.Columns[primaryColumnIndex].Width =
                CalculatePrimaryWidth(listView.ClientSize.Width, otherWidths, minPrimaryWidth);
        }
        catch (Exception)
        {
            // Defensive: assigning a column Width can throw while the control's handle is being
            // created/torn down (same guard the previous per-Form resize handlers relied on).
        }
    }

    /// <summary>
    /// Pure width calculation, unit-testable without a live Control: the primary column gets whatever
    /// client width is left after the other, fixed-width columns, never shrinking below minPrimaryWidth.
    /// </summary>
    public static int CalculatePrimaryWidth(int clientWidth, IReadOnlyList<int> otherColumnWidths, int minPrimaryWidth)
    {
        int othersTotal = 0;
        foreach (var width in otherColumnWidths)
            othersTotal += width;

        return Math.Max(clientWidth - othersTotal, minPrimaryWidth);
    }
}
