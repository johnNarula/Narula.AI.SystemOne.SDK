using Narula.AI.SystemOne.SDK.Clef.Models;

namespace Narula.AI.SystemOne.SDK.Clef.Extensions;

/// <summary>Builders for the common request shapes (also used by dry-run samples).</summary>
public static class DecisionRequests
{
    public const string DefaultMatchInstructions =
        "Which description best matches what the user is searching for in `query`?";

    // Option ids are positional so results map back to candidates by index.
    public static string OptionId(int index) => $"opt_{index}";

    public static DecisionRequest BestMatch<T>(string query, IReadOnlyList<T> candidates, Func<T, string> describe,
        string? instructions = null, ClefModel? model = null)
    {
        if (candidates.Count < 2) throw new ArgumentException("Need at least 2 candidates.", nameof(candidates));
        var options = candidates.Select((c, i) => new ChoiceOption(OptionId(i), describe(c))).ToList();
        return new DecisionRequest { State = new { query }, Model = model }
            .Add("match", new ChoiceQuestion(instructions ?? DefaultMatchInstructions, options));
    }
}
