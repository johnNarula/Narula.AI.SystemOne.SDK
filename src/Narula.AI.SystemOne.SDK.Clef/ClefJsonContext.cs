using System.Text.Json.Serialization;
using Narula.AI.SystemOne.SDK.Clef.Models;

namespace Narula.AI.SystemOne.SDK.Clef;

/// <summary>
/// Source-generated JSON serialization context. Native AOT safe — no reflection.
/// Register every model type here so the trimmer preserves their serialization metadata.
/// Currently the wire format is built with JsonNode (also AOT-safe); this context
/// exists for any future POCO serialization and as an explicit AOT-safe registration point.
/// </summary>
// Pattern: Explicit AOT-safe registration — no reflection; Native AOT clean.
[JsonSerializable(typeof(DecisionState))]
[JsonSerializable(typeof(TextState))]
[JsonSerializable(typeof(QueryState))]
[JsonSerializable(typeof(DecisionRequest))]
[JsonSerializable(typeof(Question))]
[JsonSerializable(typeof(NoulQuestion))]
[JsonSerializable(typeof(ChoiceQuestion))]
[JsonSerializable(typeof(ScoreQuestion))]
[JsonSerializable(typeof(ChoiceOption))]
[JsonSerializable(typeof(Answer))]
[JsonSerializable(typeof(NoulAnswer))]
[JsonSerializable(typeof(ChoiceAnswer))]
[JsonSerializable(typeof(ScoreAnswer))]
[JsonSerializable(typeof(DecisionResult))]
[JsonSerializable(typeof(TokenUsage))]
[JsonSerializable(typeof(ImageInput))]
[JsonSerializable(typeof(VideoInput))]
internal partial class ClefJsonContext : JsonSerializerContext
{
}
