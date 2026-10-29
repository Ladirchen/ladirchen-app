using System;
using System.Collections.Generic;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Economy;

public sealed class SavingGoal : Entity<SavingGoalId>
{
  private readonly HashSet<FamilyMemberId> _participants;

  public const decimal StarterBonusPerMember = 5;

  internal SavingGoal(
    SavingGoalId id,
    GoalOwner owner,
    string title,
    LadirchenAmount target,
    GoalVisibility visibility,
    DateTimeOffset createdAt,
    IEnumerable<FamilyMemberId>? participantIds = null
  ) : base(id)
  {
    ArgumentNullException.ThrowIfNull(owner);
    Owner = owner;
    Title = Rules.Text(title);
    Target = Rules.Positive(target);
    Visibility = owner.IsFamily ? GoalVisibility.Family : Rules.Defined(visibility);
    _participants = owner.IsFamily
      ? new HashSet<FamilyMemberId>(participantIds ?? Array.Empty<FamilyMemberId>())
      : new HashSet<FamilyMemberId>();
    StarterBonus = owner.IsFamily
      ? Math.Min(StarterBonusPerMember * _participants.Count, Target)
      : Math.Min(StarterBonusPerMember, Target);
    Saved = StarterBonus;
    CreatedAt = createdAt.ToUniversalTime();
  }

  public GoalOwner Owner
  {
    get;
  }
  public string Title
  {
    get; private set;
  }
  public decimal Target
  {
    get; private set;
  }
  public decimal Saved
  {
    get; private set;
  }
  public decimal StarterBonus
  {
    get;
  }
  public decimal InterestEarned
  {
    get; private set;
  }
  public IReadOnlySet<FamilyMemberId> Participants
  {
    get
    {
      return _participants;
    }
  }
  public GoalVisibility Visibility
  {
    get; private set;
  }
  public bool Cheered
  {
    get; private set;
  }
  public bool Cancelled
  {
    get; private set;
  }
  public bool Completed
  {
    get
    {

      return Saved >= Target;
    }
  }

  public decimal Withdrawable
  {
    get
    {

      return Completed ? Saved : Math.Max(0, Saved - StarterBonus);
    }
  }

  public DateTimeOffset CreatedAt
  {
    get;
  }
  public IsoWeekPeriod? LastInterestPeriod { get; private set; }
  public Percentage? LastInterestRate { get; private set; }

  internal void SetSaved(decimal saved)
  {
    Saved = saved;
  }

  internal void SetInterest(
    decimal saved,
    decimal interestEarned,
    IsoWeekPeriod? period = null,
    Percentage? rate = null
  )
  {
    Saved = saved;
    InterestEarned = interestEarned;
    LastInterestPeriod = period;
    LastInterestRate = rate;
  }

  internal void Restore(
    decimal saved,
    decimal starterBonus,
    decimal interestEarned,
    bool cheered,
    bool cancelled,
    IsoWeekPeriod? lastInterestPeriod,
    Percentage? lastInterestRate
  )
  {
    Rules.Require(saved >= 0, DomainError.InvalidValue, "Restored savings cannot be negative.");
    Rules.Require(
      starterBonus == StarterBonus,
      DomainError.InvalidValue,
      "The restored starter bonus does not match the goal participants."
    );
    Rules.Require(
      interestEarned >= 0 && interestEarned <= saved,
      DomainError.InvalidValue,
      "The restored interest must be covered by the goal balance."
    );
    Saved = cancelled ? 0 : saved;
    InterestEarned = interestEarned;
    Cheered = cheered;
    Cancelled = cancelled;
    LastInterestPeriod = lastInterestPeriod;
    LastInterestRate = lastInterestRate;
  }
  internal void Update(string title, decimal target, GoalVisibility visibility)
  {
    Title = title;
    Target = target;
    Visibility = visibility;
  }
  internal void Cancel()
  {
    Cancelled = true;
    Saved = 0;
  }
  internal void ToggleCheer()
  {
    Cheered = !Cheered;
  }
}
