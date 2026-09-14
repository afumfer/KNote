using KNote.ClientWin.Utils;

namespace KNote.ClientWin.Tests;

[TestClass]
public class ListViewColumnResizerTests
{
    [TestMethod]
    public void CalculatePrimaryWidth_ExtraSpace_GrowsPrimaryColumn()
    {
        var width = ListViewColumnResizer.CalculatePrimaryWidth(clientWidth: 500, otherColumnWidths: [100, 100], minPrimaryWidth: 80);

        Assert.AreEqual(300, width);
    }

    [TestMethod]
    public void CalculatePrimaryWidth_NotEnoughSpace_ShrinksDownToMinimum()
    {
        var width = ListViewColumnResizer.CalculatePrimaryWidth(clientWidth: 150, otherColumnWidths: [100, 100], minPrimaryWidth: 80);

        Assert.AreEqual(80, width);
    }

    [TestMethod]
    public void CalculatePrimaryWidth_NoOtherColumns_UsesFullClientWidth()
    {
        var width = ListViewColumnResizer.CalculatePrimaryWidth(clientWidth: 300, otherColumnWidths: [], minPrimaryWidth: 80);

        Assert.AreEqual(300, width);
    }

    [TestMethod]
    public void Resize_InvalidPrimaryColumnIndex_DoesNotThrow()
    {
        using var listView = new ListView();
        listView.Columns.Add("Only");

        ListViewColumnResizer.Resize(listView, primaryColumnIndex: 5);
        ListViewColumnResizer.Resize(listView, primaryColumnIndex: -1);
    }

    [TestMethod]
    public void Resize_ValidPrimaryColumn_SetsItsWidthFromClientSize()
    {
        using var listView = new ListView { ClientSize = new Size(400, 200) };
        listView.Columns.Add("Fixed", 100);
        listView.Columns.Add("Primary", 100);

        ListViewColumnResizer.Resize(listView, primaryColumnIndex: 1, minPrimaryWidth: 50);

        Assert.AreEqual(300, listView.Columns[1].Width);
        Assert.AreEqual(100, listView.Columns[0].Width);
    }
}
