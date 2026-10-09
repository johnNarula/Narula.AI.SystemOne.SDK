using System.Text.Json;
using System.Text.Json.Nodes;
using Narula.AI.SystemOne.SDK.Clef.Models;

namespace Narula.AI.SystemOne.SDK.Clef.Providers;

/// <summary>
/// The only place that knows the JSON wire format. If the real API differs from the
/// ASSUMPTIONS marked below, this is the one file to adjust.
/// </summary>
public static class ClefWire
{
    /// <summary>Serializes a decision request to the provider JSON body.</summary>
    /// <param name="req">The request.</param>
    /// <param name="modelName">Model name for the "model" field.</param>
    /// <param name="imagesInState">True when images travel inside the state array (OpenRouter).</param>
    public static string BuildBody(DecisionRequest req, string modelName, bool imagesInState = false)
    {
        var questions = new JsonObject();
        foreach (var (id, q) in req.Questions) questions[id] = BuildQuestion(q);

        var body = new JsonObject { ["model"] = modelName };
        if (imagesInState && req.Images.Count > 0)
        {
            // OpenRouter: images are parts of the top-level state array.
            var parts = new JsonArray();
            if (req.State is not null)
                parts.Add(req.State.ToJsonNode());
            foreach (var img in req.Images)
                parts.Add(new JsonObject { ["type"] = "image_url", ["image_url"] = new JsonObject { ["url"] = img.ToDataUri() } });
            body["state"] = parts;
        }
        else
        {
            if (req.State is not null) body["state"] = req.State.ToJsonNode();
            if (req.Images.Count > 0)
                body["images"] = new JsonArray(req.Images.Select(i => (JsonNode?)JsonValue.Create(i.ToDataUri())).ToArray());
        }
        if (req.Videos.Count > 0)   // ASSUMPTION: {"frames":[...], "fps":n}
            body["videos"] = new JsonArray(req.Videos.Select(v => (JsonNode?)BuildVideo(v)).ToArray());
        body["questions"] = questions;
        return body.ToJsonString();
    }

    private static JsonObject BuildVideo(VideoInput v)
    {
        var o = new JsonObject
        {
            ["frames"] = new JsonArray(v.Frames.Select(f => (JsonNode?)JsonValue.Create(f.ToDataUri())).ToArray())
        };
        if (v.FramesPerSecond is { } fps) o["fps"] = fps;
        return o;
    }

    private static JsonObject BuildQuestion(Question q)
    {
        var o = new JsonObject { ["type"] = q.Type.ToWire(), ["instructions"] = q.Instructions };
        switch (q)
        {
            case ChoiceQuestion c:        // criteria: { optionId: description }
                var crit = new JsonObject();
                foreach (var opt in c.Options) crit[opt.Id] = opt.Description;
                o["criteria"] = crit;
                break;
            case ScoreQuestion s:         // ASSUMPTION: criteria is an ordered array of level descriptions
                o["criteria"] = new JsonArray(s.Levels.Select(l => (JsonNode?)JsonValue.Create(l)).ToArray());
                break;
        }
        return o;
    }

    /// <summary>Parses a provider response (Cloudflare envelope or bare SystemOne body).</summary>
    /// <param name="body">Raw response JSON.</param>
    /// <exception cref="ClefException">Not JSON, or no answers present.</exception>
    /// <exception cref="ClefApiException">Provider reported failure.</exception>
    public static DecisionResult Parse(string body)
    {
        JsonObject root;
        try { root = JsonNode.Parse(body) as JsonObject ?? throw new ClefException("Response was not a JSON object."); }
        catch (JsonException ex) { throw new ClefException("Response was not valid JSON.", ex); }

        if (root["success"] is JsonValue sv && sv.TryGetValue<bool>(out var ok) && !ok)
            throw new ClefApiException(0, body, $"Provider reported failure: {root["errors"]}");

        var r = root["result"] as JsonObject ?? root;
        var answersNode = r["answers"] as JsonObject
            ?? throw new ClefException($"No 'answers' in response: {body}");

        var map = new Dictionary<string, Answer>();
        foreach (var (id, node) in answersNode)
            if (node is JsonObject a) map[id] = ParseAnswer(id, a);

        TokenUsage? usage = null;
        if (r["usage"] is JsonObject u)
            usage = new TokenUsage((int)(Num(u["input_tokens"]) ?? Num(u["prompt_tokens"]) ?? 0),
                                   (int)(Num(u["output_tokens"]) ?? Num(u["completion_tokens"]) ?? 0));

        var model = r["model"] is JsonValue mv && mv.TryGetValue<string>(out var m) ? m : null;
        return new DecisionResult(model, map, usage, body);
    }

    private static Answer ParseAnswer(string id, JsonObject a)
    {
        var conf = Num(a["confidence"]);
        var probs = a["probabilities"];

        // Choice: has a string "choice" and per-option probabilities.
        if (a["choice"] is JsonValue cv && cv.TryGetValue<string>(out var choice))
        {
            var dict = new Dictionary<string, double>();
            if (probs is JsonObject po)
                foreach (var (k, v) in po) if (Num(v) is { } d) dict[k] = d;
            return new ChoiceAnswer(choice, dict, conf);
        }

        // Score: numeric "score" plus per-level probabilities (array, or object keyed by level).
        if (Num(a["score"]) is { } score)
        {
            var list = new List<double>();
            if (probs is JsonArray pa) list.AddRange(pa.Select(Num).Where(d => d.HasValue).Select(d => d!.Value));
            else if (probs is JsonObject pob)
                list.AddRange(pob.OrderBy(kv => int.TryParse(kv.Key, out var n) ? n : int.MaxValue)
                                 .Select(kv => Num(kv.Value)).Where(d => d.HasValue).Select(d => d!.Value));
            return new ScoreAnswer(score, list, conf);
        }

        // Noul: ASSUMPTION - probability of "true" under one of several plausible field names.
        var po2 = probs as JsonObject;
        var p = Num(a["noul"]) ?? Num(a["probability"]) ?? Num(a["true"]) ?? Num(a["yes"]) ?? Num(po2?["true"]) ?? Num(po2?["yes"]);
        return p is { } pv
            ? new NoulAnswer(pv, conf)
            : throw new ClefException($"Unrecognised answer shape for '{id}': {a.ToJsonString()}");
    }

    private static double? Num(JsonNode? n) => n is JsonValue v && v.TryGetValue<double>(out var d) ? d : null;
}
