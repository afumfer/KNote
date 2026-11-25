using KNote.ClientWin.Controllers;
using KNote.ClientWin.Tests.Fakes;
using KNote.ClientWin.Tests.Helpers;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// NoteCompareCtrl («Compare», H22): it asks the Service what the change did or that version against the note now, and its HTML
/// escapes every text (a note can contain HTML) and marks the words that changed. NoteVersionCtrl: «Restore» only when the
/// editor gives it (H23).
/// </summary>
[TestClass]
public class NoteCompareCtrlTests
{
    private static readonly KLogDto Row = new()
    {
        KLogId = Guid.NewGuid(), UserName = "marta", RegistryDateTime = new DateTime(2026, 11, 25, 10, 5, 0), LastDateTime = new DateTime(2026, 11, 25, 10, 31, 0)
    };

    [TestMethod]
    public async Task ItAsksForTheComparisonChosen()
    {
        var service = new FakeKntService();
        var asked = new List<bool>();
        service.HistoryFake.GetNoteComparisonAsyncImpl = (_, _, withCurrent) =>
        {
            asked.Add(withCurrent);
            return Task.FromResult(new Result<NoteComparisonDto>(new NoteComparisonDto { WithCurrentNote = withCurrent }));
        };
        var ctrl = new NoteCompareCtrl(TestStoreFactory.CreateEmpty(), service, Guid.NewGuid(), Row, "#12 · CPC");

        Assert.IsTrue(await ctrl.LoadAsync(withCurrentNote: false));
        Assert.IsTrue(await ctrl.LoadAsync(withCurrentNote: true));

        CollectionAssert.AreEqual(new[] { false, true }, asked);
        Assert.IsTrue(ctrl.Comparison.WithCurrentNote);
        Assert.AreEqual("Compare versions of #12 · CPC", ctrl.Title);
        StringAssert.EndsWith(ctrl.Subtitle, " by marta");
    }

    [TestMethod]
    public async Task AComparisonThatCantBeRead_GivesItsError()
    {
        var service = new FakeKntService();
        service.HistoryFake.GetNoteComparisonAsyncImpl = (_, _, _) =>
        {
            var result = new Result<NoteComparisonDto>();
            result.AddErrorMessage("This change didn't touch the note's own data: there is nothing to compare.");
            return Task.FromResult(result);
        };
        var ctrl = new NoteCompareCtrl(TestStoreFactory.CreateEmpty(), service, Guid.NewGuid(), Row, null);

        Assert.IsFalse(await ctrl.LoadAsync(false));
        StringAssert.Contains(ctrl.Error, "nothing to compare");
    }

    [TestMethod]
    public void TheHtml_EscapesTheTexts_AndMarksTheWords()
    {
        var comparison = new NoteComparisonDto
        {
            BeforeLabel = "Before the change", AfterLabel = "After the change",
            Fields = { new NoteComparisonFieldDto { Name = "Topic", Before = "<b>Old</b>", After = "New" } },
            Text = new NoteTextComparisonDto
            {
                Same = false, Added = 1, Removed = 1,
                Lines =
                {
                    new NoteComparisonLineDto { Kind = EnumComparisonLine.Folded, Skipped = 4 },
                    new NoteComparisonLineDto
                    {
                        Kind = EnumComparisonLine.Removed, BeforeNumber = 5, Text = "<script>alert(1)</script> works",
                        Segments = new() { new() { Text = "<script>alert(1)</script>", Changed = true }, new() { Text = " works" } }
                    },
                    new NoteComparisonLineDto { Kind = EnumComparisonLine.Added, AfterNumber = 5, Text = "It works" }
                }
            }
        };

        var html = NoteCompareCtrl.Html(comparison);

        Assert.IsFalse(html.Contains("<script>"), "the texts of the note are escaped");
        Assert.IsFalse(html.Contains("<b>Old</b>"));
        StringAssert.Contains(html, "&lt;b&gt;Old&lt;/b&gt;");
        StringAssert.Contains(html, "<mark>&lt;script&gt;alert(1)&lt;/script&gt;</mark>");
        StringAssert.Contains(html, "… 4 unchanged lines");
        StringAssert.Contains(html, "(+1 −1 lines)");
    }

    [TestMethod]
    public void NoDifferences_SaysSo()
    {
        StringAssert.Contains(NoteCompareCtrl.Html(new NoteComparisonDto()), "No differences.");
    }

    [TestMethod]
    public async Task AVersion_IsRestoredOnlyIfTheEditorAllowsIt()
    {
        var store = TestStoreFactory.CreateEmpty();
        var restored = 0;
        var withRestore = new NoteVersionCtrl(store, new FakeKntService(), Guid.NewGuid(), Row, null, () => { restored++; return Task.FromResult(true); });
        var withoutRestore = new NoteVersionCtrl(store, new FakeKntService(), Guid.NewGuid(), Row, null);

        Assert.IsTrue(withRestore.CanRestore);
        Assert.IsTrue(await withRestore.RestoreAsync());
        Assert.AreEqual(1, restored);
        Assert.IsFalse(withoutRestore.CanRestore);
        Assert.IsFalse(await withoutRestore.RestoreAsync());
    }
}
