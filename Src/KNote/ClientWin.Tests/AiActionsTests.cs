using KNote.ClientWin.Controllers;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.ClientWin.Tests.Helpers;
using KNote.Model;
using KNote.Model.Dto;

namespace KNote.ClientWin.Tests;

/// <summary>
/// The actions of the assistant in ClientWin (KNote Atlas, 1.4): the card and the record in the chat page, the answer it posts
/// and what KNoteAIAssistantCtrl does with it. The tools themselves are tested against a real repository in Tests/ServiceTests.
/// </summary>
[TestClass]
public class AiActionsTests
{
    private static AiActionDto Proposed(string text = "Tags of #12 (QL): add *x*") =>
        new() { Kind = AiActionRules.SetTags, Status = EnumAiActionStatus.Proposed, Text = text, NoteId = Guid.NewGuid(), NoteNumber = 12 };

    private static KNoteAIAssistantCtrl CtrlWithTurn(AiChatTurnDto turn)
    {
        var ctrl = new KNoteAIAssistantCtrl(new Store(new TestFactoryViews()));
        ctrl.SetChatClientForTesting(new FakeChatClient());
        ctrl.AddTurnForTesting(turn);
        return ctrl;
    }

    [TestMethod]
    public void AssistantMessage_WithAProposal_ShowsTheCard_WithCheckboxesAndButtons()
    {
        var action = Proposed();

        var html = AiChatHtml.AssistantMessage(new AiChatTurnDto { Answer = "I propose it.", Actions = { action } }, "Provider");

        StringAssert.Contains(html, "The assistant wants to do this:");
        StringAssert.Contains(html, $"data-action=\"{action.ActionId}\"");
        StringAssert.Contains(html, "Don't ask again for tags in this conversation");
        StringAssert.Contains(html, "data-answer=\"accept\"");
        StringAssert.Contains(html, "data-answer=\"reject\"");
        StringAssert.Contains(html, "href=\"knt-note:12\"");
    }

    [TestMethod]
    public void AssistantMessage_OnceAnswered_ShowsItsLine_AndNoCard()
    {
        var action = Proposed();
        action.Status = EnumAiActionStatus.Done;

        var html = AiChatHtml.AssistantMessage(new AiChatTurnDto { Answer = "Done.", Actions = { action } }, "Provider");

        StringAssert.Contains(html, "action-line");
        StringAssert.Contains(html, "✔");
        Assert.IsFalse(html.Contains("actions-card"), html);
    }

    [TestMethod]
    public void AssistantMessage_TheTextOfAnActionCannotBringHtml()
    {
        var html = AiChatHtml.AssistantMessage(new AiChatTurnDto { Answer = "A", Actions = { Proposed("<script>alert(1)</script>") } }, "Provider");

        Assert.IsFalse(html.Contains("<script>"), html);
    }

    [TestMethod]
    public void Page_AnAutomaticTurn_HasNoQuestion()
    {
        var html = AiChatHtml.Page(new[] { new AiChatTurnDto { Prompt = AiActionRules.ContinuePrompt, Answer = "Done.", Automatic = true } }, "Provider", "");

        Assert.IsFalse(html.Contains("has answered your proposal"), html);
        StringAssert.Contains(html, "Done.");
    }

    [TestMethod]
    public void UserMessage_ShowsAnImageByItsPreview_AndAFileByItsName()
    {
        var image = new AiChatAttachmentDto { ResourceId = Guid.NewGuid(), FileName = "photo.jpg", ContentType = "image/jpeg" };
        var pdf = new AiChatAttachmentDto { ResourceId = Guid.NewGuid(), FileName = "manual.pdf", ContentType = "application/pdf" };

        var html = AiChatHtml.UserMessage("Look", new[] { image, pdf }, id => id == image.ResourceId ? "data:image/jpeg;base64,AAAA" : null);

        StringAssert.Contains(html, "<img class=\"attachment-image\" src=\"data:image/jpeg;base64,AAAA\" alt=\"photo.jpg\">");
        StringAssert.Contains(html, "<span class=\"attachment-file\">manual.pdf</span>");
    }

    [TestMethod]
    public void UserMessage_EscapesTheNameOfAFile()
    {
        var file = new AiChatAttachmentDto { ResourceId = Guid.NewGuid(), FileName = "<script>.pdf", ContentType = "application/pdf" };

        var html = AiChatHtml.UserMessage("Look", new[] { file });

        Assert.IsFalse(html.Contains("<script>"), html);
    }

    [TestMethod]
    public void UserMessage_WithoutFiles_IsTheBubbleOfAlways() =>
        Assert.AreEqual("<div class=\"msg user\"><div class=\"bubble\">Look</div></div>", AiChatHtml.UserMessage("Look"));

    [TestMethod]
    public void AReplacement_ShowsTheNewTextInTheCard_WithTheDontAskBox()
    {
        var action = new AiActionDto { Kind = AiActionRules.ReplaceNoteText, Text = "Replace the whole text of #1", Content = "The new text." };

        var html = AiChatHtml.AssistantMessage(new AiChatTurnDto { Answer = "A", Actions = { action } }, "Provider");

        StringAssert.Contains(html, "The new text.");
        StringAssert.Contains(html, "cannot be undone");
        StringAssert.Contains(html, "Don't ask again for replacing the text of notes");
    }

    [TestMethod]
    public void TryReadActionsAnswer_ReadsWhatThePagePosts_AndNothingElse()
    {
        var id = Guid.NewGuid();

        Assert.IsTrue(AiChatHtml.TryReadActionsAnswer($"knt-actions:{{\"accepted\":[\"{id}\"],\"dontAsk\":true}}", out var answer));
        CollectionAssert.AreEqual(new[] { id }, answer.Accepted);
        Assert.IsTrue(answer.DontAsk);
        Assert.IsFalse(AiChatHtml.TryReadActionsAnswer("Ctrl+K", out _));
        Assert.IsFalse(AiChatHtml.TryReadActionsAnswer("knt-actions:not json", out _));
        Assert.IsFalse(AiChatHtml.TryReadActionsAnswer(null, out _));
    }

    [TestMethod]
    public async Task AnswerActions_Rejecting_RunsNothing_AndDoesNotGoOn()
    {
        var action = Proposed();
        var ctrl = CtrlWithTurn(new AiChatTurnDto { Answer = "A", Actions = { action } });

        var goOn = await ctrl.AnswerActionsAsync(new List<Guid>(), false);

        Assert.IsFalse(goOn);
        Assert.AreEqual(EnumAiActionStatus.Rejected, action.Status);
        Assert.AreEqual(0, ctrl.PendingActions.Count);
    }

    [TestMethod]
    public async Task RejectPendingActions_ANewMessageRejectsTheCard()
    {
        var action = Proposed();
        var ctrl = CtrlWithTurn(new AiChatTurnDto { Answer = "A", Actions = { action } });
        Assert.AreEqual(1, ctrl.PendingActions.Count);

        Assert.IsTrue(ctrl.RejectPendingActions());

        Assert.AreEqual(EnumAiActionStatus.Rejected, action.Status);
        Assert.IsFalse(ctrl.RejectPendingActions());
        await Task.CompletedTask;
    }
}
