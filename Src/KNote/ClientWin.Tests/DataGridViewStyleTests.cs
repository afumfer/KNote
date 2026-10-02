using KNote.ClientWin.Utils;

namespace KNote.ClientWin.Tests;

[TestClass]
public class DataGridViewStyleTests
{
    [TestMethod]
    public void ApplyDark_GivesFlatDarkHeadersAndDarkRows()
    {
        using var grid = new DataGridView();

        DataGridViewStyle.ApplyDark(grid);

        // Light visual styles would keep the headers light whatever their colors.
        Assert.IsFalse(grid.EnableHeadersVisualStyles);
        Assert.AreEqual(DataGridViewStyle.DarkHeaderBack, grid.ColumnHeadersDefaultCellStyle.BackColor);
        Assert.AreEqual(DataGridViewStyle.DarkHeaderText, grid.ColumnHeadersDefaultCellStyle.ForeColor);
        Assert.AreEqual(DataGridViewStyle.DarkBack, grid.BackgroundColor);
        Assert.AreEqual(DataGridViewStyle.DarkBack, grid.DefaultCellStyle.BackColor);
        Assert.AreEqual(DataGridViewStyle.DarkText, grid.DefaultCellStyle.ForeColor);
        Assert.AreEqual(DataGridViewStyle.DarkAlternateBack, grid.AlternatingRowsDefaultCellStyle.BackColor);
        Assert.AreEqual(DataGridViewStyle.DarkSelectionBack, grid.DefaultCellStyle.SelectionBackColor);
        Assert.AreEqual(DataGridViewStyle.DarkGridLines, grid.GridColor);
        Assert.AreEqual(DataGridViewCellBorderStyle.SingleHorizontal, grid.CellBorderStyle);
    }

    [TestMethod]
    public void ApplyDark_KeepsAColumnsOwnAlignment()
    {
        using var grid = new DataGridView();
        grid.Columns.Add("number", "Number");
        grid.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

        DataGridViewStyle.ApplyDark(grid);

        Assert.AreEqual(DataGridViewContentAlignment.MiddleRight, grid.Columns[0].InheritedStyle.Alignment);
        Assert.AreEqual(DataGridViewStyle.DarkText, grid.Columns[0].InheritedStyle.ForeColor);
    }

    [TestMethod]
    public void ApplyDark_RowsAreAtLeast24LogicalPixelsHigh()
    {
        using var grid = new DataGridView();

        DataGridViewStyle.ApplyDark(grid);

        Assert.IsTrue(grid.RowTemplate.Height >= grid.LogicalToDeviceUnits(24));
    }
}
