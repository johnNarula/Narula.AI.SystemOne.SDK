namespace Narula.AI.SystemOne.SDK.Clef.Models;

/// <summary>Base answer; Confidence is the model's own certainty when provided.</summary>
public abstract record Answer(QuestionType Type, double? Confidence);

public sealed record NoulAnswer(double Probability, double? Confidence)
    : Answer(QuestionType.Noul, Confidence);

public sealed record ChoiceAnswer(string Choice, IReadOnlyDictionary<string, double> Probabilities, double? Confidence)
    : Answer(QuestionType.Choice, Confidence);

public sealed record ScoreAnswer(double Score, IReadOnlyList<double> LevelProbabilities, double? Confidence)
    : Answer(QuestionType.Score, Confidence);

public sealed record TokenUsage(int InputTokens, int OutputTokens);

public sealed class DecisionResult
{
    public DecisionResult(string? model, IReadOnlyDictionary<string, Answer> answers, TokenUsage? usage, string rawResponse)
    { Model = model; Answers = answers; Usage = usage; RawResponse = rawResponse; }

    public string? Model { get; }
    public IReadOnlyDictionary<string, Answer> Answers { get; }
    public TokenUsage? Usage { get; }
    /// <summary>Untouched provider JSON, handy for debugging schema differences.</summary>
    public string RawResponse { get; }

    public T Get<T>(string questionId) where T : Answer =>
        Answers.TryGetValue(questionId, out var a) && a is T t
            ? t
            : throw new ClefException($"No {typeof(T).Name} found for question '{questionId}'.");
}
