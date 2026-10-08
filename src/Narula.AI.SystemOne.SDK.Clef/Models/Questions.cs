namespace Narula.AI.SystemOne.SDK.Clef.Models;

/// <summary>Base type for a typed question asked about the state.</summary>
/// <param name="Instructions">What the model is asked to decide.</param>
public abstract record Question(string Instructions)
{
    /// <summary>The question kind, sent as "noul", "choice", or "score" on the wire.</summary>
    public abstract QuestionType Type { get; }
}

/// <summary>Yes/no question; the answer is the probability of "true".</summary>
/// <param name="Instructions">The yes/no question to ask.</param>
public sealed record NoulQuestion(string Instructions) : Question(Instructions)
{
    /// <inheritdoc/>
    public override QuestionType Type => QuestionType.Noul;
}

/// <summary>One selectable option: your id plus a description the model reads.</summary>
/// <param name="Id">Caller-chosen option id, returned in the answer.</param>
/// <param name="Description">What the model scores this option against.</param>
public sealed record ChoiceOption(string Id, string Description);

/// <summary>Pick exactly one option from a set.</summary>
/// <param name="Instructions">How to choose between the options.</param>
/// <param name="Options">2+ options to pick from.</param>
public sealed record ChoiceQuestion(string Instructions, IReadOnlyList<ChoiceOption> Options) : Question(Instructions)
{
    /// <inheritdoc/>
    public override QuestionType Type => QuestionType.Choice;
}

/// <summary>Rate against an ordered rubric (lowest level first).</summary>
/// <param name="Instructions">What to rate.</param>
/// <param name="Levels">2+ ordered level descriptions, lowest first.</param>
public sealed record ScoreQuestion(string Instructions, IReadOnlyList<string> Levels) : Question(Instructions)
{
    /// <inheritdoc/>
    public override QuestionType Type => QuestionType.Score;
}
