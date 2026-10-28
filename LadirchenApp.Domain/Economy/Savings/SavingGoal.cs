using System;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Economy;

public sealed class SavingGoal : Entity<SavingGoalId>
{
  internal SavingGoal(
    SavingGoalId id,
    GoalOwner owner,
    string title,
    LadirchenAmount target,
    GoalVisibility visibility,
    DateTimeOffset createdAt,
    int familyMemberCount
  ) : base(id)
  {
    ArgumentNullException.ThrowIfNull(owner);
    Owner = owner;
    Title = Rules.Text(title);
    Target = Rules.Positive(target);
    Visibility = owner.IsFamily ? GoalVisibility.Family : Rules.Defined(visibility);
    StarterBonus = owner.IsFamily ? Math.Min(5m * familyMemberCount, Target) : Math.Min(5, Target);
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

  internal void SetSaved(decimal saved)
  {
    Saved = saved;
  }

  internal void SetInterest(decimal saved, decimal interestEarned)
  {
    Saved = saved;
    InterestEarned = interestEarned;
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
