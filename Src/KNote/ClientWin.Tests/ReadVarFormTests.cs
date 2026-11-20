using KNote.ClientWin.Views;
using KntScript;

namespace KNote.ClientWin.Tests;

/// <summary>
/// ReadVarForm, the dialog that reads one or more values (text search in the note editor, KntScript's readvar...): its rows are
/// created in code, so they are laid out once the form has its scale. With fixed pixels the label was cut and the box was tiny
/// above 100 %.
/// </summary>
[TestClass]
public class ReadVarFormTests
{
    internal sealed record Row(Rectangle Label, int LabelPreferredWidth, Rectangle Text);

    internal sealed record Measure(Row[] Rows, int PanelWidth, int PanelHeight, int ButtonsHeight, int ClientHeight, string[] Values);

    internal static Measure Run(params string[] labels)
    {
        Measure measure = null;
        Exception error = null;
        var thread = new Thread(() =>
        {
            try
            {
                var items = labels.Select((l, i) => new ReadVarItem { Label = l, VarIdent = $"v{i}", VarValue = $"value {i}", VarNewValueText = "" }).ToList();
                using var form = new ReadVarForm(items);
                form.Width = form.LogicalToDeviceUnits(500);
                form.StartPosition = FormStartPosition.Manual;
                form.Location = new Point(-32000, -32000);
                form.ShowInTaskbar = false;
                form.Show();
                Application.DoEvents();

                var panel = form.Controls.Find("panelControls", true).Single();
                var buttons = form.Controls.Find("panelButtons", true).Single();
                var rows = panel.Controls.OfType<Label>().Zip(panel.Controls.OfType<TextBox>())
                    .Select(p => new Row(p.First.Bounds, p.First.PreferredWidth, p.Second.Bounds)).ToArray();
                ((Button)form.AcceptButton).PerformClick();
                measure = new Measure(rows, panel.ClientSize.Width, panel.ClientSize.Height, buttons.Height, form.ClientSize.Height,
                    items.Select(i => i.VarNewValueText).ToArray());
                form.Hide();
            }
            catch (Exception ex)
            {
                error = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (error != null)
            throw new AssertFailedException($"The dialog failed: {error}");
        return measure;
    }

    [TestMethod]
    public void EachLabel_IsWhole_AndItsBoxFillsTheRestOfTheRow()
    {
        var measure = Run("Text for search", "A much longer label for a value");
        var margin = measure.Rows[0].Label.Left;

        foreach (var row in measure.Rows)
        {
            Assert.AreEqual(row.LabelPreferredWidth, row.Label.Width, "the label is not cut");
            Assert.IsTrue(row.Label.Right < row.Text.Left);
            Assert.AreEqual(measure.PanelWidth - margin, row.Text.Right);
            Assert.AreEqual(row.Text.Top + row.Text.Height / 2, row.Label.Top + row.Label.Height / 2, 1, "centered on its box");
        }
        Assert.AreEqual(1, measure.Rows.Select(r => r.Text.Left).Distinct().Count(), "the boxes start after the widest label");
        Assert.IsTrue(measure.Rows[1].Text.Top > measure.Rows[0].Text.Bottom);
    }

    [TestMethod]
    public void TheDialog_IsAsTallAsItsRows()
    {
        var one = Run("Text for search");
        var two = Run("Text for search", "Another");

        Assert.AreEqual(one.PanelHeight + one.ButtonsHeight, one.ClientHeight);
        Assert.IsTrue(one.Rows[0].Text.Bottom < one.PanelHeight);
        Assert.AreEqual(two.Rows[1].Text.Top - two.Rows[0].Text.Top, two.PanelHeight - one.PanelHeight, "one more row, one row taller");
    }

    [TestMethod]
    public void Accept_ReadsEachValue()
    {
        CollectionAssert.AreEqual(new[] { "value 0", "value 1" }, Run("First", "Second").Values);
    }
}
