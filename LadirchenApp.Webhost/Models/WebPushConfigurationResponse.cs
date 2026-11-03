namespace LadirchenApp.Webhost.Models;

public sealed record WebPushConfigurationResponse(bool Configured, string? PublicKey);