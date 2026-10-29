namespace LadirchenApp.Webhost.Models;

public sealed record AuthenticateAccountRequest(string Password, string Username);
