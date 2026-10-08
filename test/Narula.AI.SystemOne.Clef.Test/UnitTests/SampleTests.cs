using Narula.AI.SystemOne.Clef.Abstractions;
using Narula.AI.SystemOne.Clef.Configuration;
using Narula.AI.SystemOne.Clef.Providers;
using Xunit;

namespace Narula.AI.SystemOne.Clef.Test.UnitTests;

/// <summary>Every shipped sample request must pass the library's own validation.</summary>
public class SampleTests
{
    [Fact]
    public void AllSampleRequests_AreValid()
    {
        var limits = new LimitSettings { MaxQuestions = 64, MaxImages = 4, MaxImageBytes = 4_194_304 };
        foreach (var req in new[] { ClefSamples.TextMatchRequest(), ClefSamples.NoulRequest(), ClefSamples.ScoreRequest() })
            RequestValidator.Validate(req, limits, ProviderCapabilities.Text);
    }
}
