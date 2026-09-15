#nullable enable

namespace KNote.ClientWin.Utils;

/// <summary>
/// Standardizes which row ends up selected in a ListView after a CRUD operation: the affected entity
/// after Add/Edit, the first remaining row after Delete. Every CRUD screen in the app already sets
/// ListViewItem.Name to the entity's id (or, for AiProvidersManageForm, its Alias) when building rows,
/// so selection is keyed by that same string.
///
/// If the list has an active column sort (see ListViewSortHelper), pass its ListViewColumnSorter so the
/// sort is re-applied before selecting - ListView does not re-sort automatically when Items changes, so
/// without this a newly added/edited row could be "selected" while sitting out of sorted order.
/// </summary>
public static class ListViewSelectionHelper
{
    public static bool SelectByKey(ListView listView, string? key, ListViewColumnSorter? sorter = null)
    {
        if (sorter != null)
            ListViewSortHelper.ReapplySort(listView, sorter);

        if (string.IsNullOrEmpty(key))
            return false;

        var item = listView.Items[key];
        if (item == null)
            return false;

        Select(listView, item);
        return true;
    }

    public static void SelectFirst(ListView listView, ListViewColumnSorter? sorter = null)
    {
        if (sorter != null)
            ListViewSortHelper.ReapplySort(listView, sorter);

        ClearSelection(listView);

        if (listView.Items.Count > 0)
            Select(listView, listView.Items[0]);
    }

    private static void Select(ListView listView, ListViewItem item)
    {
        ClearSelection(listView);
        item.Selected = true;
        item.Focused = true;
        item.EnsureVisible();
    }

    private static void ClearSelection(ListView listView)
    {
        foreach (ListViewItem selected in listView.SelectedItems)
            selected.Selected = false;
    }
}
