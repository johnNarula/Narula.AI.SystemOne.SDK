using Narula.AI.SystemOne.SDK.Clef.Test.Config;
using System.Net;
using Narula.AI.SystemOne.SDK.Clef.Configuration;
using Narula.AI.SystemOne.SDK.Clef.Extensions;
using Narula.AI.SystemOne.SDK.Clef.Models;
using Narula.AI.SystemOne.SDK.Clef.Providers;
using Xunit;

namespace Narula.AI.SystemOne.SDK.Clef.Test.UnitTests;

public class ClientTests
{
    private const string MatchJson = """
        {"success":true,"result":{"answers":{"match":{"choice":"opt_2",
         "probabilities":{"opt_0":0.05,"opt_1":0.1,"opt_2":0.8,"opt_3":0.05},"confidence":0.77}}}}
        """;

    private static ClefSettings Settings(int attempts = 3) =>
        new SettingsStore(Path.Combine(Path.GetTempPath(), $"clef-{Guid.NewGuid():N}.cfg")).Load() with
        {
            Cloudflare = new CloudflareSettings
            { AccountId = "acc", ApiToken = "tok", BaseUrl = "https://example.test/client/v4", Model = "clef-flash" },
            Retry = new RetrySettings { MaxAttempts = attempts, BaseDelayMs = 1 }
        };

    [Fact]
    public async Task Cloudflare_BuildsUrlAndAuth_AndBestMatchRanks()
    {
        var h = new FakeHandler(_ => FakeHandler.Json(MatchJson));
        var client = new CloudflareClefClient(new HttpClient(h), Settings());

        var m = await client.BestMatchAsync("q", new[] { "a", "b", "c", "d" });

        Assert.Equal("c", m.Best);
        Assert.Equal("c", m.Ranked[0].Item);
        Assert.Equal("https://example.test/client/v4/accounts/acc/ai/run/@cf/cloudflare/clef-flash",
            h.LastRequest!.RequestUri!.ToString());
        Assert.Equal("Bearer tok", h.LastRequest.Headers.Authorization!.ToString());
    }

    [Fact]
    public async Task Retries_TransientFailures_ThenSucceeds()
    {
        var h = new FakeHandler(n => n < 3 ? FakeHandler.Json("{}", HttpStatusCode.ServiceUnavailable) : FakeHandler.Json(MatchJson));
        var client = new CloudflareClefClient(new HttpClient(h), Settings(3));
        await client.BestMatchAsync("q", new[] { "a", "b", "c", "d" });
        Assert.Equal(3, h.Calls);
    }

    [Fact]
    public async Task ClientError_IsNotRetried_AndMapsStatus()
    {
        var h = new FakeHandler(_ => FakeHandler.Json("{\"error\":\"auth\"}", HttpStatusCode.Unauthorized));
        var client = new CloudflareClefClient(new HttpClient(h), Settings(3));
        var ex = await Assert.ThrowsAsync<ClefApiException>(() => client.BestMatchAsync("q", new[] { "a", "b" }));
        Assert.Equal(401, ex.StatusCode);
        Assert.Equal(1, h.Calls);
    }

    [Fact]
    public async Task Extensions_WorkAgainstAnyProvider()
    {
        var fake = new FakeClient(ClefWire.Parse("""{"answers":{"q":{"probability":0.9},"s":{"score":1.5,"probabilities":[0.5,0.5]}}}"""));
        Assert.Equal(0.9, (await fake.AskAsync(new TextState("state"), "ok?")).Probability);
        Assert.Equal(1.5, (await fake.ScoreAsync(new TextState("state"), "rate", ["low", "high"])).Score);
    }

    [Fact]
    public void Factory_PicksProviderFromSettings()
    {
        var s = Settings();
        Assert.IsType<CloudflareClefClient>(ClientFactory.Create(s, new HttpClient()));
        var so = s with { Provider = ProviderKind.SystemOne };
        Assert.IsType<SystemOneHttpClient>(ClientFactory.Create(so, new HttpClient()));
    }
}
