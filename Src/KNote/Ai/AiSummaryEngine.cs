using KNote.Model;
using KNote.Model.Core;
using KNote.Model.Dto;
using Microsoft.Extensions.AI;

namespace KNote.Ai;

/// <summary>
/// The engine of the summaries of KNote Atlas (stage 2, D11): reads pieces (pages of a document, later notes) in batches that fit in
/// the model's context and writes notes of each one; joins the notes while they don't fit together («summaries of summaries»);
/// and writes the final text from them. It doesn't know where the pieces come from: what each call asks is given by the caller
/// (LongDocumentRules for a document). Tool-less calls (AiResourceTask), with the reasoning turned off; it stops between calls when
/// canceled, and nothing is kept: what it writes is only the result.
/// </summary>
public static class AiSummaryEngine
{
    /// <summary>What a summary asks the model: the request of a batch of pieces, of a join of notes and of the final text.</summary>
    public sealed record Requests(
        Func<IReadOnlyList<SummaryPiece>, string> Batch,
        Func<IReadOnlyList<string>, string> Join,
        Func<IReadOnlyList<string>, string> Final,
        int FinalOutputTokens);

    /// <summary>How it goes: the phase and the calls done of the ones planned (the joins are known as they come).</summary>
    public sealed record Step(EnumDocumentProcessPhase Phase, int CallsDone, int CallsTotal);

    /// <summary>The final text; the characters read and the calls; and the notes it was written from (a period summary writes more
    /// parts from them, 2.6).</summary>
    public sealed record Outcome(string Text, int CharactersRead, int Calls, IReadOnlyList<string> Notes);

    /// <summary>Notes joined until they fit together in a final call: the notes, the characters read and the calls of the joins.</summary>
    public sealed record Joined(List<string> Notes, int CharactersRead, int Calls);

    /// <summary>The characters of the pieces of a batch and of the final call, with this system prompt and context.</summary>
    public static (int Batch, int Final) Budgets(int contextTokens, int systemPromptCharacters, int finalOutputTokens) =>
        (SummaryBatches.InputCharacters(contextTokens, systemPromptCharacters, SummaryBatches.NotesOutputTokens),
         SummaryBatches.InputCharacters(contextTokens, systemPromptCharacters, finalOutputTokens));

    /// <summary>Why the context is too small for these budgets (a summary, a shorter one or a larger «Context size»), or null.</summary>
    public static string ContextTooSmall(int contextTokens, int batchCharacters, int finalCharacters) =>
        batchCharacters < SummaryBatches.MinInputCharacters || finalCharacters < SummaryBatches.MinInputCharacters
            ? $"The context of the model ({contextTokens} tokens) is too small for this summary: ask for a shorter one, or give its provider a larger «Context size»."
            : null;

    public static async Task<Outcome> RunAsync(IChatClient chatClient, AiProviderRef provider, string systemPrompt, int contextTokens,
        IReadOnlyList<SummaryPiece> pieces, Requests requests, Action<Step> progress, CancellationToken cancellationToken)
    {
        var (batchCharacters, finalCharacters) = Budgets(contextTokens, systemPrompt.Length, requests.FinalOutputTokens);
        var tooSmall = ContextTooSmall(contextTokens, batchCharacters, finalCharacters);
        if (tooSmall != null)
            throw new InvalidOperationException(tooSmall);

        var batches = SummaryBatches.Group(pieces, SummaryBatches.PieceLength, batchCharacters);
        var callsTotal = SummaryEstimate.For(pieces, batchCharacters, finalCharacters).Calls;
        var callsDone = 0;
        var read = 0;

        var notes = new List<string>();
        foreach (var batch in batches)
        {
            progress?.Invoke(new Step(EnumDocumentProcessPhase.Reading, callsDone, callsTotal));
            var request = requests.Batch(batch);
            var answer = await AskAsync(chatClient, provider, systemPrompt, request, SummaryBatches.NotesOutputTokens, cancellationToken);
            read += request.Length;
            callsDone++;
            if (answer.Length > 0)
                notes.Add(answer);
        }
        if (notes.Count == 0)
            throw new InvalidOperationException($"The model {provider.Model} gave no notes of the document.");

        var before = callsDone;
        var joined = await JoinAsync(chatClient, provider, systemPrompt, notes, batchCharacters, finalCharacters, requests.Join,
            (done, planned) =>
            {
                callsTotal = Math.Max(callsTotal, before + planned + 1);
                progress?.Invoke(new Step(EnumDocumentProcessPhase.Joining, before + done, callsTotal));
            }, cancellationToken);
        read += joined.CharactersRead;
        callsDone += joined.Calls;

        progress?.Invoke(new Step(EnumDocumentProcessPhase.Writing, callsDone, callsDone + 1));
        var finalRequest = requests.Final(joined.Notes);
        var text = await AskAsync(chatClient, provider, systemPrompt, finalRequest, requests.FinalOutputTokens, cancellationToken);
        read += finalRequest.Length;
        callsDone++;
        if (text.Length == 0)
            throw new InvalidOperationException($"The model {provider.Model} gave no summary.");

        return new Outcome(text, read, callsDone, joined.Notes);
    }

    /// <summary>
    /// Summaries of summaries: the notes joined in batches while they don't fit together in a final call of finalCharacters.
    /// progress: before each join, the joins done and the ones planned so far (done included).
    /// </summary>
    public static async Task<Joined> JoinAsync(IChatClient chatClient, AiProviderRef provider, string systemPrompt, IReadOnlyList<string> notes,
        int batchCharacters, int finalCharacters, Func<IReadOnlyList<string>, string> join, Action<int, int> progress, CancellationToken cancellationToken)
    {
        var current = notes.ToList();
        var read = 0;
        var calls = 0;
        while (current.Count > 1 && !SummaryBatches.Fit(current, finalCharacters))
        {
            var groups = SummaryBatches.Group(current, n => n.Length + LongDocumentRules.NotesSeparator.Length, batchCharacters);
            if (groups.Count >= current.Count)
                break;

            var planned = calls + groups.Count(g => g.Count > 1);
            var joined = new List<string>();
            foreach (var group in groups)
            {
                if (group.Count == 1)
                {
                    joined.Add(group[0]);
                    continue;
                }
                progress?.Invoke(calls, planned);
                var request = join(group);
                var answer = await AskAsync(chatClient, provider, systemPrompt, request, SummaryBatches.NotesOutputTokens, cancellationToken);
                read += request.Length;
                calls++;
                joined.Add(answer.Length > 0 ? answer : string.Join(LongDocumentRules.NotesSeparator, group));
            }
            current = joined;
        }
        return new Joined(current, read, calls);
    }

    /// <summary>One call of the summary: a request with the system prompt, without tools; its answer, trimmed.</summary>
    public static async Task<string> AskAsync(IChatClient chatClient, AiProviderRef provider, string systemPrompt, string request,
        int maxOutputTokens, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        List<ChatMessage> messages = [new(ChatRole.System, systemPrompt), new(ChatRole.User, request)];
        var answer = await AiResourceTask.AnswerAsync(chatClient, provider, messages, cancellationToken, maxOutputTokens);
        return answer.Trim();
    }
}
