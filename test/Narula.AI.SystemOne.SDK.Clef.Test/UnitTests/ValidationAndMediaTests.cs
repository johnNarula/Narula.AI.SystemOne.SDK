using Narula.AI.SystemOne.SDK.Clef.Abstractions;
using Narula.AI.SystemOne.SDK.Clef.Configuration;
using Narula.AI.SystemOne.SDK.Clef.Models;
using Narula.AI.SystemOne.SDK.Clef.Providers;
using Xunit;

namespace Narula.AI.SystemOne.SDK.Clef.Test.UnitTests;

public class ValidationAndMediaTests
{
    private static readonly LimitSettings Limits = new() { MaxQuestions = 64, MaxImages = 4, MaxImageBytes = 100 };
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0];

    [Fact]
    public void ImageFormat_DetectedFromMagicBytes()
    {
        Assert.Equal(ImageFormat.Png, ImageInput.FromBytes(Png).Format);
        Assert.Equal(ImageFormat.Jpeg, ImageInput.FromBytes([0xFF, 0xD8, 0xFF, 0xE0]).Format);
        Assert.Throws<ArgumentException>(() => ImageInput.FromBytes([1, 2, 3]));
    }

    [Fact]
    public void Video_RejectedByCloudflareCapabilities()
    {
        var req = new DecisionRequest().WithVideo(new VideoInput([ImageInput.FromBytes(Png)]))
            .Add("q", new NoulQuestion("x"));
        var caps = ProviderCapabilities.Text | ProviderCapabilities.Images;
        Assert.Throws<ClefValidationException>(() => RequestValidator.Validate(req, Limits, caps));
        RequestValidator.Validate(req, Limits, caps | ProviderCapabilities.Videos);   // allowed elsewhere
    }

    [Fact]
    public void Limits_Enforced()
    {
        var big = new ImageInput(ImageFormat.Png, new byte[101]);
        var req = new DecisionRequest().WithImage(big).Add("q", new NoulQuestion("x"));
        Assert.Throws<ClefValidationException>(() => RequestValidator.Validate(req, Limits, ProviderCapabilities.Images));
        Assert.Throws<ClefValidationException>(() =>
            RequestValidator.Validate(new DecisionRequest(), Limits, ProviderCapabilities.Text));   // no questions
    }

    [Fact]
    public void BadQuestionId_Rejected()
    {
        var req = new DecisionRequest().Add("bad id!", new NoulQuestion("x"));
        Assert.Throws<ClefValidationException>(() => RequestValidator.Validate(req, Limits, ProviderCapabilities.Text));
    }
}
