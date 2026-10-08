using Narula.AI.SystemOne.SDK.Clef.Models;

namespace Narula.AI.SystemOne.SDK.Clef.Configuration;

/// <summary>Which backend the SDK talks to. Exactly one is active per <see cref="ClefSettings"/>.</summary>
public enum ProviderKind
{
    /// <summary>Cloudflare Workers AI (Clef / Clef Flash) called directly.</summary>
    Cloudflare,
    /// <summary>Any SystemOne-compatible HTTP server (local Clef, Jev, ...).</summary>
    SystemOne,
    /// <summary>Cloudflare Clef / Clef Flash through the OpenRouter Decisions API.</summary>
    OpenRouter
}

/// <summary>Options for the SDK. Supplied by the caller (code, appsettings, environment, a secret store); the library never reads files.</summary>
public sealed record ClefSettings
{
    /// <summary>The active provider. Only this provider's settings section is validated and used.</summary>
    public ProviderKind Provider { get; init; }
    /// <summary>Settings for <see cref="ProviderKind.Cloudflare"/>.</summary>
    public CloudflareSettings Cloudflare { get; init; } = new();
    /// <summary>Settings for <see cref="ProviderKind.SystemOne"/>.</summary>
    public SystemOneSettings SystemOne { get; init; } = new();
    /// <summary>Settings for <see cref="ProviderKind.OpenRouter"/>.</summary>
    public OpenRouterSettings OpenRouter { get; init; } = new();
    /// <summary>HTTP behavior (timeouts).</summary>
    public HttpSettings Http { get; init; } = new();
    /// <summary>Retry behavior for transient failures.</summary>
    public RetrySettings Retry { get; init; } = new();
    /// <summary>Client-side request limits (checked before any billable call).</summary>
    public LimitSettings Limits { get; init; } = new();

    /// <summary>Fails early with a readable message instead of a 401 later.</summary>
    /// <exception cref="ClefConfigurationException">A required setting for the active provider is missing.</exception>
    public void Validate()
    {
        if (Provider == ProviderKind.Cloudflare)
        {
            if (string.IsNullOrWhiteSpace(Cloudflare.AccountId))
                throw new ClefConfigurationException($"Cloudflare.AccountId is required.");
            if (string.IsNullOrWhiteSpace(Cloudflare.ApiToken))
                throw new ClefConfigurationException($"Cloudflare.ApiToken is required.");
        }
        else if (Provider == ProviderKind.OpenRouter)
        {
            if (string.IsNullOrWhiteSpace(OpenRouter.ApiKey))
                throw new ClefConfigurationException($"OpenRouter.ApiKey is required.");
            if (string.IsNullOrWhiteSpace(OpenRouter.BaseUrl))
                throw new ClefConfigurationException($"OpenRouter.BaseUrl is required.");
        }
        else if (string.IsNullOrWhiteSpace(SystemOne.BaseUrl))
            throw new ClefConfigurationException($"SystemOne.BaseUrl is required.");
    }
}

/// <summary>Settings for calling Cloudflare Workers AI directly.</summary>
public sealed record CloudflareSettings
{
    /// <summary>Cloudflare account ID (used in the request URL).</summary>
    public string AccountId { get; init; } = "";
    /// <summary>API token with Workers AI permission. Sent as a Bearer token.</summary>
    public string ApiToken { get; init; } = "";
    /// <summary>Cloudflare API base URL (defaults to https://api.cloudflare.com/client/v4).</summary>
    public string BaseUrl { get; init; } = "";
    /// <summary>Model name appended to <c>@cf/cloudflare/</c> (e.g. "clef-flash", "clef").</summary>
    public string Model { get; init; } = "clef-flash";
}

/// <summary>Settings for a SystemOne-compatible HTTP server.</summary>
public sealed record SystemOneSettings
{
    /// <summary>Server base URL (e.g. http://localhost:11434).</summary>
    public string BaseUrl { get; init; } = "";
    /// <summary>Endpoint path appended to the base URL.</summary>
    public string Path { get; init; } = "";
    /// <summary>Optional Bearer token for the server.</summary>
    public string ApiKey { get; init; } = "";
    /// <summary>Model name sent to the server.</summary>
    public string Model { get; init; } = "";
}

/// <summary>HTTP behavior settings.</summary>
public sealed record HttpSettings
{
    /// <summary>HTTP timeout in seconds per attempt.</summary>
    public int TimeoutSeconds { get; init; } = 60;
}

/// <summary>Retry behavior for transient failures (network errors, 429, 5xx).</summary>
public sealed record RetrySettings
{
    /// <summary>Total attempts per call (1 = no retry).</summary>
    public int MaxAttempts { get; init; } = 3;
    /// <summary>Base backoff delay in ms; doubles each retry (0 = no delay).</summary>
    public int BaseDelayMs { get; init; } = 0;
}

/// <summary>Client-side request limits, checked before any billable network call.</summary>
public sealed record LimitSettings
{
    /// <summary>Max questions per request.</summary>
    public int MaxQuestions { get; init; } = int.MaxValue;
    /// <summary>Max images per request.</summary>
    public int MaxImages { get; init; } = int.MaxValue;
    /// <summary>Max bytes per image.</summary>
    public long MaxImageBytes { get; init; } = long.MaxValue;
}

/// <summary>Settings for the OpenRouter Decisions API.</summary>
public sealed record OpenRouterSettings
{
    /// <summary>OpenRouter API key. Sent as a Bearer token.</summary>
    public string ApiKey { get; init; } = "";
    /// <summary>Decisions endpoint URL (https://openrouter.ai/api/alpha/decisions).</summary>
    public string BaseUrl { get; init; } = "";
    /// <summary>Full model name (e.g. "cloudflare/clef-flash", "cloudflare/clef").</summary>
    public string Model { get; init; } = "";
}
