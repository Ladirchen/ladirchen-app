using System;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Family.Setup;

public sealed record FamilyPetState
{
  public FamilyPetState(FamilyPetId id, string name, FamilyPetKind kind)
  {
    ArgumentNullException.ThrowIfNull(id);
    Id = id;
    Name = Rules.Text(name);
    Kind = Rules.Defined(kind);
  }

  public FamilyPetId Id { get; }
  public string Name { get; }
  public FamilyPetKind Kind { get; }
}
