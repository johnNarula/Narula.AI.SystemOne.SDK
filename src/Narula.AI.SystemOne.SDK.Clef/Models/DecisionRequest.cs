namespace Narula.AI.SystemOne.SDK.Clef.Models;

/// <summary>Everything sent in one call: a state, optional media, and 1..N typed questions.</summary>
public sealed class DecisionRequest
{
    /// <summary>Model name override for this call only (e.g. "cloudflare/clef-flash"). Null uses the configured model.</summary>
    public string? Model { get; set; }

    /// <summary>Text, or any object that serializes to JSON.</summary>
    public object? State { get; set; }

    /// <summary>Images sent with the request (provider must support <c>ProviderCapabilities.Images</c>).</summary>
    public List<ImageInput> Images { get; } = new();
    /// <summary>Videos sent with the request (provider must support <c>ProviderCapabilities.Videos</c>).</summary>
    public List<VideoInput> Videos { get; } = new();
    /// <summary>Questions keyed by caller-chosen id (1..100 chars: letters, digits, underscore, dot, dash).</summary>
    public Dictionary<string, Question> Questions { get; } = new();

    /// <summary>Adds a question. Fluent: <c>new DecisionRequest().Add("q", new NoulQuestion("..."))</c>.</summary>
    public DecisionRequest Add(string id, Question q) { Questions[id] = q; return this; }
    /// <summary>Adds an image. Fluent.</summary>
    public DecisionRequest WithImage(ImageInput image) { Images.Add(image); return this; }
    /// <summary>Adds a video. Fluent.</summary>
    public DecisionRequest WithVideo(VideoInput video) { Videos.Add(video); return this; }
}
