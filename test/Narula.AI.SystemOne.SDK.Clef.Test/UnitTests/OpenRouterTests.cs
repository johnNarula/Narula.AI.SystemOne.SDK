using Narula.AI.SystemOne.SDK.Clef.Test.Config;
using Narula.AI.SystemOne.SDK.Clef.Configuration;
using Narula.AI.SystemOne.SDK.Clef.Extensions;
using Narula.AI.SystemOne.SDK.Clef.Models;
using Narula.AI.SystemOne.SDK.Clef.Providers;
using Xunit;

namespace Narula.AI.SystemOne.SDK.Clef.Test.UnitTests;

public class OpenRouterTests
{
    private const string Json = """
        {"id":"gen-dec-1","model":"cloudflare/clef-flash","provider":"PrimeIntellect",
         "answers":{"q":{"type":"noul","noul":0.2767}},
         "usage":{"input_tokens":16384,"output_tokens":0,"cost":0.000344064}}
        """;

    [Fact]
    public async Task OpenRouter_ParsesFlatResponse_AndSendsBearer()
    {
        var path = Path.Combine(Path.GetTempPath(), $"clef-{Guid.NewGuid():N}.cfg");
        var s = new SettingsStore(path).Load() with
        {
            Provider = ProviderKind.OpenRouter,
            OpenRouter = new OpenRouterSettings { ApiKey = "k", BaseUrl = "https://example.test/api/alpha/decisions", Model = "cloudflare/clef-flash" }
        };
        var h = new FakeHandler(_ => FakeHandler.Json(Json));
        var client = ClientFactory.Create(s, new HttpClient(h));
        var a = await client.AskAsync(new TextState("state"), "ok?");
        Assert.Equal(0.2767, a.Probability, 4);
        Assert.Equal("Bearer k", h.LastRequest!.Headers.Authorization!.ToString());
        Assert.Equal("https://example.test/api/alpha/decisions", h.LastRequest.RequestUri!.ToString());
    }
}
