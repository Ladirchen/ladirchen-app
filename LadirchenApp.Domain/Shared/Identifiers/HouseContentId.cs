namespace LadirchenApp.Domain.Shared;

public sealed record HouseContentId : DomainIdentifier
{
  public HouseContentId(string value) : base(value) { }
}
