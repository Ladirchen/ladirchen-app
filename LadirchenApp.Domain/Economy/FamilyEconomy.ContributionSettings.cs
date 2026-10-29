using System;

using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Domain.Economy;

public sealed partial class FamilyEconomy
{
  private ContributionSettings _contributionSettings = ContributionSettings.Default;

  public ContributionSettings ContributionSettings
  {
    get
    {
      return _contributionSettings;
    }
  }

  public void UpdateContributionSettings(
    FamilyMemberId actor,
    ContributionSettings settings,
    DateTimeOffset now
  )
  {
    _family.RequireAdministrator(actor);
    ArgumentNullException.ThrowIfNull(settings);
    _contributionSettings = settings;
    AddDomainEvent(
      new ContributionSettingsChanged(Id, settings.PerfectRatingBonus, now)
    );
  }

  public void RestoreContributionSettings(ContributionSettings settings)
  {
    ArgumentNullException.ThrowIfNull(settings);
    _contributionSettings = settings;
  }
}
