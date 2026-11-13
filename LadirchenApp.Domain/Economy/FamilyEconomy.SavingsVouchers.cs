using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

public sealed partial class FamilyEconomy
{
  private readonly List<GoalSavingsVoucher> _savingsVouchers = new();

  public IReadOnlyList<GoalSavingsVoucher> SavingsVouchers
  {
    get
    {
      return _savingsVouchers.AsReadOnly();
    }
  }

  public bool ConsentSavingsVoucher(
    FamilyMemberId actor,
    SavingGoalId voucherId,
    DateTimeOffset now
  )
  {
    ActiveMember(actor);
    var voucher = FindSavingsVoucher(voucherId);
    Rules.Require(voucher.OwnerId is null, DomainError.InvalidState, "A personal voucher needs no family consent.");
    Rules.Require(voucher.RedemptionCount == 0, DomainError.InvalidState, "A redeemed voucher cannot receive consent.");
    Rules.Require(
      voucher.Participants.Contains(actor),
      DomainError.Forbidden,
      "Only a voucher participant may consent."
    );
    var changed = voucher.Consent(actor);

    if (changed)
    {
      AddDomainEvent(new GoalChanged(Id, voucher.Id, "VoucherConsentChanged", 0, now));
    }

    return changed;
  }

  public void RedeemSavingsVoucher(
    FamilyMemberId actor,
    SavingGoalId voucherId,
    TransactionId operationId,
    DateTimeOffset now
  )
  {
    ActiveMember(actor);
    var voucher = FindSavingsVoucher(voucherId);
    var goal = FindGoal(voucherId);
    Rules.Require(
      voucher.OwnerId == actor || voucher.Participants.Contains(actor),
      DomainError.Forbidden,
      "Only the voucher owner or a participating family member may redeem it."
    );
    RequireNewOperation(operationId);
    Rules.Require(
      voucher.Redeem(now),
      DomainError.InvalidState,
      "The savings voucher is not redeemable or was already redeemed."
    );
    _guardianGifts.RemoveAll(gift => gift.GoalId == goal.Id);
    _savingsVouchers.Remove(voucher);
    _goals.Remove(goal);
    Record(
      operationId,
      new LedgerEntry(
        operationId,
        actor,
        goal.Id,
        LedgerKind.GoalDebit,
        voucher.Amount,
        goal.Id,
        "Savings voucher redeemed",
        now.ToUniversalTime()
      )
    );
    AddDomainEvent(new GoalChanged(Id, voucher.Id, "VoucherRedeemed", voucher.Amount, now));
  }

  public decimal ConvertSavingsVoucherToLadirchen(
    FamilyMemberId actor,
    SavingGoalId voucherId,
    TransactionId operationId,
    DateTimeOffset now
  )
  {
    ActiveMember(actor);
    var voucher = FindSavingsVoucher(voucherId);
    var goal = FindGoal(voucherId);
    Rules.Require(
      voucher.OwnerId == actor || voucher.Participants.Contains(actor),
      DomainError.Forbidden,
      "Only the voucher owner or a participating family member may convert it."
    );
    RequireNewOperation(operationId);
    Rules.Require(
      voucher.Redeem(now),
      DomainError.InvalidState,
      "The savings voucher is not convertible or was already redeemed."
    );
    var refunds = voucher.OwnerId is null
      ? goal.RefundableContributions.ToArray()
      : [new KeyValuePair<FamilyMemberId, decimal>(voucher.OwnerId, voucher.Amount)];
    var returned = refunds.Sum(refund => refund.Value);
    var forfeited = voucher.Amount - returned;
    Rules.Require(forfeited >= 0, DomainError.InvalidState, "Voucher refunds exceed the voucher amount.");
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
    var entries = new List<LedgerEntry>
    {
      new LedgerEntry(
        operationId,
        actor,
        goal.Id,
        LedgerKind.GoalDebit,
        voucher.Amount,
        goal.Id,
        "Savings voucher converted",
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
        "Savings voucher converted to Ladirchen",
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
        "Non-refundable voucher credits forfeited",
        now.ToUniversalTime()
      ));
    }

    _guardianGifts.RemoveAll(gift => gift.GoalId == goal.Id);
    _savingsVouchers.Remove(voucher);
    _goals.Remove(goal);
    Record(operationId, entries.ToArray());
    AddDomainEvent(new GoalChanged(Id, voucher.Id, "VoucherConverted", returned, now));

    return returned;
  }

  public void ConsumeGuardianGift(FamilyMemberId actor, GuardianGiftId giftId)
  {
    ActiveMember(actor);
    ArgumentNullException.ThrowIfNull(giftId);
    var gift = _guardianGifts.Find(item => item.Id == giftId)
      ?? throw new DomainRuleException(DomainError.NotFound, "The pending gift does not exist.");
    Rules.Require(gift.OwnerId == actor, DomainError.Forbidden, "Only the recipient may acknowledge this gift.");
    _guardianGifts.Remove(gift);
  }

  private void IssueVoucherIfCompleted(SavingGoal goal)
  {
    if (!goal.Completed || _savingsVouchers.Exists(item => item.Id == goal.Id))
    {
      return;
    }

    _savingsVouchers.Add(
      new GoalSavingsVoucher(
        goal.Id,
        goal.Saved,
        goal.Owner.MemberId,
        goal.Participants
      )
    );
  }

  private void AddPendingGift(GuardianGoalGift gift)
  {
    ArgumentNullException.ThrowIfNull(gift);
    Rules.Require(
      gift.Id is null || _guardianGifts.All(item => item.Id != gift.Id),
      DomainError.Duplicate,
      "This pending gift identity was already used."
    );
    _guardianGifts.Add(gift);
  }

  private void RequireNewPendingGiftId(GuardianGiftId? giftId)
  {
    Rules.Require(
      giftId is null || _guardianGifts.All(item => item.Id != giftId),
      DomainError.Duplicate,
      "This pending gift identity was already used."
    );
  }

  private GoalSavingsVoucher FindSavingsVoucher(SavingGoalId id)
  {
    ArgumentNullException.ThrowIfNull(id);

    return _savingsVouchers.Find(item => item.Id == id)
      ?? throw new DomainRuleException(DomainError.NotFound, "The savings voucher does not exist.");
  }
}
