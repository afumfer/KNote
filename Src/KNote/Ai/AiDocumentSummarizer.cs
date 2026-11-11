using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace KNote.Ai;

/// <summary>
/// Summarizes a document of a note with the AI (KNote Atlas, stage 4, D13, 4.4c), the same in the Server and in ClientWin: what
/// the AI receives for the task SummarizeDocument (rules, instructions, profile and the glossary terms of the note and of the
/// document) and the request with the note and the beginning of the document (AiDocumentRules.Request). A document not read yet
/// is read first (IKntResourceContentService.GetDocumentContextAsync). No tools: the model can only answer with text. The summary
/// is saved in the resource's description only if the resource has not changed meanwhile and is not locked (D12).
/// </summary>
public static class AiDocumentSummarizer
{
    /// <summary>
    /// Summarizes a document and saves its summary (Entity: the resource as it is now). A document whose description a person
    /// wrote is only summarized with <paramref name="replacePersonDescription"/>: the apps ask first.
    /// </summary>
    public static async Task<Result<ResourceInfoDto>> SummarizeAsync(IKntService service, IChatClient chatClient, AiProviderRef provider,
        Guid resourceId, bool replacePersonDescription, ILogger logger = null, CancellationToken cancellationToken = default)
    {
        var result = new Result<ResourceInfoDto>();

        var context = await service.ResourceContent.GetDocumentContextAsync(resourceId);
        if (!context.IsValid)
        {
            result.AddListErrorMessage(context.ListErrorMessage);
            return result;
        }
        var document = context.Entity;
        var resource = document.Resource;
        if (!replacePersonDescription && AiDocumentRules.HasPersonDescription(resource))
        {
            result.AddErrorMessage("Its description was written by a person: it is not replaced without asking.");
            return result;
        }

        var system = await AiResourceTask.SystemPromptAsync(service, AiTaskCatalog.SummarizeDocument,
            [document.NoteTopic, document.NoteText, document.PersonDescription, document.DocumentText], logger);
        List<ChatMessage> messages =
        [
            new(ChatRole.System, system),
            new(ChatRole.User, AiDocumentRules.Request(document.NoteTopic, document.NoteText, resource.NameOut, document.Pages,
                document.DocumentText, document.PersonDescription))
        ];

        var summary = AiPhotoRules.CleanDescription(await AiResourceTask.AnswerAsync(chatClient, provider, messages, cancellationToken));
        if (summary.Length == 0)
        {
            result.AddErrorMessage($"The model {provider.Model} gave no summary.");
            return result;
        }

        return await service.ResourceContent.SaveAiDescriptionAsync(new AiDescriptionSaveDto
        {
            ResourceId = resource.ResourceId,
            Description = summary,
            Model = provider.Model,
            ExpectedDescription = resource.Description,
            ExpectedName = resource.Name
        });
    }
}
