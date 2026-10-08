using System.Net;
using System.Text;
using Narula.AI.SystemOne.SDK.Clef.Abstractions;
using Narula.AI.SystemOne.SDK.Clef.Configuration;
using Narula.AI.SystemOne.SDK.Clef.Models;

namespace Narula.AI.SystemOne.SDK.Clef.Providers;

/// <summary>Template-method base: validation, retry/backoff, error mapping. Subclasses supply URL, auth, model.</summary>
public abstract class HttpDecisionClientBase : ISystemOneClient
{
    private readonly HttpClient _http;
    /// <summary>Validated settings for the active provider.</summary>
    protected readonly ClefSettings Settings;

    /// <summary>Builds a provider client.</summary>
    /// <param name="http">Caller-owned HttpClient.</param>
    /// <param name="settings">Validated settings.</param>
    protected HttpDecisionClientBase(HttpClient http, ClefSettings settings) { _http = http; Settings = settings; }

    /// <inheritdoc/>
    public abstract ProviderCapabilities Capabilities { get; }
    /// <summary>Builds the request URI for one call.</summary>
    protected abstract Uri BuildUri(DecisionRequest request);
    /// <summary>Resolves the model name (per-call override wins, else configured).</summary>
    protected abstract string ModelName(DecisionRequest request);
    /// <summary>Applies auth headers to the request.</summary>
    protected abstract void Authorize(HttpRequestMessage message);
    /// <summary>True when this provider takes images inside the state array.</summary>
    protected virtual bool ImagesInState => false;

    /// <inheritdoc/>
    public async Task<DecisionResult> DecideAsync(DecisionRequest request, CancellationToken ct = default)
    {
        RequestValidator.Validate(request, Settings.Limits, Capabilities);
        var json = ClefWire.BuildBody(request, ModelName(request), ImagesInState);
        var uri = BuildUri(request);
        var max = Math.Max(1, Settings.Retry.MaxAttempts);

        for (var attempt = 1; ; attempt++)
        {
            using var msg = new HttpRequestMessage(HttpMethod.Post, uri)
            { Content = new StringContent(json, Encoding.UTF8, "application/json") };
            Authorize(msg);

            HttpResponseMessage resp;
            try { resp = await _http.SendAsync(msg, ct).ConfigureAwait(false); }
            catch (Exception ex) when (attempt < max && IsTransient(ex, ct))
            { await BackoffAsync(attempt, ct); continue; }

            using (resp)
            {
                var body = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                // 429 and 5xx are worth retrying; other 4xx are caller errors.
                var retryable = resp.StatusCode == HttpStatusCode.TooManyRequests || (int)resp.StatusCode >= 500;
                if (retryable && attempt < max) { await BackoffAsync(attempt, ct); continue; }
                if (!resp.IsSuccessStatusCode)
                    throw new ClefApiException((int)resp.StatusCode, body, $"HTTP {(int)resp.StatusCode} from provider: {body}");
                return ClefWire.Parse(body);
            }
        }
    }

    // Network errors and client timeouts (but not caller cancellation) are transient.
    private static bool IsTransient(Exception ex, CancellationToken ct) =>
        ex is HttpRequestException || (ex is TaskCanceledException && !ct.IsCancellationRequested);

    // Exponential backoff: base, 2x base, 4x base...
    private Task BackoffAsync(int attempt, CancellationToken ct) =>
        Task.Delay(TimeSpan.FromMilliseconds(Settings.Retry.BaseDelayMs * Math.Pow(2, attempt - 1)), ct);
}
