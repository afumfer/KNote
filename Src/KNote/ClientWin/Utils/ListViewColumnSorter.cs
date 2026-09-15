#nullable enable

using System.Collections;
using System.Globalization;

namespace KNote.ClientWin.Utils;

/// <summary>
/// IComparer used as a ListView's ListViewItemSorter to sort its rows by one column, ascending or
/// descending. Column/order state (which column, which direction) is owned by this instance so
/// ListViewSortHelper can flip it on each header click and simply call ListView.Sort() again.
///
/// By default a column is compared numerically when both values parse as a number (so "Order"/"Min"/
/// "Priority" style columns sort 2 before 10, not the reverse a text compare would give), then as a
/// date/time when both parse as one (so "Start"/"Ex end"/... columns sort chronologically, not by the
/// formatted string), and otherwise as case-insensitive text. Pass customComparers only for the rare
/// column that needs something this default can't express (e.g. a custom enum ordering).
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
            : CompareValues(GetColumnText(itemX, SortColumn), GetColumnText(itemY, SortColumn));

        return Order == SortOrder.Descending ? -result : result;
    }

    /// <summary>
    /// The same numeric/date/text comparison used above, exposed for reuse by
    /// ListViewSortHelper.ApplyDefaultSort's multi-column comparer.
    /// </summary>
    public static int CompareValues(string textX, string textY)
    {
        if (double.TryParse(textX, NumberStyles.Number, CultureInfo.CurrentCulture, out var numberX) &&
            double.TryParse(textY, NumberStyles.Number, CultureInfo.CurrentCulture, out var numberY))
            return numberX.CompareTo(numberY);

        if (DateTime.TryParse(textX, CultureInfo.CurrentCulture, DateTimeStyles.None, out var dateX) &&
            DateTime.TryParse(textY, CultureInfo.CurrentCulture, DateTimeStyles.None, out var dateY))
            return dateX.CompareTo(dateY);

        return string.Compare(textX, textY, StringComparison.CurrentCultureIgnoreCase);
    }

    /// <summary>Exposed for reuse by ListViewSortHelper.ApplyDefaultSort's multi-column comparer.</summary>
    public static string GetColumnText(ListViewItem item, int columnIndex)
    {
        if (columnIndex == 0)
            return item.Text;

        return columnIndex < item.SubItems.Count ? item.SubItems[columnIndex].Text : string.Empty;
    }
}
