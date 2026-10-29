using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Family;

public sealed class Family : AggregateRoot<FamilyId>
{
  public const int MaximumNicknameLength = 18;

  private readonly List<FamilyMember> _members = new();

  public Family(FamilyId id, FamilyMemberId administratorId, string administratorName, IanaTimeZone timeZone)
    : base(id)
  {
    ArgumentNullException.ThrowIfNull(administratorId);
    ArgumentNullException.ThrowIfNull(timeZone);
    _members.Add(
      new FamilyMember(administratorId, administratorName, MemberRole.Guardian, GuardianAccess.Admin, false)
    );
    TimeZone = timeZone;
  }

  public IReadOnlyList<FamilyMember> Members
  {
    get
    {

      return _members.AsReadOnly();
    }
  }

  public IanaTimeZone TimeZone
  {
    get; private set;
  }
  public FamilyCurrency Currency { get; private set; } = FamilyCurrency.CHF;
  public ExchangeRate ExchangeRate { get; private set; } = new(1);
  public FamilyMemberId? ActiveChildId { get; private set; }

  public FamilyMember ActiveMember(FamilyMemberId id)
  {
    ArgumentNullException.ThrowIfNull(id);
    var member =
      _members.Find(item => item.Id == id)
      ?? throw new DomainRuleException(DomainError.Forbidden, "An active family member is required.");
    Rules.Require(!member.InvitationPending, DomainError.Forbidden, "An active family member is required.");

    return member;
  }

  public void RequireAdministrator(FamilyMemberId actor)
  {
    Rules.Require(
      ActiveMember(actor).IsAdministrator,
      DomainError.Forbidden,
      "Only an administrator may manage this content."
    );
  }

  public void AddMember(
    FamilyMemberId actor,
    FamilyMemberId id,
    string name,
    MemberRole role,
    GuardianAccess access,
    bool invitationPending,
    DateTimeOffset now
  )
  {
    RequireAdministrator(actor);
    ArgumentNullException.ThrowIfNull(id);
    Rules.Require(_members.All(item => item.Id != id), DomainError.Duplicate, "The member already exists.");
    var member = new FamilyMember(id, name, role, access, invitationPending);
    _members.Add(member);

    if (role == MemberRole.Child && !invitationPending && ActiveChildId is null)
    {
      ActiveChildId = id;
    }

    AddDomainEvent(new FamilyMemberChanged(Id, id, invitationPending ? "Invited" : "Added", now));
  }

  public void InviteGuardian(
    FamilyMemberId actor,
    FamilyMemberId id,
    string name,
    string emailAddress,
    GuardianAccess access,
    DateTimeOffset now
  )
  {
    var normalizedName = Rules.Text(name);
    Rules.Require(normalizedName.Length >= 2, DomainError.InvalidValue, "A guardian name requires at least two characters.");
    var normalizedEmail = Rules.Text(emailAddress);
    Rules.Require(
      normalizedEmail.Contains('@', StringComparison.Ordinal)
        && normalizedEmail.IndexOf('@', StringComparison.Ordinal) > 0
        && normalizedEmail.LastIndexOf('.') > normalizedEmail.IndexOf('@', StringComparison.Ordinal) + 1,
      DomainError.InvalidValue,
      "A valid email address is required."
    );
    AddMember(actor, id, normalizedName, MemberRole.Guardian, access, true, now);
    var member = _members.Find(item => item.Id == id) ?? throw new InvalidOperationException();
    member.SetEmailAddress(normalizedEmail);
    member.SetWeeklyGoalParticipation(false);
  }

  public void SaveNickname(FamilyMemberId actor, FamilyMemberId activeChildId, string nickname, DateTimeOffset now)
  {
    var member = ActiveMember(actor);
    Rules.Require(member.Role == MemberRole.Child, DomainError.Forbidden, "Only a child can save a child nickname.");
    Rules.Require(actor == activeChildId && ActiveChildId == actor, DomainError.Forbidden, "Only the active child can save their nickname.");
    var normalized = nickname.Trim();

    if (normalized.Length > MaximumNicknameLength)
    {
      normalized = normalized[..MaximumNicknameLength];
    }

    member.SetNickname(normalized.Length == 0 ? null : normalized);
    AddDomainEvent(new FamilyMemberChanged(Id, actor, "NicknameChanged", now));
  }

  public void SetGuardianWeeklyGoalParticipation(
    FamilyMemberId actor,
    FamilyMemberId guardianId,
    bool participates,
    DateTimeOffset now
  )
  {
    var actorMember = ActiveMember(actor);
    var guardian = ActiveMember(guardianId);
    Rules.Require(guardian.Role == MemberRole.Guardian, DomainError.InvalidValue, "Weekly participation can be changed only for guardians.");
    Rules.Require(
      actor == guardianId || actorMember.IsAdministrator,
      DomainError.Forbidden,
      "Only that guardian or an administrator may change participation."
    );
    guardian.SetWeeklyGoalParticipation(participates);
    AddDomainEvent(new FamilyMemberChanged(Id, guardianId, "WeeklyGoalParticipationChanged", now));
  }

  public void SelectActiveChild(FamilyMemberId actor, FamilyMemberId childId, DateTimeOffset now)
  {
    var actorMember = ActiveMember(actor);
    var child = ActiveMember(childId);
    Rules.Require(actorMember.Role == MemberRole.Guardian, DomainError.Forbidden, "Only guardians may select the active child.");
    Rules.Require(child.Role == MemberRole.Child, DomainError.InvalidValue, "The active profile must be a child.");
    ActiveChildId = childId;
    AddDomainEvent(new FamilyMemberChanged(Id, childId, "ActiveChildSelected", now));
  }

  public void AcceptInvitation(FamilyMemberId memberId, DateTimeOffset now)
  {
    ArgumentNullException.ThrowIfNull(memberId);
    var member =
      _members.Find(item => item.Id == memberId)
      ?? throw new DomainRuleException(DomainError.InvalidState, "No pending invitation exists.");
    Rules.Require(member.InvitationPending, DomainError.InvalidState, "No pending invitation exists.");
    member.AcceptInvitation();
    AddDomainEvent(new FamilyMemberChanged(Id, memberId, "InvitationAccepted", now));
  }

  public void RenameMember(FamilyMemberId actor, FamilyMemberId memberId, string name, DateTimeOffset now)
  {
    RequireAdministrator(actor);
    var member = ActiveMember(memberId);
    var normalized = Rules.Text(name);
    member.Rename(normalized);
    AddDomainEvent(new FamilyMemberChanged(Id, memberId, "Renamed", now));
  }

  public void ChangeGuardianAccess(
    FamilyMemberId actor,
    FamilyMemberId memberId,
    GuardianAccess access,
    DateTimeOffset now
  )
  {
    RequireAdministrator(actor);
    var member = ActiveMember(memberId);
    Rules.Defined(access);
    Rules.Require(member.Role == MemberRole.Guardian, DomainError.InvalidValue, "Only guardians have guardian access.");
    Rules.Require(
      !member.IsAdministrator || access == GuardianAccess.Admin || _members.Count(item => item.IsAdministrator) > 1,
      DomainError.LastAdministrator,
      "The family must retain an active administrator."
    );
    member.ChangeAccess(access);
    AddDomainEvent(new FamilyMemberChanged(Id, memberId, "AccessChanged", now));
  }

  public void UpdateSettings(
    FamilyMemberId actor,
    IanaTimeZone zone,
    FamilyCurrency currency,
    ExchangeRate rate,
    DateTimeOffset now
  )
  {
    RequireAdministrator(actor);
    ArgumentNullException.ThrowIfNull(zone);
    ArgumentNullException.ThrowIfNull(rate);
    Rules.Defined(currency);
    TimeZone = zone;
    Currency = currency;
    ExchangeRate = rate;
    AddDomainEvent(new FamilySettingsChanged(Id, now));
  }
}
