using System;
using System.Collections.Generic;
using System.Linq;
using KNote.Model.Dto;

namespace KNote.Model.Core;

/// <summary>The fields of the structure of a repository that the history tells (HistoryFieldChangeDto.Field).</summary>
public static class HistoryStructureFields
{
    public const string Name = "Name";
    public const string Path = "Path";
    public const string Parent = "Parent";
    public const string Description = "Description";
    public const string Tags = "Tags";
    public const string Script = "Script";
    public const string DataType = "DataType";
    public const string Required = "Required";
    public const string Disabled = "Disabled";
    public const string NoteType = "NoteType";
    public const string UserName = "UserName";
    public const string FullName = "FullName";
    public const string Email = "Email";
    public const string Role = "Role";
    public const string Password = "Password";
    public const string Value = "Value";
    public const string EmbeddingModel = "EmbeddingModel";
    public const string EmbeddingDimensions = "EmbeddingDimensions";
    public const string QueryInstruction = "QueryInstruction";
    public const string LlmModel = "LlmModel";

    // Lists
    public const string Values = "Values";
    public const string ExcludedNoteTypes = "ExcludedNoteTypes";
    public const string ExcludedFolders = "ExcludedFolders";
}

/// <summary>How a password changed (HistoryStructureFields.Password): never the password, only this.</summary>
public static class HistoryPasswordChanges
{
    /// <summary>By its user.</summary>
    public const string Changed = "changed";
    /// <summary>By an Admin.</summary>
    public const string Set = "set";
    /// <summary>With the link of «Forgot password».</summary>
    public const string Reset = "reset";
}

/// <summary>
/// What changed in the structure of a repository (H.2b): a folder, a note type, an attribute (and its values), a type of
/// relation, a user or a setting, as StructureChangeDetailsDto; how two changes of a session add up (Merge) and the line of the
/// row (Render, with the kind of entity). Never a password nor a hash; the email of a user and the value of a generic setting go
/// without their values (H15). What only orders things (the order of an attribute, of the notes of a folder, H17) is no change.
/// </summary>
public static class StructureChangeDescriber
{
    private const string Yes = "Yes";
    private const string No = "No";

    #region Describe

    /// <summary>A folder created (before null), deleted (after null), renamed or moved: parentPath*, the path of its parent.</summary>
    public static StructureChangeDetailsDto Folder(FolderInfoDto before, FolderInfoDto after, string parentPathBefore, string parentPathAfter)
    {
        if (before == null && after == null)
            return new StructureChangeDetailsDto();
        if (before == null)
            return Created(Field(HistoryStructureFields.Path, null, parentPathAfter));
        if (after == null)
            return new StructureChangeDetailsDto { Deleted = true };

        var fields = new List<HistoryFieldChangeDto>();
        AddText(fields, HistoryStructureFields.Name, before.Name, after.Name);
        if (before.ParentId != after.ParentId)
            fields.Add(Field(HistoryStructureFields.Path, parentPathBefore, parentPathAfter));
        AddText(fields, HistoryStructureFields.Tags, before.Tags, after.Tags);
        AddPrivate(fields, HistoryStructureFields.Script, before.Script, after.Script);
        return Changed(fields);
    }

    public static StructureChangeDetailsDto NoteType(NoteTypeDto before, NoteTypeDto after, HistoryNames names)
    {
        names ??= new HistoryNames();
        if (before == null && after == null)
            return new StructureChangeDetailsDto();
        if (before == null)
            return Created();
        if (after == null)
            return new StructureChangeDetailsDto { Deleted = true };

        var fields = new List<HistoryFieldChangeDto>();
        AddText(fields, HistoryStructureFields.Name, before.Name, after.Name);
        AddText(fields, HistoryStructureFields.Description, before.Description, after.Description);
        if (before.ParenNoteTypeId != after.ParenNoteTypeId)
            fields.Add(Field(HistoryStructureFields.Parent, names.NoteType(before.ParenNoteTypeId), names.NoteType(after.ParenNoteTypeId)));
        return Changed(fields);
    }

    /// <summary>An attribute and its values (tabulated), by their ids: added, removed and renamed.</summary>
    public static StructureChangeDetailsDto KAttribute(KAttributeDto before, KAttributeDto after, HistoryNames names)
    {
        names ??= new HistoryNames();
        if (before == null && after == null)
            return new StructureChangeDetailsDto();
        if (after == null)
            return new StructureChangeDetailsDto { Deleted = true };

        var fields = new List<HistoryFieldChangeDto>();
        if (before == null)
        {
            var created = Created(Field(HistoryStructureFields.DataType, null, after.KAttributeDataType.ToString()));
            created.Lists = Lists(ValuesChange(null, after.KAttributeValues));
            return created;
        }

        AddText(fields, HistoryStructureFields.Name, before.Name, after.Name);
        AddText(fields, HistoryStructureFields.Description, before.Description, after.Description);
        if (before.KAttributeDataType != after.KAttributeDataType)
            fields.Add(Field(HistoryStructureFields.DataType, before.KAttributeDataType.ToString(), after.KAttributeDataType.ToString()));
        if (before.RequiredValue != after.RequiredValue)
            fields.Add(Field(HistoryStructureFields.Required, YesNo(before.RequiredValue), YesNo(after.RequiredValue)));
        if (before.Disabled != after.Disabled)
            fields.Add(Field(HistoryStructureFields.Disabled, YesNo(before.Disabled), YesNo(after.Disabled)));
        if (before.NoteTypeId != after.NoteTypeId)
            fields.Add(Field(HistoryStructureFields.NoteType, names.NoteType(before.NoteTypeId), names.NoteType(after.NoteTypeId)));
        AddPrivate(fields, HistoryStructureFields.Script, before.Script, after.Script);

        var details = Changed(fields);
        details.Lists = Lists(ValuesChange(before.KAttributeValues, after.KAttributeValues));
        return details;
    }

    public static StructureChangeDetailsDto TraceNoteType(TraceNoteTypeDto before, TraceNoteTypeDto after)
    {
        if (before == null && after == null)
            return new StructureChangeDetailsDto();
        if (before == null)
            return Created();
        if (after == null)
            return new StructureChangeDetailsDto { Deleted = true };

        var fields = new List<HistoryFieldChangeDto>();
        AddText(fields, HistoryStructureFields.Name, before.Name, after.Name);
        AddText(fields, HistoryStructureFields.Description, before.Description, after.Description);
        return Changed(fields);
    }

    /// <summary>A user: created with its role, its name, full name, email (without values, H15), role and whether it is disabled.</summary>
    public static StructureChangeDetailsDto User(UserDto before, UserDto after)
    {
        if (before == null && after == null)
            return new StructureChangeDetailsDto();
        if (before == null)
            return Created(Field(HistoryStructureFields.Role, null, after.RoleDefinition));
        if (after == null)
            return new StructureChangeDetailsDto { Deleted = true };

        var fields = new List<HistoryFieldChangeDto>();
        AddText(fields, HistoryStructureFields.UserName, before.UserName, after.UserName);
        AddText(fields, HistoryStructureFields.FullName, before.FullName, after.FullName);
        AddPrivate(fields, HistoryStructureFields.Email, before.EMail, after.EMail);
        AddText(fields, HistoryStructureFields.Role, before.RoleDefinition, after.RoleDefinition);
        if (before.Disabled != after.Disabled)
            fields.Add(Field(HistoryStructureFields.Disabled, YesNo(before.Disabled), YesNo(after.Disabled)));
        return Changed(fields);
    }

    /// <summary>The password of a user changed (HistoryPasswordChanges): only how, never the password.</summary>
    public static StructureChangeDetailsDto Password(string how) =>
        Changed(new List<HistoryFieldChangeDto> { new() { Field = HistoryStructureFields.Password, After = how } });

    /// <summary>A setting (a value of SystemValues) created, deleted or changed; without its values unless showValues (H15).</summary>
    public static StructureChangeDetailsDto Setting(string before, string after, bool showValues)
    {
        if (before == null && after == null)
            return new StructureChangeDetailsDto();
        if (before == null)
            return Created(showValues ? Field(HistoryStructureFields.Value, null, after) : null);
        if (after == null)
            return new StructureChangeDetailsDto { Deleted = true };

        var fields = new List<HistoryFieldChangeDto>();
        if (showValues)
            AddText(fields, HistoryStructureFields.Value, before, after);
        else
            AddPrivate(fields, HistoryStructureFields.Value, before, after);
        return Changed(fields);
    }

    /// <summary>The AI models of the repository: the models with their names, the instruction of the query without its text.</summary>
    public static StructureChangeDetailsDto AiModels(RepositoryAiModelsDto before, RepositoryAiModelsDto after)
    {
        before ??= new RepositoryAiModelsDto();
        after ??= new RepositoryAiModelsDto();
        var fields = new List<HistoryFieldChangeDto>();
        AddText(fields, HistoryStructureFields.EmbeddingModel, before.EmbeddingModel, after.EmbeddingModel);
        AddText(fields, HistoryStructureFields.EmbeddingDimensions, before.EmbeddingDimensions?.ToString(), after.EmbeddingDimensions?.ToString());
        AddPrivate(fields, HistoryStructureFields.QueryInstruction, before.QueryInstruction, after.QueryInstruction);
        AddText(fields, HistoryStructureFields.LlmModel, before.LlmModel, after.LlmModel);
        return Changed(fields);
    }

    /// <summary>The noise policy: the note types and folders that entered it or left it, by their names.</summary>
    public static StructureChangeDetailsDto NoisePolicy(NoisePolicyDto before, NoisePolicyDto after, Func<Guid, string> noteTypeName,
        Func<Guid, string> folderName)
    {
        before ??= new NoisePolicyDto();
        after ??= new NoisePolicyDto();
        var details = new StructureChangeDetailsDto();
        details.Lists = Lists(
            IdsChange(HistoryStructureFields.ExcludedNoteTypes, before.ExcludedNoteTypeIds, after.ExcludedNoteTypeIds, noteTypeName),
            IdsChange(HistoryStructureFields.ExcludedFolders, before.ExcludedFolderIds, after.ExcludedFolderIds, folderName));
        return details;
    }

    #endregion

    #region Merge

    /// <summary>
    /// Two changes of the same entity in a session as one: each field from its first value to its last (changed back to how it
    /// was is nothing), the lists added up (added and then removed is nothing), created stays created, deleted ends it.
    /// </summary>
    public static StructureChangeDetailsDto Merge(StructureChangeDetailsDto earlier, StructureChangeDetailsDto later)
    {
        if (earlier == null)
            return later ?? new StructureChangeDetailsDto();
        if (later == null)
            return earlier;

        var merged = new StructureChangeDetailsDto
        {
            Created = earlier.Created == true || later.Created == true ? true : null,
            Deleted = later.Deleted == true || earlier.Deleted == true ? true : null
        };

        var fields = (earlier.Fields ?? new()).Select(Copy).ToList();
        foreach (var change in later.Fields ?? new())
        {
            var existing = fields.FirstOrDefault(f => f.Field == change.Field);
            if (existing == null)
                fields.Add(Copy(change));
            else
                existing.After = change.After;
        }
        // Back to how it was; the private ones and the password can't tell (they have no values).
        fields.RemoveAll(f => HasValues(f) && NoteChangeRules.SameText(f.Before, f.After));
        merged.Fields = fields.Count > 0 ? fields : null;

        var lists = (earlier.Lists ?? new()).Select(Copy).ToList();
        foreach (var change in later.Lists ?? new())
        {
            var existing = lists.FirstOrDefault(l => l.Field == change.Field);
            if (existing == null)
                lists.Add(Copy(change));
            else
                MergeList(existing, change);
        }
        lists.RemoveAll(IsEmpty);
        merged.Lists = lists.Count > 0 ? lists : null;
        return merged;
    }

    private static void MergeList(HistoryListChangeDto into, HistoryListChangeDto later)
    {
        var comparer = StringComparer.OrdinalIgnoreCase;
        var added = into.Added ?? new();
        var removed = into.Removed ?? new();
        foreach (var item in later.Added ?? new())
        {
            if (removed.Contains(item, comparer))
                removed.RemoveAll(r => comparer.Equals(r, item));
            else if (!added.Contains(item, comparer))
                added.Add(item);
        }
        foreach (var item in later.Removed ?? new())
        {
            if (added.Contains(item, comparer))
                added.RemoveAll(a => comparer.Equals(a, item));
            else if (!removed.Contains(item, comparer))
                removed.Add(item);
        }
        var renamed = into.Renamed ?? new();
        foreach (var rename in later.Renamed ?? new())
        {
            var chain = renamed.FirstOrDefault(r => comparer.Equals(r.After, rename.Before));
            var addedNow = added.FindIndex(a => comparer.Equals(a, rename.Before));
            if (addedNow >= 0)
                added[addedNow] = rename.After;
            else if (chain != null)
                chain.After = rename.After;
            else
                renamed.Add(new HistoryValueChangeDto { Before = rename.Before, After = rename.After });
        }
        renamed.RemoveAll(r => comparer.Equals(r.Before, r.After));
        into.Added = added.Count > 0 ? added : null;
        into.Removed = removed.Count > 0 ? removed : null;
        into.Renamed = renamed.Count > 0 ? renamed : null;
    }

    #endregion

    #region Render

    /// <summary>The line of the row of an entity of the structure (HistoryEntities): what happened, in order.</summary>
    public static string Render(string entityName, StructureChangeDetailsDto details, int maxLength = NoteChangeDescriber.MaxMessageLength)
    {
        details ??= new StructureChangeDetailsDto();
        var kind = Kind(entityName);
        var parts = new List<string>();
        var fields = details.Fields ?? new();

        if (details.Created == true)
        {
            var path = fields.FirstOrDefault(f => f.Field == HistoryStructureFields.Path && f.Before == null)?.After;
            var role = fields.FirstOrDefault(f => f.Field == HistoryStructureFields.Role && f.Before == null)?.After;
            var dataType = fields.FirstOrDefault(f => f.Field == HistoryStructureFields.DataType && f.Before == null)?.After;
            parts.Add($"{kind} created" + (path != null ? $" in {path}" : "") + (role != null ? $" ({role})" : "")
                + (dataType != null ? $" ({dataType})" : ""));
            fields = fields.Where(f => f.Before != null || f.Field is not (HistoryStructureFields.Path or HistoryStructureFields.Role
                or HistoryStructureFields.DataType or HistoryStructureFields.Value)).ToList();
        }

        foreach (var field in fields)
            parts.Add(FieldLine(field));
        foreach (var list in details.Lists ?? new())
            parts.Add(ListLine(list));

        if (details.Deleted == true)
            parts.Add($"{kind} deleted");

        return NoteChangeDescriber.Join(parts, maxLength);
    }

    /// <summary>How people call the kind of an entity of the history.</summary>
    public static string Kind(string entityName) => entityName switch
    {
        HistoryEntities.Folder => "Folder",
        HistoryEntities.NoteType => "Note type",
        HistoryEntities.KAttribute => "Attribute",
        HistoryEntities.TraceNoteType => "Relation type",
        HistoryEntities.User => "User",
        HistoryEntities.SystemValue => "Setting",
        HistoryEntities.Note => "Note",
        _ => entityName
    };

    private static string FieldLine(HistoryFieldChangeDto field)
    {
        string Change(string what) => $"{what}: {field.Before ?? NoteChangeDescriber.NoValue} → {field.After ?? NoteChangeDescriber.NoValue}";
        return field.Field switch
        {
            HistoryStructureFields.Name => Change("Renamed"),
            HistoryStructureFields.Path => $"Moved from {field.Before ?? NoteChangeDescriber.NoValue} to {field.After ?? NoteChangeDescriber.NoValue}",
            HistoryStructureFields.Parent => Change("Parent type"),
            HistoryStructureFields.Description => "Description changed",
            HistoryStructureFields.Tags => Change("Tags"),
            HistoryStructureFields.Script => "Script changed",
            HistoryStructureFields.DataType => Change("Data type"),
            HistoryStructureFields.Required => Change("Required"),
            HistoryStructureFields.Disabled => field.After == Yes ? "Disabled" : "Enabled",
            HistoryStructureFields.NoteType => Change("Note type"),
            HistoryStructureFields.UserName => Change("User name"),
            HistoryStructureFields.FullName => Change("Full name"),
            HistoryStructureFields.Email => "Email changed",
            HistoryStructureFields.Role => Change("Role"),
            HistoryStructureFields.Password => $"Password {field.After ?? HistoryPasswordChanges.Changed}",
            HistoryStructureFields.Value => HasValues(field) ? Change("Value") : "Value changed",
            HistoryStructureFields.EmbeddingModel => Change("Embedding model"),
            HistoryStructureFields.EmbeddingDimensions => Change("Embedding dimensions"),
            HistoryStructureFields.QueryInstruction => "Query instruction changed",
            HistoryStructureFields.LlmModel => Change("Recommended LLM"),
            _ => Change(field.Field)
        };
    }

    private static string ListLine(HistoryListChangeDto list)
    {
        var what = list.Field switch
        {
            HistoryStructureFields.Values => "Values",
            HistoryStructureFields.ExcludedNoteTypes => "Excluded note types",
            HistoryStructureFields.ExcludedFolders => "Excluded folders",
            _ => list.Field
        };
        var items = (list.Added ?? new()).Select(a => "+" + a)
            .Concat((list.Removed ?? new()).Select(r => "−" + r))
            .Concat((list.Renamed ?? new()).Select(r => $"{r.Before} → {r.After}"));
        return $"{what}: {string.Join(", ", items)}";
    }

    #endregion

    #region Values

    private static HistoryListChangeDto ValuesChange(List<KAttributeTabulatedValueDto> before, List<KAttributeTabulatedValueDto> after)
    {
        var was = (before ?? new()).Where(v => v != null).ToList();
        var now = (after ?? new()).Where(v => v != null).ToList();
        var change = new HistoryListChangeDto { Field = HistoryStructureFields.Values };

        // A new value has no id yet (or one the stored ones don't have).
        var wasIds = was.Where(v => v.KAttributeTabulatedValueId != Guid.Empty).Select(v => v.KAttributeTabulatedValueId).ToHashSet();
        var nowIds = now.Where(v => v.KAttributeTabulatedValueId != Guid.Empty).Select(v => v.KAttributeTabulatedValueId).ToHashSet();
        change.Added = now.Where(v => !wasIds.Contains(v.KAttributeTabulatedValueId)).Select(v => Cut(v.Value)).ToList();
        change.Removed = was.Where(v => !nowIds.Contains(v.KAttributeTabulatedValueId)).Select(v => Cut(v.Value)).ToList();
        change.Renamed = now.Where(v => wasIds.Contains(v.KAttributeTabulatedValueId))
            .Select(v => (Before: was.First(w => w.KAttributeTabulatedValueId == v.KAttributeTabulatedValueId).Value, After: v.Value))
            .Where(p => !NoteChangeRules.SameText(p.Before, p.After))
            .Select(p => new HistoryValueChangeDto { Before = Cut(p.Before), After = Cut(p.After) })
            .ToList();
        return Clean(change);
    }

    private static HistoryListChangeDto IdsChange(string field, List<Guid> before, List<Guid> after, Func<Guid, string> name)
    {
        var was = (before ?? new()).ToHashSet();
        var now = (after ?? new()).ToHashSet();
        return Clean(new HistoryListChangeDto
        {
            Field = field,
            Added = now.Where(id => !was.Contains(id)).Select(id => Cut(name?.Invoke(id) ?? id.ToString())).ToList(),
            Removed = was.Where(id => !now.Contains(id)).Select(id => Cut(name?.Invoke(id) ?? id.ToString())).ToList()
        });
    }

    private static HistoryListChangeDto Clean(HistoryListChangeDto list)
    {
        if (list.Added?.Count == 0) list.Added = null;
        if (list.Removed?.Count == 0) list.Removed = null;
        if (list.Renamed?.Count == 0) list.Renamed = null;
        return IsEmpty(list) ? null : list;
    }

    private static bool IsEmpty(HistoryListChangeDto list) =>
        list == null || !(list.Added?.Count > 0) && !(list.Removed?.Count > 0) && !(list.Renamed?.Count > 0);

    private static List<HistoryListChangeDto> Lists(params HistoryListChangeDto[] lists)
    {
        var present = lists.Where(l => l != null).ToList();
        return present.Count > 0 ? present : null;
    }

    private static StructureChangeDetailsDto Created(params HistoryFieldChangeDto[] fields)
    {
        var present = fields.Where(f => f != null).ToList();
        return new StructureChangeDetailsDto { Created = true, Fields = present.Count > 0 ? present : null };
    }

    private static StructureChangeDetailsDto Changed(List<HistoryFieldChangeDto> fields) =>
        new() { Fields = fields.Count > 0 ? fields : null };

    private static void AddText(List<HistoryFieldChangeDto> fields, string field, string before, string after)
    {
        if (!NoteChangeRules.SameText(before, after))
            fields.Add(Field(field, before, after));
    }

    // A change told without its values (H15).
    private static void AddPrivate(List<HistoryFieldChangeDto> fields, string field, string before, string after)
    {
        if (!NoteChangeRules.SameText(before, after))
            fields.Add(new HistoryFieldChangeDto { Field = field });
    }

    // The private ones (and the password) have neither value.
    private static bool HasValues(HistoryFieldChangeDto field) =>
        field.Field != HistoryStructureFields.Password && (field.Before != null || field.After != null);

    private static HistoryFieldChangeDto Field(string field, string before, string after) => new()
    {
        Field = field,
        Before = string.IsNullOrEmpty(before) ? null : Cut(before),
        After = string.IsNullOrEmpty(after) ? null : Cut(after)
    };

    private static HistoryFieldChangeDto Copy(HistoryFieldChangeDto f) => new() { Field = f.Field, Before = f.Before, After = f.After };

    private static HistoryListChangeDto Copy(HistoryListChangeDto l) => new()
    {
        Field = l.Field,
        Added = l.Added?.ToList(),
        Removed = l.Removed?.ToList(),
        Renamed = l.Renamed?.Select(r => new HistoryValueChangeDto { Before = r.Before, After = r.After }).ToList()
    };

    private static string YesNo(bool value) => value ? Yes : No;

    private static string Cut(string text) => string.IsNullOrEmpty(text) ? text : HistoryRules.Truncate(text, NoteChangeDescriber.MaxValueLength);

    #endregion
}
