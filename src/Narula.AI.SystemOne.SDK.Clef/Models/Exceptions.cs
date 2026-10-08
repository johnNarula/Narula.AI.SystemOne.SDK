namespace Narula.AI.SystemOne.SDK.Clef.Models;

/// <summary>Base exception for all SDK errors.</summary>
public class ClefException : Exception
{
    /// <summary>Builds an SDK exception.</summary>
    public ClefException(string message, Exception? inner = null) : base(message, inner) { }
}

/// <summary>The request broke a configured limit or provider capability before being sent.</summary>
public sealed class ClefValidationException(string message) : ClefException(message);

/// <summary>Settings are missing or invalid.</summary>
public sealed class ClefConfigurationException(string message) : ClefException(message);

/// <summary>The provider returned an error status or an error payload.</summary>
public sealed class ClefApiException(int statusCode, string body, string message) : ClefException(message)
{
    /// <summary>HTTP status code (0 when the failure came from an error payload, not HTTP).</summary>
    public int StatusCode { get; } = statusCode;
    /// <summary>The raw response body.</summary>
    public string Body { get; } = body;
}
