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
}

public class AiProviderRef : SmartModelDtoBase
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
             ($"KMSG: Provider is invalid. (Supported providers are {string.Join(", ", EnumAiProvider.All)})."
             , new[] { "Provider" }));
        }

        if (Provider == EnumAiProvider.Ollama && string.IsNullOrEmpty(Host))
        {
            results.Add(new ValidationResult
             ("KMSG: Host is required for the Ollama provider."
             , new[] { "Host" }));
        }

        return results;
    }
}
