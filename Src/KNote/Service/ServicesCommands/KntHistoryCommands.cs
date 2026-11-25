using System;
using System.Linq;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.Service.ServicesCommands;

/// <summary>A page of the history of a note, the newest first, without the copies (the lists never read them).</summary>
[KntAuthorize(EnumRoles.Guest)]
public class KntHistoryGetNoteHistoryAsyncCommand : KntCommandServiceBase<(Guid NoteId, int Skip, int Take), Result<NoteHistoryPageDto>>
{
    public KntHistoryGetNoteHistoryAsyncCommand(IKntService service, Guid noteId, int skip, int take) : base(service, (noteId, skip, take))
    {

    }

    public override async Task<Result<NoteHistoryPageDto>> Execute()
    {
        var result = new Result<NoteHistoryPageDto>();
        var rows = await Repository.History.GetEntityHistoryAsync(HistoryEntities.Note, Param.NoteId);
        if (!rows.IsValid)
        {
            result.AddListErrorMessage(rows.ListErrorMessage);
            return result;
        }

        var all = rows.Entity ?? new();
        var skip = Math.Max(0, Param.Skip);
        var take = Param.Take <= 0 ? NoteHistoryPageDto.PageSize : Math.Min(Param.Take, NoteHistoryPageDto.PageSize);
        result.Entity = new NoteHistoryPageDto
        {
            NoteId = Param.NoteId,
            Rows = all.Skip(skip).Take(take).ToList(),
            Total = all.Count,
            Skip = skip
        };
        return result;
    }
}

/// <summary>
/// The note as it was after a change of its history (H18, HistoryRules.VersionAfter): the copy of a later row or the note as it
/// is now, with the path of its folder, the names of its type and of its attributes («(deleted attribute)» for one that no longer
/// exists) and without its script (H21).
/// </summary>
[KntAuthorize(EnumRoles.Guest)]
public class KntHistoryGetNoteVersionAsyncCommand : KntCommandServiceBase<(Guid NoteId, Guid KLogId), Result<NoteVersionViewDto>>
{
    public const string NotInHistory = "This change is not in the history of the note.";
    public const string CopyNotKept = "The copy of this version is no longer kept.";

    public KntHistoryGetNoteVersionAsyncCommand(IKntService service, Guid noteId, Guid klogId) : base(service, (noteId, klogId))
    {

    }

    public override async Task<Result<NoteVersionViewDto>> Execute()
    {
        var result = new Result<NoteVersionViewDto>();
        var rows = (await Repository.History.GetEntityHistoryAsync(HistoryEntities.Note, Param.NoteId)).Entity ?? new();
        var row = rows.FirstOrDefault(r => r.KLogId == Param.KLogId);
        var source = row == null ? null : HistoryRules.VersionAfter(rows, Param.KLogId);
        if (source == null)
        {
            result.AddErrorMessage(NotInHistory);
            return result;
        }

        var note = (await Repository.Notes.GetAsync(Param.NoteId)).Entity;
        NoteVersionDto version;
        if (source.CopyRow != null)
            version = NoteVersionDto.FromJson((await Repository.History.GetPreviousContentAsync(source.CopyRow.KLogId)).Entity);
        else
            version = note == null ? null : NoteVersionDto.From(note);
        if (version == null)
        {
            result.AddErrorMessage(CopyNotKept);
            return result;
        }

        var view = new NoteVersionViewDto
        {
            NoteId = Param.NoteId,
            KLogId = row.KLogId,
            NoteNumber = note?.NoteNumber ?? NumberOf(row.EntityTitle),
            AsOf = row.LastDateTime,
            UserName = row.UserName,
            Change = row.RegistryMessage,
            Exact = source.Exact,
            IncludesUntil = source.IncludesUntil,
            Notice = HistoryRules.VersionNotice(source.IncludesUntil),
            Topic = version.Topic,
            FolderPath = await KntHistory.FolderPathAsync(Service, version.FolderId) ?? "(unknown folder)",
            NoteTypeName = version.NoteTypeId is Guid typeId ? (await Repository.NoteTypes.GetAsync(typeId)).Entity?.Name ?? "(deleted type)" : null,
            Tags = version.Tags,
            Priority = version.Priority,
            DescriptionFormat = ContentTypeExtJsonHelper.Read(version.ContentType).ForDescription,
            Description = version.Description
        };
        foreach (var attribute in version.Attributes)
        {
            var stored = await Repository.KAttributes.GetAsync(attribute.Id);
            view.Attributes.Add(new NoteVersionAttributeViewDto
            {
                Name = stored.IsValid && stored.Entity != null ? stored.Entity.Name : "(deleted attribute)",
                Value = attribute.Value
            });
        }

        result.Entity = view;
        return result;
    }

    // The number of a note from the title of its row (#340 Topic), for one that no longer exists.
    private static int NumberOf(string title) =>
        title?.StartsWith('#') == true && int.TryParse(new string(title.Skip(1).TakeWhile(char.IsDigit).ToArray()), out var number) ? number : 0;
}
