using Narula.AI.SystemOne.SDK.Clef.Abstractions;
using Narula.AI.SystemOne.SDK.Clef.Models;

namespace Narula.AI.SystemOne.SDK.Clef.Extensions;

public sealed record Match<T>(T Item, double Probability);

public sealed record MatchResult<T>(T Best, double Probability, double? Confidence, IReadOnlyList<Match<T>> Ranked);

/// <summary>One-line helpers over <see cref="ISystemOneClient"/>.</summary>
public static class DecisionClientExtensions
{
    /// <summary>Which candidate best matches the query? Returns the winner plus the full ranking.</summary>
    public static async Task<MatchResult<T>> BestMatchAsync<T>(this ISystemOneClient client, string query,
        IReadOnlyList<T> candidates, Func<T, string> describe, string? instructions = null,
        ClefModel? model = null, CancellationToken ct = default)
    {
        var req = DecisionRequests.BestMatch(query, candidates, describe, instructions, model);
        var ans = (await client.DecideAsync(req, ct)).Get<ChoiceAnswer>("match");

        bool TryIndex(string key, out int idx)
        {
            idx = -1;
            return key.StartsWith("opt_") && int.TryParse(key.AsSpan(4), out idx) && idx >= 0 && idx < candidates.Count;
        }

        if (!TryIndex(ans.Choice, out var best))
            throw new ClefException($"Provider returned unknown choice '{ans.Choice}'.");

        var ranked = ans.Probabilities
            .Where(kv => TryIndex(kv.Key, out _))
            .OrderByDescending(kv => kv.Value)
            .Select(kv => { TryIndex(kv.Key, out var i); return new Match<T>(candidates[i], kv.Value); })
            .ToList();

        var p = ans.Probabilities.GetValueOrDefault(ans.Choice, ranked.FirstOrDefault()?.Probability ?? 0);
        return new MatchResult<T>(candidates[best], p, ans.Confidence, ranked);
    }

    /// <summary>Convenience overload when candidates are plain description strings.</summary>
    public static Task<MatchResult<string>> BestMatchAsync(this ISystemOneClient client, string query,
        IReadOnlyList<string> descriptions, string? instructions = null, ClefModel? model = null,
        CancellationToken ct = default)
        => client.BestMatchAsync(query, descriptions, d => d, instructions, model, ct);

    /// <summary>Yes/no question about a state (optionally with images).</summary>
    public static async Task<NoulAnswer> AskAsync(this ISystemOneClient client, object state, string question,
        IEnumerable<ImageInput>? images = null, ClefModel? model = null, CancellationToken ct = default)
    {
        var req = new DecisionRequest { State = state, Model = model }.Add("q", new NoulQuestion(question));
        foreach (var i in images ?? []) req.WithImage(i);
        return (await client.DecideAsync(req, ct)).Get<NoulAnswer>("q");
    }

    /// <summary>Rate a state against an ordered rubric (lowest level first).</summary>
    public static async Task<ScoreAnswer> ScoreAsync(this ISystemOneClient client, object state, string instructions,
        IReadOnlyList<string> levels, ClefModel? model = null, CancellationToken ct = default)
    {
        var req = new DecisionRequest { State = state, Model = model }.Add("s", new ScoreQuestion(instructions, levels));
        return (await client.DecideAsync(req, ct)).Get<ScoreAnswer>("s");
    }
}
