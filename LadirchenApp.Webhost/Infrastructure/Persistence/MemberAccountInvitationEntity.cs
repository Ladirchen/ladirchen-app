using System;

namespace LadirchenApp.Webhost.Infrastructure.Persistence;

public sealed class MemberAccountInvitationEntity
{
  public Guid InvitationId
  {
    get; set;
  }
  public Guid FamilyId
  {
    get; set;
  }
  public string FamilyName { get; set; } = string.Empty;
  public string MemberId { get; set; } = string.Empty;
  public string EmailAddress { get; set; } = string.Empty;
  public string NormalizedEmail { get; set; } = string.Empty;
  public byte[] ActivationCodeHash { get; set; } = [];
  public DateTimeOffset CreatedAt
  {
    get; set;
  }
  public DateTimeOffset ExpiresAt
  {
    get; set;
  }
  public DateTimeOffset? RedeemedAt
  {
    get; set;
  }
}