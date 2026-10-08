namespace Narula.AI.SystemOne.SDK.Clef.Models;

/// <summary>An image sent inline (base64). Remote URLs are intentionally not supported.</summary>
public sealed class ImageInput
{
    /// <summary>Builds an image input from a known format and raw bytes.</summary>
    public ImageInput(ImageFormat format, ReadOnlyMemory<byte> data) { Format = format; Data = data; }

    /// <summary>The image format.</summary>
    public ImageFormat Format { get; }
    /// <summary>The raw image bytes.</summary>
    public ReadOnlyMemory<byte> Data { get; }

    /// <summary>Detects the format from magic bytes so callers never pass a format by hand.</summary>
    /// <exception cref="ArgumentException">Not a PNG, JPEG, or WebP.</exception>
    public static ImageInput FromBytes(byte[] bytes)
    {
        if (bytes.Length >= 8 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47)
            return new ImageInput(ImageFormat.Png, bytes);
        if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
            return new ImageInput(ImageFormat.Jpeg, bytes);
        if (bytes.Length >= 12 && bytes[0] == 'R' && bytes[1] == 'I' && bytes[2] == 'F' && bytes[3] == 'F'
            && bytes[8] == 'W' && bytes[9] == 'E' && bytes[10] == 'B' && bytes[11] == 'P')
            return new ImageInput(ImageFormat.WebP, bytes);
        throw new ArgumentException("Unsupported image. Expected PNG, JPEG, or WebP.");
    }

    /// <summary>Reads a file and detects its format.</summary>
    public static async Task<ImageInput> FromFileAsync(string path, CancellationToken ct = default)
        => FromBytes(await File.ReadAllBytesAsync(path, ct));

    /// <summary>Encodes as a data URI for the wire. ASSUMPTION: images travel as data URIs; verify against the provider's schema.</summary>
    public string ToDataUri() => $"data:{Format.MediaType()};base64,{Convert.ToBase64String(Data.Span)}";
}

/// <summary>A video expressed as ordered frames (the form SystemOne-style APIs use).</summary>
public sealed class VideoInput
{
    /// <summary>Builds a video input from ordered frames.</summary>
    /// <param name="frames">1+ frames in playback order.</param>
    /// <param name="framesPerSecond">Frame rate hint, when known.</param>
    /// <exception cref="ArgumentException">No frames supplied.</exception>
    public VideoInput(IReadOnlyList<ImageInput> frames, double? framesPerSecond = null)
    {
        if (frames.Count == 0) throw new ArgumentException("A video needs at least one frame.", nameof(frames));
        Frames = frames;
        FramesPerSecond = framesPerSecond;
    }

    /// <summary>Frames in playback order.</summary>
    public IReadOnlyList<ImageInput> Frames { get; }
    /// <summary>Frame rate hint, when known.</summary>
    public double? FramesPerSecond { get; }
}
