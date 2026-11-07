using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using Microsoft.Extensions.AI;

namespace KNote.Ai;

// The attachments of the message being sent (KNote Atlas, D12): all of them for the conversation and, as content for the model,
// the images it can see, reduced. Only in this turn: in the next ones it works with what it answered. Common to both hosts.
public sealed class AiTurnAttachments
{
    public IReadOnlyList<AiChatAttachmentDto> All { get; }

    public IReadOnlyList<DataContent> Images { get; }

    public IReadOnlyCollection<Guid> Seen { get; }

    private AiTurnAttachments(IReadOnlyList<AiChatAttachmentDto> all, IReadOnlyList<DataContent> images, IReadOnlyCollection<Guid> seen)
    {
        All = all;
        Images = images;
        Seen = seen;
    }

    // An image that cannot be read does not stop the message: the model is told that it does not see it.
    public static async Task<AiTurnAttachments> LoadAsync(IKntService service, AiProviderRef provider, Guid sessionNoteId,
        IReadOnlyList<AiChatAttachmentDto> attachments, CancellationToken cancellationToken = default)
    {
        var images = new List<DataContent>();
        var seen = new List<Guid>();

        if (attachments.Any(a => a.IsImage) && await AiModelCapabilities.SupportsImagesAsync(provider, cancellationToken))
            foreach (var attachment in attachments.Where(a => a.IsImage))
            {
                var file = await service.AiSessions.GetAttachmentAsync(sessionNoteId, attachment.ResourceId);
                if (!file.IsValid)
                    continue;

                var reduced = AiImageReducer.Reduce(Convert.FromBase64String(file.Entity.ContentBase64), file.Entity.ContentType);
                if (reduced == null)
                    continue;

                images.Add(new DataContent(reduced.Content, reduced.ContentType));
                seen.Add(attachment.ResourceId);
            }

        return new AiTurnAttachments(attachments, images, seen);
    }
}
