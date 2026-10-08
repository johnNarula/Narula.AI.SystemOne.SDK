using System.Net.Http.Headers;
using Narula.AI.SystemOne.SDK.Clef.Abstractions;
using Narula.AI.SystemOne.SDK.Clef.Configuration;
using Narula.AI.SystemOne.SDK.Clef.Models;

namespace Narula.AI.SystemOne.SDK.Clef.Providers;

/// <summary>OpenRouter Decisions API (alpha). Text and images; images travel inside the state array.</summary>
public sealed class OpenRouterClefClient(HttpClient http, ClefSettings settings) : HttpDecisionClientBase(http, settings)
{
    /// <inheritdoc/>
    public override ProviderCapabilities Capabilities => ProviderCapabilities.Text | ProviderCapabilities.Images;
    /// <inheritdoc/>
    protected override bool ImagesInState => true;

    /// <inheritdoc/>
    protected override string ModelName(DecisionRequest r) =>
        string.IsNullOrWhiteSpace(r.Model) ? Settings.OpenRouter.Model : r.Model;

    /// <inheritdoc/>
    protected override Uri BuildUri(DecisionRequest r) => new(Settings.OpenRouter.BaseUrl);

    /// <inheritdoc/>
    protected override void Authorize(HttpRequestMessage m) =>
        m.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Settings.OpenRouter.ApiKey);
}
