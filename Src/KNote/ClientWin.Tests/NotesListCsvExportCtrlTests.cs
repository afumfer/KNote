using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Core.Reports;
using KNote.ClientWin.Tests.Fakes;
using KNote.Model;

namespace KNote.ClientWin.Tests;

/// <summary>
/// Tests for NotesListCsvExportCtrl: exporting the notes list as displayed to a CSV file, which requires the
/// Staff role in the repository the notes come from (checked before asking for the file).
/// </summary>
[TestClass]
public class NotesListCsvExportCtrlTests
{
    private static (NotesListCsvExportCtrl ctrl, FakeFileExportView view, List<string> notified, List<string> opened) CreateCtrl(EnumRoles role, int rows = 2)
    {
        var factoryViews = new TestFactoryViews();
        var view = new FakeFileExportView();
        factoryViews.Registry.Register<NotesListCsvExportCtrl, IViewFileExport>(c => view);

        var store = new Store(factoryViews);
        var notified = new List<string>();
        store.AccessDeniedNotifier = notified.Add;
        var service = new FakeKntService { RepositoryRef = new RepositoryRef { Alias = "Shared" } };
        store.Security.SetRepositoryRole(service, role);

        var snapshot = new NotesListSnapshot();
        snapshot.Columns.Add(new NotesListColumn("NoteNumber", "Number", true, 80));
        snapshot.Columns.Add(new NotesListColumn("Topic", "Topic", false, 400));
        for (var i = 1; i <= rows; i++)
            snapshot.Rows.Add(new[] { i.ToString(), $"Note {i}" });

        var opened = new List<string>();
        var ctrl = new NotesListCsvExportCtrl(store)
        {
            Service = service,
            Snapshot = snapshot,
            GetContextAsync = _ => Task.FromResult(new NotesListContext { Source = NotesListSource.Folder, RepositoryAlias = "Shared" }),
            OpenFile = opened.Add
        };

        return (ctrl, view, notified, opened);
    }

    [TestMethod]
    public async Task Guest_IsRefusedBeforeBeingAskedForTheFile()
    {
        var (ctrl, view, notified, _) = CreateCtrl(EnumRoles.Guest);

        var exported = await ctrl.ExportAsync();

        Assert.IsFalse(exported);
        Assert.AreEqual(0, view.PromptForSaveFileCallCount);
        StringAssert.Contains(notified.Single(), "in the repository 'Shared'");
    }

    [TestMethod]
    public async Task Staff_ExportsTheListAndCanOpenTheFile()
    {
        var (ctrl, view, notified, opened) = CreateCtrl(EnumRoles.Staff);
        var path = Path.Combine(Path.GetTempPath(), $"knote-export-{Guid.NewGuid():N}.csv");
        view.PromptForSaveFileImpl = () => path;
        view.ShowInfoAnswer = DialogResult.Yes;

        try
        {
            var exported = await ctrl.ExportAsync();

            Assert.IsTrue(exported);
            Assert.AreEqual(0, notified.Count);
            StringAssert.Contains(File.ReadAllText(path), "Note 2");
            CollectionAssert.AreEqual(new[] { path }, opened);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod]
    public async Task EmptyList_IsReportedWithoutAskingForTheFile()
    {
        var (ctrl, view, _, _) = CreateCtrl(EnumRoles.Staff, rows: 0);

        var exported = await ctrl.ExportAsync();

        Assert.IsFalse(exported);
        Assert.AreEqual(0, view.PromptForSaveFileCallCount);
        StringAssert.Contains(view.LastShownInfo, "no notes");
    }

    [TestMethod]
    public async Task CanceledFileDialog_ExportsNothing()
    {
        var (ctrl, view, _, opened) = CreateCtrl(EnumRoles.Staff);
        view.PromptForSaveFileImpl = () => null;

        Assert.IsFalse(await ctrl.ExportAsync());
        Assert.AreEqual(0, opened.Count);
    }
}
