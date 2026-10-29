using System;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

public sealed class EconomyAppliedOperationEntity
{
  public Guid FamilyId { get; set; }
  public string OperationId { get; set; } = string.Empty;
  public FamilyEconomyRootEntity Economy { get; set; } = null!;
}
