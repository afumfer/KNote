using KNote.ClientWin.Utils;

namespace KNote.ClientWin.Tests;

[TestClass]
public class ListViewSelectionHelperTests
{
    private static ListView BuildListWithItems(params string[] keys)
    {
        var listView = new ListView();
        // Force native handle creation: ListView only syncs ListViewItem.Selected into
        // SelectedItems once its window handle exists (matches how these lists behave once shown
        // in a real Form - unlike Items/Columns, which are safe to populate before the handle exists).
        _ = listView.Handle;
        foreach (var key in keys)
            listView.Items.Add(new ListViewItem(key) { Name = key });
        return listView;
    }

    [TestMethod]
    public void SelectByKey_ExistingKey_SelectsOnlyThatItem()
    {
        using var listView = BuildListWithItems("a", "b", "c");

        var result = ListViewSelectionHelper.SelectByKey(listView, "b");

        Assert.IsTrue(result);
        Assert.AreEqual(1, listView.SelectedItems.Count);
        Assert.AreEqual("b", listView.SelectedItems[0].Name);
    }

    [TestMethod]
    public void SelectByKey_ReplacesAnyPreviousSelection()
    {
        using var listView = BuildListWithItems("a", "b");
        listView.Items[0].Selected = true;

        ListViewSelectionHelper.SelectByKey(listView, "b");

        Assert.AreEqual(1, listView.SelectedItems.Count);
        Assert.AreEqual("b", listView.SelectedItems[0].Name);
    }

    [TestMethod]
    public void SelectByKey_MissingKey_ReturnsFalseAndSelectsNothing()
    {
        using var listView = BuildListWithItems("a", "b");

        var result = ListViewSelectionHelper.SelectByKey(listView, "does-not-exist");

        Assert.IsFalse(result);
        Assert.AreEqual(0, listView.SelectedItems.Count);
    }

    [TestMethod]
    public void SelectByKey_NullOrEmptyKey_ReturnsFalse()
    {
        using var listView = BuildListWithItems("a");

        Assert.IsFalse(ListViewSelectionHelper.SelectByKey(listView, null));
        Assert.IsFalse(ListViewSelectionHelper.SelectByKey(listView, string.Empty));
    }

    [TestMethod]
    public void SelectFirst_ItemsPresent_SelectsFirstOnly()
    {
        using var listView = BuildListWithItems("a", "b", "c");
        listView.Items[2].Selected = true;

        ListViewSelectionHelper.SelectFirst(listView);

        Assert.AreEqual(1, listView.SelectedItems.Count);
        Assert.AreEqual("a", listView.SelectedItems[0].Name);
    }

    [TestMethod]
    public void SelectFirst_EmptyList_SelectsNothing()
    {
        using var listView = new ListView();

        ListViewSelectionHelper.SelectFirst(listView);

        Assert.AreEqual(0, listView.SelectedItems.Count);
    }

    [TestMethod]
    public void SelectByKey_WithActiveSort_ReapplySortBeforeSelecting()
    {
        using var listView = BuildListWithItems("c", "a", "b");
        var sorter = ListViewSortHelper.Attach(listView);
        sorter.SortColumn = 0;
        sorter.Order = SortOrder.Ascending;

        var result = ListViewSelectionHelper.SelectByKey(listView, "b", sorter);

        Assert.IsTrue(result);
        Assert.AreEqual("a", listView.Items[0].Name);
        Assert.AreEqual("b", listView.Items[1].Name);
        Assert.AreEqual("c", listView.Items[2].Name);
        Assert.AreEqual("b", listView.SelectedItems[0].Name);
    }
}
