using KNote.ClientWin.Utils;

namespace KNote.ClientWin.Tests;

[TestClass]
public class ListViewSortHelperTests
{
    private static ListView BuildListView(int columnCount, params string[][] rows)
    {
        var listView = new ListView();
        for (int i = 0; i < columnCount; i++)
            listView.Columns.Add("Col" + i);

        foreach (var row in rows)
        {
            var item = new ListViewItem(row[0]) { Name = row[0] };
            for (int i = 1; i < row.Length; i++)
                item.SubItems.Add(row[i]);
            listView.Items.Add(item);
        }
        return listView;
    }

    private static string[] TextsOf(ListView listView, int column) =>
        listView.Items.Cast<ListViewItem>().Select(_ => ListViewColumnSorter.GetColumnText(_, column)).ToArray();

    [TestMethod]
    public void ApplyDefaultSort_MultipleColumns_SortsAscendingLeftToRightByAllColumns()
    {
        using var listView = BuildListView(2,
            ["B", "2"],
            ["A", "2"],
            ["A", "1"]);

        ListViewSortHelper.ApplyDefaultSort(listView);

        CollectionAssert.AreEqual(new[] { "A", "A", "B" }, TextsOf(listView, 0));
        CollectionAssert.AreEqual(new[] { "1", "2", "2" }, TextsOf(listView, 1));
    }

    [TestMethod]
    public void ApplyDefaultSort_NumericColumn_SortsNumericallyNotLexically()
    {
        using var listView = BuildListView(1, ["10"], ["2"], ["1"]);

        ListViewSortHelper.ApplyDefaultSort(listView);

        CollectionAssert.AreEqual(new[] { "1", "2", "10" }, TextsOf(listView, 0));
    }

    [TestMethod]
    public void ApplyDefaultSort_FewerThanTwoItems_DoesNotThrow()
    {
        using var empty = BuildListView(2);
        using var single = BuildListView(2, ["A", "1"]);

        ListViewSortHelper.ApplyDefaultSort(empty);
        ListViewSortHelper.ApplyDefaultSort(single);

        Assert.AreEqual(0, empty.Items.Count);
        Assert.AreEqual(1, single.Items.Count);
    }

    [TestMethod]
    public void ApplyInitialOrder_NoActiveSort_AppliesDefaultMultiColumnOrder()
    {
        using var listView = BuildListView(1, ["B"], ["A"]);
        var sorter = new ListViewColumnSorter();

        ListViewSortHelper.ApplyInitialOrder(listView, sorter);

        CollectionAssert.AreEqual(new[] { "A", "B" }, TextsOf(listView, 0));
    }

    [TestMethod]
    public void ApplyInitialOrder_NullSorter_AppliesDefaultOrderInsteadOfThrowing()
    {
        // A list can be populated before its Form has wired up a ListViewColumnSorter (e.g. an
        // ordering bug fixed in NoteEditorForm: RefreshView() ran before the constructor had attached
        // the sorters) - null must behave like "no active sort" rather than crashing.
        using var listView = BuildListView(1, ["B"], ["A"]);

        ListViewSortHelper.ApplyInitialOrder(listView, null);

        CollectionAssert.AreEqual(new[] { "A", "B" }, TextsOf(listView, 0));
    }

    [TestMethod]
    public void ReapplySort_NullSorter_DoesNotThrow()
    {
        using var listView = BuildListView(1, ["B"], ["A"]);

        ListViewSortHelper.ReapplySort(listView, null);

        CollectionAssert.AreEqual(new[] { "B", "A" }, TextsOf(listView, 0));
    }

    [TestMethod]
    public void ApplyInitialOrder_ActiveDescendingSort_ReappliesUserSortInsteadOfDefault()
    {
        using var listView = BuildListView(1, ["A"], ["B"]);
        // Unlike ApplyDefaultSort (which reorders Items directly), ReapplySort goes through
        // ListView.Sort(), which - like ListViewItem.Selected - only takes effect once the native
        // handle exists (see ListViewSelectionHelperTests' BuildListWithItems for the same pattern).
        _ = listView.Handle;
        var sorter = new ListViewColumnSorter { SortColumn = 0, Order = SortOrder.Descending };
        listView.ListViewItemSorter = sorter;

        ListViewSortHelper.ApplyInitialOrder(listView, sorter);

        CollectionAssert.AreEqual(new[] { "B", "A" }, TextsOf(listView, 0));
    }
}
