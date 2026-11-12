using System.ComponentModel;
using KNote.Model;
using KNote.Model.Dto;
using Microsoft.Extensions.AI;

namespace KNote.Ai;

// KNote Atlas, stage 4 (D14, 4.7a): the assistant reads the documents of a note, or the ones attached to the conversation. Only reads:
// read_document gives the text kept (a document not read yet is read now and not kept, decided by the owner), by pages, at most
// AiDocumentToolRules.MaxTextLength each time; view_document_pages draws pages of a PDF and gives them as images in the next
// message, as view_note_images (AiToolImages). The attachments, of the conversation's session (its owner only), are read on the fly.
public partial class KNoteAiTools
{
    private const string ReadDocumentName = "read_document";
    private const string ViewDocumentPagesName = "view_document_pages";

    private IEnumerable<AITool> GetDocumentTools()
    {
        yield return AIFunctionFactory.Create(ReadDocumentAsync, new AIFunctionFactoryOptions
        {
            Name = ReadDocumentName,
            Description = "Reads a document attached to a note (PDF, DOCX, ODT, TXT or MD) or, without a note, one the user attached to " +
                "this conversation. Gives its text by pages, " + $"{AiDocumentToolRules.MaxTextLength:N0} characters at most each time: " +
                "it says how many pages there are and how to ask for the next ones. The scanned pages say whether the AI transcribed them; " +
                "the ones not transcribed yet can be looked at with view_document_pages. Use get_note_items to see the documents of a note."
        });

        yield return AIFunctionFactory.Create(ViewDocumentPagesAsync, new AIFunctionFactoryOptions
        {
            Name = ViewDocumentPagesName,
            Description = "Looks at pages of a PDF attached to a note or, without a note, to this conversation, as images: to see a figure, a " +
                "diagram, a table or a scanned page. You receive them in the next message, " +
                $"{AiDocumentToolRules.MaxPagesPerLook} at most each time. To read its text, read_document is better."
        });
    }

    // A document found: its name and how to read it or draw a page of it.
    private sealed record ToolDocument(string FileName, EnumDocumentFormat Format, Func<Task<(ResourceTextDto Text, string Error)>> ReadAsync,
        Func<int, Task<(byte[] Image, string Error)>> DrawAsync, Guid Id);

    [Description("Reads a document of a note or of the conversation.")]
    private async Task<string> ReadDocumentAsync(
        [Description("The note's NoteNumber (a short integer, e.g. 123). Without it, a document attached to this conversation.")]
        int? noteNumber = null,
        [Description("The name of the file. Without it, the only document of the note or of the last message.")]
        string fileName = null,
        [Description("The first page to read (from 1). By default, 1.")]
        int fromPage = 1,
        [Description("The last page to read. By default, as many as fit.")]
        int? toPage = null)
    {
        var (document, error) = await FindToolDocumentAsync(noteNumber, fileName);
        if (error != null)
            return error;

        var (text, readError) = await document.ReadAsync();
        if (readError != null)
            return readError;
        return AiDocumentToolRules.ForModel(document.FileName, text.Text, DocumentTextRules.IsPaged(document.Format), fromPage, toPage);
    }

    [Description("Looks at pages of a PDF of a note or of the conversation.")]
    private async Task<string> ViewDocumentPagesAsync(
        [Description("The note's NoteNumber (a short integer, e.g. 123). Without it, a PDF attached to this conversation.")]
        int? noteNumber = null,
        [Description("The name of the file. Without it, the only document of the note or of the last message.")]
        string fileName = null,
        [Description("Which pages, from 1. By default, the first ones.")]
        int[] pages = null)
    {
        if (TurnImages.IsFull)
            return "Error: you have seen enough images in this answer. Answer with them, and say that you can look at more.";

        var (document, error) = await FindToolDocumentAsync(noteNumber, fileName);
        if (error != null)
            return error;
        if (document.Format != EnumDocumentFormat.Pdf)
            return $"Error: {document.FileName} is not a PDF: read it with read_document.";

        var (text, readError) = await document.ReadAsync();
        if (readError != null)
            return readError;
        var (picked, pickError) = AiDocumentToolRules.PickPages(DocumentTextRules.Pages(text.Text).Count, pages);
        if (pickError != null)
            return pickError;

        var seen = new List<int>();
        var failed = new List<string>();
        foreach (var page in picked)
        {
            var (image, drawError) = await document.DrawAsync(page);
            if (image == null)
            {
                failed.Add($"page {page} ({drawError})");
                continue;
            }
            TurnImages.Add(PageId(document.Id, page), image, "image/jpeg");
            seen.Add(page);
        }

        if (seen.Count == 0)
            return $"Error: the pages of {document.FileName} could not be drawn: {string.Join("; ", failed)}.";
        var notDrawn = failed.Count > 0 ? $" These could not be drawn: {string.Join("; ", failed)}." : "";
        return $"{document.FileName} has {DocumentTextRules.Pages(text.Text).Count} pages. You receive these in the next message: " +
            $"{string.Join(", ", seen.Select(p => $"page {p}"))}.{notDrawn}";
    }

    // The same page of a document, always the same image (AiToolImages keeps one of each).
    private static Guid PageId(Guid documentId, int page)
    {
        var bytes = documentId.ToByteArray();
        var number = BitConverter.GetBytes(page);
        for (var i = 0; i < number.Length; i++)
            bytes[12 + i] ^= number[i];
        return new Guid(bytes);
    }

    private Task<(ToolDocument Document, string Error)> FindToolDocumentAsync(int? noteNumber, string fileName) =>
        noteNumber is int number ? FindNoteDocumentAsync(number, fileName) : Task.FromResult(FindAttachedDocument(fileName));

    private async Task<(ToolDocument Document, string Error)> FindNoteDocumentAsync(int noteNumber, string fileName)
    {
        var note = await _service.Notes.GetCitedNoteAsync(noteNumber);
        if (!note.IsValid)
            return (null, $"Error getting note: {note.ErrorMessage}");
        if (note.Entity == null)
            return (null, "Note not found.");
        var resources = await _service.Notes.GetResourcesInfoAsync(note.Entity.NoteId);
        if (!resources.IsValid)
            return (null, $"Error reading the resources of the note: {resources.ErrorMessage}");

        var documents = (resources.Entity ?? new()).Where(DocumentTextRules.IsDocument).ToList();
        var (resource, error) = AiDocumentToolRules.Find(documents, r => r.NameOut, fileName, $"The note #{noteNumber}");
        if (error != null)
            return (null, error);

        var id = resource.ResourceId;
        return (new ToolDocument(resource.NameOut, DocumentTextRules.FormatOf(resource.FileType, resource.Name),
            async () =>
            {
                var text = await _service.ResourceContent.GetDocumentToReadAsync(id);
                return text.IsValid ? (text.Entity, null) : (null, $"Error reading {resource.NameOut}: {text.ErrorMessage}");
            },
            async page =>
            {
                var drawn = await _service.ResourceContent.DrawDocumentPageAsync(id, page);
                return drawn.IsValid ? (Convert.FromBase64String(drawn.Entity.ContentBase64), null) : (null, drawn.ErrorMessage);
            }, id), null);
    }

    // A document attached to this conversation: read from the session (its owner only), on the fly; nothing is kept.
    private (ToolDocument Document, string Error) FindAttachedDocument(string fileName)
    {
        var documents = _attachments.Where(a => DocumentTextRules.IsDocument(a.ContentType, a.FileName)).ToList();
        var last = _lastAttachments.Where(a => DocumentTextRules.IsDocument(a.ContentType, a.FileName)).ToList();
        if (documents.Count == 0)
            return (null, "Error: this conversation has no documents. Say which note to read.");
        var (file, error) = string.IsNullOrWhiteSpace(fileName) && last.Count == 1
            ? (last[0], null)
            : AiDocumentToolRules.Find(documents, a => a.FileName, fileName, "This conversation");
        if (error != null)
            return (null, error);

        var format = DocumentTextRules.FormatOf(file.ContentType, file.FileName);
        return (new ToolDocument(file.FileName, format,
            async () =>
            {
                var (bytes, readError) = await ReadAttachedFileAsync(file);
                if (bytes == null)
                    return (null, readError);
                try
                {
                    using var stream = new MemoryStream(bytes);
                    var pages = _service.DocumentReader.Read(stream, format);
                    return (new ResourceTextDto
                    {
                        ResourceId = file.ResourceId, Name = file.FileName, FileType = file.ContentType,
                        State = DocumentTextRules.StateOf(pages), Text = DocumentTextRules.Compose(pages, DocumentTextRules.IsPaged(format))
                    }, null);
                }
                catch (KNote.Service.Core.KntDocumentUnreadableException)
                {
                    return (null, $"Error: {file.FileName}: {AiDocumentRules.UnreadableMessage}");
                }
            },
            async page =>
            {
                var (bytes, readError) = await ReadAttachedFileAsync(file);
                if (bytes == null)
                    return (null, readError);
                try
                {
                    using var stream = new MemoryStream(bytes);
                    return (_service.DocumentReader.DrawPage(stream, page, AiTranscriptionRules.PageImageSide), null);
                }
                catch (Exception ex) when (ex is KNote.Service.Core.KntDocumentUnreadableException or PlatformNotSupportedException)
                {
                    return (null, ex.Message);
                }
            }, file.ResourceId), null);
    }

    private async Task<(byte[] Bytes, string Error)> ReadAttachedFileAsync(AiChatAttachmentDto file)
    {
        if (_service.DocumentReader == null)
            return (null, "Error: this installation can't read documents.");
        var content = await _service.AiSessions.GetAttachmentAsync(_sessionNoteId, file.ResourceId);
        return content.IsValid && content.Entity?.ContentBase64 != null
            ? (Convert.FromBase64String(content.Entity.ContentBase64), null)
            : (null, $"Error reading {file.FileName}: {content.ErrorMessage}");
    }
}
