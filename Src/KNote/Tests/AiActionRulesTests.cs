using KNote.Model;
using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>
/// What the actions of the assistant have in common (KNote Atlas, 1.4): their level, their record, what the model is told,
/// and how the conversation keeps them: nothing to run them again, and what was left unanswered is not confirmed.
/// </summary>
[TestClass]
public class AiActionRulesTests
{
    private static AiActionDto Action(EnumAiActionStatus status, string kind = AiActionRules.SetTags, string text = "Tags of #1") =>
        new() { Kind = kind, Status = status, Text = text, NoteId = Guid.NewGuid(), NoteNumber = 1 };

    [TestMethod]
    public void Levels_OnlyCreatingNeedsNoConfirmation()
    {
        Assert.IsFalse(AiActionRules.NeedsConfirmation(AiActionRules.CreateNote));
        Assert.IsTrue(AiActionRules.NeedsConfirmation(AiActionRules.AppendToNote));
        Assert.IsTrue(AiActionRules.NeedsConfirmation(AiActionRules.SetTags));
    }

    [TestMethod]
    [DataRow(EnumAiActionStatus.Done, "✔ Tags of #1")]
    [DataRow(EnumAiActionStatus.Rejected, "✖ Rejected: Tags of #1")]
    [DataRow(EnumAiActionStatus.NotConfirmed, "✖ Not confirmed: Tags of #1")]
    public void Line_SaysHowItEnded(EnumAiActionStatus status, string expected) =>
        Assert.AreEqual(expected, AiActionRules.Line(Action(status)));

    [TestMethod]
    public void Line_OfAFailure_SaysWhy()
    {
        var action = Action(EnumAiActionStatus.Failed);
        action.Result = "It is locked.";

        Assert.AreEqual("✖ Tags of #1: It is locked.", AiActionRules.Line(action));
    }

    [TestMethod]
    public void Outcome_TellsTheModelHowItEnded()
    {
        Assert.AreEqual("Done: first", AiActionRules.Outcome(Action(EnumAiActionStatus.Done, text: "first")));
        Assert.AreEqual("Rejected by the user, not done: second", AiActionRules.Outcome(Action(EnumAiActionStatus.Rejected, text: "second")));
    }

    [TestMethod]
    public void RejectPending_OnlyTouchesWhatWasProposed()
    {
        var done = Action(EnumAiActionStatus.Done);
        var proposed = Action(EnumAiActionStatus.Proposed);
        var turns = new[] { new AiChatTurnDto { Actions = { done, proposed } } };

        Assert.IsTrue(AiActionRules.RejectPending(turns));

        Assert.AreEqual(EnumAiActionStatus.Done, done.Status);
        Assert.AreEqual(EnumAiActionStatus.Rejected, proposed.Status);
        Assert.IsFalse(AiActionRules.RejectPending(turns), "Nothing left.");
    }

    [TestMethod]
    public void NewNoteIds_AreTheNotesCreatedInTheConversation()
    {
        var created = Action(EnumAiActionStatus.Done, AiActionRules.CreateNote);
        var task = Action(EnumAiActionStatus.Done, AiActionRules.CreateTask);
        var failed = Action(EnumAiActionStatus.Failed, AiActionRules.CreateNote);
        var changed = Action(EnumAiActionStatus.Done, AiActionRules.SetTags);

        var ids = AiActionRules.NewNoteIds(new[] { new AiChatTurnDto { Actions = { created, task, failed, changed } } }).ToList();

        CollectionAssert.AreEquivalent(new[] { created.NoteId, task.NoteId }.Select(i => i!.Value).ToList(), ids);
    }

    [TestMethod]
    [DataRow("a b", "c", "", "a b c")]
    [DataRow("a b", "B", "", "a b")]
    [DataRow("a, b; c", "", "B", "a c")]
    [DataRow("", "x y", "", "x y")]
    [DataRow(null, "", "", "")]
    public void ChangeTags_AddsAndRemovesWholeTags(string tags, string add, string remove, string expected) =>
        Assert.AreEqual(expected, AiActionRules.ChangeTags(tags, new[] { add }, new[] { remove }));

    [TestMethod]
    public void ChangeTags_DoesNotRemoveAPartOfATag() =>
        Assert.AreEqual("restored unrestored", AiActionRules.ChangeTags("restored unrestored", null, new[] { "store" }));

    [TestMethod]
    public void Transcript_KeepsTheRecord_WithoutTheDataToRunIt_NorInTheAnswer()
    {
        var action = Action(EnumAiActionStatus.Done);
        action.Content = "Secret text of the action";
        var turn = new AiChatTurnDto { Prompt = "Q", Answer = "The answer", Actions = { action } };

        var transcript = AiChatSessionTranscript.Write(new[] { turn });
        var parsed = AiChatSessionTranscript.Parse(transcript).Single();

        StringAssert.Contains(transcript, "- ✔ Tags of #1");
        Assert.IsFalse(transcript.Contains("Secret text"), transcript);
        Assert.AreEqual("The answer", parsed.Answer);
        Assert.AreEqual(EnumAiActionStatus.Done, parsed.Actions.Single().Status);
        Assert.AreEqual(action.NoteId, parsed.Actions.Single().NoteId);
        Assert.IsNull(parsed.Actions.Single().Content);
    }

    [TestMethod]
    public void Transcript_AProposalLeftWithoutAnAnswer_IsReadAsNotConfirmed()
    {
        var turn = new AiChatTurnDto { Prompt = "Q", Answer = "A", Actions = { Action(EnumAiActionStatus.Proposed) } };

        var parsed = AiChatSessionTranscript.Parse(AiChatSessionTranscript.Write(new[] { turn })).Single();

        Assert.AreEqual(EnumAiActionStatus.NotConfirmed, parsed.Actions.Single().Status);
    }

    [TestMethod]
    public void Transcript_TurnsAfterOneWithActions_AreReadWhole()
    {
        var turns = new[]
        {
            new AiChatTurnDto { Prompt = "First", Answer = "One", Actions = { Action(EnumAiActionStatus.Done) } },
            new AiChatTurnDto { Prompt = AiActionRules.ContinuePrompt, Answer = "Two", Automatic = true }
        };

        var parsed = AiChatSessionTranscript.Parse(AiChatSessionTranscript.Write(turns));

        Assert.AreEqual(2, parsed.Count);
        Assert.AreEqual("One", parsed[0].Answer);
        Assert.IsFalse(parsed[0].Automatic);
        Assert.AreEqual("Two", parsed[1].Answer);
        Assert.IsTrue(parsed[1].Automatic);
    }

    [TestMethod]
    public void Transcript_WithoutActions_IsWrittenAsBefore()
    {
        var transcript = AiChatSessionTranscript.Write(new[] { new AiChatTurnDto { Prompt = "Q", Answer = "A" } });

        Assert.IsFalse(transcript.Contains("actions", StringComparison.OrdinalIgnoreCase), transcript);
        Assert.IsFalse(transcript.Contains("automatic", StringComparison.OrdinalIgnoreCase), transcript);
        Assert.AreEqual(0, AiChatSessionTranscript.Parse(transcript).Single().Actions.Count);
    }

    [TestMethod]
    public void Transcript_ATurnWithOnlyTheCard_KeepsItsActionsAndNoText()
    {
        var turn = new AiChatTurnDto { Prompt = "Q", Answer = "" };
        turn.Actions.Add(Action(EnumAiActionStatus.Done));

        var parsed = AiChatSessionTranscript.Parse(AiChatSessionTranscript.Write(new[] { turn })).Single();

        Assert.AreEqual("Q", parsed.Prompt);
        Assert.AreEqual("", parsed.Answer);
        Assert.AreEqual(1, parsed.Actions.Count);
    }

    [TestMethod]
    public void DontAskAgain_NamesWhatItCovers()
    {
        var tags = Action(EnumAiActionStatus.Proposed);
        var text = Action(EnumAiActionStatus.Proposed, AiActionRules.AppendToNote);

        Assert.AreEqual("Don't ask again for tags in this conversation", AiActionRules.DontAskAgainText(new[] { tags, tags }));
        Assert.AreEqual("Don't ask again for text added to notes in this conversation", AiActionRules.DontAskAgainText(new[] { text }));
        Assert.AreEqual("Don't ask again for these kinds of change in this conversation", AiActionRules.DontAskAgainText(new[] { tags, text }));
    }

    [TestMethod]
    public void IsRepeated_InTheTurn_AnyKind_ButNotWhatFailed()
    {
        var tags = Action(EnumAiActionStatus.Proposed);

        Assert.IsTrue(AiActionRules.IsRepeated(tags, new[] { Action(EnumAiActionStatus.Proposed) }, Array.Empty<AiActionDto>()));
        Assert.IsFalse(AiActionRules.IsRepeated(tags, new[] { Action(EnumAiActionStatus.Failed) }, Array.Empty<AiActionDto>()));
        Assert.IsFalse(AiActionRules.IsRepeated(tags, new[] { Action(EnumAiActionStatus.Proposed, text: "Tags of #2") }, Array.Empty<AiActionDto>()));
    }

    [TestMethod]
    public void IsRepeated_InEarlierTurns_OnlyTextThatWasAdded()
    {
        var text = Action(EnumAiActionStatus.Proposed, AiActionRules.AppendToNote);
        var none = Array.Empty<AiActionDto>();

        Assert.IsTrue(AiActionRules.IsRepeated(text, none, new[] { Action(EnumAiActionStatus.Done, AiActionRules.AppendToNote) }));
        Assert.IsFalse(AiActionRules.IsRepeated(text, none, new[] { Action(EnumAiActionStatus.Rejected, AiActionRules.AppendToNote) }));
        Assert.IsFalse(AiActionRules.IsRepeated(Action(EnumAiActionStatus.Proposed), none, new[] { Action(EnumAiActionStatus.Done) }));
    }

    [TestMethod]
    public void Levels_TheStructureAndWhatGoesOut_AlwaysAsk_EvenOnANewNote()
    {
        Assert.AreEqual(EnumAiActionLevel.Structure, AiActionRules.LevelOf(AiActionRules.CreateFolder));
        Assert.AreEqual(EnumAiActionLevel.Structure, AiActionRules.LevelOf(AiActionRules.AddEmailAlarm));
        Assert.AreEqual(EnumAiActionLevel.Change, AiActionRules.LevelOf(AiActionRules.AddAlarm));

        Assert.IsFalse(AiActionRules.NeedsCard(AiActionRules.AddAlarm, approved: false, onNewNote: true));
        Assert.IsTrue(AiActionRules.NeedsCard(AiActionRules.AddEmailAlarm, approved: false, onNewNote: true));
        Assert.IsFalse(AiActionRules.NeedsCard(AiActionRules.AddEmailAlarm, approved: true, onNewNote: false));
        Assert.IsFalse(AiActionRules.NeedsCard(AiActionRules.CreateNote, approved: false, onNewNote: false));
    }

    [TestMethod]
    public void ReplacingText_AsksAlsoInANewNote_UntilItIsApproved()
    {
        Assert.AreEqual(EnumAiActionLevel.Structure, AiActionRules.LevelOf(AiActionRules.ReplaceNoteText));
        Assert.IsTrue(AiActionRules.NeedsCard(AiActionRules.ReplaceNoteText, approved: false, onNewNote: false));
        Assert.IsTrue(AiActionRules.NeedsCard(AiActionRules.ReplaceNoteText, approved: false, onNewNote: true));
        Assert.IsFalse(AiActionRules.NeedsCard(AiActionRules.ReplaceNoteText, approved: true, onNewNote: false));
    }

    [TestMethod]
    public void MovingANote_IsAChange_SoANewNoteDoesNotAsk()
    {
        Assert.AreEqual(EnumAiActionLevel.Change, AiActionRules.LevelOf(AiActionRules.MoveNote));
        Assert.IsTrue(AiActionRules.NeedsCard(AiActionRules.MoveNote, approved: false, onNewNote: false));
        Assert.IsFalse(AiActionRules.NeedsCard(AiActionRules.MoveNote, approved: true, onNewNote: false));
        Assert.IsFalse(AiActionRules.NeedsCard(AiActionRules.MoveNote, approved: false, onNewNote: true));
    }

    [TestMethod]
    public void CardText_OfAReplacement_WarnsAndShowsTheNewText_AndTheOthersAreTheirText()
    {
        var replace = Action(EnumAiActionStatus.Proposed, AiActionRules.ReplaceNoteText, "Replace the whole text of #1");
        replace.Content = "The new text.";

        var card = AiActionRules.CardText(replace);

        StringAssert.StartsWith(card, "Replace the whole text of #1");
        StringAssert.Contains(card, "cannot be undone");
        StringAssert.EndsWith(card, "The new text.");
        Assert.AreEqual("Tags of #1", AiActionRules.CardText(Action(EnumAiActionStatus.Proposed)));
        Assert.AreEqual("✖ Rejected: Replace the whole text of #1", AiActionRules.Line(new AiActionDto { Status = EnumAiActionStatus.Rejected, Text = replace.Text }));
    }

    [TestMethod]
    public void CardText_OfAVeryLongReplacement_SaysWhatItLeavesOut()
    {
        var replace = Action(EnumAiActionStatus.Proposed, AiActionRules.ReplaceNoteText);
        replace.Content = new string('x', 4010);

        StringAssert.EndsWith(AiActionRules.CardText(replace), "… (10 more characters)");
    }

    [TestMethod]
    public void TextStamp_DependsOnTheText_NotOnItsLineEnds()
    {
        Assert.AreEqual(AiActionRules.TextStamp("One\r\nTwo\r\n"), AiActionRules.TextStamp("One\nTwo"));
        Assert.AreEqual(AiActionRules.TextStamp(null), AiActionRules.TextStamp(""));
        Assert.AreNotEqual(AiActionRules.TextStamp("One"), AiActionRules.TextStamp("Two"));
    }

    [TestMethod]
    public void Roles_OnlyCreatingFoldersNeedsMoreThanStaff()
    {
        Assert.AreEqual(EnumRoles.ProjectManager, AiActionRules.MinRoleOf(AiActionRules.CreateFolder));
        foreach (var kind in AiActionRules.Kinds.Where(k => k != AiActionRules.CreateFolder))
            Assert.AreEqual(EnumRoles.Staff, AiActionRules.MinRoleOf(kind), kind);
    }

    [TestMethod]
    public void IsRepeated_WhatAddsEachTime_AlsoInEarlierTurns_AndANoteByItsTitle()
    {
        var none = Array.Empty<AiActionDto>();
        var task = Action(EnumAiActionStatus.Proposed, AiActionRules.AddTask);
        var note = new AiActionDto { Kind = AiActionRules.CreateNote, Topic = "QL", Text = "Create the note (QL)" };
        var created = new AiActionDto { Kind = AiActionRules.CreateNote, Topic = "QL", Text = "Created the note #2 (QL)", Status = EnumAiActionStatus.Done };
        var attribute = Action(EnumAiActionStatus.Proposed, AiActionRules.SetAttribute);

        Assert.IsTrue(AiActionRules.IsRepeated(task, none, new[] { Action(EnumAiActionStatus.Done, AiActionRules.AddTask) }));
        Assert.IsTrue(AiActionRules.IsRepeated(note, none, new[] { created }));
        Assert.IsFalse(AiActionRules.IsRepeated(attribute, none, new[] { Action(EnumAiActionStatus.Done, AiActionRules.SetAttribute) }));
    }

    [TestMethod]
    public void AlarmDate_FromADate_OrFromTheMinutes()
    {
        var now = new DateTime(2026, 11, 6, 10, 30, 45);

        Assert.AreEqual(new DateTime(2026, 11, 9, 9, 0, 0), AiActionRules.AlarmDate("2026-11-09 09:00", null, now).Date);
        Assert.AreEqual(new DateTime(2026, 11, 6, 12, 30, 0), AiActionRules.AlarmDate(null, 120, now).Date);
    }

    [TestMethod]
    [DataRow("2026-11-05 09:00", "has passed")]
    [DataRow("2026-11-09", "time of the alarm is missing")]
    [DataRow("next monday", "is not a date")]
    [DataRow("", "give when")]
    public void AlarmDate_SaysWhatIsWrong(string when, string expected)
    {
        var (date, error) = AiActionRules.AlarmDate(when, null, new DateTime(2026, 11, 6, 10, 30, 0));

        Assert.IsNull(date);
        StringAssert.Contains(error, expected);
    }

    [TestMethod]
    public void AlarmRepeat_TheNamesOfTheModel_NeverEveryFewMinutes()
    {
        Assert.AreEqual(EnumAlarmType.Standard, AiActionRules.AlarmRepeat(null).Type);
        Assert.AreEqual(EnumAlarmType.Weekly, AiActionRules.AlarmRepeat("Weekly").Type);
        Assert.AreEqual(EnumAlarmType.Annual, AiActionRules.AlarmRepeat("yearly").Type);
        StringAssert.Contains(AiActionRules.AlarmRepeat("hourly").Error, "not a repetition");
    }

    [TestMethod]
    public void DateText_HasTheDayOfTheWeek() =>
        Assert.AreEqual("Monday, 2026-11-09 09:00", AiActionRules.DateText(new DateTime(2026, 11, 9, 9, 0, 0)));

    [TestMethod]
    [DataRow(EnumKAttributeDataType.Bool, "sí", "True")]
    [DataRow(EnumKAttributeDataType.Bool, "no", "False")]
    [DataRow(EnumKAttributeDataType.Int, "12", "12")]
    [DataRow(EnumKAttributeDataType.String, " Text ", "Text")]
    public void AttributeValue_AsTheNoteKeepsIt(EnumKAttributeDataType type, string value, string expected) =>
        Assert.AreEqual(expected, AiActionRules.AttributeValue(type, value).Value);

    [TestMethod]
    public void AttributeValue_ANumberOrADate_InTheCultureOfTheApp_WhateverTheModelWrites()
    {
        var culture = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("es-ES");

            Assert.AreEqual("1,5", AiActionRules.AttributeValue(EnumKAttributeDataType.Double, "1.5").Value);
            Assert.AreEqual("1,5", AiActionRules.AttributeValue(EnumKAttributeDataType.Double, "1,5").Value);
            StringAssert.StartsWith(AiActionRules.AttributeValue(EnumKAttributeDataType.DateTime, "2026-11-09 09:00").Value, "09/11/2026");
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = culture;
        }
    }

    [TestMethod]
    public void AttributeValue_OfAList_IsCorrected_AndOneThatIsNotThereGivesThem()
    {
        var values = new[] { "Funciona", "En reparación" };

        Assert.AreEqual("En reparación", AiActionRules.AttributeValue(EnumKAttributeDataType.TabulatedValue, "en reparacion", values).Value);
        Assert.AreEqual("Funciona, En reparación", AiActionRules.AttributeValue(EnumKAttributeDataType.TagsValue, "funciona, en reparacion", values).Value);
        StringAssert.Contains(AiActionRules.AttributeValue(EnumKAttributeDataType.TabulatedValue, "Roto", values).Error, "Funciona, En reparación");
        StringAssert.Contains(AiActionRules.AttributeValue(EnumKAttributeDataType.TagsValue, "funciona, roto", values).Error, "'roto'");
    }

    [TestMethod]
    [DataRow("abc", EnumKAttributeDataType.Int)]
    [DataRow("abc", EnumKAttributeDataType.Double)]
    [DataRow("abc", EnumKAttributeDataType.DateTime)]
    [DataRow("abc", EnumKAttributeDataType.Bool)]
    public void AttributeValue_ThatIsNotOfItsType_IsAnError(string value, EnumKAttributeDataType type) =>
        Assert.IsNotNull(AiActionRules.AttributeValue(type, value).Error);

    [TestMethod]
    [DataRow(AiActionRules.AddTask, "tasks")]
    [DataRow(AiActionRules.AddAlarm, "alarms")]
    [DataRow(AiActionRules.AddEmailAlarm, "email alarms")]
    [DataRow(AiActionRules.SetAttribute, "attributes")]
    [DataRow(AiActionRules.RelateNotes, "note relations")]
    [DataRow(AiActionRules.CreateFolder, "new folders")]
    [DataRow(AiActionRules.ReplaceNoteText, "replacing the text of notes")]
    [DataRow(AiActionRules.MoveNote, "moving notes")]
    public void DontAskAgain_NamesEachKind(string kind, string name) =>
        Assert.AreEqual($"Don't ask again for {name} in this conversation", AiActionRules.DontAskAgainText(new[] { Action(EnumAiActionStatus.Proposed, kind) }));
}
