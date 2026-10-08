using System.Net;
using System.Text;
using Narula.AI.SystemOne.Clef.Abstractions;
using Narula.AI.SystemOne.Clef.Models;

namespace Narula.AI.SystemOne.Clef.Test;

/// <summary>Returns canned HTTP responses and records what was sent.</summary>
public sealed class FakeHandler(Func<int, HttpResponseMessage> responder) : HttpMessageHandler
{
    public int Calls;
    public string? LastBody;
    public HttpRequestMessage? LastRequest;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage r, CancellationToken ct)
    {
        Calls++;
        LastRequest = r;
        LastBody = r.Content is null ? null : await r.Content.ReadAsStringAsync(ct);
        return responder(Calls);
    }

    public static HttpResponseMessage Json(string json, HttpStatusCode code = HttpStatusCode.OK) =>
        new(code) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
}

/// <summary>An in-memory provider: proves callers depend only on the interface.</summary>
public sealed class FakeClient(DecisionResult result) : ISystemOneClient
{
    public DecisionRequest? LastRequest;
    public ProviderCapabilities Capabilities => ProviderCapabilities.Text | ProviderCapabilities.Images;

    public Task<DecisionResult> DecideAsync(DecisionRequest request, CancellationToken ct = default)
    { LastRequest = request; return Task.FromResult(result); }
}
