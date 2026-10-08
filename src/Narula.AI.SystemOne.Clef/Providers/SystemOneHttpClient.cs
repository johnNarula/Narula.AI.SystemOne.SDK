using System.Net.Http.Headers;
using Narula.AI.SystemOne.Clef.Abstractions;
using Narula.AI.SystemOne.Clef.Configuration;
using Narula.AI.SystemOne.Clef.Models;

namespace Narula.AI.SystemOne.Clef.Providers;

/// <summary>Any SystemOne-compatible server (local Clef, Jev, ...). Text, images and video frames.</summary>
public sealed class SystemOneHttpClient(HttpClient http, ClefSettings settings) : HttpDecisionClientBase(http, settings)
{
    public override ProviderCapabilities Capabilities =>
        ProviderCapabilities.Text | ProviderCapabilities.Images | ProviderCapabilities.Videos;

    protected override string ModelName(DecisionRequest r) => r.Model?.ToWire() ?? Settings.SystemOne.Model;

    protected override Uri BuildUri(DecisionRequest r) =>
        new($"{Settings.SystemOne.BaseUrl.TrimEnd('/')}/{Settings.SystemOne.Path.TrimStart('/')}");

    protected override void Authorize(HttpRequestMessage m)
    {
        if (!string.IsNullOrWhiteSpace(Settings.SystemOne.ApiKey))
            m.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Settings.SystemOne.ApiKey);
    }
}
