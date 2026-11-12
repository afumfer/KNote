using System.Text;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace KNote.Ai;

/// <summary>
/// What the tasks of the resources share (KNote Atlas, stage 4, D13): describing a photo (AiPhotoDescriber), summarizing a
/// document (AiDocumentSummarizer) and transcribing a scanned page (AiPageTranscriber). What the AI receives for the task
/// (rules, instructions, profile and the glossary terms of the texts), the options measured in 4.0 and the answer read through
/// a stream. No tools: the model can only answer with text.
/// </summary>
internal static class AiResourceTask
{
    // Measured in 4.0 with qwen3.5:9b: about 70 tokens per description (and some 130 per summary, 4.4c). The cap stops a model
    // that repeats itself; it is only given to Ollama, whose reasoning is turned off: a reasoning model of another provider
    // would spend it thinking.
    private const int OllamaMaxOutputTokens = 500;

    /// <summary>
    /// The temperature, the cap and the reasoning turned off were measured with Ollama (4.0), and only go to it: OpenAI's
    /// reasoning models answer 400 to a temperature, as the assistant, which never sends one.
    /// </summary>
    public static ChatOptions Options(AiProviderRef provider, int maxOutputTokens = OllamaMaxOutputTokens)
    {
        var options = new ChatOptions();
        if (provider.Provider == EnumAiProvider.Ollama)
        {
            options.Temperature = 0.2f;
            options.MaxOutputTokens = maxOutputTokens;
            options.Reasoning = new ReasoningOptions { Effort = ReasoningEffort.None };
        }
        return options;
    }

    /// <summary>Through a stream, as the assistant: a whole answer in one request would have to come before HttpClient's timeout.</summary>
    public static async Task<string> AnswerAsync(IChatClient chatClient, AiProviderRef provider, List<ChatMessage> messages,
        CancellationToken cancellationToken, int maxOutputTokens = OllamaMaxOutputTokens)
    {
        var answer = new StringBuilder();
        await foreach (var update in chatClient.GetStreamingResponseAsync(messages, Options(provider, maxOutputTokens), cancellationToken))
            foreach (var text in update.Contents.OfType<TextContent>())
                answer.Append(text.Text);
        return answer.ToString();
    }

    /// <summary>What the AI receives for the task, with the glossary terms of the texts. Without it (an error), KNote's rules and instructions.</summary>
    public static async Task<string> SystemPromptAsync(IKntService service, AiTask task, IEnumerable<string> texts, ILogger logger)
    {
        try
        {
            var composed = await service.AtlasContext.ComposeAsync(new AiComposeRequestDto
            {
                TaskKey = task.Key,
                Texts = texts.Where(t => !string.IsNullOrWhiteSpace(t)).ToList()
            });
            if (composed.IsValid && composed.Entity != null)
                return composed.Entity.Text;
            logger?.LogWarning("The context of «{task}» could not be put together: {error}", task.Name, composed.ErrorMessage);
        }
        catch (Exception ex)
        {
            logger?.LogWarning(ex, "The context of «{task}» could not be put together.", task.Name);
        }

        return AiPromptComposer.Compose(new AiPromptComposer.Parts
        {
            Task = task,
            RepositoryName = service.RepositoryRef?.Name,
            UserName = service.UserIdentityName
        }).Text;
    }
}
