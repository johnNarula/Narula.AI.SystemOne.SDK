# Narula.AI.SystemOne.SDK.Clef

A .NET client for **decision models** — AI models that answer structured questions instead of generating text. Think "which option is best?", "is this true?", or "rate this 1–5", with probabilities attached.

The primary model is [Cloudflare Clef](https://developers.cloudflare.com/workers-ai/models/clef/), a small, fast decision model. The SDK is provider-neutral: point it at Cloudflare Workers AI, OpenRouter, or any compatible server with a settings change.

## Install

```bash
dotnet add package Narula.AI.SystemOne.SDK.Clef
```

## Pick the best match (choice)

```csharp
using Narula.AI.SystemOne.SDK.Clef.Configuration;
using Narula.AI.SystemOne.SDK.Clef.Extensions;

var settings = new ClefSettings
{
    Provider = ProviderKind.OpenRouter,
    OpenRouter = new OpenRouterSettings
    {
        ApiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY")!,
        Model = "cloudflare/clef-flash",
    },
};

using var http = new HttpClient();
var client = ClientFactory.Create(settings, http);

var voices = new[] { "warm male narrator", "bright female host", "deep movie-trailer voice" };
var best = await client.BestMatchAsync("a calm bedtime story", voices);

Console.WriteLine($"Winner: {best.Best} ({best.Probability:P0})");
foreach (var m in best.Ranked)
    Console.WriteLine($"  {m.Item}: {m.Probability:P0}");
```

## Yes/no question

```csharp
var answer = await client.AskAsync(
    state: "The server responded in 120ms with no errors.",
    question: "Is the service healthy?");

Console.WriteLine($"Healthy: {answer.Probability:P0}");
```

## Score against a rubric

```csharp
var score = await client.ScoreAsync(
    state: "The audio had background hiss and clipped peaks.",
    instructions: "Rate the audio quality.",
    levels: new[] { "unusable", "poor", "acceptable", "good", "excellent" });

Console.WriteLine($"Score: {score.Score:F1}");
```

## Providers

| Provider | Setting | Notes |
|---|---|---|
| OpenRouter | `ProviderKind.OpenRouter` | Clef via OpenRouter's Decisions API. Set `OpenRouter.ApiKey` and `OpenRouter.Model` (e.g. `cloudflare/clef-flash`). |
| Cloudflare | `ProviderKind.Cloudflare` | Workers AI directly. Set `Cloudflare.AccountId`, `Cloudflare.ApiToken`, and `Cloudflare.Model` (e.g. `clef-flash`). |
| SystemOne | `ProviderKind.SystemOne` | Any compatible HTTP server. Set `SystemOne.BaseUrl`. |

With dependency injection:

```csharp
services.AddClef(settings);   // then inject ISystemOneClient
```

`ClefSettings.Validate()` runs when the client is created and tells you exactly which setting is missing.

## Design notes

- **No config files, no hidden state.** You pass in `ClefSettings` and an `HttpClient`; the library never reads files or environment variables.
- **Fail fast.** Bad settings, bad request shapes, and unsupported media throw before any billable network call.
- **Typed answers.** Yes/no returns a probability, choice returns the winner plus a full ranking, score returns a numeric rating — all with the raw provider JSON available for debugging.

## Links

- Source: https://github.com/johnNarula/Narula.AI.SystemOne.SDK
- Cloudflare Clef: https://developers.cloudflare.com/workers-ai/models/clef/
- License: MIT
