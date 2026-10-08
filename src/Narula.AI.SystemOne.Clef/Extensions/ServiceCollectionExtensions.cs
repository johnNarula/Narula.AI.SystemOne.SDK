using Microsoft.Extensions.DependencyInjection;
using Narula.AI.SystemOne.Clef.Abstractions;
using Narula.AI.SystemOne.Clef.Configuration;
using Narula.AI.SystemOne.Clef.Providers;

namespace Narula.AI.SystemOne.Clef.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>Registers settings and an <see cref="ISystemOneClient"/> chosen by the SQLite config.</summary>
    public static IServiceCollection AddClef(this IServiceCollection services, string? configPath = null)
    {
        services.AddSingleton(new SettingsStore(configPath));
        services.AddSingleton(sp => sp.GetRequiredService<SettingsStore>().Load());
        services.AddSingleton<ISystemOneClient>(sp => ClientFactory.Create(sp.GetRequiredService<ClefSettings>()));
        return services;
    }
}
