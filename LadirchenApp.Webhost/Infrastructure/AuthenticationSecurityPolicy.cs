using System;

namespace LadirchenApp.Webhost.Infrastructure;

public static class AuthenticationSecurityPolicy
{
  public const int MaximumFailedLoginAttempts = 5;
  public const int PasswordIterations = 100_000;
  public static readonly TimeSpan AccountLockDuration = TimeSpan.FromMinutes(15);
}
