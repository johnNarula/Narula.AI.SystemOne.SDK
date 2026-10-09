using System.Text.Json.Nodes;
using Narula.AI.SystemOne.SDK.Clef.Models;
using Narula.AI.SystemOne.SDK.Clef.Providers;
using Xunit;

namespace Narula.AI.SystemOne.SDK.Clef.Test.UnitTests;

public class WireTests
{
    [Fact]
    public void EnumWire_RoundTrips()
    {
        Assert.Equal("noul", QuestionType.Noul.ToWire());
        Assert.Equal("image/png", ImageFormat.Png.MediaType());
    }

    [Fact]
    public void BuildBody_ChoiceHasCriteria_NoulHasType()
    {
        var body = JsonNode.Parse(ClefWire.BuildBody(ClefSamples.TextMatchRequest(), "clef"))!;
        Assert.Equal("clef", (string?)body["model"]);
        Assert.Equal("choice", (string?)body["questions"]!["match"]!["type"]);
        Assert.Equal(ClefSamples.Descriptions[3], (string?)body["questions"]!["match"]!["criteria"]!["opt_3"]);

        var noul = JsonNode.Parse(ClefWire.BuildBody(ClefSamples.NoulRequest(), "clef"))!;
        Assert.Equal("noul", (string?)noul["questions"]!["urgent"]!["type"]);
    }

    [Fact]
    public void BuildBody_ScoreUsesArrayCriteria()
    {
        var body = JsonNode.Parse(ClefWire.BuildBody(ClefSamples.ScoreRequest(), "clef"))!;
        Assert.Equal(4, body["questions"]!["severity"]!["criteria"]!.AsArray().Count);
    }

    [Fact]
    public void Parse_Envelope_Choice()
    {
        var r = ClefWire.Parse("""
            {"success":true,"result":{"model":"clef","answers":{"match":{"choice":"opt_2",
             "probabilities":{"opt_0":0.1,"opt_2":0.9},"confidence":0.8}},"usage":{"input_tokens":10,"output_tokens":0}}}
            """);
        var a = r.Get<ChoiceAnswer>("match");
        Assert.Equal("opt_2", a.Choice);
        Assert.Equal(0.9, a.Probabilities["opt_2"]);
        Assert.Equal(10, r.Usage!.InputTokens);
    }

    [Fact]
    public void Parse_Noul_And_Score_BareBody()
    {
        var r = ClefWire.Parse("""
            {"answers":{"u":{"probability":0.97},"s":{"score":2.4,"probabilities":[0.1,0.2,0.6,0.1]}}}
            """);
        Assert.Equal(0.97, r.Get<NoulAnswer>("u").Probability);
        Assert.Equal(4, r.Get<ScoreAnswer>("s").LevelProbabilities.Count);
    }

    [Fact]
    public void Parse_ErrorPayload_Throws() =>
        Assert.Throws<ClefApiException>(() => ClefWire.Parse("""{"success":false,"errors":[{"message":"bad"}]}"""));


    /// <summary>
    /// AOT-safety: BuildBody must work for every DecisionState subtype without reflection.
    /// Under Native AOT, the old reflection-based serialization threw InvalidOperationException.
    /// </summary>
    [Fact]
    public void BuildBody_EachStateType_NoReflection()
    {
        // TextState renders as a JSON string
        var textReq = new DecisionRequest { State = new TextState("hello") }
            .Add("q", new NoulQuestion("ok?"));
        var textBody = JsonNode.Parse(ClefWire.BuildBody(textReq, "m"))!;
        Assert.Equal("hello", textBody["state"]!.GetValue<string>());

        // QueryState renders as { "query": "..." }
        var queryReq = new DecisionRequest { State = new QueryState("women") }
            .Add("q", new NoulQuestion("ok?"));
        var queryBody = JsonNode.Parse(ClefWire.BuildBody(queryReq, "m"))!;
        Assert.Equal("women", queryBody["state"]!["query"]!.GetValue<string>());

        // Null state omits the field entirely
        var nullReq = new DecisionRequest { State = null }
            .Add("q", new NoulQuestion("ok?"));
        var nullBody = JsonNode.Parse(ClefWire.BuildBody(nullReq, "m"))!;
        Assert.Null(nullBody["state"]);
    }
}
