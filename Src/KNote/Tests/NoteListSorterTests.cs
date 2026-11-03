using KNote.Model.Core;
using KNote.Model.Dto;

namespace KNote.Tests;

/// <summary>NoteListSorter: a notes list sorted in memory as the repository sorts the paged ones, and by similarity.</summary>
[TestClass]
public class NoteListSorterTests
{
    private static NoteSearchHitDto Note(int number, string topic, float similarity = 0, int priority = 0) =>
        new() { NoteId = Guid.NewGuid(), NoteNumber = number, Topic = topic, Similarity = similarity, Priority = priority };

    private static int[] Numbers(IEnumerable<NoteMinimalDto> notes) => notes.Select(n => n.NoteNumber).ToArray();

    [TestMethod]
    public void Sort_ByText_WithoutCaseOrAccents_NullsFirst()
    {
        var notes = new[] { Note(1, "Zebra"), Note(2, "árbol"), Note(3, null!), Note(4, "Barco") };

        CollectionAssert.AreEqual(new[] { 3, 2, 4, 1 }, Numbers(NoteListSorter.Sort(notes, "Topic", true)));
        CollectionAssert.AreEqual(new[] { 1, 4, 2, 3 }, Numbers(NoteListSorter.Sort(notes, "Topic", false)));
    }

    [TestMethod]
    public void Sort_BySimilarity_AndTheNoteNumberBreaksTies()
    {
        var notes = new[] { Note(3, "c", 0.5f), Note(1, "a", 0.7f), Note(2, "b", 0.5f) };

        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, Numbers(NoteListSorter.Sort(notes, NoteListSorter.SimilarityColumn, false)));
        CollectionAssert.AreEqual(new[] { 2, 3, 1 }, Numbers(NoteListSorter.Sort(notes, NoteListSorter.SimilarityColumn, true)));
    }

    [TestMethod]
    public void Sort_ByAnUnknownColumn_OrNone_KeepsTheOrder()
    {
        var notes = new[] { Note(3, "c", priority: 1), Note(1, "a", priority: 2) };

        CollectionAssert.AreEqual(new[] { 3, 1 }, Numbers(NoteListSorter.Sort(notes, null!, true)));
        CollectionAssert.AreEqual(new[] { 3, 1 }, Numbers(NoteListSorter.Sort(notes, "Description", true)));
        CollectionAssert.AreEqual(new[] { 1, 3 }, Numbers(NoteListSorter.Sort(notes, "Priority", false)));
    }
}
