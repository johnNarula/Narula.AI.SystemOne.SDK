using Microsoft.Extensions.DependencyInjection;
using Narula.AI.SystemOne.SDK.Clef.Abstractions;
using Narula.AI.SystemOne.SDK.Clef.Configuration;
using Narula.AI.SystemOne.SDK.Clef.Providers;

namespace Narula.AI.SystemOne.SDK.Clef.Extensions;

/// <summary>DI registration for the SDK.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>The named HttpClient used for provider calls.</summary>
    public const string HttpClientName = "Narula.AI.SystemOne.SDK.Clef";

    /// <summary>Registers <see cref="ISystemOneClient"/> using the supplied settings and IHttpClientFactory.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="settings">SDK settings (validated when the client is built).</param>
    public static IServiceCollection AddClef(this IServiceCollection services, ClefSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        services.AddHttpClient(HttpClientName, c => c.Timeout = TimeSpan.FromSeconds(settings.Http.TimeoutSeconds));
        services.AddSingleton(settings);
        services.AddTransient<ISystemOneClient>(sp =>
            ClientFactory.Create(settings, sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName)));
        return services;
    }
}
