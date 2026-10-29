namespace LadirchenApp.Domain.Shared;

public sealed record FamilyPetId : DomainIdentifier
{
  public FamilyPetId(string value)
    : base(value)
  {
  }
}
