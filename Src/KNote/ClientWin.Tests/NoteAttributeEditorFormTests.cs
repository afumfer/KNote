using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.ClientWin.Views;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// The note attribute editor (NoteAttributeEditorForm) with each data type: it opens (a Bool without a value made it throw while
/// it was prepared, so a double click did nothing), keeps the value, and places the value inside the panel with the label's
/// margin on both sides (fixed logical widths passed the right edge at 200 % and hid the combo's arrow).
/// </summary>
[TestClass]
public class NoteAttributeEditorFormTests
{
    private static readonly string[] Tabulated = { "Alpha", "Beta", "Gamma" };

    internal sealed record Measure(string Value, bool Checked, Rectangle Bounds, int Margin, int PanelWidth, Rectangle? DateButton,
        string[] CheckedItems, int ColumnWidth, int ListClientWidth, int LabelTextLeft, int DescriptionTextLeft, int LabelBottom,
        int Gap, int DescriptionTop, int DescriptionHeight, int FormHeight);

    internal static Measure Run(EnumKAttributeDataType type, string value, Action<Control> change = null)
    {
        Measure measure = null;
        Exception error = null;
        var thread = new Thread(() =>
        {
            try
            {
                var factory = new TestFactoryViews();
                factory.Registry.Register<NoteAttributeEditorCtrl, IViewEditor<NoteKAttributeDto>>(c => new NoteAttributeEditorForm(c));
                var store = new Store(factory);
                var service = new FakeKntService();
                service.KAttributesFake.GetKAttributeTabulatedValuesAsyncImpl = _ => Task.FromResult(new Result<List<KAttributeTabulatedValueDto>>
                {
                    Entity = Tabulated.Select(v => new KAttributeTabulatedValueDto { Value = v }).ToList()
                });

                var ctrl = new NoteAttributeEditorCtrl(store) { AutoDBSave = false };
                ctrl.LoadModel(service, new NoteKAttributeDto
                {
                    NoteKAttributeId = Guid.NewGuid(), KAttributeId = Guid.NewGuid(), Name = "A rather long attribute name",
                    Description = "Its description", KAttributeDataType = type, Value = value
                }, false);
                using var form = (NoteAttributeEditorForm)ctrl.View;
                form.RefreshView();
                form.StartPosition = FormStartPosition.Manual;
                form.Location = new Point(-32000, -32000);
                form.ShowInTaskbar = false;
                form.Show();
                Application.DoEvents();

                var panel = form.Controls.Find("panelForm", true).Single();
                var label = (Label)form.Controls.Find("labelAttribute", true).Single();
                var description = (Label)form.Controls.Find("labelDescription", true).Single();
                var textValue = form.Controls.Find("textValue", true).Single();
                var control = panel.Controls.Cast<Control>().Single(c => c.Visible && c is TextBox or ComboBox or CheckBox or ListView);
                change?.Invoke(control);
                form.RefreshModel();
                var date = form.Controls.Find("buttonSelDate", true).Single();
                var list = control as ListView;
                measure = new Measure(ctrl.Model.Value, (control as CheckBox)?.Checked ?? false, control.Bounds, textValue.Left, panel.ClientSize.Width,
                    date.Visible ? date.Bounds : null,
                    list?.Items.Cast<ListViewItem>().Where(i => i.Checked).Select(i => i.Text).ToArray() ?? Array.Empty<string>(),
                    list?.Columns[0].Width ?? 0, list?.ClientSize.Width ?? 0, TextLeft(label), TextLeft(description), label.Bottom,
                    form.LogicalToDeviceUnits(6), description.Top, description.Height, form.Height);
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
            throw new AssertFailedException($"The editor failed: {error}");
        return measure;
    }

    // Where a label's text starts: its left plus the padding GDI adds before the text.
    private static int TextLeft(Label label)
    {
        const TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.SingleLine;
        var padded = TextRenderer.MeasureText(label.Text, label.Font, Size.Empty, flags).Width;
        var unpadded = TextRenderer.MeasureText(label.Text, label.Font, Size.Empty, flags | TextFormatFlags.NoPadding).Width;
        return label.Left + (padded - unpadded) / 2;
    }

    [TestMethod]
    [DataRow(EnumKAttributeDataType.String)]
    [DataRow(EnumKAttributeDataType.Int)]
    [DataRow(EnumKAttributeDataType.Bool)]
    [DataRow(EnumKAttributeDataType.TabulatedValue)]
    [DataRow(EnumKAttributeDataType.TextArea)]
    [DataRow(EnumKAttributeDataType.TagsValue)]
    public void TheLabels_StartWhereTheValueDoes_AndTheValueHasRoomUnderItsLabel(EnumKAttributeDataType type)
    {
        var measure = Run(type, null);

        Assert.AreEqual(measure.Bounds.Left, measure.LabelTextLeft);
        Assert.AreEqual(measure.Bounds.Left, measure.DescriptionTextLeft);
        Assert.AreEqual(measure.LabelBottom + measure.Gap, measure.Bounds.Top);
    }

    [TestMethod]
    [DataRow(EnumKAttributeDataType.String)]
    [DataRow(EnumKAttributeDataType.Int)]
    [DataRow(EnumKAttributeDataType.Double)]
    [DataRow(EnumKAttributeDataType.DateTime)]
    [DataRow(EnumKAttributeDataType.Bool)]
    [DataRow(EnumKAttributeDataType.TabulatedValue)]
    public void AOneLineValue_HasAShorterDialog_WithTheDescriptionUnderIt(EnumKAttributeDataType type)
    {
        var measure = Run(type, null);
        var textArea = Run(EnumKAttributeDataType.TextArea, null);

        Assert.AreEqual(measure.Bounds.Bottom + measure.DescriptionHeight, measure.DescriptionTop);
        Assert.AreEqual(textArea.Bounds.Bottom + textArea.DescriptionHeight, textArea.DescriptionTop, "the same space in the text area");
        Assert.IsTrue(measure.FormHeight < textArea.FormHeight, $"{measure.FormHeight} against {textArea.FormHeight}");
    }

    [TestMethod]
    public void TheTextAreaAndTheTags_KeepTheDialogSize()
    {
        Assert.AreEqual(Run(EnumKAttributeDataType.TextArea, null).FormHeight, Run(EnumKAttributeDataType.TagsValue, null).FormHeight);
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("1")]
    public void ABoolWithoutAValidValue_OpensUnchecked(string value)
    {
        var measure = Run(EnumKAttributeDataType.Bool, value);

        Assert.IsFalse(measure.Checked);
        Assert.AreEqual(bool.FalseString, measure.Value);
    }

    [TestMethod]
    public void ABool_IsEditedWithItsCheckBox()
    {
        Assert.IsTrue(Run(EnumKAttributeDataType.Bool, "True").Checked);
        Assert.AreEqual(bool.TrueString, Run(EnumKAttributeDataType.Bool, "False", c => ((CheckBox)c).Checked = true).Value);
    }

    [TestMethod]
    [DataRow(EnumKAttributeDataType.String)]
    [DataRow(EnumKAttributeDataType.TextArea)]
    [DataRow(EnumKAttributeDataType.TabulatedValue)]
    [DataRow(EnumKAttributeDataType.TagsValue)]
    [DataRow(EnumKAttributeDataType.Bool)]
    public void TheValue_FillsThePanelWithTheLabelMarginOnBothSides(EnumKAttributeDataType type)
    {
        var measure = Run(type, null);

        Assert.AreEqual(measure.Margin, measure.Bounds.Left);
        Assert.AreEqual(measure.PanelWidth - measure.Margin, measure.Bounds.Right, $"{measure.Bounds} in a panel of {measure.PanelWidth}");
    }

    [TestMethod]
    [DataRow(EnumKAttributeDataType.Int)]
    [DataRow(EnumKAttributeDataType.Double)]
    [DataRow(EnumKAttributeDataType.DateTime)]
    public void AShortValue_AndTheDateButton_StayInsideThePanel(EnumKAttributeDataType type)
    {
        var measure = Run(type, "12");

        Assert.AreEqual(measure.Margin, measure.Bounds.Left);
        Assert.IsTrue(measure.Bounds.Right <= measure.PanelWidth - measure.Margin);
        Assert.AreEqual(type == EnumKAttributeDataType.DateTime, measure.DateButton != null);
        if (measure.DateButton is { } button)
        {
            Assert.IsTrue(button.Left > measure.Bounds.Right);
            Assert.IsTrue(button.Right <= measure.PanelWidth - measure.Margin, $"{button} in a panel of {measure.PanelWidth}");
        }
    }

    [TestMethod]
    public void ATabulatedValue_KeepsItsValue_AndWithoutOneSavesEmpty()
    {
        Assert.AreEqual("Beta", Run(EnumKAttributeDataType.TabulatedValue, "Beta").Value);
        Assert.AreEqual("", Run(EnumKAttributeDataType.TabulatedValue, null, c => ((ComboBox)c).SelectedIndex = -1).Value);
    }

    [TestMethod]
    public void TagsValues_AreCheckedWhole_AndTheColumnFillsTheList()
    {
        var measure = Run(EnumKAttributeDataType.TagsValue, "Gamma, Bet");

        CollectionAssert.AreEqual(new[] { "Gamma" }, measure.CheckedItems, "\"Bet\" is not \"Beta\"");
        Assert.AreEqual("Gamma", measure.Value);
        Assert.AreEqual(measure.ListClientWidth, measure.ColumnWidth);
        CollectionAssert.AreEqual(Array.Empty<string>(), Run(EnumKAttributeDataType.TagsValue, null).CheckedItems);
    }
}
