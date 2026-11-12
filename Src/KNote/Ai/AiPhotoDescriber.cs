using System.Text;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace KNote.Ai;

/// <summary>
/// Describes a photo of a note with the AI (KNote Atlas, stage 4, D13), the same in the Server and in ClientWin: the photo
/// reduced (AiImageReducer), what the AI receives for the task DescribePhoto (rules, instructions, profile and the glossary
/// terms of the note) and the context of the note (AiPhotoRules.Request). No tools: the model can only answer with text. The
/// description is saved only if the resource has not changed meanwhile and is not locked (D12).
/// </summary>
public static class AiPhotoDescriber
{
    /// <summary>
    /// The model that describes photos here: the repository's recommended LLM if one of these providers has it, else
    /// the fallback (the default chat one). Message says why it can't (none, or a model that doesn't see images).
    /// </summary>
    public static async Task<(AiProviderRef Provider, ResourceDescriberDto Info)> ChooseAsync(IEnumerable<AiProviderRef> chatProviders,
        AiProviderRef fallback, string recommendedModel, CancellationToken cancellationToken = default)
    {
        var provider = AiProviderSelection.ForRepository(chatProviders, recommendedModel, fallback);
        if (provider == null)
            return (null, new ResourceDescriberDto { Message = "There is no AI provider configured to describe the photos." });

        var info = new ResourceDescriberDto
        {
            Provider = provider.Provider,
            Model = provider.Model,
            IsPaid = provider.IsPaid,
            SupportsImages = await AiModelCapabilities.SupportsImagesAsync(provider, cancellationToken)
        };
        if (!info.SupportsImages)
            info.Message = $"The model {provider.Model} can't see images: describing photos needs one that can (the repository's recommended LLM).";
        return (provider, info);
    }

    /// <summary>
    /// Describes a photo and saves its description (Entity: the resource as it is now). A photo whose description a person
    /// wrote is only described with <paramref name="replacePersonDescription"/>: the apps ask first.
    /// </summary>
    public static async Task<Result<ResourceInfoDto>> DescribeAsync(IKntService service, IChatClient chatClient, AiProviderRef provider,
        Guid resourceId, bool replacePersonDescription, ILogger logger = null, CancellationToken cancellationToken = default)
    {
        var result = new Result<ResourceInfoDto>();

        var context = await service.ResourceContent.GetPhotoContextAsync(resourceId);
        if (!context.IsValid)
        {
            result.AddListErrorMessage(context.ListErrorMessage);
            return result;
        }
        var resource = context.Entity.Resource;
        if (!replacePersonDescription && AiPhotoRules.HasPersonDescription(resource))
        {
            result.AddErrorMessage("Its description was written by a person: it is not replaced without asking.");
            return result;
        }

        var image = AiImageReducer.Reduce(resource.ContentArrayBytes, AiPhotoRules.ContentType(resource.FileType, resource.Name));
        if (image == null)
        {
            result.AddErrorMessage("The photo could not be read.");
            return result;
        }

        var system = await AiResourceTask.SystemPromptAsync(service, AiTaskCatalog.DescribePhoto,
            new List<string> { context.Entity.NoteTopic, context.Entity.NoteText, context.Entity.PhotoDescription }.Concat(context.Entity.PersonDescriptions),
            logger);
        List<ChatMessage> messages =
        [
            new(ChatRole.System, system),
            new(ChatRole.User, [
                new TextContent(AiPhotoRules.Request(context.Entity.NoteTopic, context.Entity.NoteText, context.Entity.PersonDescriptions,
                    context.Entity.PhotoDescription)),
                new DataContent(image.Content, image.ContentType)])
        ];

        var description = AiPhotoRules.CleanDescription(await AiResourceTask.AnswerAsync(chatClient, provider, messages, cancellationToken));
        if (description.Length == 0)
        {
            result.AddErrorMessage($"The model {provider.Model} gave no description.");
            return result;
        }

        return await service.ResourceContent.SaveAiDescriptionAsync(new AiDescriptionSaveDto
        {
            ResourceId = resource.ResourceId,
            Description = description,
            Model = provider.Model,
            ExpectedDescription = resource.Description,
            ExpectedName = resource.Name
        });
    }

}
