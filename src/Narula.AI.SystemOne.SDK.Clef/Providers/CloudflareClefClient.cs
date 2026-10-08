using System.Net.Http.Headers;
using Narula.AI.SystemOne.SDK.Clef.Abstractions;
using Narula.AI.SystemOne.SDK.Clef.Configuration;
using Narula.AI.SystemOne.SDK.Clef.Models;

namespace Narula.AI.SystemOne.SDK.Clef.Providers;

/// <summary>Cloudflare Workers AI. Text and images; video is rejected client-side.</summary>
public sealed class CloudflareClefClient(HttpClient http, ClefSettings settings) : HttpDecisionClientBase(http, settings)
{
    /// <inheritdoc/>
    public override ProviderCapabilities Capabilities => ProviderCapabilities.Text | ProviderCapabilities.Images;

    /// <inheritdoc/>
    protected override string ModelName(DecisionRequest r) => r.Model ?? Settings.Cloudflare.Model;

    // POST {base}/accounts/{id}/ai/run/@cf/cloudflare/{model}
    /// <inheritdoc/>
    protected override Uri BuildUri(DecisionRequest r) =>
        new($"{Settings.Cloudflare.BaseUrl.TrimEnd('/')}/accounts/{Settings.Cloudflare.AccountId}/ai/run/@cf/cloudflare/{ModelName(r)}");

    /// <inheritdoc/>
    protected override void Authorize(HttpRequestMessage m) =>
        m.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Settings.Cloudflare.ApiToken);
}
