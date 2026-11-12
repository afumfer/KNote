using KNote.Ai;
using KNote.ClientWin.Core;
using KNote.ClientWin.Tests.Fakes;
using KNote.ClientWin.Tests.Helpers;
using KNote.Model;
using KNote.Model.Dto;
using Microsoft.Extensions.AI;

namespace KNote.ClientWin.Tests;

/// <summary>
/// The citations of notes (#123) in the assistant of ClientWin and what a click on one does when the note cannot be opened
/// (the editor is the one of always, opened like any other), and the tools of each turn (KNote Atlas, delivery 1.2).
/// </summary>
[TestClass]
public class NoteCitationsTests
{
    private static async Task<string> OpenAsync(Result<NoteMinimalDto> answer)
    {
        var service = new FakeKntService();
        service.NotesFake.GetCitedNoteAsyncImpl = _ => Task.FromResult(answer);
        string message = null!;

        await NoteCitations.OpenAsync(TestStoreFactory.CreateEmpty(), service, 123, text => message = text);

        return message;
    }

    [TestMethod]
    public async Task OpenAsync_ANoteThatIsNotThere_SaysIt() =>
        Assert.AreEqual("Note #123 not found.", await OpenAsync(new Result<NoteMinimalDto>()));

    [TestMethod]
    public async Task OpenAsync_AnError_ShowsIt()
    {
        var failed = new Result<NoteMinimalDto>();
        failed.AddErrorMessage("boom");

        StringAssert.Contains(await OpenAsync(failed), "boom");
    }

    [TestMethod]
    public void TryOpen_AnotherLink_IsNotACitation()
    {
        var opened = NoteCitations.TryOpen("https://example.com/", null, null, null, null);

        Assert.IsFalse(opened);
    }

    [TestMethod]
    public void AssistantAnswers_HaveTheirCitationsAsLinks_AndThePageLetsThemThrough()
    {
        var html = AiChatHtml.MarkdownToHtml("See #123 and `#456`.");

        StringAssert.Contains(html, "href=\"knt-note:123\"");
        Assert.IsFalse(html.Contains("knt-note:456"), html);
        StringAssert.Contains(AiChatHtml.Page(Array.Empty<AiChatTurnDto>(), "Provider", ""), "knt-note");
    }

    [TestMethod]
    public async Task Streamer_SendsTheToolsOfTheTurn()
    {
        ChatOptions sent = null!;
        var chatClient = new FakeChatClient
        {
            GetStreamingResponseImpl = (_, options, _) =>
            {
                sent = options;
                return AsyncEnumerable.Empty<ChatResponseUpdate>();
            }
        };
        var tools = new List<AITool> { AIFunctionFactory.Create(() => "x", "some_tool") };

        await foreach (var _ in AiChatTurnStreamer.StreamAsync(chatClient, "system", null, "prompt", tools))
        {
        }

        CollectionAssert.AreEqual(tools, sent.Tools!.ToList());
    }

    [TestMethod]
    public void PendingAnswer_ShowsTheToolInUse()
    {
        StringAssert.Contains(AiChatHtml.PendingAnswer("Provider", null, "Using find_notes_about"), "Using find_notes_about");
        StringAssert.Contains(AiChatHtml.PendingAnswer("Provider", "Some text", "Using get_tasks"), "Using get_tasks");
        Assert.IsFalse(AiChatHtml.PendingAnswer("Provider", "Some text").Contains("Using"));
    }

    [TestMethod]
    public void AssistantForm_Page_HasTheStyleOfTheToolInUse() =>
        StringAssert.Contains(AiChatHtml.Page(Array.Empty<AiChatTurnDto>(), "Provider", ""), ".activity");

    [TestMethod]
    public async Task Tools_WithoutAnIndexOrAGlossary_AreOfferedWithoutThoseTwo_EvenIfTheyFail()
    {
        // The fake's services throw: the assistant goes on without these tools.
        var tools = new KNoteAiTools(new FakeKntService(), new KNoteAiToolsHost(TestStoreFactory.CreateEmpty()));

        var names = (await tools.GetToolsAsync()).Select(t => t.Name).ToList();

        Assert.AreEqual(22, names.Count, "view_note_images, read_document and view_document_pages come in, and attach_file only with files in the conversation.");
        CollectionAssert.DoesNotContain(names, "find_notes_about");
        CollectionAssert.DoesNotContain(names, "lookup_glossary");
    }
}
