namespace Narula.AI.SystemOne.SDK.Clef.Models;

/// <summary>The three typed question kinds a decision model answers.</summary>
public enum QuestionType
{
    /// <summary>Yes/no question.</summary>
    Noul,
    /// <summary>Pick one option from a set.</summary>
    Choice,
    /// <summary>Rate against an ordered rubric.</summary>
    Score
}

/// <summary>Image formats accepted as inputs.</summary>
public enum ImageFormat
{
    /// <summary>PNG image.</summary>
    Png,
    /// <summary>JPEG image.</summary>
    Jpeg,
    /// <summary>WebP image.</summary>
    WebP
}

/// <summary>Maps enums to their wire (JSON / HTTP) representation and back.</summary>
public static class EnumWire
{
    /// <summary>Question type to wire string ("noul", "choice", "score").</summary>
    public static string ToWire(this QuestionType t) => t switch
    {
        QuestionType.Noul => "noul",
        QuestionType.Choice => "choice",
        QuestionType.Score => "score",
        _ => throw new ArgumentOutOfRangeException(nameof(t))
    };

    /// <summary>Image format to MIME type.</summary>
    public static string MediaType(this ImageFormat f) => f switch
    {
        ImageFormat.Png => "image/png",
        ImageFormat.Jpeg => "image/jpeg",
        ImageFormat.WebP => "image/webp",
        _ => throw new ArgumentOutOfRangeException(nameof(f))
    };
}
