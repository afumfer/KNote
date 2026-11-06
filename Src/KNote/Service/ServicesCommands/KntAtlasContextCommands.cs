using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;

namespace KNote.Service.ServicesCommands;

// The context KNote Atlas gives the AI (stage 1, chapters 19–21): the windows of the profile, the glossary and the task
// instructions (any user), the creation of their notes (ProjectManager) and what the AI receives (Staff). See
// KntAtlasNotes and AiPromptComposer.

[KntAuthorize(EnumRoles.Guest)]
public class KntAtlasContextGetProfileInfoAsyncCommand : KntCommandServiceBase<Result<AtlasProfileInfoDto>>
{
    public KntAtlasContextGetProfileInfoAsyncCommand(IKntService service) : base(service)
    {

    }

    public override async Task<Result<AtlasProfileInfoDto>> Execute()
    {
        var result = new Result<AtlasProfileInfoDto>();
        var info = new AtlasProfileInfoDto { MaxWords = AiPromptComposer.MaxProfileWords };
        result.Entity = info;

        var notes = await KntAtlasNotes.FindAsync(Service, KntConst.ProfileTag, result);
        if (notes.Count == 0)
            return result;
        var note = (await Service.Notes.GetAsync(notes[0].NoteId)).Entity;
        info.Note = KntAtlasNotes.Ref(notes[0]);
        info.Words = AtlasMarkdown.CountWords(AtlasMarkdown.Clean(KntAtlasNotes.Text(Service, note)));
        if (info.Words > info.MaxWords)
            info.Notices.Add($"The profile has {info.Words} words: the AI receives only the first {info.MaxWords}.");
        if (notes.Count > 1)
            info.Notices.Add($"There are {notes.Count} profiles: KNote uses the oldest one, #{notes[0].NoteNumber}.");
        return result;
    }
}

[KntAuthorize(EnumRoles.ProjectManager)]
public class KntAtlasContextCreateProfileAsyncCommand : KntCommandServiceBase<Result<NoteDto>>
{
    public KntAtlasContextCreateProfileAsyncCommand(IKntService service) : base(service)
    {

    }

    public override async Task<Result<NoteDto>> Execute()
    {
        var result = new Result<NoteDto>();
        result.Entity = await AtlasContextNotes.ProfileAsync(Service, result);
        return result;
    }
}

[KntAuthorize(EnumRoles.ProjectManager)]
public class KntAtlasContextFillProfileFoldersAndTypesAsyncCommand : KntCommandServiceBase<Result<NoteDto>>
{
    public KntAtlasContextFillProfileFoldersAndTypesAsyncCommand(IKntService service) : base(service)
    {

    }

    public override async Task<Result<NoteDto>> Execute()
    {
        var result = new Result<NoteDto>();
        var note = await AtlasContextNotes.ProfileAsync(Service, result);
        if (note == null)
            return result;
        if (string.Equals(note.GetContentTypeExt().ForDescription, "html", StringComparison.OrdinalIgnoreCase))
        {
            result.AddErrorMessage("The profile is written in HTML: folders and types can only be filled in a profile in Markdown.");
            return result;
        }

        var resFolders = await Service.Folders.GetAllAsync();
        var resTypes = await Service.NoteTypes.GetAllAsync();
        if (!resFolders.IsValid || !resTypes.IsValid)
        {
            result.AddListErrorMessage(resFolders.ListErrorMessage);
            result.AddListErrorMessage(resTypes.ListErrorMessage);
            return result;
        }

        // The folders of the first two levels, in the order of the tree, without KNote's own ones.
        var folders = resFolders.Entity.Where(f => !KntConst.SystemFolderNames.Contains(f.Name)).ToList();
        var children = folders.ToLookup(f => f.ParentId);
        var folderLines = new List<string>();
        foreach (var root in children[null].OrderBy(f => f.Order).ThenBy(f => f.Name))
        {
            folderLines.Add($"- {root.Name}:");
            folderLines.AddRange(children[root.FolderId].OrderBy(f => f.Order).ThenBy(f => f.Name).Select(f => $"- {root.Name} › {f.Name}:"));
        }

        // The note types with notes, without KNote's own ones (their names start with @).
        var typeLines = new List<string>();
        foreach (var type in resTypes.Entity.Where(t => !(t.Name ?? "").StartsWith('@')).OrderBy(t => t.Name))
        {
            var resNotes = await Service.Notes.GetFilterMinimalAsync(new NotesFilterDto
            {
                NoteTypeId = type.NoteTypeId,
                PageIdentifier = new PageIdentifier { PageNumber = 1, PageSize = 1 }
            });
            if (resNotes.IsValid && resNotes.Entity.Count > 0)
                typeLines.Add($"- {type.Name}:");
        }

        var text = AtlasMarkdown.AddMissingLines(note.Description, AtlasTemplates.FoldersSection, folderLines);
        text = AtlasMarkdown.AddMissingLines(text, AtlasTemplates.NoteTypesSection, typeLines);
        if (text == note.Description)
        {
            result.Entity = note;
            return result;
        }

        note.Description = text;
        var resSave = await Service.Notes.SaveAsync(note);
        if (!resSave.IsValid)
            result.AddListErrorMessage(resSave.ListErrorMessage);
        result.Entity = resSave.Entity;
        return result;
    }
}

[KntAuthorize(EnumRoles.Guest)]
public class KntAtlasContextGetGlossaryInfoAsyncCommand : KntCommandServiceBase<Result<AtlasGlossaryInfoDto>>
{
    public KntAtlasContextGetGlossaryInfoAsyncCommand(IKntService service) : base(service)
    {

    }

    public override async Task<Result<AtlasGlossaryInfoDto>> Execute()
    {
        var result = new Result<AtlasGlossaryInfoDto>();
        var info = new AtlasGlossaryInfoDto();
        result.Entity = info;

        var notes = await KntAtlasNotes.FindAsync(Service, KntConst.GlossaryTag, result);
        if (notes.Count == 0)
            return result;
        var note = (await Service.Notes.GetAsync(notes[0].NoteId)).Entity;
        var (terms, issues) = GlossaryTable.Parse(KntAtlasNotes.Text(Service, note));
        info.Note = KntAtlasNotes.Ref(notes[0]);
        info.Terms = terms.Count;
        info.Issues = issues;
        if (notes.Count > 1)
            info.Notices.Add($"There are {notes.Count} glossaries: KNote uses the oldest one, #{notes[0].NoteNumber}.");
        return result;
    }
}

// The terms of the glossary that appear in some texts (GlossaryTable.Find): a note the AI reads, or the terms it looks up.
[KntAuthorize(EnumRoles.Guest)]
public class KntAtlasContextFindGlossaryTermsAsyncCommand : KntCommandServiceBase<List<string>, Result<List<GlossaryTermDto>>>
{
    public KntAtlasContextFindGlossaryTermsAsyncCommand(IKntService service, List<string> texts) : base(service, texts)
    {
    }

    public override async Task<Result<List<GlossaryTermDto>>> Execute()
    {
        var result = new Result<List<GlossaryTermDto>>(new List<GlossaryTermDto>());
        var glossary = await KntAtlasNotes.FindFirstAsync(Service, KntConst.GlossaryTag, result);
        if (result.IsValid && glossary != null)
            result.Entity = GlossaryTable.Find(GlossaryTable.Parse(KntAtlasNotes.Text(Service, glossary)).Terms, Param);
        return result;
    }
}

[KntAuthorize(EnumRoles.ProjectManager)]
public class KntAtlasContextCreateGlossaryAsyncCommand : KntCommandServiceBase<Result<NoteDto>>
{
    public KntAtlasContextCreateGlossaryAsyncCommand(IKntService service) : base(service)
    {

    }

    public override async Task<Result<NoteDto>> Execute()
    {
        var result = new Result<NoteDto>();
        result.Entity = await KntAtlasNotes.EnsureSingleAsync(Service, KntConst.GlossaryTag,
            "KNote Atlas: the glossary of the repository (one note).", KntConst.GlossaryTopic, GlossaryTable.Template, result);
        return result;
    }
}

[KntAuthorize(EnumRoles.Guest)]
public class KntAtlasContextGetTaskPromptsAsyncCommand : KntCommandServiceBase<Result<List<AtlasTaskPromptStatusDto>>>
{
    public KntAtlasContextGetTaskPromptsAsyncCommand(IKntService service) : base(service)
    {

    }

    public override async Task<Result<List<AtlasTaskPromptStatusDto>>> Execute()
    {
        var result = new Result<List<AtlasTaskPromptStatusDto>>();
        result.Entity = new List<AtlasTaskPromptStatusDto>();

        foreach (var task in AiTaskCatalog.Tasks)
        {
            var prompts = await KntAtlasNotes.FindTaskPromptsAsync(Service, task, result);
            var active = prompts.Where(p => p.Active).ToList();
            var used = active.FirstOrDefault();
            var status = new AtlasTaskPromptStatusDto
            {
                TaskKey = task.Key,
                TaskName = task.Name,
                State = used == null ? EnumAtlasTaskPromptState.Default
                    : used.Replaces ? EnumAtlasTaskPromptState.Replaced
                    : EnumAtlasTaskPromptState.Added,
                Note = KntAtlasNotes.Ref((used ?? prompts.FirstOrDefault())?.Note)
            };
            if (active.Count > 1)
                status.Notices.Add($"The task «{task.Name}» has {active.Count} active notes: KNote uses the oldest one, #{used.Note.NoteNumber}.");
            result.Entity.Add(status);
        }
        return result;
    }
}

[KntAuthorize(EnumRoles.ProjectManager)]
public class KntAtlasContextCustomizeTaskAsyncCommand : KntCommandServiceBase<string, Result<NoteDto>>
{
    public KntAtlasContextCustomizeTaskAsyncCommand(IKntService service, string taskKey) : base(service, taskKey)
    {

    }

    public override Result ValidateParam()
    {
        var result = new Result();
        if (AiTaskCatalog.Find(Param) == null)
            result.AddErrorMessage($"«{Param}» is not a task of KNote Atlas.");
        return result;
    }

    public override async Task<Result<NoteDto>> Execute()
    {
        var result = new Result<NoteDto>();
        result.Entity = await KntAtlasNotes.EnsureTaskPromptAsync(Service, AiTaskCatalog.Find(Param), result);
        return result;
    }
}

[KntAuthorize(EnumRoles.Staff)]
public class KntAtlasContextComposeAsyncCommand : KntCommandServiceBase<AiComposeRequestDto, Result<AiComposedPromptDto>>
{
    public KntAtlasContextComposeAsyncCommand(IKntService service, AiComposeRequestDto request) : base(service, request)
    {

    }

    public override Result ValidateParam()
    {
        var result = new Result();
        if (Param == null || AiTaskCatalog.Find(Param.TaskKey) == null)
            result.AddErrorMessage($"«{Param?.TaskKey}» is not a task of KNote Atlas.");
        return result;
    }

    public override async Task<Result<AiComposedPromptDto>> Execute()
    {
        var result = new Result<AiComposedPromptDto>();
        var task = AiTaskCatalog.Find(Param.TaskKey);

        var user = await Service.GetCurrentUserAsync();
        var role = await Service.GetCurrentUserRoleAsync();

        var profile = await KntAtlasNotes.FindFirstAsync(Service, KntConst.ProfileTag, result);
        var glossary = await KntAtlasNotes.FindFirstAsync(Service, KntConst.GlossaryTag, result);
        var prompt = (await KntAtlasNotes.FindTaskPromptsAsync(Service, task, result)).FirstOrDefault(p => p.Active);
        if (!result.IsValid)
            return result;

        var terms = glossary == null
            ? new List<GlossaryTermDto>()
            : GlossaryTable.Find(GlossaryTable.Parse(KntAtlasNotes.Text(Service, glossary)).Terms, Param.Texts);

        result.Entity = AiPromptComposer.Compose(new AiPromptComposer.Parts
        {
            Task = task,
            RepositoryName = Service.RepositoryRef?.Name,
            UserName = string.IsNullOrWhiteSpace(user?.FullName) ? user?.UserName ?? Service.UserIdentityName : user.FullName,
            Role = role,
            Today = DateTime.Today,
            InstructionsOverride = Param.InstructionsOverride,
            TaskPromptText = prompt == null ? null : KntAtlasNotes.Text(Service, prompt.Note),
            TaskPromptReplaces = prompt?.Replaces ?? false,
            TaskPromptNoteNumber = prompt?.Note.NoteNumber,
            ProfileText = profile == null ? null : KntAtlasNotes.Text(Service, profile),
            ProfileNoteNumber = profile?.NoteNumber,
            GlossaryTerms = terms,
            GlossaryNoteNumber = glossary?.NoteNumber
        });
        return result;
    }
}

// The profile: the one there is, or a new one with its template.
internal static class AtlasContextNotes
{
    public static Task<NoteDto> ProfileAsync(IKntService service, ResultBase result) =>
        KntAtlasNotes.EnsureSingleAsync(service, KntConst.ProfileTag, "KNote Atlas: the profile of the repository (one note).",
            KntConst.ProfileTopic, AtlasTemplates.Profile, result);
}
