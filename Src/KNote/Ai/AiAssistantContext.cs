using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using Microsoft.Extensions.Logging;

namespace KNote.Ai;

/// <summary>
/// The system message of the assistant of the two apps (KNote Atlas, chapter 21): put together by the repository's
/// service (IKntAtlasContextService.ComposeAsync: rules, instructions, profile and glossary), with the conversation's
/// prompts and answers to find the glossary terms (D7) and, in ClientWin, the instructions that replace the task's ones in
/// that conversation (D6). When the repository cannot give it (an error, no service), the assistant still works: with
/// KNote's rules and instructions only.
/// </summary>
public static class AiAssistantContext
{
    public static async Task<AiComposedPromptDto> ComposeAsync(IKntService service, string instructionsOverride,
        IEnumerable<AiChatTurnDto> history, string prompt, ILogger logger = null)
    {
        var texts = (history ?? Enumerable.Empty<AiChatTurnDto>())
            .SelectMany(t => new[] { t.Prompt, t.Answer })
            .Append(prompt)
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToList();

        if (service != null)
        {
            try
            {
                var res = await service.AtlasContext.ComposeAsync(new AiComposeRequestDto
                {
                    TaskKey = AiTaskCatalog.AssistantKey,
                    InstructionsOverride = instructionsOverride,
                    Texts = texts
                });
                if (res.IsValid && res.Entity != null)
                    return res.Entity;
                logger?.LogWarning("The context of the AI assistant could not be put together: {error}", res.ErrorMessage);
            }
            catch (Exception ex)
            {
                logger?.LogWarning(ex, "The context of the AI assistant could not be put together.");
            }
        }

        return AiPromptComposer.Compose(new AiPromptComposer.Parts
        {
            RepositoryName = service?.RepositoryRef?.Name,
            UserName = service?.UserIdentityName,
            InstructionsOverride = instructionsOverride
        });
    }
}
