namespace LvlUp.Application.Abstractions.Authentication;

public interface IOtpGenerator
{
    /// <summary>Generates a cryptographically random numeric one-time code.</summary>
    string Generate();
}
