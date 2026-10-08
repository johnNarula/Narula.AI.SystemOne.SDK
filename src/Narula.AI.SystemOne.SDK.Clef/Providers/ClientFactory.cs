using Narula.AI.SystemOne.SDK.Clef.Abstractions;
using Narula.AI.SystemOne.SDK.Clef.Configuration;
using Narula.AI.SystemOne.SDK.Clef.Models;

namespace Narula.AI.SystemOne.SDK.Clef.Providers;

/// <summary>Chooses the provider named in settings. The caller owns the HttpClient (lifetime, timeout, handlers).</summary>
public static class ClientFactory
{
    /// <summary>Creates the <see cref="ISystemOneClient"/> for the configured provider.</summary>
    /// <param name="settings">Validated settings; <see cref="ClefSettings.Validate"/> runs first.</param>
    /// <param name="http">Caller-owned HttpClient.</param>
    /// <exception cref="ClefConfigurationException">Settings invalid or provider unsupported.</exception>
    public static ISystemOneClient Create(ClefSettings settings, HttpClient http)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(http);
        settings.Validate();
        return settings.Provider switch
        {
            ProviderKind.Cloudflare => new CloudflareClefClient(http, settings),
            ProviderKind.OpenRouter => new OpenRouterClefClient(http, settings),
            ProviderKind.SystemOne => new SystemOneHttpClient(http, settings),
            _ => throw new ClefConfigurationException($"Unsupported provider {settings.Provider}.")
        };
    }
}
