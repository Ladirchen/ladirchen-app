using System;

namespace LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

public sealed class FamilyPetEntity
{
  public Guid FamilyId { get; set; }
  public string PetId { get; set; } = string.Empty;
  public string Name { get; set; } = string.Empty;
  public int Kind { get; set; }
  public string? Avatar { get; set; }
  public string? Color { get; set; }
  public FamilyRootEntity Family { get; set; } = null!;
}
