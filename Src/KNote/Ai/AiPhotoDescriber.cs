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
    // Measured in 4.0 with qwen3.5:9b: about 70 tokens per description. The cap stops a model that repeats itself; it is only
    // given to Ollama, whose reasoning is turned off: a reasoning model of another provider would spend it thinking.
    private const int OllamaMaxOutputTokens = 500;

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

        var system = await SystemPromptAsync(service, context.Entity, logger);
        List<ChatMessage> messages =
        [
            new(ChatRole.System, system),
            new(ChatRole.User, [
                new TextContent(AiPhotoRules.Request(context.Entity.NoteTopic, context.Entity.NoteText, context.Entity.PersonDescriptions,
                    context.Entity.PhotoDescription)),
                new DataContent(image.Content, image.ContentType)])
        ];

        // The temperature, the cap and the reasoning turned off were measured with Ollama (4.0), and only go to it: OpenAI's
        // reasoning models answer 400 to a temperature, as the assistant, which never sends one.
        var options = new ChatOptions();
        if (provider.Provider == EnumAiProvider.Ollama)
        {
            options.Temperature = 0.2f;
            options.MaxOutputTokens = OllamaMaxOutputTokens;
            options.Reasoning = new ReasoningOptions { Effort = ReasoningEffort.None };
        }

        // Through a stream, as the assistant: a whole answer in one request would have to come before HttpClient's timeout.
        var answer = new StringBuilder();
        await foreach (var update in chatClient.GetStreamingResponseAsync(messages, options, cancellationToken))
            foreach (var text in update.Contents.OfType<TextContent>())
                answer.Append(text.Text);

        var description = AiPhotoRules.CleanDescription(answer.ToString());
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

    // What the AI receives for the task, with the glossary terms of the note. Without it (an error), KNote's rules and instructions.
    private static async Task<string> SystemPromptAsync(IKntService service, PhotoDescriptionContextDto context, ILogger logger)
    {
        try
        {
            var texts = new List<string> { context.NoteTopic, context.NoteText, context.PhotoDescription }.Concat(context.PersonDescriptions)
                .Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
            var composed = await service.AtlasContext.ComposeAsync(new AiComposeRequestDto { TaskKey = AiTaskCatalog.DescribePhotoKey, Texts = texts });
            if (composed.IsValid && composed.Entity != null)
                return composed.Entity.Text;
            logger?.LogWarning("The context of «Describe photo» could not be put together: {error}", composed.ErrorMessage);
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "The context of «Describe photo» could not be put together.");
        }

        return AiPromptComposer.Compose(new AiPromptComposer.Parts
        {
            Task = AiTaskCatalog.DescribePhoto,
            RepositoryName = service.RepositoryRef?.Name,
            UserName = service.UserIdentityName
        }).Text;
    }
}
