namespace LadirchenApp.Domain.Shared;

public sealed record BehaviorIncidentId : DomainIdentifier
{
  public BehaviorIncidentId(string value) : base(value)
  {
  }
}
