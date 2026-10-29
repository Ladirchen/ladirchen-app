using System;

using LadirchenApp.Domain.Avatar;
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
    bool participatesInWeeklyGoal,
    string? avatar = null,
    string? color = null,
    AvatarAppearance? appearance = null
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
    Avatar = NormalizeOptionalText(avatar);
    Color = NormalizeOptionalText(color);

    if (appearance is not null)
    {
      new AvatarCatalog().Validate(
        appearance,
        role == MemberRole.Child ? AvatarProfile.Child : AvatarProfile.Guardian
      );
    }

    Appearance = appearance;
  }

  public FamilyMemberId Id { get; }
  public string Name { get; }
  public MemberRole Role { get; }
  public GuardianAccess Access { get; }
  public bool InvitationPending { get; }
  public string? Nickname { get; }
  public string? EmailAddress { get; }
  public bool ParticipatesInWeeklyGoal { get; }
  public string? Avatar { get; }
  public string? Color { get; }
  public AvatarAppearance? Appearance { get; }

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

  private static string? NormalizeOptionalText(string? value)
  {
    var normalized = value?.Trim();

    return string.IsNullOrEmpty(normalized) ? null : normalized;
  }
}
