using System.Diagnostics;
using KNote.Model;
using Microsoft.Extensions.AI;

namespace KNote.Ai;

// "Test" of a configured AI provider, common to ClientWin (AiProviderEditorCtrl) and KNote Local (api/local/ai/test):
// a chat model answers a minimal question; an embedding model gives the vector of a short text, with its dimensions. An
// Ollama chat model with a web search key also searches once with it (ollama.com's web search).
// The result says what happened in words a user understands (a model not downloaded, a host that does not answer...).
public static class AiProviderTester
{
    public const string ChatTestPrompt = "Reply with the single word OK.";
    public const string EmbeddingsTestText = "KNote test";

    public static async Task<Result<string>> TestAsync(AiProviderRef providerRef, CancellationToken cancellationToken = default,
        HttpClient webSearchHttpClient = null)
    {
        var result = new Result<string>();
        if (providerRef is null)
        {
            result.AddErrorMessage("There is no AI provider to test.");
            return result;
        }
        if (!providerRef.IsValid())
        {
            result.AddErrorMessage(providerRef.GetErrorMessage());
            return result;
        }

        var watch = Stopwatch.StartNew();
        try
        {
            if (providerRef.IsEmbeddings)
            {
                var generator = AiEmbeddingGeneratorFactory.CreateKntGenerator(providerRef);
                var vectors = await generator.GenerateAsync(new[] { EmbeddingsTestText }, cancellationToken);
                result.Entity = $"The embedding model answered in {watch.ElapsedMilliseconds} ms: vectors of {vectors[0].Length} dimensions.";
            }
            else
            {
                var client = AiChatClientFactory.Create(providerRef);
                var response = await client.GetResponseAsync(ChatTestPrompt, cancellationToken: cancellationToken);
                var text = (response.Text ?? "").Trim();
                if (text.Length > 80)
                    text = text[..80] + "…";
                result.Entity = $"The model answered in {watch.ElapsedMilliseconds} ms: \"{text}\".";

                if (providerRef.Provider == EnumAiProvider.Ollama && !string.IsNullOrWhiteSpace(providerRef.WebSearchApiKey))
                {
                    var error = await new OllamaWebSearch(providerRef.WebSearchApiKey, webSearchHttpClient).TestAsync(cancellationToken);
                    if (error != null)
                        result.AddErrorMessage($"{result.Entity} But the web search key does not work: {error.Replace("Error: ", "")}");
                    else
                        result.Entity += " The web search key works.";
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            result.AddErrorMessage($"The provider did not answer in {watch.Elapsed.TotalSeconds:0} seconds.");
        }
        catch (Exception ex)
        {
            result.AddErrorMessage(Explain(providerRef, ex));
        }
        return result;
    }

    // The provider's error, with what to do in the usual cases.
    internal static string Explain(AiProviderRef providerRef, Exception ex)
    {
        var message = ex.GetBaseException().Message;

        // No answer at all (connection refused, unknown host...): an HttpRequestException without a status code.
        if (Chain(ex).Any(e => e is HttpRequestException { StatusCode: null }))
            return providerRef.Provider == EnumAiProvider.Ollama
                ? $"Ollama does not answer at {providerRef.Host}: check that it is running and the address. ({message})"
                : $"The provider could not be reached: check the connection to the Internet. ({message})";

        if (message.Contains("not found", StringComparison.OrdinalIgnoreCase) && providerRef.Provider == EnumAiProvider.Ollama)
            return $"Ollama does not have the model {providerRef.Model}: download it with \"ollama pull {providerRef.Model}\". ({message})";

        if (message.Contains("401") || message.Contains("invalid x-api-key", StringComparison.OrdinalIgnoreCase)
            || message.Contains("Incorrect API key", StringComparison.OrdinalIgnoreCase))
            return $"The provider refused the API key: check it. ({message})";

        if (message.Contains("does not support", StringComparison.OrdinalIgnoreCase))
            return $"The model {providerRef.Model} cannot be used as a {(providerRef.IsEmbeddings ? "embedding" : "chat")} model. ({message})";

        return message;
    }

    private static IEnumerable<Exception> Chain(Exception ex)
    {
        for (var e = ex; e != null; e = e.InnerException)
            yield return e;
    }
}
