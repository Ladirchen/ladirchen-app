using System;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Family;

public sealed record FamilyMemberState
{
  public FamilyMemberState(
    FamilyMemberId id,
    string name,
    MemberRole role,
    GuardianAccess access,
    bool invitationPending,
    string? nickname,
    string? emailAddress,
    bool participatesInWeeklyGoal
  )
  {
    ArgumentNullException.ThrowIfNull(id);
    Id = id;
    Name = Rules.Text(name);
    Role = Rules.Defined(role);
    Access = Rules.Defined(access);
    Rules.Require(
      role == MemberRole.Guardian || access == GuardianAccess.Supporter,
      DomainError.InvalidValue,
      "Children cannot have guardian access."
    );
    Rules.Require(
      !invitationPending || role == MemberRole.Guardian,
      DomainError.InvalidValue,
      "Only guardians can have a pending invitation."
    );
    InvitationPending = invitationPending;
    Nickname = NormalizeNickname(nickname);
    EmailAddress = emailAddress is null ? null : Family.NormalizeEmailAddress(emailAddress);
    ParticipatesInWeeklyGoal = participatesInWeeklyGoal;
  }

  public FamilyMemberId Id { get; }
  public string Name { get; }
  public MemberRole Role { get; }
  public GuardianAccess Access { get; }
  public bool InvitationPending { get; }
  public string? Nickname { get; }
  public string? EmailAddress { get; }
  public bool ParticipatesInWeeklyGoal { get; }

  private static string? NormalizeNickname(string? nickname)
  {
    var normalized = nickname?.Trim();

    if (string.IsNullOrEmpty(normalized))
    {
      return null;
    }

    Rules.Require(
      normalized.Length <= Family.MaximumNicknameLength,
      DomainError.InvalidValue,
      $"A nickname cannot exceed {Family.MaximumNicknameLength} characters."
    );

    return normalized;
  }
}
