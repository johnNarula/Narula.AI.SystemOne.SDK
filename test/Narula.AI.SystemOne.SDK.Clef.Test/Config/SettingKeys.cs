namespace Narula.AI.SystemOne.SDK.Clef.Test.Config;

/// <summary>Every key stored in the settings table, in one place.</summary>
public static class SettingKeys
{
    public const string Provider = "provider";
    public const string CfAccountId = "cloudflare.account_id";
    public const string CfApiToken = "cloudflare.api_token";
    public const string CfBaseUrl = "cloudflare.base_url";
    public const string CfModel = "cloudflare.model";
    public const string SoBaseUrl = "systemone.base_url";
    public const string SoPath = "systemone.path";
    public const string SoApiKey = "systemone.api_key";
    public const string SoModel = "systemone.model";
    public const string OrApiKey = "openrouter.api_key";
    public const string OrBaseUrl = "openrouter.base_url";
    public const string OrModel = "openrouter.model";
    public const string HttpTimeout = "http.timeout_seconds";
    public const string RetryMax = "retry.max_attempts";
    public const string RetryDelay = "retry.base_delay_ms";
    public const string LimQuestions = "limits.max_questions";
    public const string LimImages = "limits.max_images";
    public const string LimImageBytes = "limits.max_image_bytes";
}
