using Narula.AI.SystemOne.SDK.Clef.Models;

namespace Narula.AI.SystemOne.SDK.Clef.Abstractions;

/// <summary>What a provider can accept as input. Used to fail fast on unsupported media.</summary>
[Flags]
public enum ProviderCapabilities { None = 0, Text = 1, Images = 2, Videos = 4 }

/// <summary>
/// Provider-neutral contract. Callers depend on this only; Cloudflare Clef, a local
/// SystemOne server, or Jev are interchangeable implementations.
/// </summary>
public interface ISystemOneClient
{
    ProviderCapabilities Capabilities { get; }

    Task<DecisionResult> DecideAsync(DecisionRequest request, CancellationToken cancellationToken = default);
}
