using System.ComponentModel;
using System.Globalization;
using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Core.Reports;
using KNote.ClientWin.Tests.Fakes;
using KNote.ClientWin.Views;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

[TestClass]
public class NotesListContextTests
{
    [TestMethod]
    public void Folder_HeadingIsThePathAndSubheadingTheFolderNumber()
    {
        var context = new NotesListContext { Source = NotesListSource.Folder, FolderPath = @"Notes\Projects", FolderNumber = 12 };

        Assert.AreEqual(@"Notes\Projects", context.Heading);
        Assert.AreEqual("Folder number 12", context.Subheading);
        Assert.AreEqual(@"Notes - Notes\Projects - 2026-10-02", context.FileNameBase(new DateTime(2026, 10, 2)));
    }

    [TestMethod]
    public void Search_DescribesTextAndScope()
    {
        var context = new NotesListContext { Source = NotesListSource.Search, SearchText = "kanote", SearchInDescription = true, SearchInNoteTasks = true };

        Assert.AreEqual("Search: kanote", context.Heading);
        Assert.AreEqual("Searched in: topic, description, tasks", context.Subheading);
        Assert.AreEqual("Notes - Search kanote - 2026-10-02", context.FileNameBase(new DateTime(2026, 10, 2)));
    }

    [TestMethod]
    public void Filter_ListsEveryCriterion()
    {
        var context = new NotesListContext
        {
            Source = NotesListSource.Filter,
            FilterCriteria = { new ReportMetaItem("Tags", "work"), new ReportMetaItem("Note type", "Meeting") }
        };

        Assert.AreEqual("Filter: Tags = work · Note type = Meeting", context.Heading);
        Assert.IsNull(context.Subheading);
        Assert.AreEqual("Notes - Filter Tags = work Note type = Meeting - 2026-10-02", context.FileNameBase(new DateTime(2026, 10, 2)));
    }

    [TestMethod]
    public void Filter_WithoutCriteria_SaysSo()
    {
        var context = new NotesListContext { Source = NotesListSource.Filter };

        Assert.AreEqual("Filter: (no criteria)", context.Heading);
    }

    [TestMethod]
    public void TextFilter_IsAddedToSubheadingAndFileName()
    {
        var context = new NotesListContext { Source = NotesListSource.Folder, FolderPath = "Notes", TextFilter = " #12 " };

        Assert.AreEqual("List text filter: #12", context.Subheading);
        Assert.AreEqual("Notes - Notes (#12) - 2026-10-02", context.FileNameBase(new DateTime(2026, 10, 2)));
    }

    [TestMethod]
    public void Repository_IncludesProviderWhenKnown()
    {
        Assert.AreEqual("Demo (Microsoft.Data.Sqlite)", new NotesListContext { RepositoryAlias = "Demo", RepositoryProvider = "Microsoft.Data.Sqlite" }.Repository);
        Assert.AreEqual("Demo", new NotesListContext { RepositoryAlias = "Demo" }.Repository);
    }
}

/// <summary>
/// NotesSelectorForm.GetDisplayedNotes against the real WinForms grid (run on an STA thread): what the
/// print/CSV use cases receive must be what the grid shows - visible columns in order, sorted rows,
/// formatted cells.
/// </summary>
[TestClass]
public class NotesSelectorFormSnapshotTests
{
    private static readonly List<NoteMinimalDto> Notes = new()
    {
        new() { NoteId = Guid.NewGuid(), NoteNumber = 30, Topic = "Gamma", Priority = 1, Tags = "b", InternalTags = "", ModificationDateTime = new DateTime(2026, 3, 1, 10, 0, 0), CreationDateTime = new DateTime(2026, 1, 1), FolderId = Guid.NewGuid() },
        new() { NoteId = Guid.NewGuid(), NoteNumber = 10, Topic = "Alpha", Priority = 2, Tags = "a", InternalTags = "done", ModificationDateTime = new DateTime(2026, 2, 1, 9, 30, 0), CreationDateTime = new DateTime(2026, 1, 2), FolderId = Guid.NewGuid() },
        new() { NoteId = Guid.NewGuid(), NoteNumber = 20, Topic = "Beta", Priority = 3, Tags = "c", InternalTags = "", ModificationDateTime = new DateTime(2026, 4, 1, 8, 15, 0), CreationDateTime = new DateTime(2026, 1, 3), FolderId = Guid.NewGuid() },
    };

    private static NotesListSnapshot TakeSnapshot(bool compactView = false, string hiddenColumns = "")
    {
        NotesListSnapshot snapshot = null;
        Exception error = null;

        var thread = new Thread(() =>
        {
            try
            {
                var factory = new TestFactoryViews();
                factory.Registry.Register<NotesSelectorCtrl, IViewNotesSelector>(c => new NotesSelectorForm(c));
                var store = new Store(factory) { AppUserName = "jdoe" };
                store.State.ManagementWindow.NotesList.CompactView = compactView;
                store.State.ManagementWindow.NotesList.SortColumn = 0;    // "not set": the list sorts by Number, ascending

                var service = new FakeKntService();
                service.NotesFake.GetAllMinimalAsyncImpl = () => Task.FromResult(new Result<List<NoteMinimalDto>> { Entity = Notes.ToList() });

                var ctrl = new NotesSelectorCtrl(store) { HiddenColumns = hiddenColumns };
                using var form = (NotesSelectorForm)ctrl.View;
                _ = form.Handle;

                // The fake completes synchronously, so the load (and its view refresh) is done on return.
                ctrl.LoadEntities(service).GetAwaiter().GetResult();
                snapshot = ctrl.GetDisplayedNotes();
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
            throw new AssertFailedException($"Snapshot failed: {error}");
        return snapshot;
    }

    [TestMethod]
    public void Snapshot_HasTheVisibleColumnsInDisplayOrder()
    {
        var snapshot = TakeSnapshot();

        CollectionAssert.AreEqual(
            new[] { "NoteNumber", "Topic", "Priority", "Tags", "InternalTags", "ModificationDateTime", "CreationDateTime" },
            snapshot.Columns.Select(c => c.Name).ToArray());
        Assert.AreEqual("Status", snapshot.Columns.Single(c => c.Name == "InternalTags").Header);
        Assert.IsTrue(snapshot.Columns.Single(c => c.Name == "NoteNumber").AlignRight);
        Assert.IsFalse(snapshot.Columns.Single(c => c.Name == "Topic").AlignRight);
    }

    [TestMethod]
    public void Snapshot_RowsFollowTheGridSortAndFormatting()
    {
        var snapshot = TakeSnapshot();

        Assert.AreEqual(3, snapshot.Rows.Count);
        Assert.AreEqual(3, snapshot.LoadedCount);
        CollectionAssert.AreEqual(new[] { "10", "20", "30" }, snapshot.Rows.Select(r => r[0]).ToArray());
        CollectionAssert.AreEqual(new[] { "Alpha", "Beta", "Gamma" }, snapshot.Rows.Select(r => r[1]).ToArray());
        CollectionAssert.AreEqual(new[] { Notes[1].NoteId, Notes[2].NoteId, Notes[0].NoteId }, snapshot.NoteIds, "The note of each row, in the same order");
        // The grid's own formatting (DateTime's TypeConverter, which e.g. drops zero seconds), not ToString().
        var displayedDate = TypeDescriptor.GetConverter(typeof(DateTime)).ConvertToString(null, CultureInfo.CurrentCulture, new DateTime(2026, 2, 1, 9, 30, 0));
        Assert.AreEqual(displayedDate, snapshot.Rows[0][5]);
        Assert.AreEqual("", snapshot.TextFilter);
    }

    [TestMethod]
    public void Snapshot_CompactViewAndHiddenColumns_AreLeftOut()
    {
        var snapshot = TakeSnapshot(compactView: true, hiddenColumns: "Priority");

        CollectionAssert.AreEqual(new[] { "Topic", "Tags", "InternalTags" }, snapshot.Columns.Select(c => c.Name).ToArray());
        Assert.AreEqual(3, snapshot.Rows[0].Length);
    }
}
