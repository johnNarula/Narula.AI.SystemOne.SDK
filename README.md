# Narula.AI.SystemOne.SDK.Clef

Provider-neutral .NET SDK for decision models (yes/no, choice, score questions over text, images and video). The library has no config file and no hidden dependencies: you pass in the settings and the `HttpClient`.

Providers:

- **OpenRouter** (`ProviderKind.OpenRouter`) - Cloudflare Clef / Clef Flash through the OpenRouter Decisions API. Live-verified for text.
- **Cloudflare** (`ProviderKind.Cloudflare`) - Clef on Workers AI directly.
- **SystemOne** (`ProviderKind.SystemOne`) - any SystemOne-compatible HTTP server (local Clef, Jev, ...).

## Install

    dotnet add package Narula.AI.SystemOne.SDK.Clef --prerelease

## Use in code

    using Narula.AI.SystemOne.SDK.Clef.Configuration;
    using Narula.AI.SystemOne.SDK.Clef.Extensions;
    using Narula.AI.SystemOne.SDK.Clef.Providers;

    var settings = new ClefSettings
    {
        Provider = ProviderKind.OpenRouter,
        OpenRouter = new OpenRouterSettings
        {
            ApiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY")!,
            BaseUrl = "https://openrouter.ai/api/alpha/decisions",
            Model = "cloudflare/clef-flash"
        }
    };

    var http = new HttpClient { Timeout = TimeSpan.FromSeconds(settings.Http.TimeoutSeconds) };
    var client = ClientFactory.Create(settings, http);
    var best = await client.BestMatchAsync("keep files synced", descriptions);

With dependency injection (uses `IHttpClientFactory`):

    services.AddClef(settings);   // then inject ISystemOneClient

Where the settings come from (appsettings, environment, a secret store) is up to your application. `ClefSettings.Validate()` runs when the client is created and throws `ClefConfigurationException` naming the missing property. You own the `HttpClient`: set its timeout and handlers yourself when you call `ClientFactory.Create` directly.

## OpenRouter notes

- Endpoint: `POST https://openrouter.ai/api/alpha/decisions` with `Authorization: Bearer <key>`. It is an alpha API and may change.
- Body: `model`, `state`, `questions`. Responses are flat (`answers`, `usage`, plus the routed `provider`); a yes/no answer is `{"type":"noul","noul":0.57}`.
- Images go inside the top-level `state` array as `image_url` parts (Cloudflare uses a separate `images` field). One image costs roughly 16,000 input tokens.
- Clef Flash is the default (fast, cheaper); use `cloudflare/clef` for more accuracy.

## Repository layout

- `src/Narula.AI.SystemOne.SDK.Clef` - the library (`Narula.AI.SystemOne.SDK.Clef.dll`, packaged for NuGet)
- `test/Narula.AI.SystemOne.SDK.Clef.Test` - unit tests plus `ClefTest.exe`, a sample runner that keeps its settings in SQLite

## Sample runner (ClefTest.exe)

Only the sample reads a config file. It stores settings in `Clef.sqlite.config` (SQLite) next to `ClefTest.exe`, created with defaults on first run:

    dotnet run --project test/Narula.AI.SystemOne.SDK.Clef.Test -- config show
    dotnet run --project test/Narula.AI.SystemOne.SDK.Clef.Test -- config set provider openrouter
    dotnet run --project test/Narula.AI.SystemOne.SDK.Clef.Test -- config set openrouter.api_key <KEY>
    dotnet run --project test/Narula.AI.SystemOne.SDK.Clef.Test -- sample text | noul | score | image <file> | video <frames...>

Samples are dry runs by default; add `--live` to call the API. Keys are stored in plaintext (the file is chmod 600 on Unix), so keep `Clef.sqlite.config` out of git or blank the secrets before committing:

    sqlite3 Clef.sqlite.config "UPDATE settings SET value='' WHERE is_secret=1;"

## Unverified assumptions (see ClefWire.cs)

Verified live on OpenRouter: text requests, the noul/choice/score request shape, and the noul response field. Still unverified: OpenRouter image and video responses, the direct Cloudflare and SystemOne wire formats, and the score `criteria` response. Compare `DecisionResult.RawResponse` when trying them.
