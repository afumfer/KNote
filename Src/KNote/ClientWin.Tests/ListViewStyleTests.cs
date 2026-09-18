using KNote.ClientWin.Utils;

namespace KNote.ClientWin.Tests;

[TestClass]
public class ListViewStyleTests
{
    [TestMethod]
    public void ApplyStandard_SetsTheSharedDetailViewLook()
    {
        using var listView = new ListView { View = View.LargeIcon, LabelEdit = true, AllowColumnReorder = true, CheckBoxes = true, Sorting = SortOrder.Ascending };

        ListViewStyle.ApplyStandard(listView);

        Assert.AreEqual(View.Details, listView.View);
        Assert.IsFalse(listView.LabelEdit);
        Assert.IsFalse(listView.AllowColumnReorder);
        Assert.IsFalse(listView.CheckBoxes);
        Assert.IsTrue(listView.FullRowSelect);
        Assert.IsTrue(listView.GridLines);
        Assert.AreEqual(SortOrder.None, listView.Sorting);
    }

    [TestMethod]
    public void ApplyStandard_WithCheckBoxes_EnablesThemAndKeepsTheRestOfTheLook()
    {
        using var listView = new ListView();

        ListViewStyle.ApplyStandard(listView, checkBoxes: true);

        Assert.IsTrue(listView.CheckBoxes);
        Assert.AreEqual(View.Details, listView.View);
        Assert.IsTrue(listView.FullRowSelect);
    }
}
