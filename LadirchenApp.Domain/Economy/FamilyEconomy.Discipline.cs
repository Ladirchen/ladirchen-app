using System;
using System.Collections.Generic;
using System.Linq;

using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

public sealed partial class FamilyEconomy
{
  public ChildBehaviorStatus BehaviorStatusFor(FamilyMemberId childId, DateTimeOffset now)
  {
    RequireChild(childId);
    var entries = TodayBehaviorEntries(childId, now).ToArray();
    var warningEntries = EntriesAfterLastReset(entries)
      .Where(item => item.Kind == LedgerKind.BehaviorWarning)
      .Take(3)
      .ToArray();
    var warningCount = warningEntries.Length;
    var latest = warningEntries.LastOrDefault();

    return new ChildBehaviorStatus(
      childId,
      warningCount switch
      {
        0 => BehaviorLightLevel.Green,
        1 => BehaviorLightLevel.Yellow,
        2 => BehaviorLightLevel.Orange,
        _ => BehaviorLightLevel.Red,
      },
      warningCount,
      warningCount >= 3,
      latest?.Reason,
      latest?.OccurredAtUtc
    );
  }

  public IReadOnlyList<PendingBehaviorNotice> PendingBehaviorNoticesFor(
    FamilyMemberId childId,
    DateTimeOffset now
  )
  {
    RequireChild(childId);
    var entries = TodayBehaviorEntries(childId, now).ToArray();
    var acknowledgedIds = entries
      .Where(item => item.Kind == LedgerKind.BehaviorNoticeAcknowledged)
      .Select(item => item.SourceId)
      .OfType<BehaviorIncidentId>()
      .ToHashSet();
    var level = 0;

    return EntriesAfterLastReset(entries)
      .Where(item => item.Kind == LedgerKind.BehaviorWarning)
      .Select(item =>
      {
        level = Math.Min(3, level + 1);
        var id = item.SourceId as BehaviorIncidentId
          ?? throw new DomainRuleException(DomainError.InvalidState, "A behavior incident identity is required.");
        var guardianId = item.InitiatorId
          ?? throw new DomainRuleException(DomainError.InvalidState, "A behavior incident guardian is required.");

        return new PendingBehaviorNotice(
          id,
          childId,
          guardianId,
          (BehaviorLightLevel)level,
          item.Amount,
          item.Reason,
          item.OccurredAtUtc
        );
      })
      .Where(item => !acknowledgedIds.Contains(item.Id))
      .ToArray();
  }

  public ChildBehaviorStatus IssueBehaviorWarning(
    FamilyMemberId actor,
    FamilyMemberId childId,
    LadirchenAmount deduction,
    string reason,
    BehaviorIncidentId incidentId,
    TransactionId operationId,
    DateTimeOffset now
  )
  {
    RequireInternalGuardian(actor);
    RequireChild(childId);
    ArgumentNullException.ThrowIfNull(incidentId);
    RequireNewOperation(operationId);
    var text = Rules.Text(reason);
    var status = BehaviorStatusFor(childId, now);
    Rules.Require(!status.TasksBlocked, DomainError.InvalidState, "The child is already on red for today.");
    Rules.Require(
      _ledger.All(item => item.SourceId != incidentId),
      DomainError.Duplicate,
      "This behavior incident already exists."
    );
    var requestedDeduction = Rules.Round(deduction.Value);
    var nextWarning = status.WarningCount + 1;
    var wallet = WalletFor(childId);

    if (nextWarning == 2)
    {
      Rules.Require(requestedDeduction >= 0, DomainError.InvalidValue, "The deduction cannot be negative.");
      Rules.Require(
        wallet.Available == 0 || requestedDeduction > 0,
        DomainError.InvalidValue,
        "The second warning requires a positive deduction when Ladirchen are available."
      );
      Rules.Require(
        requestedDeduction <= wallet.Available,
        DomainError.InvalidValue,
        "The deduction cannot exceed the available balance."
      );
    }
    else
    {
      Rules.Require(requestedDeduction == 0, DomainError.InvalidValue, "Only the second warning deducts Ladirchen.");
    }

    wallet.Set(wallet.Balance - requestedDeduction, wallet.Reserved);
    Record(
      operationId,
      new LedgerEntry(
        operationId,
        childId,
        null,
        LedgerKind.BehaviorWarning,
        requestedDeduction,
        incidentId,
        text,
        now.ToUniversalTime(),
        actor
      )
    );

    return BehaviorStatusFor(childId, now);
  }

  public ChildBehaviorStatus ResetBehaviorStatus(
    FamilyMemberId actor,
    FamilyMemberId childId,
    TransactionId operationId,
    DateTimeOffset now
  )
  {
    RequireInternalGuardian(actor);
    RequireChild(childId);
    RequireNewOperation(operationId);
    Record(
      operationId,
      new LedgerEntry(
        operationId,
        childId,
        null,
        LedgerKind.BehaviorStatusReset,
        0,
        childId,
        "Behavior status reset",
        now.ToUniversalTime(),
        actor
      )
    );

    return BehaviorStatusFor(childId, now);
  }

  public void AcknowledgeBehaviorNotice(
    FamilyMemberId actor,
    BehaviorIncidentId incidentId,
    TransactionId operationId,
    DateTimeOffset now
  )
  {
    RequireChild(actor);
    RequireNewOperation(operationId);
    var notice = PendingBehaviorNoticesFor(actor, now).SingleOrDefault(item => item.Id == incidentId);
    Rules.Require(notice is not null, DomainError.NotFound, "The behavior notice is not pending.");
    Record(
      operationId,
      new LedgerEntry(
        operationId,
        actor,
        null,
        LedgerKind.BehaviorNoticeAcknowledged,
        0,
        incidentId,
        "Behavior notice acknowledged",
        now.ToUniversalTime(),
        actor
      )
    );
  }

  private IEnumerable<LedgerEntry> TodayBehaviorEntries(FamilyMemberId childId, DateTimeOffset now)
  {
    var localDate = _timeZone.LocalDate(now);

    return _ledger
      .Where(item => item.MemberId == childId)
      .Where(item => item.Kind is LedgerKind.BehaviorWarning
        or LedgerKind.BehaviorNoticeAcknowledged
        or LedgerKind.BehaviorStatusReset)
      .Where(item => _timeZone.LocalDate(item.OccurredAtUtc) == localDate)
      .OrderBy(item => item.OccurredAtUtc)
      .ThenBy(item => item.EntryId);
  }

  private static IEnumerable<LedgerEntry> EntriesAfterLastReset(IReadOnlyList<LedgerEntry> entries)
  {
    var lastResetIndex = -1;

    for (var index = 0; index < entries.Count; index++)
    {
      if (entries[index].Kind == LedgerKind.BehaviorStatusReset)
      {
        lastResetIndex = index;
      }
    }

    return entries.Skip(lastResetIndex + 1);
  }

  private void RequireInternalGuardian(FamilyMemberId actor)
  {
    var member = ActiveMember(actor);
    Rules.Require(
      member.Role == MemberRole.Guardian && !member.IsExternalGuardian,
      DomainError.Forbidden,
      "Only an internal guardian may manage child behavior."
    );
  }

  private void RequireTasksAvailableFor(FamilyMemberId childId, DateTimeOffset now)
  {
    if (ActiveMember(childId).Role != MemberRole.Child)
    {
      return;
    }

    Rules.Require(
      !BehaviorStatusFor(childId, now).TasksBlocked,
      DomainError.Forbidden,
      "The child cannot work on tasks while the behavior light is red."
    );
  }
}
