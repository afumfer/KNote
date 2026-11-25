using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// NoteChangeDescriber (the history of the notes): what changed between two versions of a note's own data, its line in the
/// history and the merge of the changes of a session. A part changes exactly when NoteChangeRules says so.
/// </summary>
[TestClass]
public class NoteChangeDescriberTests
{
    private static readonly Guid Equipment = Guid.NewGuid();
    private static readonly Guid Restored = Guid.NewGuid();
    private static readonly Guid Status = Guid.NewGuid();
    private static readonly Guid Model = Guid.NewGuid();
    private static readonly Guid Computer = Guid.NewGuid();

    private static HistoryNames Names()
    {
        var names = new HistoryNames();
        names.Folders[Equipment] = @"Home\Equipment";
        names.Folders[Restored] = @"Home\Equipment\Restored";
        names.NoteTypes[Computer] = "Computer";
        names.Attributes[Status] = "Status";
        names.Attributes[Model] = "Model";
        return names;
    }

    private static NoteVersionDto Cpc() => new()
    {
        Topic = "Amstrad CPC 464",
        Description = "# Status\r\nRestoring.\r\n\r\n# Restoration\r\nKeyboard cleaned.\r\n",
        Tags = "amstrad 8bit",
        FolderId = Equipment,
        NoteTypeId = Computer,
        Priority = 1,
        ContentType = "markdown",
        Attributes = { new NoteVersionAttributeDto { Id = Status, Value = "Restoring" } }
    };

    [TestMethod]
    public void ANewNote_SaysWhereAndItsSections()
    {
        var details = NoteChangeDescriber.Describe(null, Cpc(), Names());

        Assert.AreEqual(@"Home\Equipment", details.Created.Folder);
        CollectionAssert.AreEqual(new[] { "Status", "Restoration" }, details.Created.Sections);
        Assert.AreEqual(EnumHistoryFields.None, details.Fields);
        Assert.AreEqual(@"New note in Home\Equipment · Sections: «Status», «Restoration»", NoteChangeDescriber.Render(details));
    }

    [TestMethod]
    public void TheSameData_AsAnEditorGivesItBack_IsNoChange()
    {
        var after = Cpc();
        after.Description = after.Description.Replace("\r\n", "\n").TrimEnd();
        after.Script = "";

        var details = NoteChangeDescriber.Describe(Cpc(), after, Names());

        Assert.AreEqual(EnumHistoryFields.None, details.Fields);
        Assert.AreEqual("No changes", NoteChangeDescriber.Render(details));
    }

    [TestMethod]
    public void AttributesTagsAndText_AreToldAsPeopleReadThem()
    {
        var after = Cpc();
        after.Attributes[0].Value = "Works";
        after.Tags = "amstrad 8bit restored";
        after.Description = "# Status\r\nRestoring.\r\n\r\n# Restoration\r\nKeyboard cleaned.\r\nNew capacitors.\r\nIt works.\r\n";

        var details = NoteChangeDescriber.Describe(Cpc(), after, Names());

        Assert.AreEqual(EnumHistoryFields.Attributes | EnumHistoryFields.Tags | EnumHistoryFields.Description, details.Fields);
        Assert.AreEqual("Restoring", details.Attributes.Single().Before);
        Assert.AreEqual("Works", details.Attributes.Single().After);
        CollectionAssert.AreEqual(new[] { "restored" }, details.Tags.Added);
        Assert.AreEqual(2, details.Text.Added);
        Assert.AreEqual("Restoration", details.Text.Sections.Single().Title);
        Assert.AreEqual("Changes in «Restoration» (+2 lines) · Status: Restoring → Works · Tags: +restored",
            NoteChangeDescriber.Render(details));
    }

    [TestMethod]
    public void LinesBeforeTheFirstHeading_AreSaidToo()
    {
        var after = Cpc();
        after.Description = "AAA\r\nBBB\r\n# Status\r\nRestoring.\r\n\r\n# Restoration\r\nKeyboard cleaned and tested.\r\n";

        var details = NoteChangeDescriber.Describe(Cpc(), after, Names());

        Assert.AreEqual("Changes at the start and in «Restoration» (+3 −1 lines)", NoteChangeDescriber.Render(details));

        var onlyAtTheStart = Cpc();
        onlyAtTheStart.Description = "AAA\r\n" + Cpc().Description;
        Assert.AreEqual("Text changed (+1 line)", NoteChangeDescriber.Render(NoteChangeDescriber.Describe(Cpc(), onlyAtTheStart, Names())));
    }

    [TestMethod]
    public void MovedRetypedAndTheRest()
    {
        var after = Cpc();
        after.Topic = "Amstrad CPC 464 (green screen)";
        after.FolderId = Restored;
        after.NoteTypeId = null;
        after.Priority = 3;
        after.ContentType = "html";
        after.Script = "printline(\"x\");";
        after.Attributes.Add(new NoteVersionAttributeDto { Id = Model, Value = "GT65" });

        var details = NoteChangeDescriber.Describe(Cpc(), after, Names());
        var line = NoteChangeDescriber.Render(details, int.MaxValue);

        StringAssert.Contains(line, "Topic: «Amstrad CPC 464» → «Amstrad CPC 464 (green screen)»");
        StringAssert.Contains(line, @"Moved from Home\Equipment to Home\Equipment\Restored");
        StringAssert.Contains(line, "Type: Computer → (none)");
        StringAssert.Contains(line, "Priority: 1 → 3");
        StringAssert.Contains(line, "Description format: markdown → html");
        StringAssert.Contains(line, "Script changed");
        StringAssert.Contains(line, "Model: (empty) → GT65");
        Assert.AreEqual(EnumHistoryFields.Topic | EnumHistoryFields.Folder | EnumHistoryFields.NoteType | EnumHistoryFields.Priority
            | EnumHistoryFields.ContentType | EnumHistoryFields.Script | EnumHistoryFields.Attributes, details.Fields);
    }

    [TestMethod]
    public void ItChangesExactlyWhen_NoteChangeRulesSaysSo()
    {
        var changes = new Action<NoteDto>[]
        {
            n => n.Topic = "Other", n => n.Description = "Other text", n => n.Tags = "x", n => n.FolderId = Restored,
            n => n.NoteTypeId = null, n => n.Priority = 9, n => n.Script = "s", n => n.ContentType = "html",
            n => n.KAttributesDto[0].Value = "Works", n => n.InternalTags = "Alarms pending",
            n => n.Description = n.Description.Replace("\r\n", "\n") + "\n\n"
        };
        foreach (var change in changes)
        {
            var stored = StoredNote();
            var edited = StoredNote();
            change(edited);

            var changed = NoteChangeRules.ChangedField(stored, edited) != null;
            var details = NoteChangeDescriber.Describe(NoteVersionDto.From(stored), NoteVersionDto.From(edited), Names());

            Assert.AreEqual(changed, details.Fields != EnumHistoryFields.None);
        }
    }

    [TestMethod]
    public void TheContentType_IsToldByItsParts_NeverAsItsJson()
    {
        static string Json(string des, string scr = "knt", bool blk = false) =>
            ContentTypeExtJsonHelper.Serialize(new ContentTypeExt { ForDescription = des, ForScript = scr, DescriptionBlocked = blk });

        NoteChangeDetailsDto Change(string before, string after)
        {
            var was = Cpc();
            was.ContentType = before;
            var now = Cpc();
            now.ContentType = after;
            return NoteChangeDescriber.Describe(was, now, Names());
        }

        var format = Change(Json("navigation"), Json("markdown"));
        Assert.AreEqual("Description format: navigation → markdown", NoteChangeDescriber.Render(format));
        Assert.AreEqual(EnumHistoryFields.ContentType, format.Fields);
        Assert.AreEqual("{\"descriptionFormat\":{\"before\":\"navigation\",\"after\":\"markdown\"}}", format.ToJson());

        Assert.AreEqual("Description blocked", NoteChangeDescriber.Render(Change(Json("markdown"), Json("markdown", blk: true))));
        Assert.AreEqual("Description unblocked", NoteChangeDescriber.Render(Change("markdown#", Json("markdown"))));
        Assert.AreEqual("Script format: knt → (none)", NoteChangeDescriber.Render(Change(Json("markdown"), Json("markdown", scr: ""))));

        // The old format and the JSON with the same values: written in another form, not changed.
        Assert.AreEqual(EnumHistoryFields.None, Change("markdown", Json("markdown")).Fields);
    }

    [TestMethod]
    public void TheContentType_ThatCantBeRead_IsTheFormatOfTheDescription()
    {
        var was = Cpc();
        was.ContentType = "{ not json";
        var now = Cpc();
        now.ContentType = ContentTypeExtJsonHelper.Serialize(new ContentTypeExt { ForDescription = "html", ForScript = "knt" });

        Assert.AreEqual("Description format: { not json → html", NoteChangeDescriber.Render(NoteChangeDescriber.Describe(was, now, Names())));
    }

    [TestMethod]
    public void BlockedAndUnblocked_InOneSession_IsAsItWas()
    {
        var blocked = new NoteChangeDetailsDto { DescriptionBlocked = true };
        var unblocked = new NoteChangeDetailsDto { DescriptionBlocked = false };

        Assert.IsNull(NoteChangeDescriber.Merge(blocked, unblocked).DescriptionBlocked);
        Assert.IsTrue(NoteChangeDescriber.Merge(blocked, new NoteChangeDetailsDto()).DescriptionBlocked);
    }

    [TestMethod]
    public void ALongLine_StopsWithHowManyMore()
    {
        var after = Cpc();
        after.Topic = new string('t', 300);
        after.Tags = "a b c d e f g";
        after.Priority = 4;
        after.ContentType = "html";

        var line = NoteChangeDescriber.Render(NoteChangeDescriber.Describe(Cpc(), after, Names()));

        Assert.IsLessThanOrEqualTo(NoteChangeDescriber.MaxMessageLength, line.Length);
        StringAssert.EndsWith(line, "more changes");
    }

    [TestMethod]
    public void Deleted_SaysFromWhere()
    {
        var details = NoteChangeDescriber.DescribeDeleted(Cpc(), Names());

        Assert.AreEqual(@"Note deleted from Home\Equipment", NoteChangeDescriber.Render(details));
    }

    [TestMethod]
    public void Merge_KeepsTheFirstBeforeAndTheLastAfter_AndDropsWhatWentBack()
    {
        var first = NoteChangeDescriber.Describe(Cpc(), With(Cpc(), v => { v.Attributes[0].Value = "Testing"; v.Tags = "amstrad"; }), Names());
        var second = NoteChangeDescriber.Describe(
            With(Cpc(), v => { v.Attributes[0].Value = "Testing"; v.Tags = "amstrad"; }),
            With(Cpc(), v => { v.Attributes[0].Value = "Works"; v.Tags = "amstrad 8bit"; v.Description += "More.\r\n"; }), Names());

        var merged = NoteChangeDescriber.Merge(first, second);

        Assert.AreEqual("Restoring", merged.Attributes.Single().Before);
        Assert.AreEqual("Works", merged.Attributes.Single().After);
        Assert.IsTrue(merged.Tags.Added.Count == 0 && merged.Tags.Removed.Count == 0, "8bit went and came back");
        Assert.AreEqual(1, merged.Text.Added);
    }

    [TestMethod]
    public void Details_RoundTripAsJson()
    {
        var after = With(Cpc(), v => { v.Attributes[0].Value = "Works"; v.FolderId = Restored; });
        var details = NoteChangeDescriber.Describe(Cpc(), after, Names());

        var read = NoteChangeDetailsDto.FromJson(details.ToJson());

        Assert.AreEqual(details.Fields, read.Fields);
        Assert.AreEqual(NoteChangeDescriber.Render(details), NoteChangeDescriber.Render(read));
        StringAssert.Contains(details.ToJson(), "\"attributes\":[{");
    }

    [TestMethod]
    public void Version_RoundTripsAsJson_WithOnlyTheAttributesWithAValue()
    {
        var note = StoredNote();
        note.KAttributesDto.Add(new NoteKAttributeDto { KAttributeId = Model, Value = "" });

        var version = NoteVersionDto.FromJson(NoteVersionDto.From(note).ToJson());

        Assert.AreEqual(note.Topic, version.Topic);
        Assert.AreEqual(note.Description, version.Description);
        Assert.AreEqual(note.FolderId, version.FolderId);
        Assert.AreEqual(NoteVersionDto.CurrentFormat, version.Format);
        Assert.AreEqual("Restoring", version.AttributeValue(Status));
        Assert.IsNull(version.AttributeValue(Model));
        Assert.IsNull(NoteVersionDto.FromJson("not json"));
    }

    private static NoteVersionDto With(NoteVersionDto version, Action<NoteVersionDto> change)
    {
        change(version);
        return version;
    }

    private static NoteDto StoredNote() => new()
    {
        NoteId = Guid.NewGuid(), Topic = "QL", Description = "Line one\r\nLine two\r\n", Tags = "ql", FolderId = Equipment,
        NoteTypeId = Computer, Priority = 1, ContentType = "markdown", InternalTags = "",
        KAttributesDto = { new NoteKAttributeDto { KAttributeId = Status, Name = "Status", Value = "Restoring" } }
    };
}
