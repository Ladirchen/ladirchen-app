using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using LadirchenApp.Domain.Shared;
using LadirchenApp.Prototypes;

namespace LadirchenApp.Domain.Economy;

public sealed class SavingGoal : Entity<SavingGoalId>
{
  private readonly HashSet<FamilyMemberId> _participants;
  private readonly Dictionary<FamilyMemberId, decimal> _refundableContributions = new();
  private readonly ReadOnlyDictionary<FamilyMemberId, decimal> _readOnlyRefundableContributions;

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
    _readOnlyRefundableContributions = new ReadOnlyDictionary<FamilyMemberId, decimal>(
      _refundableContributions
    );
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
  public IReadOnlyDictionary<FamilyMemberId, decimal> RefundableContributions
  {
    get
    {
      return _readOnlyRefundableContributions;
    }
  }
  public GoalCancellationRequest? CancellationRequest
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
      if (Completed)
      {
        return Saved;
      }

      return Owner.MemberId is null
        ? Math.Max(0, Saved - StarterBonus)
        : WithdrawableBy(Owner.MemberId);
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

  internal void AddRefundableContribution(FamilyMemberId contributorId, decimal amount)
  {
    ArgumentNullException.ThrowIfNull(contributorId);
    Rules.Require(amount > 0, DomainError.InvalidValue, "A refundable contribution must be positive.");
    var saved = checked(Saved + amount);
    var previous = _refundableContributions.GetValueOrDefault(contributorId);
    var contributed = checked(previous + amount);
    Saved = saved;
    _refundableContributions[contributorId] = contributed;
  }

  internal decimal WithdrawableBy(FamilyMemberId memberId)
  {
    ArgumentNullException.ThrowIfNull(memberId);

    return Completed
      ? Saved
      : _refundableContributions.GetValueOrDefault(memberId);
  }

  internal void Withdraw(FamilyMemberId memberId, decimal amount)
  {
    ArgumentNullException.ThrowIfNull(memberId);
    Rules.Require(amount > 0, DomainError.InvalidValue, "A savings withdrawal must be positive.");
    Rules.Require(
      amount <= WithdrawableBy(memberId),
      DomainError.InvalidValue,
      "The withdrawal exceeds available savings."
    );
    var ownContribution = _refundableContributions.GetValueOrDefault(memberId);
    var remainingContribution = ownContribution - Math.Min(ownContribution, amount);
    Saved -= amount;

    if (remainingContribution == 0)
    {
      _refundableContributions.Remove(memberId);
    }
    else
    {
      _refundableContributions[memberId] = remainingContribution;
    }
  }

  internal void RequestCancellation(FamilyMemberId requesterId)
  {
    ArgumentNullException.ThrowIfNull(requesterId);
    Rules.Require(Owner.IsFamily, DomainError.InvalidState, "Only a family goal uses cancellation consent.");
    Rules.Require(
      _participants.Contains(requesterId),
      DomainError.Forbidden,
      "Only a selected goal participant may request cancellation."
    );
    Rules.Require(
      CancellationRequest is null,
      DomainError.InvalidState,
      "A cancellation request is already pending."
    );
    CancellationRequest = new GoalCancellationRequest(requesterId);
  }

  internal void DecideCancellation(FamilyMemberId memberId, bool approved)
  {
    ArgumentNullException.ThrowIfNull(memberId);
    Rules.Require(
      CancellationRequest is not null,
      DomainError.InvalidState,
      "No goal-cancellation request is pending."
    );
    Rules.Require(
      _participants.Contains(memberId),
      DomainError.Forbidden,
      "Only a selected goal participant may decide a cancellation request."
    );
    Rules.Require(
      !CancellationRequest!.HasConsented(memberId),
      DomainError.Duplicate,
      "This family member already consented to the goal cancellation."
    );

    if (approved)
    {
      CancellationRequest.Approve(memberId);
    }
    else
    {
      CancellationRequest = null;
    }
  }

  internal void RestoreCancellationRequest(
    FamilyMemberId requestedByMemberId,
    IReadOnlyCollection<FamilyMemberId> consentedMemberIds
  )
  {
    ArgumentNullException.ThrowIfNull(requestedByMemberId);
    ArgumentNullException.ThrowIfNull(consentedMemberIds);
    Rules.Require(Owner.IsFamily, DomainError.InvalidState, "Only a family goal uses cancellation consent.");
    Rules.Require(
      _participants.Contains(requestedByMemberId),
      DomainError.InvalidValue,
      "The cancellation requester must be a selected goal participant."
    );
    Rules.Require(
      consentedMemberIds.Contains(requestedByMemberId),
      DomainError.InvalidValue,
      "The cancellation requester must have consented."
    );
    Rules.Require(
      consentedMemberIds.All(_participants.Contains),
      DomainError.InvalidValue,
      "Every cancellation consent must belong to a selected goal participant."
    );
    Rules.Require(
      consentedMemberIds.Count == consentedMemberIds.Distinct().Count(),
      DomainError.InvalidValue,
      "A cancellation consent cannot be restored more than once."
    );
    Rules.Require(
      !new HashSet<FamilyMemberId>(consentedMemberIds).SetEquals(_participants),
      DomainError.InvalidValue,
      "A unanimously approved cancellation request cannot remain pending."
    );
    CancellationRequest = new GoalCancellationRequest(requestedByMemberId, consentedMemberIds);
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
    IReadOnlyDictionary<FamilyMemberId, decimal> refundableContributions,
    bool cheered,
    bool cancelled,
    IsoWeekPeriod? lastInterestPeriod,
    Percentage? lastInterestRate
  )
  {
    ArgumentNullException.ThrowIfNull(refundableContributions);
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
    _refundableContributions.Clear();

    foreach (var contribution in refundableContributions)
    {
      ArgumentNullException.ThrowIfNull(contribution.Key);
      Rules.Require(
        contribution.Value > 0,
        DomainError.InvalidValue,
        "A restored refundable contribution must be positive."
      );
      _refundableContributions.Add(contribution.Key, contribution.Value);
    }

    Rules.Require(
      _refundableContributions.Values.Sum() <= Math.Max(0, Saved - StarterBonus),
      DomainError.InvalidValue,
      "Refundable contributions must be covered by the goal balance."
    );
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
    _refundableContributions.Clear();
    CancellationRequest = null;
  }
  internal void ToggleCheer()
  {
    Cheered = !Cheered;
  }
}
