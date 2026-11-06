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
}
