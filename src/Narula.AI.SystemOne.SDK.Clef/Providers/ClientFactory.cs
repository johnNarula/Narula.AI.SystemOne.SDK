using Narula.AI.SystemOne.SDK.Clef.Abstractions;
using Narula.AI.SystemOne.SDK.Clef.Configuration;
using Narula.AI.SystemOne.SDK.Clef.Models;

namespace Narula.AI.SystemOne.SDK.Clef.Providers;

/// <summary>Chooses the provider named in settings. Callers never reference a concrete client.</summary>
public static class ClientFactory
{
    public static ISystemOneClient Create(ClefSettings settings, HttpClient? http = null)
    {
        settings.Validate();
        http ??= new HttpClient { Timeout = TimeSpan.FromSeconds(settings.Http.TimeoutSeconds) };
        return settings.Provider switch
        {
            ProviderKind.Cloudflare => new CloudflareClefClient(http, settings),
            ProviderKind.SystemOne => new SystemOneHttpClient(http, settings),
            _ => throw new ClefConfigurationException($"Unsupported provider {settings.Provider}.")
        };
    }

    public static ISystemOneClient Create(SettingsStore store) => Create(store.Load());
}
