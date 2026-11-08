using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace KNote.Model;

/// <summary>
/// Well-known, fixed set of AI providers supported by KNoteAIAssistant. Kept as plain strings
/// (not a C# enum) on AiProviderRef.Provider to follow the same convention already used by
/// RepositoryRef.Provider/Orm in this file.
/// </summary>
public static class EnumAiProvider
{
    public const string OpenAI = "OpenAI";
    public const string Anthropic = "Anthropic";
    public const string Ollama = "Ollama";

    public static readonly string[] All = { OpenAI, Anthropic, Ollama };

    // The providers that offer embedding models (Anthropic does not).
    public static readonly string[] WithEmbeddings = { OpenAI, Ollama };
}

/// <summary>
/// What a configured AI provider is used for (AiProviderRef.Kind): a chat model (the assistant and the other LLM
/// tasks) or an embedding model (KNote Atlas' semantic index). Plain strings, like EnumAiProvider.
/// </summary>
public static class EnumAiProviderKind
{
    public const string Chat = "Chat";
    public const string Embeddings = "Embeddings";

    public static readonly string[] All = { Chat, Embeddings };
}

public class AiProviderRef : SmartModelDtoBase, IAiProviderIdentity
{
    private string _alias;
    [Required(ErrorMessage = KMSG)]
    public string Alias
    {
        get { return _alias; }
        set
        {
            if (_alias != value)
            {
                _alias = value;
                OnPropertyChanged("Alias");
            }
        }
    }

    private string _provider;
    [Required(ErrorMessage = KMSG)]
    public string Provider
    {
        get { return _provider; }
        set
        {
            if (_provider != value)
            {
                _provider = value;
                OnPropertyChanged("Provider");
            }
        }
    }

    private string _model;
    [Required(ErrorMessage = KMSG)]
    public string Model
    {
        get { return _model; }
        set
        {
            if (_model != value)
            {
                _model = value;
                OnPropertyChanged("Model");
            }
        }
    }

    // Optional: when empty, AiChatClientFactory falls back to the provider's environment
    // variable (OPENAI_API_KEY / ANTHROPIC_API_KEY). Not used for Ollama.
    private string _apiKey;
    public string ApiKey
    {
        get { return _apiKey; }
        set
        {
            if (_apiKey != value)
            {
                _apiKey = value;
                OnPropertyChanged("ApiKey");
            }
        }
    }

    // Only for an Ollama chat model: the API key of an ollama.com account, used only by the Internet search of the
    // assistant (ollama.com's web search; the model still runs where Host says). A secret, like ApiKey.
    private string _webSearchApiKey;
    public string WebSearchApiKey
    {
        get { return _webSearchApiKey; }
        set
        {
            if (_webSearchApiKey != value)
            {
                _webSearchApiKey = value;
                OnPropertyChanged("WebSearchApiKey");
            }
        }
    }

    // Only required/used when Provider == EnumAiProvider.Ollama.
    private string _host;
    public string Host
    {
        get { return _host; }
        set
        {
            if (_host != value)
            {
                _host = value;
                OnPropertyChanged("Host");
            }
        }
    }

    // A chat model by default, also for the configurations saved before the property existed.
    private string _kind = EnumAiProviderKind.Chat;
    [Required(ErrorMessage = KMSG)]
    public string Kind
    {
        get { return _kind; }
        set
        {
            if (_kind != value)
            {
                _kind = value;
                OnPropertyChanged("Kind");
            }
        }
    }

    // Context size (tokens) of an Ollama chat model, sent as num_ctx; null: the model's own. Not used by the other
    // providers (their context is the provider's) nor by embedding models (they read small pieces of text).
    private int? _contextWindow;
    public int? ContextWindow
    {
        get { return _contextWindow; }
        set
        {
            if (_contextWindow != value)
            {
                _contextWindow = value;
                OnPropertyChanged("ContextWindow");
            }
        }
    }

    public const int MinContextWindow = 2048;
    public const int MaxContextWindow = 1048576;

    public bool IsChat => Kind == EnumAiProviderKind.Chat;

    public bool IsEmbeddings => Kind == EnumAiProviderKind.Embeddings;

    // A paid service: the content sent to it leaves this computer's network (KNote marks it wherever models are listed).
    public bool IsPaid => !string.IsNullOrEmpty(Provider) && Provider != EnumAiProvider.Ollama;

    // The assistant can search the Internet (KNote Atlas, chapter 23): OpenAI and Anthropic search themselves; with
    // Ollama, KNote searches with ollama.com's web search, which needs its key.
    public bool SupportsWebSearch => IsChat && (IsPaid || !string.IsNullOrWhiteSpace(WebSearchApiKey));

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var results = new List<ValidationResult>();

        // ---
        // Capture the validations implemented with attributes.
        // ---

        Validator.TryValidateProperty(this.Alias,
           new ValidationContext(this, null, null) { MemberName = "Alias" },
           results);

        Validator.TryValidateProperty(this.Provider,
           new ValidationContext(this, null, null) { MemberName = "Provider" },
           results);

        Validator.TryValidateProperty(this.Model,
           new ValidationContext(this, null, null) { MemberName = "Model" },
           results);

        // ---
        // Specific validations
        // ----

        if (Array.IndexOf(EnumAiProvider.All, Provider) < 0)
        {
            results.Add(new ValidationResult
             ($"Provider is invalid. (Supported providers are {string.Join(", ", EnumAiProvider.All)})."
             , new[] { "Provider" }));
        }

        if (Provider == EnumAiProvider.Ollama && string.IsNullOrEmpty(Host))
        {
            results.Add(new ValidationResult
             ("Host is required for the Ollama provider."
             , new[] { "Host" }));
        }

        if (Array.IndexOf(EnumAiProviderKind.All, Kind) < 0)
        {
            results.Add(new ValidationResult
             ($"Kind is invalid. (Supported kinds are {string.Join(", ", EnumAiProviderKind.All)})."
             , new[] { "Kind" }));
        }
        else if (IsEmbeddings && Array.IndexOf(EnumAiProvider.WithEmbeddings, Provider) < 0 && Array.IndexOf(EnumAiProvider.All, Provider) >= 0)
        {
            results.Add(new ValidationResult
             ($"{Provider} does not offer embedding models. (Providers with embeddings are {string.Join(", ", EnumAiProvider.WithEmbeddings)})."
             , new[] { "Kind" }));
        }

        if (ContextWindow is int contextWindow && (contextWindow < MinContextWindow || contextWindow > MaxContextWindow))
        {
            results.Add(new ValidationResult
             ($"The context size must be between {MinContextWindow} and {MaxContextWindow} tokens."
             , new[] { "ContextWindow" }));
        }

        return results;
    }
}
