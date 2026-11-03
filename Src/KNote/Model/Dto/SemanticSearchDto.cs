using System;
using System.Collections.Generic;

namespace KNote.Model.Dto;

/// <summary>
/// How the notes are searched (KNote Atlas): by their words (the search of always, with its paging) or by their meaning
/// (IKntSemanticSearchService.SearchAsync: the most similar notes, at most SemanticSearchRules.MaxResults). The two
/// are separate: the search by words does not change.
/// </summary>
public enum EnumNotesSearchMode
{
    Words,
    Meaning
}

/// <summary>
/// A search by meaning: the text and, for the filter with «Talks about…», the filter whose notes can be found (its
/// MeaningText is the text then). Without a filter, every note of the index can be found.
/// </summary>
public class SemanticSearchRequestDto
{
    public const int MaxTextLength = 1000;

    public string Text { get; set; }

    public NotesFilterDto Filter { get; set; }

    // The filter by meaning: the text is its MeaningText.
    public static SemanticSearchRequestDto ForFilter(NotesFilterDto filter) => new() { Text = filter?.MeaningText, Filter = filter };

    public override string ToString() => $"Text: {Text}, Filter: {(Filter != null ? "yes" : "no")}";
}

/// <summary>
/// A note found by meaning: its data as in any notes list, how similar it is to the text searched (the similarity of its
/// best fragment, from 0 to 1) and the start of that fragment, to see why it was found.
/// </summary>
public class NoteSearchHitDto : NoteMinimalDto
{
    public float Similarity { get; set; }

    public string Fragment { get; set; }

    public NoteChunkSourceType FragmentSource { get; set; }
}

/// <summary>
/// The notes found by meaning, the most similar first, and the notes of the index that are not up to date after the
/// search brought it up to date (D8): they may be missing or found with their previous content.
/// </summary>
public class SemanticSearchResultDto
{
    public List<NoteSearchHitDto> Notes { get; set; } = new();

    public int PendingNotes { get; set; }
}
