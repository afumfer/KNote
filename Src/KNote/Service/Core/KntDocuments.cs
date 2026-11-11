using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using Microsoft.Extensions.Logging;

namespace KNote.Service.Core;

/// <summary>How a reading of a document ended.</summary>
public enum EnumDocumentReadOutcome
{
    // Read and kept (its State says if it has pages to transcribe).
    Read,
    // It can't be read: kept as Unreadable, not tried again.
    Unreadable,
    // Its file is not in this installation (each one has its folder of resources): not an error, it is read where it is.
    NotAvailable,
    // This installation has no reader (IKntService.DocumentReader).
    NoReader,
    // It is not a document KNote reads.
    NotADocument,
    // Its file was replaced while it was being read: what was read is not kept.
    Changed
}

/// <summary>
/// Reading the documents of the notes and keeping their text (KNote Atlas, stage 4, D2, D6), for the indexer and for the
/// command «Read document». The file is read from its folder without loading it whole; a resource kept in the database
/// (ContentInDB), from memory. Reading is derived data, as the index: it doesn't touch the note nor its date.
/// </summary>
internal static class KntDocuments
{
    public static async Task<(EnumDocumentReadOutcome Outcome, ResourceTextDto Text)> ReadAndSaveAsync(IKntService service, Guid resourceId,
        CancellationToken cancellationToken = default)
    {
        var reader = service.DocumentReader;
        if (reader == null)
            return (EnumDocumentReadOutcome.NoReader, null);

        var resResource = await service.Repository.Notes.GetResourceAsync(resourceId);
        var resource = resResource.IsValid ? resResource.Entity : null;
        if (resource == null)
            return (EnumDocumentReadOutcome.NotADocument, null);
        var format = DocumentTextRules.FormatOf(resource.FileType, resource.Name);
        if (format == EnumDocumentFormat.None)
            return (EnumDocumentReadOutcome.NotADocument, null);

        using var content = OpenContent(service, resource);
        if (content == null)
            return (EnumDocumentReadOutcome.NotAvailable, null);

        string text;
        EnumExtractedState state;
        try
        {
            // Reading a long PDF takes a while: not in the caller's thread (ClientWin's UI).
            var pages = await Task.Run(() => reader.Read(content, format), cancellationToken);
            text = DocumentTextRules.Compose(pages, DocumentTextRules.IsPaged(format));
            state = DocumentTextRules.StateOf(pages);
        }
        catch (KntDocumentUnreadableException ex)
        {
            service.Logger?.LogInformation("Resource {name}: it can't be read ({reason}).", resource.Name, ex.Message);
            text = null;
            state = EnumExtractedState.Unreadable;
        }

        var saved = await service.Repository.Notes.SaveResourceTextAsync(resourceId, text, state, resource.Name);
        if (!saved.IsValid)
            throw new KntServiceException(saved.ErrorMessage);
        if (!saved.Entity)
            return (EnumDocumentReadOutcome.Changed, null);

        var read = new ResourceTextDto
        {
            ResourceId = resourceId, NoteId = resource.NoteId, Name = resource.Name, FileType = resource.FileType, State = state, Text = text
        };
        return (state == EnumExtractedState.Unreadable ? EnumDocumentReadOutcome.Unreadable : EnumDocumentReadOutcome.Read, read);
    }

    /// <summary>The documents of a note (in the order of its resources), each one with what was read of it.</summary>
    public static async Task<List<ResourceTextDto>> GetDocumentsAsync(IKntService service, Guid noteId)
    {
        var res = await service.Repository.Notes.GetResourceTextsAsync(noteId);
        if (!res.IsValid)
            throw new KntServiceException(res.ErrorMessage);
        return (res.Entity ?? new()).Where(r => DocumentTextRules.IsDocument(r.FileType, r.Name)).ToList();
    }

    // Its content: from the database (ContentInDB, the bytes come with the resource) or from its file, without loading it whole.
    // Null: it is not in this installation.
    private static Stream OpenContent(IKntService service, ResourceDto resource)
    {
        if (resource.ContentInDB && resource.ContentArrayBytes?.Length > 0)
            return new MemoryStream(resource.ContentArrayBytes, writable: false);

        var path = service.Notes.UtilGetResourceFilePath(resource);
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
            return null;
        try
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            service.Logger?.LogWarning(ex, "Resource {name}: the file {file} couldn't be read.", resource.Name, path);
            return null;
        }
    }
}
