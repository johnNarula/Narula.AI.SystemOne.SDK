using System.Text.RegularExpressions;
using Narula.AI.SystemOne.SDK.Clef.Abstractions;
using Narula.AI.SystemOne.SDK.Clef.Configuration;
using Narula.AI.SystemOne.SDK.Clef.Models;

namespace Narula.AI.SystemOne.SDK.Clef.Providers;

/// <summary>Client-side checks so mistakes surface before a billable network call.</summary>
public static partial class RequestValidator
{
    [GeneratedRegex(@"^[A-Za-z0-9_.\-]{1,100}$")]
    private static partial Regex IdPattern();

    /// <summary>Validates a request against limits and provider capabilities before any network call.</summary>
    /// <exception cref="ClefValidationException">A limit or capability is violated.</exception>
    public static void Validate(DecisionRequest req, LimitSettings limits, ProviderCapabilities caps)
    {
        if (req.Questions.Count == 0) throw new ClefValidationException("At least one question is required.");
        if (req.Questions.Count > limits.MaxQuestions)
            throw new ClefValidationException($"Too many questions ({req.Questions.Count} > {limits.MaxQuestions}).");

        foreach (var (id, q) in req.Questions)
        {
            if (!IdPattern().IsMatch(id)) throw new ClefValidationException($"Invalid question id '{id}'.");
            if (q is ChoiceQuestion { Options.Count: < 2 }) throw new ClefValidationException($"'{id}': a choice needs 2+ options.");
            if (q is ScoreQuestion { Levels.Count: < 2 }) throw new ClefValidationException($"'{id}': a score needs 2+ levels.");
        }

        if (req.Images.Count > 0 && !caps.HasFlag(ProviderCapabilities.Images))
            throw new ClefValidationException("This provider does not accept images.");
        if (req.Videos.Count > 0 && !caps.HasFlag(ProviderCapabilities.Videos))
            throw new ClefValidationException("This provider does not accept video input.");
        if (req.Images.Count > limits.MaxImages)
            throw new ClefValidationException($"Too many images ({req.Images.Count} > {limits.MaxImages}).");

        foreach (var img in req.Images.Concat(req.Videos.SelectMany(v => v.Frames)))
            if (img.Data.Length > limits.MaxImageBytes)
                throw new ClefValidationException($"Image is {img.Data.Length} bytes; limit is {limits.MaxImageBytes}.");
    }
}
