using Narula.AI.SystemOne.SDK.Clef.Models;

namespace Narula.AI.SystemOne.SDK.Clef.Abstractions;

/// <summary>What a provider can accept as input. Used to fail fast on unsupported media.</summary>
[Flags]
public enum ProviderCapabilities
{
    /// <summary>No inputs supported.</summary>
    None = 0,
    /// <summary>Text state and text questions.</summary>
    Text = 1,
    /// <summary>Image inputs alongside text.</summary>
    Images = 2,
    /// <summary>Video (frame sequence) inputs alongside text.</summary>
    Videos = 4
}

/// <summary>
/// Provider-neutral contract. Callers depend on this only; Cloudflare Clef, a local
/// SystemOne server, or Jev are interchangeable implementations.
/// </summary>
public interface ISystemOneClient
{
    /// <summary>Which input kinds this provider accepts. Checked before any network call.</summary>
    ProviderCapabilities Capabilities { get; }

    /// <summary>Sends one decision request (a state plus 1..N typed questions) and parses the answers.</summary>
    /// <param name="request">The state, optional media, and questions to decide.</param>
    /// <param name="cancellationToken">Cancels the in-flight HTTP call.</param>
    /// <returns>The parsed answers, token usage, and the raw provider JSON.</returns>
    Task<DecisionResult> DecideAsync(DecisionRequest request, CancellationToken cancellationToken = default);
}
