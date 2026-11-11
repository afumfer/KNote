using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace KNote.Ai;

/// <summary>
/// Transcribes a scanned page of a PDF with the AI (KNote Atlas, stage 4, D1, D13, 4.5), the same in the Server and in ClientWin:
/// the page drawn (IKntResourceContentService.GetPageContextAsync, AiTranscriptionRules.PageImageSide), what the AI receives for
/// the task TranscribePage (rules, instructions and profile) and the request (AiTranscriptionRules.Request). No tools: the model
/// can only answer with text. The transcription is kept in place of the text of the OCR layer, only if the file is the same and
/// the page is still pending; it needs a model that sees images (AiPhotoDescriber.ChooseAsync).
/// </summary>
public static class AiPageTranscriber
{
    /// <summary>Transcribes a page and keeps its transcription (Entity: what is left of the document).</summary>
    public static async Task<Result<DocumentReadDto>> TranscribeAsync(IKntService service, IChatClient chatClient, AiProviderRef provider,
        Guid resourceId, int page, ILogger logger = null, CancellationToken cancellationToken = default)
    {
        var result = new Result<DocumentReadDto>();

        var context = await service.ResourceContent.GetPageContextAsync(resourceId, page);
        if (!context.IsValid)
        {
            result.AddListErrorMessage(context.ListErrorMessage);
            return result;
        }
        var drawn = context.Entity;

        var system = await AiResourceTask.SystemPromptAsync(service, AiTaskCatalog.TranscribePage, [drawn.FileName], logger);
        List<ChatMessage> messages =
        [
            new(ChatRole.System, system),
            new(ChatRole.User, [
                new TextContent(AiTranscriptionRules.Request(drawn.FileName, drawn.Page, drawn.PagesCount)),
                new DataContent(drawn.Image, drawn.ContentType)])
        ];

        var answer = await AiResourceTask.AnswerAsync(chatClient, provider, messages, cancellationToken, AiTranscriptionRules.MaxOutputTokens);
        if (string.IsNullOrWhiteSpace(answer))
        {
            result.AddErrorMessage(AiTranscriptionRules.NoTextMessage);
            return result;
        }

        return await service.ResourceContent.SaveTranscriptionAsync(new PageTranscriptionSaveDto
        {
            ResourceId = drawn.ResourceId,
            Page = drawn.Page,
            Text = answer,
            ExpectedName = drawn.ExpectedName
        });
    }
}
