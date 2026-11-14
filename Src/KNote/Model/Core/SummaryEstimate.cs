using System;
using System.Collections.Generic;
using System.Linq;

namespace KNote.Model.Core;

/// <summary>
/// What a summary takes before it starts (KNote Atlas, stage 2, D18): the calls, from the same batches the engine makes, and the
/// time, from the pace of the last summary with that model (seconds per 1,000 characters read). The notes of the batches are not
/// written yet: each one counts as the length they are asked for.
/// </summary>
public static class SummaryEstimate
{
    // About six characters per word, with its space.
    public const int CharactersPerWord = 6;

    public static int NotesCharacters => SummaryBatches.NotesWords * CharactersPerWord;

    public sealed record Plan(int Batches, int Joins, int Calls, int CharactersRead);

    /// <summary>The calls of a summary of these pieces: its batches, the joins its notes need and the final call.</summary>
    public static Plan For(IReadOnlyList<SummaryPiece> pieces, int batchCharacters, int finalCharacters)
    {
        var batches = SummaryBatches.Group(pieces, SummaryBatches.PieceLength, batchCharacters).Count;
        var read = pieces.Sum(SummaryBatches.PieceLength);

        var notes = batches;
        var joins = 0;
        while (notes > 1 && notes * NotesCharacters > finalCharacters)
        {
            var groups = SummaryBatches.Group(Enumerable.Repeat(NotesCharacters, notes).ToList(), n => n, batchCharacters).Count;
            // Notes that can't be joined any further (each one fills a call): the final call reads them as they are.
            if (groups >= notes)
                break;
            joins += groups;
            read += notes * NotesCharacters;
            notes = groups;
        }
        read += notes * NotesCharacters;

        return new Plan(batches, joins, batches + joins + 1, read);
    }

    /// <summary>The seconds of these characters with a pace; none without one.</summary>
    public static double? Seconds(int characters, double? secondsPer1000Characters) =>
        secondsPer1000Characters is double pace && pace > 0 ? Math.Round(characters / 1000.0 * pace) : null;

    /// <summary>The pace of a run: seconds per 1,000 characters read, or none if it read nothing.</summary>
    public static double? Pace(int charactersRead, double seconds) =>
        charactersRead > 0 && seconds > 0 ? seconds / (charactersRead / 1000.0) : null;
}
