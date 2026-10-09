using System.Text.Json.Nodes;

namespace Narula.AI.SystemOne.SDK.Clef.Models;

/// <summary>
/// Strongly-typed decision state. Replaces the old <c>object?</c> State property.
/// Each subtype knows how to render itself as a JSON node — no reflection,
/// no runtime serialization. Native AOT safe.
/// </summary>
public abstract record DecisionState
{
    /// <summary>Renders the state as a JSON node for the wire format.</summary>
    public abstract JsonNode ToJsonNode();
}

/// <summary>Plain text state. Renders as a JSON string.</summary>
/// <param name="Text">The text content.</param>
public sealed record TextState(string Text) : DecisionState
{
    /// <inheritdoc/>
    public override JsonNode ToJsonNode() => JsonValue.Create(Text)!;
}

/// <summary>Query state for best-match / search scenarios. Renders as <c>{ "query": "..." }</c>.</summary>
/// <param name="Query">The query text.</param>
public sealed record QueryState(string Query) : DecisionState
{
    /// <inheritdoc/>
    public override JsonNode ToJsonNode() => new JsonObject { ["query"] = Query };
}
