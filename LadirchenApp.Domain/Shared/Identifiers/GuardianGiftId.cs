namespace LadirchenApp.Domain.Shared;

public sealed record GuardianGiftId : DomainIdentifier
{
  public GuardianGiftId(string value) : base(value)
  {
  }
}
