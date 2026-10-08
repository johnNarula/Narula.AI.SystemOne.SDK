using Narula.AI.SystemOne.SDK.Clef.Configuration;
using Narula.AI.SystemOne.SDK.Clef.Extensions;
using Narula.AI.SystemOne.SDK.Clef.Models;
using Narula.AI.SystemOne.SDK.Clef.Providers;

namespace Narula.AI.SystemOne.SDK.Clef.Test;

/// <summary>
/// Runnable examples. Without --live they print the request that WOULD be sent,
/// so running the samples never spends API credits by accident.
/// </summary>
public static class ClefSamples
{
    public const string SampleQuery = "how do I keep my files synced between two linux servers";

    public static readonly string[] Descriptions =
    [
        "A guide to setting up rsync-based file synchronization and backups between machines",
        "A tutorial on building video generation pipelines on cloud GPUs",
        "A walkthrough for exposing self-hosted services securely through Cloudflare Tunnel",
        "A reference for writing n8n workflows with retries and polling"
    ];

    // ---- request builders (public so unit tests can validate them) ----
    public static DecisionRequest TextMatchRequest() =>
        DecisionRequests.BestMatch(SampleQuery, Descriptions, d => d);

    public static DecisionRequest NoulRequest() =>
        new DecisionRequest { State = "Checkout has failed for every customer for the last hour." }
            .Add("urgent", new NoulQuestion("Is this support request urgent?"));

    public static DecisionRequest ScoreRequest() =>
        new DecisionRequest { State = "The app works but the export button is slow on large files." }
            .Add("severity", new ScoreQuestion("How severe is this bug report?",
                ["Cosmetic", "Minor annoyance", "Degraded feature", "Major outage"]));

    public static async Task<DecisionRequest> ImageRequestAsync(string path) =>
        new DecisionRequest { State = "Inspect the attached image." }
            .WithImage(await ImageInput.FromFileAsync(path))
            .Add("has_text", new NoulQuestion("Does the image contain readable text?"))
            .Add("kind", new ChoiceQuestion("What is the image mostly of?",
            [
                new("photo", "A photograph of a real-world scene"),
                new("screenshot", "A screenshot of software"),
                new("document", "A scanned or photographed document"),
                new("other", "Something else")
            ]));

    public static async Task<DecisionRequest> VideoRequestAsync(IEnumerable<string> framePaths)
    {
        var frames = new List<ImageInput>();
        foreach (var p in framePaths) frames.Add(await ImageInput.FromFileAsync(p));
        return new DecisionRequest { State = "Frames from one short clip." }
            .WithVideo(new VideoInput(frames))
            .Add("moving", new NoulQuestion("Is there visible motion across the frames?"));
    }

    // ---- runner ----
    public static async Task<int> RunAsync(SettingsStore store, string[] args, bool live)
    {
        var kind = args.ElementAtOrDefault(0)?.ToLowerInvariant();
        DecisionRequest? req = kind switch
        {
            "text" => TextMatchRequest(),
            "noul" => NoulRequest(),
            "score" => ScoreRequest(),
            "image" when args.Length > 1 => await ImageRequestAsync(args[1]),
            "video" when args.Length > 1 => await VideoRequestAsync(args.Skip(1)),
            _ => null
        };
        if (req is null) { Console.WriteLine("Usage: sample text|noul|score|image <file>|video <frames...> [--live]"); return 1; }

        var settings = store.Load();
        if (!live)
        {
            var model = (req.Model ?? settings.Cloudflare.Model).ToWire();
            Console.WriteLine($"DRY RUN (add --live to call {settings.Provider}). Request body:");
            Console.WriteLine(Preview(ClefWire.BuildBody(req, model)));
            return 0;
        }

        try
        {
            var client = ClientFactory.Create(settings);
            if (kind == "text")
            {
                // The friendly one-liner: best match plus full ranking.
                var m = await client.BestMatchAsync(SampleQuery, Descriptions);
                Console.WriteLine($"Best: {m.Best}\n  p={m.Probability:P1} confidence={m.Confidence:P1}");
                foreach (var r in m.Ranked) Console.WriteLine($"  {r.Probability,6:P1}  {r.Item}");
                return 0;
            }
            var result = await client.DecideAsync(req);
            foreach (var (id, a) in result.Answers) Console.WriteLine($"{id}: {a}");
            if (result.Usage is { } u) Console.WriteLine($"tokens in={u.InputTokens} out={u.OutputTokens}");
            return 0;
        }
        catch (ClefException ex)
        {
            Console.Error.WriteLine($"{ex.GetType().Name}: {ex.Message}");
            return 2;
        }
    }

    // Images make bodies huge; keep console output readable.
    private static string Preview(string json) => json.Length <= 1500 ? json : json[..1500] + $"... ({json.Length} chars total)";
}
