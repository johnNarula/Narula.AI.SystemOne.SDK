namespace Narula.AI.SystemOne.SDK.Clef.Models;

/// <summary>Everything sent in one call: a state, optional media, and 1..N typed questions.</summary>
public sealed class DecisionRequest
{
    /// <summary>Overrides the configured model for this call only.</summary>
    public ClefModel? Model { get; set; }

    /// <summary>Text, or any object that serializes to JSON.</summary>
    public object? State { get; set; }

    public List<ImageInput> Images { get; } = new();
    public List<VideoInput> Videos { get; } = new();
    public Dictionary<string, Question> Questions { get; } = new();

    // Fluent helpers keep call sites short.
    public DecisionRequest Add(string id, Question q) { Questions[id] = q; return this; }
    public DecisionRequest WithImage(ImageInput image) { Images.Add(image); return this; }
    public DecisionRequest WithVideo(VideoInput video) { Videos.Add(video); return this; }
}
