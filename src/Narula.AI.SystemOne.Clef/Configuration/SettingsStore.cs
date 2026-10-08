using System.Globalization;
using Microsoft.Data.Sqlite;
using Narula.AI.SystemOne.Clef.Models;

namespace Narula.AI.SystemOne.Clef.Configuration;

public sealed record SettingRow(string Key, string Value, string Description, bool IsSecret);

/// <summary>
/// SQLite-backed settings. The file lives next to the executable by default.
/// Defaults are seeded once (INSERT OR IGNORE), so user edits are never overwritten.
/// </summary>
public sealed class SettingsStore
{
    public const string FileName = "Clef.sqlite.config";

    // Seed values. They exist only to populate a brand-new database; runtime reads the DB.
    private static readonly SettingRow[] Defaults =
    [
        new(SettingKeys.Provider, "cloudflare", "Active provider: cloudflare | systemone", false),
        new(SettingKeys.CfAccountId, "", "Cloudflare account id", false),
        new(SettingKeys.CfApiToken, "", "Cloudflare API token (stored in plaintext; file is chmod 600 on Unix)", true),
        new(SettingKeys.CfBaseUrl, "https://api.cloudflare.com/client/v4", "Cloudflare API base URL", false),
        new(SettingKeys.CfModel, "clef-flash", "clef | clef-flash", false),
        new(SettingKeys.SoBaseUrl, "http://localhost:11434", "SystemOne-compatible server base URL", false),
        new(SettingKeys.SoPath, "/v1/systemone", "SystemOne endpoint path", false),
        new(SettingKeys.SoApiKey, "", "Optional bearer key for the SystemOne server", true),
        new(SettingKeys.SoModel, "clef", "Model name sent to the SystemOne server", false),
        new(SettingKeys.HttpTimeout, "60", "HTTP timeout in seconds", false),
        new(SettingKeys.RetryMax, "3", "Total attempts per call (1 = no retry)", false),
        new(SettingKeys.RetryDelay, "500", "Base backoff delay in ms (doubles each retry)", false),
        new(SettingKeys.LimQuestions, "64", "Max questions per request (client-side check)", false),
        new(SettingKeys.LimImages, "4", "Max images per request (client-side check)", false),
        new(SettingKeys.LimImageBytes, "4194304", "Max bytes per image (client-side check)", false),
    ];

    public SettingsStore(string? filePath = null)
    {
        FilePath = filePath ?? System.IO.Path.Combine(AppContext.BaseDirectory, FileName);
        EnsureCreated();
    }

    public string FilePath { get; }

    private SqliteConnection Open()
    {
        var cs = new SqliteConnectionStringBuilder
        {
            DataSource = FilePath, Mode = SqliteOpenMode.ReadWriteCreate, Pooling = false
        }.ToString();
        var c = new SqliteConnection(cs);
        c.Open();
        return c;
    }

    private void EnsureCreated()
    {
        using var c = Open();
        using (var cmd = c.CreateCommand())
        {
            cmd.CommandText = """
                CREATE TABLE IF NOT EXISTS settings(
                    key TEXT PRIMARY KEY, value TEXT NOT NULL,
                    description TEXT NOT NULL, is_secret INTEGER NOT NULL);
                """;
            cmd.ExecuteNonQuery();
        }
        foreach (var d in Defaults)
        {
            using var cmd = c.CreateCommand();
            cmd.CommandText = "INSERT OR IGNORE INTO settings(key,value,description,is_secret) VALUES($k,$v,$d,$s)";
            cmd.Parameters.AddWithValue("$k", d.Key);
            cmd.Parameters.AddWithValue("$v", d.Value);
            cmd.Parameters.AddWithValue("$d", d.Description);
            cmd.Parameters.AddWithValue("$s", d.IsSecret ? 1 : 0);
            cmd.ExecuteNonQuery();
        }
        // The file may hold a token: keep it owner-only where the OS supports it.
        if (!OperatingSystem.IsWindows())
            File.SetUnixFileMode(FilePath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
    }

    public IReadOnlyList<SettingRow> GetAll()
    {
        using var c = Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT key,value,description,is_secret FROM settings ORDER BY key";
        using var r = cmd.ExecuteReader();
        var rows = new List<SettingRow>();
        while (r.Read()) rows.Add(new(r.GetString(0), r.GetString(1), r.GetString(2), r.GetInt32(3) == 1));
        return rows;
    }

    public void Set(string key, string value)
    {
        using var c = Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "UPDATE settings SET value=$v WHERE key=$k";
        cmd.Parameters.AddWithValue("$k", key);
        cmd.Parameters.AddWithValue("$v", value);
        if (cmd.ExecuteNonQuery() == 0)
            throw new ClefConfigurationException($"Unknown setting '{key}'. Run 'config show' to list keys.");
    }

    /// <summary>Reads the table into the typed settings object.</summary>
    public ClefSettings Load()
    {
        var all = GetAll().ToDictionary(r => r.Key, r => r.Value);
        string S(string k) => all.TryGetValue(k, out var v) ? v : "";
        int I(string k) => int.Parse(S(k), CultureInfo.InvariantCulture);
        long L(string k) => long.Parse(S(k), CultureInfo.InvariantCulture);

        return new ClefSettings
        {
            Provider = Enum.Parse<ProviderKind>(S(SettingKeys.Provider), ignoreCase: true),
            Cloudflare = new CloudflareSettings
            {
                AccountId = S(SettingKeys.CfAccountId), ApiToken = S(SettingKeys.CfApiToken),
                BaseUrl = S(SettingKeys.CfBaseUrl), Model = EnumWire.ParseModel(S(SettingKeys.CfModel))
            },
            SystemOne = new SystemOneSettings
            {
                BaseUrl = S(SettingKeys.SoBaseUrl), Path = S(SettingKeys.SoPath),
                ApiKey = S(SettingKeys.SoApiKey), Model = S(SettingKeys.SoModel)
            },
            Http = new HttpSettings { TimeoutSeconds = I(SettingKeys.HttpTimeout) },
            Retry = new RetrySettings { MaxAttempts = I(SettingKeys.RetryMax), BaseDelayMs = I(SettingKeys.RetryDelay) },
            Limits = new LimitSettings
            {
                MaxQuestions = I(SettingKeys.LimQuestions), MaxImages = I(SettingKeys.LimImages),
                MaxImageBytes = L(SettingKeys.LimImageBytes)
            }
        };
    }
}
