using Narula.AI.SystemOne.SDK.Clef.Test.Config;
using System.Net;
using Narula.AI.SystemOne.SDK.Clef.Configuration;
using Narula.AI.SystemOne.SDK.Clef.Extensions;
using Narula.AI.SystemOne.SDK.Clef.Models;
using Narula.AI.SystemOne.SDK.Clef.Providers;
using Xunit;

namespace Narula.AI.SystemOne.SDK.Clef.Test.UnitTests;

public class ModelAndSingleCandidateTests
{
    private static ClefSettings Settings() =>
        new SettingsStore(Path.Combine(Path.GetTempPath(), $"clef-{Guid.NewGuid():N}.cfg")).Load() with
        {
            Cloudflare = new CloudflareSettings
            { AccountId = "acc", ApiToken = "tok", BaseUrl = "https://example.test/client/v4", Model = "clef-flash" },
            OpenRouter = new OpenRouterSettings
            { ApiKey = "k", BaseUrl = "https://example.test/decisions", Model = "cloudflare/clef-flash" },
            Retry = new RetrySettings { MaxAttempts = 1, BaseDelayMs = 1 }
        };

    [Fact]
    public async Task BestMatch_SingleCandidate_ReturnsDirectly_NoHttpCall()
    {
        var h = new FakeHandler(_ => throw new InvalidOperationException("must not call HTTP"));
        var client = new CloudflareClefClient(new HttpClient(h), Settings());

        var m = await client.BestMatchAsync("q", new[] { "only" });

        Assert.Equal("only", m.Best);
        Assert.Equal(1.0, m.Probability);
        Assert.Single(m.Ranked);
        Assert.Equal(0, h.Calls);
    }

    [Fact]
    public async Task BestMatch_ZeroCandidates_Throws()
    {
        var h = new FakeHandler(_ => FakeHandler.Json("{}"));
        var client = new CloudflareClefClient(new HttpClient(h), Settings());
        await Assert.ThrowsAsync<ArgumentException>(() => client.BestMatchAsync("q", Array.Empty<string>()));
        Assert.Equal(0, h.Calls);
    }

    [Fact]
    public async Task Cloudflare_CustomModelString_InUri()
    {
        var h = new FakeHandler(_ => FakeHandler.Json(
            """{"result":{"answers":{"match":{"choice":"opt_0","probabilities":{"opt_0":1.0}}}}}"""));
        var s = Settings() with
        { Cloudflare = new CloudflareSettings
            { AccountId = "acc", ApiToken = "tok", BaseUrl = "https://example.test/client/v4", Model = "my-variant-v2" } };
        var client = new CloudflareClefClient(new HttpClient(h), s);

        await client.BestMatchAsync("q", new[] { "a", "b" });

        Assert.Equal("https://example.test/client/v4/accounts/acc/ai/run/@cf/cloudflare/my-variant-v2",
            h.LastRequest!.RequestUri!.ToString());
    }

    [Fact]
    public async Task Cloudflare_PerCallModelOverride_Wins()
    {
        var h = new FakeHandler(_ => FakeHandler.Json(
            """{"result":{"answers":{"match":{"choice":"opt_0","probabilities":{"opt_0":1.0}}}}}"""));
        var client = new CloudflareClefClient(new HttpClient(h), Settings());

        await client.BestMatchAsync("q", new[] { "a", "b" }, model: "override-model");

        Assert.Contains("@cf/cloudflare/override-model", h.LastRequest!.RequestUri!.ToString());
    }

    [Fact]
    public async Task OpenRouter_CustomModelString_InBody()
    {
        var h = new FakeHandler(_ => FakeHandler.Json(
            """{"answers":{"match":{"choice":"opt_1","probabilities":{"opt_0":0.2,"opt_1":0.8}}}}"""));
        var s = Settings() with
        { Provider = ProviderKind.OpenRouter,
          OpenRouter = new OpenRouterSettings
            { ApiKey = "k", BaseUrl = "https://example.test/decisions", Model = "some-org/custom-clef" } };
        var client = new OpenRouterClefClient(new HttpClient(h), s);

        await client.BestMatchAsync("q", new[] { "a", "b" });

        Assert.Contains("\"model\":\"some-org/custom-clef\"", h.LastBody);
    }
}
