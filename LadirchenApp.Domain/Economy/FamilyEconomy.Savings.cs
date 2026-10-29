using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

public sealed partial class FamilyEconomy
{
  private readonly List<SavingGoal> _goals = new();
  private readonly List<GuardianGoalGift> _guardianGifts = new();

  public IReadOnlyList<GuardianGoalGift> GuardianGoalGifts
  {
    get
    {
      return _guardianGifts.AsReadOnly();
    }
  }

  public IReadOnlyList<SavingGoal> GoalsFor(FamilyMemberId actor)
  {
    var member = ActiveMember(actor);

    return _goals.Where(goal => !goal.Cancelled && CanViewGoal(member, goal)).ToList().AsReadOnly();
  }

  public SavingGoal GoalFor(FamilyMemberId actor, SavingGoalId goalId)
  {
    var member = ActiveMember(actor);
    var goal = FindGoal(goalId);
    Rules.Require(CanViewGoal(member, goal), DomainError.Forbidden, "The goal is not visible to this member.");

    return goal;
  }

  public void AddGoal(
    FamilyMemberId actor,
    SavingGoalId id,
    SavingGoalDefinition definition,
    TransactionId operationId,
    DateTimeOffset now
  )
  {
    ArgumentNullException.ThrowIfNull(definition);
    var member = ActiveMember(actor);
    var owner = definition.Owner;
    ArgumentNullException.ThrowIfNull(owner);
    var ownerMember = owner.IsFamily ? null : ActiveMember(owner.MemberId!);
    Rules.Require(
      owner.IsFamily
        ? member.IsAdministrator || member.Role == MemberRole.Child
        : owner.MemberId == actor || member.IsAdministrator && ownerMember?.Role == MemberRole.Child,
      DomainError.Forbidden,
      "The member cannot create this goal."
    );
    Rules.Require(_goals.All(item => item.Id != id), DomainError.Duplicate, "This goal identity was already used.");
    RequireNewOperation(operationId);
    Rules.Defined(definition.Visibility);
    var participants = definition.ParticipantIds?.Distinct().ToArray() ?? Array.Empty<FamilyMemberId>();

    if (owner.IsFamily && participants.Length == 0)
    {
      var eligible = _members
        .Where(item => !item.InvitationPending)
        .Where(item => item.Role is MemberRole.Child or MemberRole.Guardian)
        .Select(item => item.Id);
      participants = eligible.ToArray();
    }

    Rules.Require(
      !owner.IsFamily || participants.Length > 0,
      DomainError.InvalidValue,
      "A family savings goal requires at least one participant."
    );
    Rules.Require(
      owner.IsFamily || participants.Length == 0,
      DomainError.InvalidValue,
      "A personal savings goal cannot define family participants."
    );

    foreach (var participant in participants)
    {
      ActiveMember(participant);
    }

    var goal = new SavingGoal(
      id,
      owner,
      definition.Title,
      definition.Target,
      definition.Visibility,
      now,
      participants
    );
    _goals.Add(goal);
    Record(
      operationId,
      new LedgerEntry(
        operationId,
        owner.MemberId ?? actor,
        id,
        LedgerKind.GoalCredit,
        goal.StarterBonus,
        id,
        "Goal starter bonus",
        now.ToUniversalTime(),
        actor
      )
    );
    AddDomainEvent(new GoalCreated(Id, id, owner, goal.StarterBonus, now));

    if (goal.Completed)
    {
      IssueVoucherIfCompleted(goal);
      AddDomainEvent(new GoalCompleted(Id, id, now));
    }
  }

  public void UpdateGoal(
    FamilyMemberId actor,
    SavingGoalId id,
    string title,
    LadirchenAmount target,
    GoalVisibility visibility,
    DateTimeOffset now
  )
  {
    var goal = GoalFor(actor, id);
    RequireNoPendingGoalCancellation(goal);
    Rules.Require(
      _savingsVouchers.All(item => item.Id != id),
      DomainError.InvalidState,
      "A completed goal with a voucher cannot be edited."
    );
    var member = ActiveMember(actor);
    Rules.Require(
      goal.Owner.MemberId == actor || member.IsAdministrator,
      DomainError.Forbidden,
      "Only the owner or an administrator may edit a visible goal."
    );
    var normalized = Rules.Text(title);
    var amount = Rules.Positive(target);
    Rules.Require(amount >= goal.Saved, DomainError.InvalidValue, "The target cannot be lower than savings.");
    Rules.Defined(visibility);
    var wasCompleted = goal.Completed;
    goal.Update(normalized, amount, goal.Owner.IsFamily ? GoalVisibility.Family : visibility);
    AddDomainEvent(new GoalChanged(Id, id, "Updated", 0, now));

    if (!wasCompleted && goal.Completed)
    {
      IssueVoucherIfCompleted(goal);
      AddDomainEvent(new GoalCompleted(Id, id, now));
    }
  }

  public decimal DepositToGoal(
    FamilyMemberId actor,
    SavingGoalId id,
    LadirchenAmount amount,
    TransactionId operationId,
    DateTimeOffset now
  )
  {
    var goal = GoalFor(actor, id);
    Rules.Require(
      goal.Owner.MemberId == actor,
      DomainError.Forbidden,
      "Use the gift workflow for another owner's goal."
    );
    var value = Math.Min(Rules.Round(Rules.Positive(amount)), goal.Target - goal.Saved);
    RequireNewOperation(operationId);
    Rules.Require(value > 0, DomainError.InvalidState, "The goal is already complete.");
    var wallet = WalletFor(actor);
    Rules.Require(value <= wallet.Available, DomainError.InsufficientFunds, "Reserved funds cannot be deposited.");
    goal.AddRefundableContribution(actor, value);
    wallet.Set(wallet.Balance - value, wallet.Reserved);

    Record(
      operationId,
      new LedgerEntry(operationId, actor, null, LedgerKind.Debit, value, id, "Saving deposit", now.ToUniversalTime()),
      new LedgerEntry(operationId, actor, id, LedgerKind.GoalCredit, value, id, "Saving deposit", now.ToUniversalTime())
    );
    AddDomainEvent(new GoalChanged(Id, id, "Deposited", value, now));

    if (goal.Completed)
    {
      IssueVoucherIfCompleted(goal);
      AddDomainEvent(new GoalCompleted(Id, id, now));
    }

    return value;
  }

  public decimal WithdrawFromGoal(
    FamilyMemberId actor,
    SavingGoalId id,
    LadirchenAmount amount,
    TransactionId operationId,
    DateTimeOffset now
  )
  {
    var goal = GoalFor(actor, id);
    Rules.Require(
      _savingsVouchers.All(item => item.Id != id),
      DomainError.InvalidState,
      "A completed goal voucher must be redeemed instead."
    );
    Rules.Require(goal.Owner.MemberId == actor, DomainError.Forbidden, "Only the owner may withdraw savings.");
    var value = Math.Min(Rules.Positive(amount), goal.WithdrawableBy(actor));
    RequireNewOperation(operationId);
    Rules.Require(value > 0, DomainError.InvalidState, "The starter bonus is locked until completion.");
    var wallet = WalletFor(actor);
    var balance = checked(wallet.Balance + value);
    goal.Withdraw(actor, value);
    wallet.Set(balance, wallet.Reserved);
    Record(
      operationId,
      new LedgerEntry(
        operationId,
        actor,
        goal.Id,
        LedgerKind.GoalDebit,
        value,
        id,
        "Saving withdrawal",
        now.ToUniversalTime()
      ),
      new LedgerEntry(
        operationId,
        actor,
        null,
        LedgerKind.Credit,
        value,
        id,
        "Saving withdrawal",
        now.ToUniversalTime()
      )
    );
    AddDomainEvent(new GoalChanged(Id, id, "Withdrawn", value, now));

    return value;
  }

  public decimal CancelGoal(FamilyMemberId actor, SavingGoalId id, TransactionId operationId, DateTimeOffset now)
  {
    var goal = GoalFor(actor, id);
    Rules.Require(
      _savingsVouchers.All(item => item.Id != id),
      DomainError.InvalidState,
      "A completed goal voucher must be redeemed instead."
    );
    Rules.Require(
      !goal.Owner.IsFamily,
      DomainError.InvalidState,
      "A family goal requires a cancellation request and unanimous participant approval."
    );
    Rules.Require(
      goal.Owner.MemberId == actor,
      DomainError.Forbidden,
      "Only the owner may cancel a personal goal."
    );
    RequireNewOperation(operationId);

    return CompleteGoalCancellation(actor, goal, operationId, now);
  }

  public void RequestGoalCancellation(
    FamilyMemberId actor,
    SavingGoalId id,
    TransactionId operationId,
    DateTimeOffset now
  )
  {
    var goal = GoalFor(actor, id);
    Rules.Require(
      _savingsVouchers.All(item => item.Id != id),
      DomainError.InvalidState,
      "A completed goal voucher must be redeemed instead."
    );
    RequireNewOperation(operationId);
    goal.RequestCancellation(actor);

    if (goal.CancellationRequest!.IsUnanimous(goal.Participants))
    {
      _ = CompleteGoalCancellation(actor, goal, operationId, now);

      return;
    }

    Record(operationId);
    AddDomainEvent(new GoalChanged(Id, id, "CancellationRequested", 0, now));
  }

  public bool DecideGoalCancellation(
    FamilyMemberId actor,
    SavingGoalId id,
    bool approved,
    TransactionId operationId,
    DateTimeOffset now
  )
  {
    var goal = GoalFor(actor, id);
    Rules.Require(
      _savingsVouchers.All(item => item.Id != id),
      DomainError.InvalidState,
      "A completed goal voucher must be redeemed instead."
    );
    RequireNewOperation(operationId);
    goal.DecideCancellation(actor, approved);

    if (!approved)
    {
      Record(operationId);
      AddDomainEvent(new GoalChanged(Id, id, "CancellationRejected", 0, now));

      return false;
    }

    if (!goal.CancellationRequest!.IsUnanimous(goal.Participants))
    {
      Record(operationId);
      AddDomainEvent(new GoalChanged(Id, id, "CancellationApproved", 0, now));

      return false;
    }

    _ = CompleteGoalCancellation(actor, goal, operationId, now);

    return true;
  }

  private void RequireNoPendingGoalCancellation(SavingGoal goal)
  {
    Rules.Require(
      goal.CancellationRequest is null,
      DomainError.InvalidState,
      "The pending cancellation request must be resolved first."
    );
  }

  private decimal CompleteGoalCancellation(
    FamilyMemberId actor,
    SavingGoal goal,
    TransactionId operationId,
    DateTimeOffset now
  )
  {
    var refunds = goal.RefundableContributions.ToArray();
    var returned = refunds.Sum(item => item.Value);
    var forfeited = goal.Saved - returned;
    Rules.Require(forfeited >= 0, DomainError.InvalidState, "Refundable contributions exceed the goal balance.");
    var refundBalances = refunds.Select(refund =>
    {
      var wallet = WalletFor(refund.Key);

      return (
        Wallet: wallet,
        Balance: checked(wallet.Balance + refund.Value),
        MemberId: refund.Key,
        Amount: refund.Value
      );
    }).ToArray();
    var saved = goal.Saved;
    goal.Cancel();
    var entries = new List<LedgerEntry>
    {
      new LedgerEntry(
        operationId,
        actor,
        goal.Id,
        LedgerKind.GoalDebit,
        saved,
        goal.Id,
        "Goal cancellation",
        now.ToUniversalTime()
      ),
    };

    foreach (var refund in refundBalances)
    {
      refund.Wallet.Set(refund.Balance, refund.Wallet.Reserved);
      entries.Add(new LedgerEntry(
        operationId,
        refund.MemberId,
        null,
        LedgerKind.Credit,
        refund.Amount,
        goal.Id,
        "Goal cancellation refund",
        now.ToUniversalTime(),
        actor
      ));
    }

    if (forfeited > 0)
    {
      entries.Add(new LedgerEntry(
        operationId,
        actor,
        goal.Id,
        LedgerKind.BonusForfeited,
        forfeited,
        goal.Id,
        "Non-refundable goal credits forfeited",
        now.ToUniversalTime()
      ));
    }

    Record(operationId, entries.ToArray());
    AddDomainEvent(new GoalChanged(Id, goal.Id, "Cancelled", returned, now));

    return returned;
  }

  public decimal SupportGoal(
    FamilyMemberId actor,
    SavingGoalId id,
    LadirchenAmount amount,
    TransactionId operationId,
    DateTimeOffset now,
    GuardianGiftId? giftId = null
  )
  {
    var member = ActiveMember(actor);
    Rules.Require(member.Role == MemberRole.Guardian, DomainError.Forbidden, "Only guardians may issue goal support.");
    var goal = GoalFor(actor, id);
    RequireNoPendingGoalCancellation(goal);

    if (!goal.Owner.IsFamily)
    {
      RequireChild(goal.Owner.MemberId!);
      RequireNewPendingGiftId(giftId);
    }

    var value = Math.Min(Rules.Round(Rules.Positive(amount)), goal.Target - goal.Saved);
    RequireNewOperation(operationId);
    Rules.Require(value > 0, DomainError.InvalidState, "There is no positive support to apply.");
    var saved = checked(goal.Saved + value);
    goal.SetSaved(saved);

    if (!goal.Owner.IsFamily)
    {
      AddPendingGift(
        new GuardianGoalGift(giftId, goal.Owner.MemberId!, actor, value, goal.Id, GiftDestination.Goal)
      );
    }

    Record(
      operationId,
      new LedgerEntry(
        operationId,
        goal.Owner.MemberId ?? actor,
        id,
        LedgerKind.GoalCredit,
        value,
        id,
        "Guardian support",
        now.ToUniversalTime(),
        actor
      )
    );
    AddDomainEvent(new GoalChanged(Id, id, "Supported", value, now));

    if (goal.Completed)
    {
      IssueVoucherIfCompleted(goal);
      AddDomainEvent(new GoalCompleted(Id, id, now));
    }

    return value;
  }

  public decimal GiftToGoal(
    FamilyMemberId actor,
    SavingGoalId id,
    LadirchenAmount amount,
    TransactionId operationId,
    DateTimeOffset now,
    GuardianGiftId? giftId = null
  )
  {
    RequireChild(actor);
    var goal = GoalFor(actor, id);
    RequireNoPendingGoalCancellation(goal);
    Rules.Require(
      goal.Owner.MemberId != actor && goal.Visibility == GoalVisibility.Family,
      DomainError.Forbidden,
      "Only other members' public goals or family goals may receive child gifts."
    );

    if (!goal.Owner.IsFamily)
    {
      RequireChild(goal.Owner.MemberId!);
      RequireNewPendingGiftId(giftId);
    }

    var value = Math.Min(Rules.Round(Rules.Positive(amount)), goal.Target - goal.Saved);
    RequireNewOperation(operationId);
    Rules.Require(value > 0, DomainError.InvalidState, "There is no positive gift to apply.");
    var wallet = WalletFor(actor);
    Rules.Require(value <= wallet.Available, DomainError.InsufficientFunds, "The gift exceeds available funds.");
    goal.AddRefundableContribution(actor, value);
    wallet.Set(wallet.Balance - value, wallet.Reserved);
    Record(
      operationId,
      new LedgerEntry(operationId, actor, null, LedgerKind.Debit, value, id, "Gift to goal", now.ToUniversalTime()),
      new LedgerEntry(
        operationId,
        goal.Owner.MemberId ?? actor,
        id,
        LedgerKind.GoalCredit,
        value,
        id,
        "Gift to goal",
        now.ToUniversalTime(),
        actor
      )
    );
    AddDomainEvent(new LadirchenTransferred(Id, actor, goal.Owner.MemberId, id, value, operationId, now));

    if (!goal.Owner.IsFamily && giftId is not null)
    {
      AddPendingGift(
        new GuardianGoalGift(giftId, goal.Owner.MemberId!, actor, value, goal.Id, GiftDestination.Goal)
      );
    }

    if (goal.Completed)
    {
      IssueVoucherIfCompleted(goal);
      AddDomainEvent(new GoalCompleted(Id, id, now));
    }

    return value;
  }

  public void GiftToMember(
    FamilyMemberId actor,
    FamilyMemberId recipientId,
    LadirchenAmount amount,
    TransactionId operationId,
    DateTimeOffset now,
    GuardianGiftId? giftId = null
  )
  {
    RequireChild(actor);
    RequireChild(recipientId);
    Rules.Require(actor != recipientId, DomainError.InvalidValue, "A gift requires another child.");
    RequireNewPendingGiftId(giftId);
    var value = Rules.Round(Rules.Positive(amount));
    Rules.Require(value > 0, DomainError.InvalidValue, "The rounded gift must be positive.");
    RequireNewOperation(operationId);
    var sender = WalletFor(actor);
    var recipient = WalletFor(recipientId);
    Rules.Require(value <= sender.Available, DomainError.InsufficientFunds, "The gift exceeds available funds.");
    var recipientBalance = checked(recipient.Balance + value);
    sender.Set(sender.Balance - value, sender.Reserved);
    recipient.Set(recipientBalance, recipient.Reserved);
    Record(
      operationId,
      new LedgerEntry(
        operationId,
        actor,
        null,
        LedgerKind.Debit,
        value,
        recipientId,
        "Gift to family member",
        now.ToUniversalTime()
      ),
      new LedgerEntry(
        operationId,
        recipientId,
        null,
        LedgerKind.Credit,
        value,
        actor,
        "Gift from family member",
        now.ToUniversalTime(),
        actor
      )
    );
    AddDomainEvent(new LadirchenTransferred(Id, actor, recipientId, null, value, operationId, now));

    if (giftId is not null)
    {
      AddPendingGift(
        new GuardianGoalGift(giftId, recipientId, actor, value, null, GiftDestination.Balance)
      );
    }
  }

  public void ToggleGoalCheer(FamilyMemberId actor, SavingGoalId id, DateTimeOffset now)
  {
    var goal = GoalFor(actor, id);
    goal.ToggleCheer();
    AddDomainEvent(new GoalChanged(Id, id, "CheerChanged", 0, now));
  }

  private SavingGoal FindGoal(SavingGoalId id)
  {
    ArgumentNullException.ThrowIfNull(id);

    return _goals.Find(item => item.Id == id && !item.Cancelled)
      ?? throw new DomainRuleException(DomainError.NotFound, "The goal does not exist or was cancelled.");
  }

  private static bool CanViewGoal(FamilyEconomyMember member, SavingGoal goal)
  {
    return goal.Owner.MemberId == member.Id
      || goal.Visibility == GoalVisibility.Family
      || goal.Visibility == GoalVisibility.Guardians && member.IsAdministrator;
  }
}
