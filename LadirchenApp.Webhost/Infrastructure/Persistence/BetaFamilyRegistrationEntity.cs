using System;

namespace LadirchenApp.Webhost.Infrastructure.Persistence;

public sealed class BetaFamilyRegistrationEntity
{
  public int Id
  {
    get; set;
  }
  public Guid FamilyId
  {
    get; set;
  }
  public DateTimeOffset RegisteredAt
  {
    get; set;
  }
}