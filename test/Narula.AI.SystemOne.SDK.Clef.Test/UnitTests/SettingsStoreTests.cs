using Narula.AI.SystemOne.SDK.Clef.Configuration;
using Narula.AI.SystemOne.SDK.Clef.Models;
using Xunit;

namespace Narula.AI.SystemOne.SDK.Clef.Test.UnitTests;

public class SettingsStoreTests
{
    private static string TempPath() => Path.Combine(Path.GetTempPath(), $"clef-{Guid.NewGuid():N}.sqlite.config");

    [Fact]
    public void NewDatabase_SeedsDefaults_AndLoads()
    {
        var s = new SettingsStore(TempPath()).Load();
        Assert.Equal(ProviderKind.Cloudflare, s.Provider);
        Assert.Equal(ClefModel.ClefFlash, s.Cloudflare.Model);
        Assert.Equal(64, s.Limits.MaxQuestions);
        Assert.Equal(4, s.Limits.MaxImages);
    }

    [Fact]
    public void Set_Persists_AndSurvivesReopen_WithoutReseedOverwrite()
    {
        var path = TempPath();
        new SettingsStore(path).Set(SettingKeys.CfAccountId, "abc123");
        Assert.Equal("abc123", new SettingsStore(path).Load().Cloudflare.AccountId);
    }

    [Fact]
    public void UnknownKey_Throws() =>
        Assert.Throws<ClefConfigurationException>(() => new SettingsStore(TempPath()).Set("nope", "x"));

    [Fact]
    public void EmptyToken_FailsValidationWithClearMessage()
    {
        var ex = Assert.Throws<ClefConfigurationException>(() => new SettingsStore(TempPath()).Load().Validate());
        Assert.Contains("account_id", ex.Message);
    }
}
