using System;

namespace LadirchenApp.Webhost.Infrastructure;

public static class AuthenticationSecurityPolicy
{
  public const int MinimumUsernameLength = 3;
  public const int MaximumUsernameLength = 100;
  public const int MaximumEmailLength = 320;
  public const int MinimumPasswordLength = 12;
  public const int MaximumPasswordLength = 256;
  public const int MaximumFailedLoginAttempts = 5;
  public const int PasswordIterations = 100_000;
  public static TimeSpan AccountLockDuration
  {
    get
    {
      return TimeSpan.FromMinutes(15);
    }
  }

  public static TimeSpan MemberInvitationLifetime
  {
    get
    {
      return TimeSpan.FromDays(7);
    }
  }
}