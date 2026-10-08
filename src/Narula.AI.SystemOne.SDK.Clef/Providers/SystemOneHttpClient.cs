using System.Net.Http.Headers;
using Narula.AI.SystemOne.SDK.Clef.Abstractions;
using Narula.AI.SystemOne.SDK.Clef.Configuration;
using Narula.AI.SystemOne.SDK.Clef.Models;

namespace Narula.AI.SystemOne.SDK.Clef.Providers;

/// <summary>Any SystemOne-compatible server (local Clef, Jev, ...). Text, images and video frames.</summary>
public sealed class SystemOneHttpClient(HttpClient http, ClefSettings settings) : HttpDecisionClientBase(http, settings)
{
    /// <inheritdoc/>
    public override ProviderCapabilities Capabilities =>
        ProviderCapabilities.Text | ProviderCapabilities.Images | ProviderCapabilities.Videos;

    /// <inheritdoc/>
    protected override string ModelName(DecisionRequest r) => string.IsNullOrWhiteSpace(r.Model) ? Settings.SystemOne.Model : r.Model;

    /// <inheritdoc/>
    protected override Uri BuildUri(DecisionRequest r) =>
        new($"{Settings.SystemOne.BaseUrl.TrimEnd('/')}/{Settings.SystemOne.Path.TrimStart('/')}");

    /// <inheritdoc/>
    protected override void Authorize(HttpRequestMessage m)
    {
        if (!string.IsNullOrWhiteSpace(Settings.SystemOne.ApiKey))
            m.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Settings.SystemOne.ApiKey);
    }
}
