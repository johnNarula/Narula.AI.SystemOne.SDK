using Narula.AI.SystemOne.SDK.Clef.Models;

namespace Narula.AI.SystemOne.SDK.Clef.Extensions;

/// <summary>Builders for the common request shapes (also used by dry-run samples).</summary>
public static class DecisionRequests
{
    /// <summary>Default instructions for a best-match choice question.</summary>
    public const string DefaultMatchInstructions =
        "Which description best matches what the user is searching for in `query`?";

    // Option ids are positional so results map back to candidates by index.
    /// <summary>Builds the option id for a candidate index (opt_0, opt_1, ...).</summary>
    public static string OptionId(int index) => $"opt_{index}";

    /// <summary>Builds a best-match choice request over candidates.</summary>
    /// <typeparam name="T">Candidate type.</typeparam>
    /// <param name="query">What to match against.</param>
    /// <param name="candidates">2+ candidates.</param>
    /// <param name="describe">Maps a candidate to the description the model reads.</param>
    /// <param name="instructions">Custom instructions; defaults to <see cref="DefaultMatchInstructions"/>.</param>
    /// <param name="model">Model name override for this call.</param>
    /// <exception cref="ArgumentException">Fewer than 2 candidates.</exception>
    public static DecisionRequest BestMatch<T>(string query, IReadOnlyList<T> candidates, Func<T, string> describe,
        string? instructions = null, string? model = null)
    {
        if (candidates.Count < 2) throw new ArgumentException("Need at least 2 candidates.", nameof(candidates));
        var options = candidates.Select((c, i) => new ChoiceOption(OptionId(i), describe(c))).ToList();
        return new DecisionRequest { State = new QueryState(query), Model = model }
            .Add("match", new ChoiceQuestion(instructions ?? DefaultMatchInstructions, options));
    }
}
