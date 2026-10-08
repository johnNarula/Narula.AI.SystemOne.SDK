using Narula.AI.SystemOne.SDK.Clef.Configuration;
using Narula.AI.SystemOne.SDK.Clef.Models;
using Xunit;

namespace Narula.AI.SystemOne.SDK.Clef.Test.UnitTests;

public class SettingsDefaultsTests
{
    [Fact]
    public void Defaults_Are_Sensible()
    {
        var s = new ClefSettings();
        Assert.Equal("https://api.cloudflare.com/client/v4", s.Cloudflare.BaseUrl);
        Assert.Equal("clef-flash", s.Cloudflare.Model);
        Assert.Equal("https://openrouter.ai/api/alpha/decisions", s.OpenRouter.BaseUrl);
        Assert.Equal("cloudflare/clef-flash", s.OpenRouter.Model);
        Assert.Equal("clef", s.SystemOne.Model);
        Assert.Equal(3, s.Retry.MaxAttempts);
    }

    [Fact]
    public void Validate_Cloudflare_Requires_BaseUrl_And_Model()
    {
        var s = new ClefSettings { Provider = ProviderKind.Cloudflare,
            Cloudflare = new CloudflareSettings { AccountId = "a", ApiToken = "t", BaseUrl = "", Model = "m" } };
        Assert.Throws<ClefConfigurationException>(() => s.Validate());

        s = s with { Cloudflare = new CloudflareSettings { AccountId = "a", ApiToken = "t", BaseUrl = "https://x", Model = "" } };
        Assert.Throws<ClefConfigurationException>(() => s.Validate());
    }

    [Fact]
    public void Validate_OpenRouter_Requires_Model()
    {
        var s = new ClefSettings { Provider = ProviderKind.OpenRouter,
            OpenRouter = new OpenRouterSettings { ApiKey = "k", BaseUrl = "https://x", Model = "" } };
        Assert.Throws<ClefConfigurationException>(() => s.Validate());
    }
}
