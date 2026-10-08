using Narula.AI.SystemOne.Clef.Models;

namespace Narula.AI.SystemOne.Clef.Configuration;

public enum ProviderKind { Cloudflare, SystemOne }

/// <summary>Strongly typed view of the SQLite settings. Every value originates in the database.</summary>
public sealed record ClefSettings
{
    public ProviderKind Provider { get; init; }
    public CloudflareSettings Cloudflare { get; init; } = new();
    public SystemOneSettings SystemOne { get; init; } = new();
    public HttpSettings Http { get; init; } = new();
    public RetrySettings Retry { get; init; } = new();
    public LimitSettings Limits { get; init; } = new();

    /// <summary>Fails early with a readable message instead of a 401 later.</summary>
    public void Validate()
    {
        if (Provider == ProviderKind.Cloudflare)
        {
            if (string.IsNullOrWhiteSpace(Cloudflare.AccountId))
                throw new ClefConfigurationException($"Setting '{SettingKeys.CfAccountId}' is empty.");
            if (string.IsNullOrWhiteSpace(Cloudflare.ApiToken))
                throw new ClefConfigurationException($"Setting '{SettingKeys.CfApiToken}' is empty.");
        }
        else if (string.IsNullOrWhiteSpace(SystemOne.BaseUrl))
            throw new ClefConfigurationException($"Setting '{SettingKeys.SoBaseUrl}' is empty.");
    }
}

public sealed record CloudflareSettings
{
    public string AccountId { get; init; } = "";
    public string ApiToken { get; init; } = "";
    public string BaseUrl { get; init; } = "";
    public ClefModel Model { get; init; }
}

public sealed record SystemOneSettings
{
    public string BaseUrl { get; init; } = "";
    public string Path { get; init; } = "";
    public string ApiKey { get; init; } = "";
    public string Model { get; init; } = "";
}

public sealed record HttpSettings { public int TimeoutSeconds { get; init; } = 60; }

public sealed record RetrySettings
{
    public int MaxAttempts { get; init; } = 1;
    public int BaseDelayMs { get; init; } = 0;
}

public sealed record LimitSettings
{
    public int MaxQuestions { get; init; } = int.MaxValue;
    public int MaxImages { get; init; } = int.MaxValue;
    public long MaxImageBytes { get; init; } = long.MaxValue;
}
