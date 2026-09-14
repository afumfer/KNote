using System.Collections;

namespace KNote.ClientWin.Utils;

/// <summary>
/// IComparer used as a ListView's ListViewItemSorter to sort its rows by one column, ascending or
/// descending. Column/order state (which column, which direction) is owned by this instance so
/// ListViewSortHelper can flip it on each header click and simply call ListView.Sort() again.
/// By default columns are compared as case-insensitive text (SubItems[column].Text); pass
/// customComparers for columns that need a different ordering - numeric ("Order", "Min"), date, etc. -
/// where comparing the displayed text would sort "10" before "2".
/// </summary>
public class ListViewColumnSorter : IComparer
{
    private readonly IDictionary<int, Comparison<ListViewItem>>? _customComparers;

    public ListViewColumnSorter(IDictionary<int, Comparison<ListViewItem>>? customComparers = null)
    {
        _customComparers = customComparers;
    }

    public int SortColumn { get; set; }

    public SortOrder Order { get; set; } = SortOrder.None;

    public int Compare(object? x, object? y)
    {
        if (Order == SortOrder.None || x is not ListViewItem itemX || y is not ListViewItem itemY)
            return 0;

        int result = _customComparers != null && _customComparers.TryGetValue(SortColumn, out var comparer)
            ? comparer(itemX, itemY)
            : string.Compare(GetColumnText(itemX, SortColumn), GetColumnText(itemY, SortColumn), StringComparison.CurrentCultureIgnoreCase);

        return Order == SortOrder.Descending ? -result : result;
    }

    private static string GetColumnText(ListViewItem item, int columnIndex)
    {
        if (columnIndex == 0)
            return item.Text;

        return columnIndex < item.SubItems.Count ? item.SubItems[columnIndex].Text : string.Empty;
    }
}
