using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

public sealed partial class FamilyEconomy
{
  private readonly List<SavingGoal> _goals = new();

  public IReadOnlyList<SavingGoal> GoalsFor(FamilyMemberId actor)
  {
    var member = _family.ActiveMember(actor);

    return _goals.Where(goal => !goal.Cancelled && CanViewGoal(member, goal)).ToList().AsReadOnly();
  }

  public SavingGoal GoalFor(FamilyMemberId actor, SavingGoalId goalId)
  {
    var member = _family.ActiveMember(actor);
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
    var member = _family.ActiveMember(actor);
    var owner = definition.Owner;
    ArgumentNullException.ThrowIfNull(owner);
    var ownerMember = owner.IsFamily ? null : _family.ActiveMember(owner.MemberId!);
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
    var goal = new SavingGoal(id, owner, definition.Title, definition.Target, definition.Visibility, now);
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
    var member = _family.ActiveMember(actor);
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
    var value = Math.Min(Rules.Positive(amount), goal.Target - goal.Saved);
    RequireNewOperation(operationId);
    Rules.Require(value > 0, DomainError.InvalidState, "The goal is already complete.");
    var wallet = WalletFor(actor);
    Rules.Require(value <= wallet.Available, DomainError.InsufficientFunds, "Reserved funds cannot be deposited.");
    var saved = checked(goal.Saved + value);
    wallet.Set(wallet.Balance - value, wallet.Reserved);
    goal.SetSaved(saved);
    Record(
      operationId,
      new LedgerEntry(operationId, actor, null, LedgerKind.Debit, value, id, "Saving deposit", now.ToUniversalTime()),
      new LedgerEntry(operationId, actor, id, LedgerKind.GoalCredit, value, id, "Saving deposit", now.ToUniversalTime())
    );
    AddDomainEvent(new GoalChanged(Id, id, "Deposited", value, now));

    if (goal.Completed)
    {
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
    Rules.Require(goal.Owner.MemberId == actor, DomainError.Forbidden, "Only the owner may withdraw savings.");
    var value = Math.Min(Rules.Positive(amount), goal.Withdrawable);
    RequireNewOperation(operationId);
    Rules.Require(value > 0, DomainError.InvalidState, "The starter bonus is locked until completion.");
    var wallet = WalletFor(actor);
    var balance = checked(wallet.Balance + value);
    goal.SetSaved(goal.Saved - value);
    wallet.Set(balance, wallet.Reserved);
    Record(
      operationId,
      new LedgerEntry(
        operationId,
        actor,
        id,
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
    Rules.Require(goal.Owner.MemberId == actor, DomainError.Forbidden, "Only the owner may cancel a personal goal.");
    RequireNewOperation(operationId);
    var returned = goal.Withdrawable;
    var forfeited = goal.Saved - returned;
    var wallet = WalletFor(actor);
    var balance = checked(wallet.Balance + returned);
    var saved = goal.Saved;
    goal.Cancel();
    wallet.Set(balance, wallet.Reserved);
    Record(
      operationId,
      new LedgerEntry(
        operationId,
        actor,
        id,
        LedgerKind.GoalDebit,
        saved,
        id,
        "Goal cancellation",
        now.ToUniversalTime()
      ),
      new LedgerEntry(
        operationId,
        actor,
        null,
        LedgerKind.Credit,
        returned,
        id,
        "Goal cancellation",
        now.ToUniversalTime()
      ),
      new LedgerEntry(
        operationId,
        actor,
        id,
        LedgerKind.BonusForfeited,
        forfeited,
        id,
        "Uncompleted starter bonus forfeited",
        now.ToUniversalTime()
      )
    );
    AddDomainEvent(new GoalChanged(Id, id, "Cancelled", returned, now));

    return returned;
  }

  public decimal SupportGoal(
    FamilyMemberId actor,
    SavingGoalId id,
    LadirchenAmount amount,
    TransactionId operationId,
    DateTimeOffset now
  )
  {
    var member = _family.ActiveMember(actor);
    Rules.Require(member.Role == MemberRole.Guardian, DomainError.Forbidden, "Only guardians may issue goal support.");
    var goal = GoalFor(actor, id);

    if (!goal.Owner.IsFamily)
    {
      RequireChild(goal.Owner.MemberId!);
    }

    var value = Math.Min(Rules.Round(Rules.Positive(amount)), goal.Target - goal.Saved);
    RequireNewOperation(operationId);
    Rules.Require(value > 0, DomainError.InvalidState, "There is no positive support to apply.");
    var saved = checked(goal.Saved + value);
    goal.SetSaved(saved);
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
      AddDomainEvent(new GoalCompleted(Id, id, now));
    }

    return value;
  }

  public decimal GiftToGoal(
    FamilyMemberId actor,
    SavingGoalId id,
    LadirchenAmount amount,
    TransactionId operationId,
    DateTimeOffset now
  )
  {
    RequireChild(actor);
    var goal = GoalFor(actor, id);
    Rules.Require(
      goal.Owner.MemberId != actor && goal.Visibility == GoalVisibility.Family,
      DomainError.Forbidden,
      "Only other members' public goals or family goals may receive child gifts."
    );

    if (!goal.Owner.IsFamily)
    {
      RequireChild(goal.Owner.MemberId!);
    }

    var value = Math.Min(Rules.Round(Rules.Positive(amount)), goal.Target - goal.Saved);
    RequireNewOperation(operationId);
    Rules.Require(value > 0, DomainError.InvalidState, "There is no positive gift to apply.");
    var wallet = WalletFor(actor);
    Rules.Require(value <= wallet.Available, DomainError.InsufficientFunds, "The gift exceeds available funds.");
    var saved = checked(goal.Saved + value);
    wallet.Set(wallet.Balance - value, wallet.Reserved);
    goal.SetSaved(saved);
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

    if (goal.Completed)
    {
      AddDomainEvent(new GoalCompleted(Id, id, now));
    }

    return value;
  }

  public void GiftToMember(
    FamilyMemberId actor,
    FamilyMemberId recipientId,
    LadirchenAmount amount,
    TransactionId operationId,
    DateTimeOffset now
  )
  {
    RequireChild(actor);
    RequireChild(recipientId);
    Rules.Require(actor != recipientId, DomainError.InvalidValue, "A gift requires another child.");
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

  private static bool CanViewGoal(FamilyMember member, SavingGoal goal)
  {
    return goal.Owner.MemberId == member.Id
      || goal.Visibility == GoalVisibility.Family
      || goal.Visibility == GoalVisibility.Guardians && member.IsAdministrator;
  }
}
