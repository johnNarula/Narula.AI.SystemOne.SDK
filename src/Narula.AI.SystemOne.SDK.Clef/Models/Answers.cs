namespace Narula.AI.SystemOne.SDK.Clef.Models;

/// <summary>Base answer; Confidence is the model's own certainty when provided.</summary>
/// <param name="Type">The question kind this answers.</param>
/// <param name="Confidence">Model-reported confidence, when the provider sends one.</param>
public abstract record Answer(QuestionType Type, double? Confidence);

/// <summary>Answer to a yes/no question: the probability the answer is "true".</summary>
/// <param name="Probability">Probability of "true" (0.0..1.0).</param>
/// <param name="Confidence">Model-reported confidence, when provided.</param>
public sealed record NoulAnswer(double Probability, double? Confidence)
    : Answer(QuestionType.Noul, Confidence);

/// <summary>Answer to a choice question: the winning option id plus per-option probabilities.</summary>
/// <param name="Choice">The winning option id.</param>
/// <param name="Probabilities">Per-option probabilities keyed by option id.</param>
/// <param name="Confidence">Model-reported confidence, when provided.</param>
public sealed record ChoiceAnswer(string Choice, IReadOnlyDictionary<string, double> Probabilities, double? Confidence)
    : Answer(QuestionType.Choice, Confidence);

/// <summary>Answer to a score question: the numeric score plus per-level probabilities.</summary>
/// <param name="Score">The numeric score.</param>
/// <param name="LevelProbabilities">Per-level probabilities in level order.</param>
/// <param name="Confidence">Model-reported confidence, when provided.</param>
public sealed record ScoreAnswer(double Score, IReadOnlyList<double> LevelProbabilities, double? Confidence)
    : Answer(QuestionType.Score, Confidence);

/// <summary>Token usage reported by the provider, when provided.</summary>
/// <param name="InputTokens">Input (prompt) tokens consumed.</param>
/// <param name="OutputTokens">Output (completion) tokens consumed.</param>
public sealed record TokenUsage(int InputTokens, int OutputTokens);

/// <summary>The parsed result of one <c>DecideAsync</c> call.</summary>
public sealed class DecisionResult
{
    /// <summary>Builds a result. Prefer the parser; construct directly only in tests/fakes.</summary>
    public DecisionResult(string? model, IReadOnlyDictionary<string, Answer> answers, TokenUsage? usage, string rawResponse)
    { Model = model; Answers = answers; Usage = usage; RawResponse = rawResponse; }

    /// <summary>Model name reported by the provider, when provided.</summary>
    public string? Model { get; }
    /// <summary>Parsed answers keyed by the question ids from the request.</summary>
    public IReadOnlyDictionary<string, Answer> Answers { get; }
    /// <summary>Token usage, when the provider reports it.</summary>
    public TokenUsage? Usage { get; }
    /// <summary>Untouched provider JSON, handy for debugging schema differences.</summary>
    public string RawResponse { get; }

    /// <summary>Gets the typed answer for a question id.</summary>
    /// <typeparam name="T">The expected answer type (NoulAnswer, ChoiceAnswer, ScoreAnswer).</typeparam>
    /// <param name="questionId">The id used when the question was added.</param>
    /// <exception cref="ClefException">No answer of type <typeparamref name="T"/> for <paramref name="questionId"/>.</exception>
    public T Get<T>(string questionId) where T : Answer =>
        Answers.TryGetValue(questionId, out var a) && a is T t
            ? t
            : throw new ClefException($"No {typeof(T).Name} found for question '{questionId}'.");
}
