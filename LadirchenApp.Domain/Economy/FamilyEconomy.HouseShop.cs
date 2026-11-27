using System;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

public sealed partial class FamilyEconomy
{
  public void PurchaseHouseContent(
    FamilyMemberId actor,
    LadirchenAmount price,
    HouseContentId contentId,
    TransactionId operationId,
    DateTimeOffset now,
    string? contentTitle = null
  )
  {
    ArgumentNullException.ThrowIfNull(price);
    ArgumentNullException.ThrowIfNull(contentId);
    RequireChild(actor);
    var wallet = WalletFor(actor);
    Rules.Require(
      wallet.Available >= price.Value,
      DomainError.InsufficientFunds,
      "The house purchase exceeds available funds."
    );

    if (price.Value == 0)
    {
      return;
    }

    RequireNewOperation(operationId);
    wallet.Set(checked(wallet.Balance - price.Value), wallet.Reserved);
    Record(
      operationId,
      new LedgerEntry(
        operationId,
        actor,
        null,
        LedgerKind.Debit,
        price.Value,
        contentId,
        "House content purchase",
        now.ToUniversalTime(),
        null,
        contentTitle is null ? contentId.Value : Rules.Text(contentTitle)
      )
    );
  }
}
