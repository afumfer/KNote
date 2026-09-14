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
    public void Compare_NumericLookingText_ComparesNumericallyNotLexically()
    {
        var sorter = new ListViewColumnSorter { SortColumn = 1, Order = SortOrder.Ascending };

        // A text compare would put "10" before "2" (lexical '1' < '2'); the default must not do that.
        Assert.IsTrue(sorter.Compare(Row("x", "2"), Row("x", "10")) < 0);
        Assert.IsTrue(sorter.Compare(Row("x", "10"), Row("x", "2")) > 0);
    }

    [TestMethod]
    public void Compare_DateLookingText_ComparesChronologicallyNotLexically()
    {
        var sorter = new ListViewColumnSorter { SortColumn = 0, Order = SortOrder.Ascending };

        // A text compare of these two formatted dates would put 2026-01 after 2025-12 (lexical '2' >
        // '1' at the day digit); the default must compare them as dates, not as strings.
        var earlier = new DateTime(2025, 12, 31).ToString();
        var later = new DateTime(2026, 1, 5).ToString();

        Assert.IsTrue(sorter.Compare(Row(earlier), Row(later)) < 0);
    }

    [TestMethod]
    public void Compare_NonFirstColumn_UsesSubItemText()
    {
        var sorter = new ListViewColumnSorter { SortColumn = 1, Order = SortOrder.Ascending };

        Assert.IsTrue(sorter.Compare(Row("x", "apple"), Row("x", "banana")) < 0);
    }

    [TestMethod]
    public void Compare_CustomComparer_TakesPriorityOverDefault()
    {
        // Deliberately the reverse of what the numeric-aware default would produce, to prove the
        // override actually wins instead of just happening to agree with it.
        var customComparers = new Dictionary<int, Comparison<ListViewItem>>
        {
            [1] = (a, b) => int.Parse(b.SubItems[1].Text).CompareTo(int.Parse(a.SubItems[1].Text))
        };
        var sorter = new ListViewColumnSorter(customComparers) { SortColumn = 1, Order = SortOrder.Ascending };

        Assert.IsTrue(sorter.Compare(Row("x", "2"), Row("x", "10")) > 0);
    }

    [TestMethod]
    public void Compare_MissingSubItem_TreatedAsEmptyString()
    {
        var sorter = new ListViewColumnSorter { SortColumn = 2, Order = SortOrder.Ascending };

        Assert.IsTrue(sorter.Compare(Row("x"), Row("x", "y", "z")) < 0);
    }
}
