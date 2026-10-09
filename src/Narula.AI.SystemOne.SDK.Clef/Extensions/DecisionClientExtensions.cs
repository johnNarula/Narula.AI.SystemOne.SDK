using Narula.AI.SystemOne.SDK.Clef.Abstractions;
using Narula.AI.SystemOne.SDK.Clef.Models;

namespace Narula.AI.SystemOne.SDK.Clef.Extensions;

/// <summary>One ranked candidate.</summary>
/// <param name="Item">The candidate.</param>
/// <param name="Probability">Model-assigned probability (0.0..1.0).</param>
public sealed record Match<T>(T Item, double Probability);

/// <summary>The winner of a best-match call plus the full ranking.</summary>
/// <param name="Best">The winning candidate.</param>
/// <param name="Probability">Winner's probability (0.0..1.0).</param>
/// <param name="Confidence">Model-reported confidence, when provided.</param>
/// <param name="Ranked">All candidates, best first.</param>
public sealed record MatchResult<T>(T Best, double Probability, double? Confidence, IReadOnlyList<Match<T>> Ranked);

/// <summary>One-line helpers over <see cref="ISystemOneClient"/>.</summary>
public static class DecisionClientExtensions
{
    /// <summary>Which candidate best matches the query? Returns the winner plus the full ranking.
    /// A single candidate is returned directly (probability 1.0) with no network call.</summary>
    public static async Task<MatchResult<T>> BestMatchAsync<T>(this ISystemOneClient client, string query,
        IReadOnlyList<T> candidates, Func<T, string> describe, string? instructions = null,
        string? model = null, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        if (candidates.Count == 1)
            return new MatchResult<T>(candidates[0], 1.0, null, [new Match<T>(candidates[0], 1.0)]);
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
        IReadOnlyList<string> descriptions, string? instructions = null, string? model = null,
        CancellationToken ct = default)
        => client.BestMatchAsync(query, descriptions, d => d, instructions, model, ct);

    /// <summary>Yes/no question about a state (optionally with images).</summary>
    public static async Task<NoulAnswer> AskAsync(this ISystemOneClient client, DecisionState state, string question,
        IEnumerable<ImageInput>? images = null, string? model = null, CancellationToken ct = default)
    {
        var req = new DecisionRequest { State = state, Model = model }.Add("q", new NoulQuestion(question));
        foreach (var i in images ?? []) req.WithImage(i);
        return (await client.DecideAsync(req, ct)).Get<NoulAnswer>("q");
    }

    /// <summary>Rate a state against an ordered rubric (lowest level first).</summary>
    public static async Task<ScoreAnswer> ScoreAsync(this ISystemOneClient client, DecisionState state, string instructions,
        IReadOnlyList<string> levels, string? model = null, CancellationToken ct = default)
    {
        var req = new DecisionRequest { State = state, Model = model }.Add("s", new ScoreQuestion(instructions, levels));
        return (await client.DecideAsync(req, ct)).Get<ScoreAnswer>("s");
    }
}
