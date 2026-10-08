namespace Narula.AI.SystemOne.SDK.Clef.Models;

/// <summary>Base type for a typed question asked about the state.</summary>
public abstract record Question(string Instructions)
{
    public abstract QuestionType Type { get; }
}

/// <summary>Yes/no question; the answer is the probability of "true".</summary>
public sealed record NoulQuestion(string Instructions) : Question(Instructions)
{
    public override QuestionType Type => QuestionType.Noul;
}

/// <summary>One selectable option: your id plus a description the model reads.</summary>
public sealed record ChoiceOption(string Id, string Description);

/// <summary>Pick exactly one option from a set.</summary>
public sealed record ChoiceQuestion(string Instructions, IReadOnlyList<ChoiceOption> Options) : Question(Instructions)
{
    public override QuestionType Type => QuestionType.Choice;
}

/// <summary>Rate against an ordered rubric (lowest level first).</summary>
public sealed record ScoreQuestion(string Instructions, IReadOnlyList<string> Levels) : Question(Instructions)
{
    public override QuestionType Type => QuestionType.Score;
}
