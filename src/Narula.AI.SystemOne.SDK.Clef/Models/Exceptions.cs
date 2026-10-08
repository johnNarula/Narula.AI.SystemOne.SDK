namespace Narula.AI.SystemOne.SDK.Clef.Models;

public class ClefException : Exception
{
    public ClefException(string message, Exception? inner = null) : base(message, inner) { }
}

/// <summary>The request broke a configured limit or provider capability before being sent.</summary>
public sealed class ClefValidationException(string message) : ClefException(message);

/// <summary>Settings are missing or invalid.</summary>
public sealed class ClefConfigurationException(string message) : ClefException(message);

/// <summary>The provider returned an error status or an error payload.</summary>
public sealed class ClefApiException(int statusCode, string body, string message) : ClefException(message)
{
    public int StatusCode { get; } = statusCode;
    public string Body { get; } = body;
}
