using System.ComponentModel;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using KNote.Model;
using KNote.Model.Dto;
using KNote.Service.Core;
using Microsoft.Extensions.AI;

namespace KNote.Ai;

// The tools that only read (KNote Atlas, stage 1, D8): search by meaning, filter, folders, the items of a note, tasks and
// glossary. The model names folders, note types, users and attributes as people do: they are turned into their ids here,
// and a name that is missing or could be several gives back the options, for the model to choose or to ask. System notes
// and their folders are out of all of them. Results are compact (a JSON object per line) and say how many there are.
public partial class KNoteAiTools
{
    private const string FindNotesAboutName = "find_notes_about";
    private const string LookupGlossaryName = "lookup_glossary";
    private const string ViewNoteImagesName = "view_note_images";

    private const int MaxFolderLines = 200;
    private const int MaxItems = 30;
    private const int MaxOptionsShown = 30;
    private const int MaxTaskDays = 366;
    private const string DateFormat = "yyyy-MM-dd";

    private const string TaskStatusAll = "all";
    private const string TaskStatusOpen = "open";

    private static readonly string[] NoteItemKinds = { "tasks", "alarms", "resources", "relations" };

    private static readonly JsonSerializerOptions RowOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    // What depends on the repository at the moment of the question: the search by meaning needs an index that can be used
    // and the glossary some term. Fewer tools help the small models to choose well.
    public async Task<IList<AITool>> GetToolsAsync()
    {
        var hidden = new List<string>();
        if (!await CanSearchByMeaningAsync())
            hidden.Add(FindNotesAboutName);
        if (!await HasGlossaryTermsAsync())
            hidden.Add(LookupGlossaryName);
        if (!await CanActAsync())
            hidden.AddRange(AiActionRules.Kinds);
        if (_attachments.Count == 0)
            hidden.Add(AiActionRules.AttachFile);
        if (!ModelSeesImages)
            hidden.Add(ViewNoteImagesName);
        return GetTools().Where(t => !hidden.Contains(t.Name)).ToList();
    }

    private IEnumerable<AITool> GetQueryTools()
    {
        yield return AIFunctionFactory.Create(FindNotesAboutAsync, new AIFunctionFactoryOptions
        {
            Name = FindNotesAboutName,
            Description = "Searches the notes by meaning, not by exact words: it finds notes that talk about a subject even " +
                "if they use other words or another language. Use it when the user asks about a topic or an idea (\"do I have " +
                "anything about the French Revolution?\"); search_notes is for exact words, names or numbers. Returns, one JSON " +
                "object per line, the most similar notes first (at most 20): NoteNumber, Topic, Folder, Tags, dates, Similarity " +
                "(0 to 1) and Fragment, the part of the note that matched. Read a note with get_note_details before answering " +
                "about its content. The other parameters narrow the search to the notes that meet them."
        });

        yield return AIFunctionFactory.Create(FilterNotesAsync, new AIFunctionFactoryOptions
        {
            Name = "filter_notes",
            Description = "Lists the notes that meet some criteria: folder, note type, tag, user with a task in the note, " +
                "creation or modification dates, and the value of an attribute (for example, the notes of a folder modified in " +
                "September). All the criteria are optional and combined with AND. Says how many notes there are and returns, one " +
                "JSON object per line, the 20 modified last: NoteNumber, Topic, Folder, Tags and dates."
        });

        yield return AIFunctionFactory.Create(GetFolderTreeAsync, new AIFunctionFactoryOptions
        {
            Name = "get_folder_tree",
            Description = "Shows the tree of folders of the repository, or of one folder, one per line with its number. Use it " +
                "to see how the repository is organized or to find the name of a folder."
        });

        yield return AIFunctionFactory.Create(GetNoteItemsAsync, new AIFunctionFactoryOptions
        {
            Name = "get_note_items",
            Description = "Gets what a note has besides its text: its tasks, alarms, resources (attached files) and relations " +
                "with other notes."
        });

        yield return AIFunctionFactory.Create(ViewNoteImagesAsync, new AIFunctionFactoryOptions
        {
            Name = ViewNoteImagesName,
            Description = "Looks at the images attached to a note or, without a note, at the ones of this conversation, to see an " +
                $"image that the user sent earlier. You receive them in the next message, {AiAttachmentRules.MaxImagesPerLook} at most " +
                "each time; it says how many there are, and which asks for others."
        });

        yield return AIFunctionFactory.Create(GetTasksAsync, new AIFunctionFactoryOptions
        {
            Name = "get_tasks",
            Description = "Lists the tasks of the notes in a period, of everyone or of one user (\"what tasks does Marta have " +
                "this week?\"). A task is resolved when it has an end date; in_progress when it has started and pending when it " +
                "has not. Says how many there are of each status and returns, one JSON object per line, at most 20 of each one."
        });

        yield return AIFunctionFactory.Create(LookupGlossaryAsync, new AIFunctionFactoryOptions
        {
            Name = LookupGlossaryName,
            Description = "Looks up terms, acronyms or proper names in the glossary of the repository and returns what they mean."
        });
    }

    private async Task<bool> CanSearchByMeaningAsync()
    {
        try
        {
            var status = await _service.SemanticSearch.GetIndexStatusAsync();
            return status.IsValid && SemanticIndexTexts.CanSearch(status.Entity);
        }
        catch (Exception)
        {
            return false;  // The assistant goes on without this tool.
        }
    }

    private async Task<bool> HasGlossaryTermsAsync()
    {
        try
        {
            var glossary = await _service.AtlasContext.GetGlossaryInfoAsync();
            return glossary.IsValid && glossary.Entity?.Terms > 0;
        }
        catch (Exception)
        {
            return false;
        }
    }

    [Description("Searches the notes by meaning.")]
    private async Task<string> FindNotesAboutAsync(
        [Description("What the notes should talk about, in the user's words.")]
        string text,
        [Description("Only in this folder and its subfolders: its name, its path or its number.")]
        string folder = null,
        [Description("Only notes of this note type: its name.")]
        string noteType = null,
        [Description("Only notes with this tag.")]
        string tag = null,
        [Description("Only notes with a task of this user: their user name or full name.")]
        string taskUser = null,
        [Description("Created on this day or later (yyyy-MM-dd).")]
        string createdFrom = null,
        [Description("Created on this day or earlier (yyyy-MM-dd).")]
        string createdTo = null,
        [Description("Modified on this day or later (yyyy-MM-dd).")]
        string modifiedFrom = null,
        [Description("Modified on this day or earlier (yyyy-MM-dd).")]
        string modifiedTo = null,
        [Description("Only notes with this attribute (its name) with the value given in attributeValue.")]
        string attribute = null,
        [Description("The exact value of the attribute.")]
        string attributeValue = null)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "Error: text is required and cannot be empty.";

        var (filter, error) = await BuildFilterAsync(folder, noteType, tag, taskUser, createdFrom, createdTo, modifiedFrom, modifiedTo, attribute, attributeValue);
        if (error != null)
            return error;

        var (folders, foldersError) = await GetFoldersAsync();
        if (foldersError != null)
            return foldersError;

        var cut = text.Length > SemanticSearchRequestDto.MaxTextLength ? text[..SemanticSearchRequestDto.MaxTextLength] : text;
        var response = await _service.SemanticSearch.SearchAsync(new SemanticSearchRequestDto { Text = cut, Filter = filter });
        if (!response.IsValid)
            return $"Error searching notes: {response.ErrorMessage}";

        var hits = response.Entity?.Notes ?? new List<NoteSearchHitDto>();
        if (hits.Count == 0)
            return "No notes found about that.";

        var sb = new StringBuilder();
        foreach (var hit in hits.Take(MaxResults))
            sb.AppendLine(Row(new
            {
                hit.NoteNumber,
                hit.Topic,
                Folder = folders.Path(hit.FolderId),
                Tags = EmptyToNull(hit.Tags),
                Created = Day(hit.CreationDateTime),
                Modified = Day(hit.ModificationDateTime),
                Similarity = Math.Round(hit.Similarity, 2),
                hit.Fragment
            }));
        if (hits.Count > MaxResults)
            sb.AppendLine($"... and {hits.Count - MaxResults} more, less similar, not shown.");
        return sb.ToString();
    }

    [Description("Lists the notes that meet some criteria.")]
    private async Task<string> FilterNotesAsync(
        [Description("Only in this folder and its subfolders: its name, its path or its number.")]
        string folder = null,
        [Description("Only notes of this note type: its name.")]
        string noteType = null,
        [Description("Only notes with this tag.")]
        string tag = null,
        [Description("Only notes with a task of this user: their user name or full name.")]
        string taskUser = null,
        [Description("Created on this day or later (yyyy-MM-dd).")]
        string createdFrom = null,
        [Description("Created on this day or earlier (yyyy-MM-dd).")]
        string createdTo = null,
        [Description("Modified on this day or later (yyyy-MM-dd).")]
        string modifiedFrom = null,
        [Description("Modified on this day or earlier (yyyy-MM-dd).")]
        string modifiedTo = null,
        [Description("Only notes with this attribute (its name) with the value given in attributeValue.")]
        string attribute = null,
        [Description("The exact value of the attribute.")]
        string attributeValue = null)
    {
        var (filter, error) = await BuildFilterAsync(folder, noteType, tag, taskUser, createdFrom, createdTo, modifiedFrom, modifiedTo, attribute, attributeValue);
        if (error != null)
            return error;

        var (folders, foldersError) = await GetFoldersAsync();
        if (foldersError != null)
            return foldersError;

        filter.OrderColumn = nameof(NoteMinimalDto.ModificationDateTime);
        filter.OrderAscending = false;
        filter.PageIdentifier = new PageIdentifier { PageNumber = 1, PageSize = MaxResults };

        var response = await _service.Notes.GetFilterMinimalAsync(filter);
        if (!response.IsValid)
            return $"Error filtering notes: {response.ErrorMessage}";

        var notes = response.Entity ?? new List<NoteMinimalDto>();
        if (notes.Count == 0)
            return "No notes meet those criteria.";

        var total = Math.Max(response.TotalCount, notes.Count);
        var sb = new StringBuilder();
        sb.AppendLine(total > notes.Count
            ? $"{total} notes meet those criteria. These are the {notes.Count} modified last:"
            : $"{total} notes meet those criteria:");
        foreach (var note in notes)
            sb.AppendLine(Row(new
            {
                note.NoteNumber,
                note.Topic,
                Folder = folders.Path(note.FolderId),
                Tags = EmptyToNull(note.Tags),
                Created = Day(note.CreationDateTime),
                Modified = Day(note.ModificationDateTime)
            }));
        return sb.ToString();
    }

    [Description("Shows the tree of folders.")]
    private async Task<string> GetFolderTreeAsync(
        [Description("Only this folder and what is inside it: its name, its path or its number. By default, the whole tree.")]
        string folder = null)
    {
        var (folders, error) = await GetFoldersAsync();
        if (error != null)
            return error;

        Guid? start = null;
        var lines = new List<string>();
        if (!string.IsNullOrWhiteSpace(folder))
        {
            var (found, findError) = folders.Find(folder);
            if (findError != null)
                return findError;
            start = found.FolderId;
            lines.Add(FolderLine(found, 0));
        }

        AddFolderLines(folders, start, start == null ? 0 : 1, lines);
        if (lines.Count == 0)
            return "There are no folders.";

        var sb = new StringBuilder();
        foreach (var line in lines.Take(MaxFolderLines))
            sb.AppendLine(line);
        if (lines.Count > MaxFolderLines)
            sb.AppendLine($"... and {lines.Count - MaxFolderLines} more folders, not shown: ask for one folder to see what is inside it.");
        return sb.ToString();
    }

    private static void AddFolderLines(AiFolders folders, Guid? parentId, int depth, List<string> lines)
    {
        foreach (var child in folders.Children(parentId))
        {
            lines.Add(FolderLine(child, depth));
            AddFolderLines(folders, child.FolderId, depth + 1, lines);
        }
    }

    private static string FolderLine(FolderInfoDto folder, int depth) => $"{new string(' ', depth * 2)}{folder.Name} (#{folder.FolderNumber})";

    [Description("Gets the tasks, alarms, resources and relations of a note.")]
    private async Task<string> ViewNoteImagesAsync(
        [Description("The note's NoteNumber (a short integer, e.g. 123). Without it, the images of this conversation.")]
        int? noteNumber = null,
        [Description("Which images, by their position counted from 1. By default, the first ones.")]
        int[] which = null)
    {
        if (TurnImages.IsFull)
            return "Error: you have seen enough images in this answer. Answer with them, and say that you can look at more.";

        var (images, whose, error) = noteNumber is int number ? await GetNoteImagesAsync(number) : GetConversationImages();
        if (error != null)
            return error;

        var (positions, pickError) = AiAttachmentRules.PickImages(images.Count, which);
        if (pickError != null)
            return pickError;

        var seen = new List<string>();
        var failed = new List<string>();
        foreach (var position in positions)
        {
            var image = images[position];
            var content = await image.ReadAsync();
            var reduced = content == null ? null : AiImageReducer.Reduce(content.Value.Bytes, content.Value.ContentType);
            if (reduced == null)
            {
                failed.Add(image.Name);
                continue;
            }

            TurnImages.Add(image.Id, reduced.Content, reduced.ContentType);
            seen.Add($"{position + 1}. {image.Name}");
        }

        if (seen.Count == 0)
            return $"Error: the images of {whose} could not be read.";

        var notRead = failed.Count > 0 ? $" These could not be read: {string.Join(", ", failed)}." : "";
        return $"{whose} has {images.Count} images. You receive these in the next message: {string.Join(", ", seen)}.{notRead}";
    }

    private sealed record LookableImage(Guid Id, string Name, Func<Task<(byte[] Bytes, string ContentType)?>> ReadAsync);

    private (List<LookableImage> Images, string Whose, string Error) GetConversationImages()
    {
        var images = _attachments.Where(a => a.IsImage)
            .Select(a => new LookableImage(a.ResourceId, a.FileName, () => ReadAttachmentAsync(a.ResourceId)))
            .ToList();
        return images.Count > 0
            ? (images, "This conversation", null)
            : (null, null, "Error: this conversation has no images. Say which note to look at.");
    }

    private async Task<(byte[] Bytes, string ContentType)?> ReadAttachmentAsync(Guid resourceId)
    {
        var file = await _service.AiSessions.GetAttachmentAsync(_sessionNoteId, resourceId);
        return file.IsValid ? (Convert.FromBase64String(file.Entity.ContentBase64), file.Entity.ContentType) : null;
    }

    private async Task<(List<LookableImage> Images, string Whose, string Error)> GetNoteImagesAsync(int noteNumber)
    {
        var note = await _service.Notes.GetCitedNoteAsync(noteNumber);
        if (!note.IsValid)
            return (null, null, $"Error getting note: {note.ErrorMessage}");
        if (note.Entity == null)
            return (null, null, "Note not found.");

        var resources = await _service.Notes.GetResourcesInfoAsync(note.Entity.NoteId);
        if (!resources.IsValid)
            return (null, null, $"Error reading the resources of the note: {resources.ErrorMessage}");

        // The desktop app only saves the type of a few files: the extension says the rest.
        var images = (resources.Entity ?? new List<ResourceInfoDto>())
            .Where(r => AiAttachmentRules.IsImage(r.FileType) || AiAttachmentRules.IsImage(AiAttachmentRules.ContentTypeOf(r.NameOut)))
            .Select(r => new LookableImage(r.ResourceId, r.NameOut, () => ReadResourceAsync(r.ResourceId)))
            .ToList();
        return images.Count > 0
            ? (images, $"The note #{noteNumber}", null)
            : (null, null, $"The note #{noteNumber} has no images.");
    }

    private async Task<(byte[] Bytes, string ContentType)?> ReadResourceAsync(Guid resourceId)
    {
        var resource = await _service.Notes.GetResourceAsync(resourceId);
        return resource.IsValid && resource.Entity?.ContentArrayBytes?.Length > 0 ? (resource.Entity.ContentArrayBytes, resource.Entity.FileType) : null;
    }

    private async Task<string> GetNoteItemsAsync(
        [Description("The note's NoteNumber (a short integer, e.g. 123).")]
        int noteNumber,
        [Description("What to get: any of tasks, alarms, resources and relations. By default, all of them.")]
        string[] include = null)
    {
        var kinds = include?.Length > 0 ? include.Select(k => k.Trim().ToLowerInvariant()).Distinct().ToArray() : NoteItemKinds;
        var unknown = kinds.Except(NoteItemKinds).ToList();
        if (unknown.Count > 0)
            return $"Error: '{string.Join(", ", unknown)}' cannot be included. Use {string.Join(", ", NoteItemKinds)}.";

        var response = await _service.Notes.GetCitedNoteAsync(noteNumber);
        if (!response.IsValid)
            return $"Error getting note: {response.ErrorMessage}";
        if (response.Entity == null)
            return "Note not found.";

        var noteId = response.Entity.NoteId;
        var sb = new StringBuilder($"Note #{noteNumber}: {response.Entity.Topic}\n");
        foreach (var kind in kinds)
        {
            var rows = kind switch
            {
                "tasks" => await GetTaskRowsAsync(noteId),
                "alarms" => await GetAlarmRowsAsync(noteId),
                "resources" => await GetResourceRowsAsync(noteId),
                _ => await GetRelationRowsAsync(noteId)
            };

            sb.AppendLine($"{kind}: {rows.Count}");
            foreach (var row in rows.Take(MaxItems))
                sb.AppendLine(row);
            if (rows.Count > MaxItems)
                sb.AppendLine($"... and {rows.Count - MaxItems} more, not shown.");
        }
        return sb.ToString();
    }

    private async Task<List<string>> GetTaskRowsAsync(Guid noteId)
    {
        var response = await _service.Notes.GetNoteTasksAsync(noteId);
        if (!response.IsValid)
            return new List<string> { $"Error getting tasks: {response.ErrorMessage}" };

        return (response.Entity ?? new List<NoteTaskDto>()).Select(t => Row(new
        {
            Task = t.GetTopic(),
            User = t.UserFullName,
            t.Resolved,
            t.Priority,
            Start = Day(t.StartDate),
            End = Day(t.EndDate),
            ExpectedStart = Day(t.ExpectedStartDate),
            ExpectedEnd = Day(t.ExpectedEndDate)
        })).ToList();
    }

    private async Task<List<string>> GetAlarmRowsAsync(Guid noteId)
    {
        var response = await _service.Notes.GetMessagesAsync(noteId);
        if (!response.IsValid)
            return new List<string> { $"Error getting alarms: {response.ErrorMessage}" };

        return (response.Entity ?? new List<KMessageDto>()).Select(m => Row(new
        {
            Date = m.AlarmDateTime?.ToString("yyyy-MM-dd HH:mm"),
            Repeat = m.AlarmType.ToString(),
            Notification = m.NotificationType.ToString(),
            Active = m.AlarmActivated,
            User = m.UserFullName,
            // The comment of an alarm of this kind is its script: not for the model.
            Comment = m.NotificationType == EnumNotificationType.ExecuteKntScript ? null : EmptyToNull(m.Comment)
        })).ToList();
    }

    private async Task<List<string>> GetResourceRowsAsync(Guid noteId)
    {
        var response = await _service.Notes.GetResourcesInfoAsync(noteId);
        if (!response.IsValid)
            return new List<string> { $"Error getting resources: {response.ErrorMessage}" };

        return (response.Entity ?? new List<ResourceInfoDto>()).Select(r => Row(new
        {
            Name = r.NameOut ?? r.Name,
            Type = EmptyToNull(r.FileType),
            Description = EmptyToNull(r.Description)
        })).ToList();
    }

    private async Task<List<string>> GetRelationRowsAsync(Guid noteId)
    {
        var from = await _service.Notes.GetTraceNotesFromAsync(noteId);
        var to = await _service.Notes.GetTraceNotesToAsync(noteId);
        if (!from.IsValid || !to.IsValid)
            return new List<string> { $"Error getting relations: {from.ErrorMessage} {to.ErrorMessage}" };

        var types = await _service.TraceNoteTypes.GetAllAsync();
        var typeNames = types.Entity?.ToDictionary(t => t.TraceNoteTypeId, t => t.Name) ?? new Dictionary<Guid, string>();

        var relations = (from.Entity ?? new List<TraceNoteDto>()).Select(t => (Trace: t, Other: t.FromId, Direction: "from"))
            .Concat((to.Entity ?? new List<TraceNoteDto>()).Select(t => (Trace: t, Other: t.ToId, Direction: "to")))
            .Take(MaxItems);

        var rows = new List<string>();
        foreach (var (trace, other, direction) in relations)
        {
            var note = (await _service.Notes.GetAsync(other)).Entity;
            if (note == null || KntConst.IsSystemNoteType(note.NoteTypeDto?.Name))
                continue;
            rows.Add(Row(new
            {
                Direction = direction,
                Type = trace.TraceNoteTypeId is Guid typeId && typeNames.TryGetValue(typeId, out var name) ? name : null,
                note.NoteNumber,
                note.Topic
            }));
        }
        return rows;
    }

    [Description("Lists the tasks of a period.")]
    private async Task<string> GetTasksAsync(
        [Description("Only the tasks of this user: their user name or full name. By default, of everyone.")]
        string user = null,
        [Description("First day of the period (yyyy-MM-dd). With neither from nor to, the current week; with only one of them, that day.")]
        string from = null,
        [Description("Last day of the period, included (yyyy-MM-dd).")]
        string to = null,
        [Description("pending, in_progress, resolved, open (pending and in progress) or all. By default, all.")]
        string status = null,
        [Description("Only in this folder and its subfolders: its name, its path or its number.")]
        string folder = null,
        [Description("Only tasks of notes of this note type: its name.")]
        string noteType = null,
        [Description("Only tasks of notes with this tag.")]
        string tag = null,
        [Description("Only tasks with this text in their topic or description, or in the topic of their note.")]
        string text = null)
    {
        var columns = TaskColumns(status);
        if (columns == null)
            return $"Error: '{status}' is not a status. Use pending, in_progress, resolved, open or all.";

        if (!TryParseDay(from, out var first, out var error) || !TryParseDay(to, out var last, out error))
            return error;

        var today = DateTime.Today;
        var (weekStart, weekEnd) = TasksWindow.Compute(EnumTasksWindowUnit.Weeks, 1, today);
        var start = first ?? last ?? weekStart;
        var end = last ?? first ?? weekEnd;
        if (end < start)
            return "Error: the last day of the period is before the first one.";
        if ((end - start).TotalDays > MaxTaskDays)
            return $"Error: the period is too long: at most {MaxTaskDays} days.";

        var panelFilter = new TasksPanelFilterDto
        {
            WindowFrom = start,
            WindowTo = end,
            Today = today,
            Text = EmptyToNull(text),
            Tags = string.IsNullOrWhiteSpace(tag) ? new List<string>() : new List<string> { tag.Trim() },
            PageSize = MaxResults
        };

        if (!string.IsNullOrWhiteSpace(user))
        {
            var (userId, userError) = await FindUserAsync(user);
            if (userError != null)
                return userError;
            panelFilter.UserId = userId;
        }
        if (!string.IsNullOrWhiteSpace(noteType))
        {
            var (noteTypeId, noteTypeError) = await FindNoteTypeAsync(noteType);
            if (noteTypeError != null)
                return noteTypeError;
            panelFilter.NoteTypeId = noteTypeId;
        }
        if (!string.IsNullOrWhiteSpace(folder))
        {
            var (folders, foldersError) = await GetFoldersAsync();
            if (foldersError != null)
                return foldersError;
            var (found, findError) = folders.Find(folder);
            if (findError != null)
                return findError;
            panelFilter.FolderId = found.FolderId;
        }

        var response = await _service.Notes.GetTasksPanelAsync(panelFilter);
        if (!response.IsValid)
            return $"Error getting tasks: {response.ErrorMessage}";

        var panel = response.Entity;
        var sb = new StringBuilder($"Tasks from {Day(start)} to {Day(end)}: ");
        sb.AppendLine(string.Join(", ", panel.Columns.Select(c => $"{TaskStatusName(c.Column)} {c.TotalCount}")) + ".");

        foreach (var column in panel.Columns.Where(c => columns.Contains(c.Column)))
        {
            foreach (var task in column.Tasks)
                sb.AppendLine(Row(new
                {
                    task.NoteNumber,
                    Note = task.NoteTopic,
                    Task = task.GetTopic(),
                    User = task.UserFullName,
                    Status = TaskStatusName(column.Column),
                    Start = Day(task.StartDate),
                    End = Day(task.EndDate),
                    ExpectedEnd = Day(task.ExpectedEndDate)
                }));
            if (column.TotalCount > column.Tasks.Count)
                sb.AppendLine($"... and {column.TotalCount - column.Tasks.Count} more {TaskStatusName(column.Column)}, not shown.");
        }

        if (panel.Columns.All(c => c.TotalCount == 0) && panel.NearestTaskDate is DateTime nearest)
            sb.AppendLine($"The nearest task with these criteria is around {Day(nearest)}.");
        return sb.ToString();
    }

    private static EnumTaskPanelColumn[] TaskColumns(string status) => (status?.Trim().ToLowerInvariant() ?? TaskStatusAll) switch
    {
        "" or TaskStatusAll => Enum.GetValues<EnumTaskPanelColumn>(),
        TaskStatusOpen => new[] { EnumTaskPanelColumn.InProgress, EnumTaskPanelColumn.Pending },
        "pending" => new[] { EnumTaskPanelColumn.Pending },
        "in_progress" => new[] { EnumTaskPanelColumn.InProgress },
        "resolved" => new[] { EnumTaskPanelColumn.Resolved },
        _ => null
    };

    private static string TaskStatusName(EnumTaskPanelColumn column) => column switch
    {
        EnumTaskPanelColumn.Resolved => "resolved",
        EnumTaskPanelColumn.InProgress => "in_progress",
        _ => "pending"
    };

    [Description("Looks up terms in the glossary of the repository.")]
    private async Task<string> LookupGlossaryAsync(
        [Description("The terms to look up.")]
        string[] terms)
    {
        if (terms == null || terms.Length == 0)
            return "Error: give at least one term.";

        var response = await _service.AtlasContext.FindGlossaryTermsAsync(terms.ToList());
        if (!response.IsValid)
            return $"Error reading the glossary: {response.ErrorMessage}";

        return response.Entity?.Count > 0 ? GlossaryLines(response.Entity) : "None of these terms is in the glossary.";
    }

    private static string GlossaryLines(IEnumerable<GlossaryTermDto> terms)
    {
        var sb = new StringBuilder();
        foreach (var term in terms)
        {
            var variants = term.AlsoWrittenAs?.Count > 0 ? $" (also written as {string.Join(", ", term.AlsoWrittenAs)})" : "";
            sb.AppendLine($"- {term.Term}{variants}: {term.Meaning}");
        }
        return sb.ToString();
    }

    // The filter of the criteria, without the system notes, or the error for the model.
    private async Task<(NotesFilterDto Filter, string Error)> BuildFilterAsync(string folder, string noteType, string tag, string taskUser,
        string createdFrom, string createdTo, string modifiedFrom, string modifiedTo, string attribute, string attributeValue)
    {
        var filter = new NotesFilterDto
        {
            Tags = EmptyToNull(tag),
            ExcludeNoteTypeIds = await GetExcludedNoteTypeIdsAsync()
        };

        if (!TryParseDay(createdFrom, out var created1, out var error) || !TryParseDay(createdTo, out var created2, out error)
            || !TryParseDay(modifiedFrom, out var modified1, out error) || !TryParseDay(modifiedTo, out var modified2, out error))
            return (null, error);
        filter.CreatedFrom = created1;
        filter.CreatedTo = created2;
        filter.ModifiedFrom = modified1;
        filter.ModifiedTo = modified2;

        if (!string.IsNullOrWhiteSpace(folder))
        {
            var (folders, foldersError) = await GetFoldersAsync();
            if (foldersError != null)
                return (null, foldersError);
            var (found, findError) = folders.Find(folder);
            if (findError != null)
                return (null, findError);
            filter.FolderId = found.FolderId;
            filter.IncludeChildFolders = true;
        }

        if (!string.IsNullOrWhiteSpace(noteType))
        {
            var (noteTypeId, noteTypeError) = await FindNoteTypeAsync(noteType);
            if (noteTypeError != null)
                return (null, noteTypeError);
            filter.NoteTypeId = noteTypeId;
        }

        if (!string.IsNullOrWhiteSpace(taskUser))
        {
            var (userId, userError) = await FindUserAsync(taskUser);
            if (userError != null)
                return (null, userError);
            filter.TaskUserId = userId;
        }

        if (!string.IsNullOrWhiteSpace(attribute) || !string.IsNullOrWhiteSpace(attributeValue))
        {
            var (attributeFilter, attributeError) = await BuildAttributeFilterAsync(attribute, attributeValue, filter.NoteTypeId);
            if (attributeError != null)
                return (null, attributeError);
            filter.AttributesFilter.Add(attributeFilter);
        }

        return (filter, null);
    }

    private async Task<(AiFolders Folders, string Error)> GetFoldersAsync()
    {
        var response = await _service.Folders.GetAllAsync();
        return response.IsValid
            ? (new AiFolders(response.Entity ?? new List<FolderInfoDto>()), null)
            : (null, $"Error reading the folders: {response.ErrorMessage}");
    }

    private async Task<(Guid? NoteTypeId, string Error)> FindNoteTypeAsync(string name)
    {
        var response = await _service.NoteTypes.GetAllAsync();
        if (!response.IsValid)
            return (null, $"Error reading the note types: {response.ErrorMessage}");

        var types = (response.Entity ?? new List<NoteTypeDto>()).Where(t => !KntConst.IsSystemNoteType(t.Name)).ToList();
        var wanted = GlossaryTable.Normalize(name);
        var found = types.FirstOrDefault(t => GlossaryTable.Normalize(t.Name) == wanted);
        return found != null
            ? (found.NoteTypeId, null)
            : (null, $"Error: there is no note type '{name}'. The note types are: {Options(types.Select(t => t.Name))}.");
    }

    private async Task<(Guid? UserId, string Error)> FindUserAsync(string name)
    {
        var response = await _service.Users.GetAllAsync();
        if (!response.IsValid)
            return (null, $"Error reading the users: {response.ErrorMessage}");

        var users = response.Entity ?? new List<UserDto>();
        var wanted = GlossaryTable.Normalize(name);
        var matches = users.Where(u => GlossaryTable.Normalize(u.UserName) == wanted || GlossaryTable.Normalize(u.FullName) == wanted).ToList();
        if (matches.Count == 0)
            matches = users.Where(u => $" {GlossaryTable.Normalize(u.FullName)} ".Contains($" {wanted} ", StringComparison.Ordinal)).ToList();

        return matches.Count switch
        {
            1 => (matches[0].UserId, null),
            0 => (null, $"Error: there is no user '{name}'. The users are: {Options(users.Select(UserText))}."),
            _ => (null, $"Error: '{name}' could be several users: {Options(matches.Select(UserText))}. Say which one.")
        };
    }

    private static string UserText(UserDto user) => string.IsNullOrWhiteSpace(user.FullName) ? user.UserName : $"{user.FullName} ({user.UserName})";

    // The filter compares the value exactly: with a list of values, the one written in another way is corrected and one
    // that is not there gives the list.
    private async Task<(AtrFilterDto Filter, string Error)> BuildAttributeFilterAsync(string attribute, string value, Guid? noteTypeId)
    {
        if (string.IsNullOrWhiteSpace(attribute) || string.IsNullOrWhiteSpace(value))
            return (null, "Error: give both attribute and attributeValue.");

        var response = await _service.KAttributes.GetAllAsync();
        if (!response.IsValid)
            return (null, $"Error reading the attributes: {response.ErrorMessage}");

        var attributes = (response.Entity ?? new List<KAttributeInfoDto>())
            .Where(a => !KntConst.IsSystemNoteType(a.NoteTypeDto?.Name))
            .Where(a => noteTypeId == null || a.NoteTypeId == null || a.NoteTypeId == noteTypeId)
            .ToList();
        var wanted = GlossaryTable.Normalize(attribute);
        var matches = attributes.Where(a => GlossaryTable.Normalize(a.Name) == wanted).ToList();
        if (matches.Count == 0)
            return (null, $"Error: there is no attribute '{attribute}'. The attributes are: {Options(attributes.Select(a => a.Name).Distinct())}.");
        if (matches.Count > 1)
            return (null, $"Error: several note types have the attribute '{attribute}': {Options(matches.Select(a => a.NoteTypeDto?.Name ?? "(any)"))}. Give the noteType.");

        var match = matches[0];
        var filter = new AtrFilterDto { AtrId = match.KAttributeId, AtrName = match.Name, Value = value.Trim() };
        if (match.KAttributeDataType != EnumKAttributeDataType.TabulatedValue)
            return (filter, null);

        var values = await _service.KAttributes.GetKAttributeTabulatedValuesAsync(match.KAttributeId);
        if (!values.IsValid)
            return (null, $"Error reading the values of the attribute: {values.ErrorMessage}");

        var tabulated = (values.Entity ?? new List<KAttributeTabulatedValueDto>()).Select(v => v.Value).ToList();
        var normalized = GlossaryTable.Normalize(value);
        var exact = tabulated.FirstOrDefault(v => GlossaryTable.Normalize(v) == normalized);
        if (exact == null)
            return (null, $"Error: '{value}' is not a value of the attribute '{match.Name}'. Its values are: {Options(tabulated)}.");

        filter.Value = exact;
        return (filter, null);
    }

    private static string Options(IEnumerable<string> options)
    {
        var list = options.ToList();
        var shown = string.Join(", ", list.Take(MaxOptionsShown));
        return list.Count > MaxOptionsShown ? $"{shown}... ({list.Count} in all)" : shown;
    }

    private static bool TryParseDay(string text, out DateTime? day, out string error)
    {
        day = null;
        error = null;
        if (string.IsNullOrWhiteSpace(text))
            return true;

        if (DateTime.TryParseExact(text.Trim(), DateFormat, System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out var parsed))
        {
            day = parsed;
            return true;
        }

        error = $"Error: '{text}' is not a date: use yyyy-MM-dd.";
        return false;
    }

    private static string Row(object row) => JsonSerializer.Serialize(row, RowOptions);

    private static string Day(DateTime? date) => date?.ToString(DateFormat);

    private static string EmptyToNull(string text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();
}

// The folders of the repository without the system ones: their paths, as IKntFolderService.GetPathAsync writes them, and
// finding one by what the model says (number, path, end of a path or name).
internal sealed class AiFolders
{
    private const char Separator = '\\';
    private const int MaxDepth = 64;
    private const int MaxOptionsShown = 10;

    private readonly Dictionary<Guid, FolderInfoDto> _folders;

    public AiFolders(IEnumerable<FolderInfoDto> folders)
    {
        _folders = folders.GroupBy(f => f.FolderId).ToDictionary(g => g.Key, g => g.First());
    }

    public string Path(Guid folderId) => string.Join(Separator, Ancestors(folderId).Reverse().Select(f => f.Name));

    public IEnumerable<FolderInfoDto> Children(Guid? parentId) => _folders.Values
        .Where(f => f.ParentId == parentId && !IsSystem(f))
        .OrderBy(f => f.Order).ThenBy(f => f.Name);

    public (FolderInfoDto Folder, string Error) Find(string text)
    {
        var visible = _folders.Values.Where(f => !IsSystem(f)).ToList();

        if (int.TryParse(text.Trim().TrimStart('#'), out var number))
        {
            var byNumber = visible.FirstOrDefault(f => f.FolderNumber == number);
            return byNumber != null ? (byNumber, null) : (null, $"Error: there is no folder #{number}. Use get_folder_tree to see them.");
        }

        var wanted = GlossaryTable.Normalize(text).Replace('/', Separator).Trim(Separator);
        var matches = visible.Where(f =>
        {
            var path = GlossaryTable.Normalize(Path(f.FolderId));
            return path == wanted || path.EndsWith(Separator + wanted, StringComparison.Ordinal);
        }).ToList();

        if (matches.Count == 1)
            return (matches[0], null);
        if (matches.Count == 0)
            return (null, $"Error: there is no folder '{text}'. Use get_folder_tree to see them.");

        var options = string.Join("; ", matches.Take(MaxOptionsShown).Select(f => $"{Path(f.FolderId)} (#{f.FolderNumber})"));
        return (null, $"Error: '{text}' could be several folders: {options}. Say which one.");
    }

    private bool IsSystem(FolderInfoDto folder) => Ancestors(folder.FolderId).Any(f => KntConst.SystemFolderNames.Contains(f.Name));

    // The folder and its parents up to the root.
    private IEnumerable<FolderInfoDto> Ancestors(Guid folderId)
    {
        Guid? current = folderId;
        for (var depth = 0; depth < MaxDepth && current is Guid id && _folders.TryGetValue(id, out var folder); depth++)
        {
            yield return folder;
            current = folder.ParentId;
        }
    }
}
