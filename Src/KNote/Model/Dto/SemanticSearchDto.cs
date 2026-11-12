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

    // KNote Atlas, stage 4 (D14): All, the notes (as always); Photos or Documents, the resources of that kind
    // (SemanticSearchResultDto.Resources), found only by their own fragments.
    public EnumResourceKind ResourceKind { get; set; }

    // The filter by meaning: the text is its MeaningText.
    public static SemanticSearchRequestDto ForFilter(NotesFilterDto filter) => new() { Text = filter?.MeaningText, Filter = filter };

    public override string ToString() => $"Text: {Text}, Filter: {(Filter != null ? "yes" : "no")}, Resources: {ResourceKind}";
}

/// <summary>What a search by meaning finds (KNote Atlas, stage 4, D14): the notes, or the photos or the documents.</summary>
public enum EnumResourceKind
{
    All,
    Photos,
    Documents
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

    // KNote Atlas, stage 4 (D14): the file of the resource the fragment is of, and its page (null: not of a resource, or without
    // pages), to say «in manual.pdf, p. 12».
    public string FragmentResource { get; set; }

    public int? FragmentPage { get; set; }

    /// <summary>The fragment as the lists show it: after where it is, if it is of a resource («(in manual.pdf, p. 12) …»).</summary>
    public string FragmentShown() =>
        SemanticSearchRules.FragmentPlace(FragmentResource, FragmentPage) is string place ? $"({place}) {Fragment}" : Fragment;
}

/// <summary>
/// A photo or a document found by meaning (KNote Atlas, stage 4, D14): the resource (without its file), and, as a note found,
/// its note (number, topic...: a list of resources is shown with the lists of notes), the similarity of its best fragment and
/// that fragment with its page.
/// </summary>
public class ResourceSearchHitDto : NoteSearchHitDto
{
    public ResourceInfoDto Resource { get; set; }
}

/// <summary>
/// The notes found by meaning, the most similar first, and the notes of the index that are not up to date after the
/// search brought it up to date (D8): they may be missing or found with their previous content.
/// </summary>
public class SemanticSearchResultDto
{
    public List<NoteSearchHitDto> Notes { get; set; } = new();

    // Of the PendingNotes (4.6b): the ones the update before a search leaves for a full indexing (documents not read yet, too many
    // fragments), and the ones that can't be brought up to date here, with a document not read whose file is not in this installation.
    public int NotesForFullRun { get; set; }
    public int NotesWithoutFiles { get; set; }

    // With SemanticSearchRequestDto.ResourceKind Photos or Documents, the resources found (Notes is empty then).
    public List<ResourceSearchHitDto> Resources { get; set; } = new();

    public int PendingNotes { get; set; }
}
