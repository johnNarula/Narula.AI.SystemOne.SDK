namespace Narula.AI.SystemOne.SDK.Clef.Models;

/// <summary>The three typed question kinds a decision model answers.</summary>
public enum QuestionType { Noul, Choice, Score }

/// <summary>The hosted Clef model variants.</summary>
public enum ClefModel { Clef, ClefFlash }

/// <summary>Image formats accepted as inputs.</summary>
public enum ImageFormat { Png, Jpeg, WebP }

/// <summary>Maps enums to their wire (JSON / HTTP) representation and back.</summary>
public static class EnumWire
{
    public static string ToWire(this QuestionType t) => t switch
    {
        QuestionType.Noul => "noul",
        QuestionType.Choice => "choice",
        QuestionType.Score => "score",
        _ => throw new ArgumentOutOfRangeException(nameof(t))
    };

    public static string ToWire(this ClefModel m) => m switch
    {
        ClefModel.Clef => "clef",
        ClefModel.ClefFlash => "clef-flash",
        _ => throw new ArgumentOutOfRangeException(nameof(m))
    };

    public static ClefModel ParseModel(string s) => s.Trim().ToLowerInvariant() switch
    {
        "clef" => ClefModel.Clef,
        "clef-flash" or "clefflash" => ClefModel.ClefFlash,
        _ => throw new ArgumentException($"Unknown Clef model '{s}'. Use 'clef' or 'clef-flash'.")
    };

    public static string MediaType(this ImageFormat f) => f switch
    {
        ImageFormat.Png => "image/png",
        ImageFormat.Jpeg => "image/jpeg",
        ImageFormat.WebP => "image/webp",
        _ => throw new ArgumentOutOfRangeException(nameof(f))
    };
}
