using KNote.ClientWin.Utils;

namespace KNote.ClientWin.Tests;

[TestClass]
public class ListViewColumnSorterTests
{
    private static ListViewItem Row(params string[] columns)
    {
        var item = new ListViewItem(columns[0]);
        for (int i = 1; i < columns.Length; i++)
            item.SubItems.Add(columns[i]);
        return item;
    }

    [TestMethod]
    public void Compare_OrderNone_TreatsAllItemsAsEqual()
    {
        var sorter = new ListViewColumnSorter { SortColumn = 0, Order = SortOrder.None };

        Assert.AreEqual(0, sorter.Compare(Row("b"), Row("a")));
    }

    [TestMethod]
    public void Compare_Ascending_OrdersByColumnTextCaseInsensitive()
    {
        var sorter = new ListViewColumnSorter { SortColumn = 0, Order = SortOrder.Ascending };

        Assert.IsTrue(sorter.Compare(Row("apple"), Row("Banana")) < 0);
        Assert.IsTrue(sorter.Compare(Row("Banana"), Row("apple")) > 0);
        Assert.AreEqual(0, sorter.Compare(Row("Same"), Row("same")));
    }

    [TestMethod]
    public void Compare_Descending_ReversesAscendingResult()
    {
        var sorter = new ListViewColumnSorter { SortColumn = 0, Order = SortOrder.Descending };

        Assert.IsTrue(sorter.Compare(Row("apple"), Row("banana")) > 0);
    }

    [TestMethod]
    public void Compare_NonFirstColumn_ComparesSubItemText()
    {
        var sorter = new ListViewColumnSorter { SortColumn = 1, Order = SortOrder.Ascending };

        Assert.IsTrue(sorter.Compare(Row("x", "10"), Row("x", "9")) < 0);
    }

    [TestMethod]
    public void Compare_CustomComparer_UsedInsteadOfText()
    {
        var customComparers = new Dictionary<int, Comparison<ListViewItem>>
        {
            [1] = (a, b) => int.Parse(a.SubItems[1].Text).CompareTo(int.Parse(b.SubItems[1].Text))
        };
        var sorter = new ListViewColumnSorter(customComparers) { SortColumn = 1, Order = SortOrder.Ascending };

        // Numeric comparer must sort "9" before "10"; a text comparer would do the opposite.
        Assert.IsTrue(sorter.Compare(Row("x", "9"), Row("x", "10")) < 0);
    }

    [TestMethod]
    public void Compare_MissingSubItem_TreatedAsEmptyString()
    {
        var sorter = new ListViewColumnSorter { SortColumn = 2, Order = SortOrder.Ascending };

        Assert.IsTrue(sorter.Compare(Row("x"), Row("x", "y", "z")) < 0);
    }
}
