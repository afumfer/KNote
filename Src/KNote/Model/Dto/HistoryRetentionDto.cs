using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KNote.Model.Dto;

/// <summary>
/// How long the versions of the notes are kept (H7, H8): the system value SYSTEM/HISTORY_RETENTION (KntConst.HistoryRetentionKey),
/// as JSON. All of them for KeepAllMonths; then one a day until DailyUntilMonths; then one a month. A note bigger than
/// MaxContentSizeMB is noted without its copy, and a note keeps MaxCopiesPerNote copies at most (the oldest go first). A deleted
/// note keeps its last version DeletedNotesDays, then all its copies go. Applying it empties copies: it never deletes rows.
/// The Admin edits it (H.4): its limits are its validations.
/// </summary>
public class HistoryRetentionDto : SmartModelDtoBase
{
    public const int MaxMonths = 240;
    public const int MaxContentSizeLimitMB = 100;
    public const int MaxCopiesLimit = 10000;
    public const int MaxDeletedNotesDays = 3650;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    #region Property definitions

    private int _keepAllMonths = 3;
    [Range(0, MaxMonths)]
    public int KeepAllMonths
    {
        get { return _keepAllMonths; }
        set
        {
            if (_keepAllMonths != value)
            {
                _keepAllMonths = value;
                OnPropertyChanged("KeepAllMonths");
            }
        }
    }

    private int _dailyUntilMonths = 12;
    [Range(0, MaxMonths)]
    public int DailyUntilMonths
    {
        get { return _dailyUntilMonths; }
        set
        {
            if (_dailyUntilMonths != value)
            {
                _dailyUntilMonths = value;
                OnPropertyChanged("DailyUntilMonths");
            }
        }
    }

    private int _maxContentSizeMB = 2;
    [Range(0, MaxContentSizeLimitMB)]
    public int MaxContentSizeMB
    {
        get { return _maxContentSizeMB; }
        set
        {
            if (_maxContentSizeMB != value)
            {
                _maxContentSizeMB = value;
                OnPropertyChanged("MaxContentSizeMB");
            }
        }
    }

    private int _maxCopiesPerNote = 500;
    [Range(1, MaxCopiesLimit)]
    public int MaxCopiesPerNote
    {
        get { return _maxCopiesPerNote; }
        set
        {
            if (_maxCopiesPerNote != value)
            {
                _maxCopiesPerNote = value;
                OnPropertyChanged("MaxCopiesPerNote");
            }
        }
    }

    private int _deletedNotesDays = 30;
    [Range(0, MaxDeletedNotesDays)]
    public int DeletedNotesDays
    {
        get { return _deletedNotesDays; }
        set
        {
            if (_deletedNotesDays != value)
            {
                _deletedNotesDays = value;
                OnPropertyChanged("DeletedNotesDays");
            }
        }
    }

    [JsonIgnore]
    public long MaxContentSizeBytes => (long)MaxContentSizeMB * 1024 * 1024;

    #endregion

    /// <summary>
    /// The values that make sense, for a value read from the database (it may have been written by hand): none below its minimum,
    /// the daily period not shorter than the one with all of them.
    /// </summary>
    public HistoryRetentionDto Normalized() => new()
    {
        KeepAllMonths = Math.Max(0, KeepAllMonths),
        DailyUntilMonths = Math.Max(Math.Max(0, KeepAllMonths), DailyUntilMonths),
        MaxContentSizeMB = Math.Max(0, MaxContentSizeMB),
        MaxCopiesPerNote = Math.Max(1, MaxCopiesPerNote),
        DeletedNotesDays = Math.Max(0, DeletedNotesDays)
    };

    public string ToJson() => JsonSerializer.Serialize(this, JsonOptions);

    /// <summary>The retention a repository keeps, or the default one if it has none or it can't be read.</summary>
    public static HistoryRetentionDto FromJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new HistoryRetentionDto();
        try
        {
            return (JsonSerializer.Deserialize<HistoryRetentionDto>(json, JsonOptions) ?? new HistoryRetentionDto()).Normalized();
        }
        catch (JsonException)
        {
            return new HistoryRetentionDto();
        }
    }

    #region Validations

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var results = new List<ValidationResult>();

        foreach (var member in new[] { nameof(KeepAllMonths), nameof(DailyUntilMonths), nameof(MaxContentSizeMB), nameof(MaxCopiesPerNote), nameof(DeletedNotesDays) })
            Validator.TryValidateProperty(GetType().GetProperty(member).GetValue(this),
                new ValidationContext(this, null, null) { MemberName = member }, results);

        if (DailyUntilMonths < KeepAllMonths)
            results.Add(new ValidationResult(
                "The months with one version a day can't be fewer than the months with all of them.",
                new[] { nameof(DailyUntilMonths) }));

        return results;
    }

    #endregion
}
