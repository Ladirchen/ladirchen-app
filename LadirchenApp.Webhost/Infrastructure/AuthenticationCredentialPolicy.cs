using System;
using System.Linq;
using System.Security.Cryptography;

using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Webhost.Infrastructure;

public static class AuthenticationCredentialPolicy
{
  public static byte[] HashPassword(string password, byte[] salt, int iterations)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(password);
    ArgumentNullException.ThrowIfNull(salt);

    return Rfc2898DeriveBytes.Pbkdf2(
      password,
      salt,
      iterations,
      HashAlgorithmName.SHA256,
      32
    );
  }

  public static string NormalizeEmail(string email)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(email);
    var normalized = email.Trim();

    if (normalized.Length > AuthenticationSecurityPolicy.MaximumEmailLength)
    {
      throw new ArgumentException(
        "The email address does not meet the account requirements.",
        nameof(email)
      );
    }

    try
    {
      return Family.NormalizeEmailAddress(normalized).ToLowerInvariant();
    }
    catch (DomainRuleException exception)
    {
      throw new ArgumentException(
        "The email address does not meet the account requirements.",
        nameof(email),
        exception
      );
    }
  }

  public static string NormalizeUsername(string username)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(username);
    var normalized = username.Trim().ToLowerInvariant();

    if (
      normalized.Length < AuthenticationSecurityPolicy.MinimumUsernameLength
      || normalized.Length > AuthenticationSecurityPolicy.MaximumUsernameLength
      || normalized.Contains('@', StringComparison.Ordinal)
      || normalized.Any(char.IsWhiteSpace)
    )
    {
      throw new ArgumentException(
        "The username does not meet the account requirements.",
        nameof(username)
      );
    }

    return normalized;
  }

  public static void PerformDummyPasswordHash(string password)
  {
    var candidate = string.IsNullOrWhiteSpace(password)
      || password.Length > AuthenticationSecurityPolicy.MaximumPasswordLength
        ? "invalid-password"
        : password;
    _ = HashPassword(
      candidate,
      new byte[16],
      AuthenticationSecurityPolicy.PasswordIterations
    );
  }

  public static void ValidatePassword(string password)
  {
    ArgumentException.ThrowIfNullOrWhiteSpace(password);

    if (
      password.Length < AuthenticationSecurityPolicy.MinimumPasswordLength
      || password.Length > AuthenticationSecurityPolicy.MaximumPasswordLength
    )
    {
      throw new ArgumentException(
        "The password does not meet the account requirements.",
        nameof(password)
      );
    }
  }
}