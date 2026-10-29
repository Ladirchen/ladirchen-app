using System;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

public sealed class FamilyEconomyWalletEntity
{
  public Guid FamilyId { get; set; }
  public string MemberId { get; set; } = string.Empty;
  public decimal Balance { get; set; }
  public FamilyEconomyRootEntity Economy { get; set; } = null!;
}
