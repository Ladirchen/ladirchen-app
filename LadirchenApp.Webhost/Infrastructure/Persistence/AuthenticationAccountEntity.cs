using System;

namespace LadirchenApp.Webhost.Infrastructure.Persistence;

public sealed class AuthenticationAccountEntity
{
  public string Username { get; set; } = string.Empty;
  public string? EmailAddress
  {
    get; set;
  }
  public string? NormalizedEmail
  {
    get; set;
  }
  public Guid FamilyId
  {
    get; set;
  }
  public string FamilyName { get; set; } = string.Empty;
  public string MemberId { get; set; } = string.Empty;
  public byte[] PasswordHash { get; set; } = [];
  public byte[] PasswordSalt { get; set; } = [];
  public int PasswordIterations
  {
    get; set;
  }
  public int FailedLoginCount
  {
    get; set;
  }
  public DateTimeOffset? LockedUntilUtc
  {
    get; set;
  }
}