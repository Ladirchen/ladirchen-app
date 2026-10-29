using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Webhost.Tests.Application.Commands;

public sealed class InitialFamilyAggregateFactoryTests
{
  [Test]
  public async Task CreateUsesCanonicalSchemaVersionsAndCompleteInitialStates()
  {
    var familyId = new FamilyId(Guid.Parse("74586dc6-2053-40f7-bf25-339e8f8f29e4"));
    var guardianId = new FamilyMemberId("guardian");
    var expectedVersions = new Dictionary<string, int>(StringComparer.Ordinal)
    {
      ["contributions"] = 2,
      ["family-profile"] = 2,
      ["family-progression"] = 4,
      ["home-customization"] = 3,
      ["reward-shop"] = 1,
      ["savings"] = 4,
    };

    var saves = InitialFamilyAggregateFactory.Create(familyId, guardianId, "Guardian");
    var savesByType = saves.ToDictionary(item => item.Command.AggregateType, StringComparer.Ordinal);

    await Assert.That(savesByType.Keys).IsEquivalentTo(expectedVersions.Keys);

    foreach (var expected in expectedVersions)
    {
      var save = savesByType[expected.Key];
      await Assert.That(save.SchemaVersion).IsEqualTo(expected.Value);
      await Assert.That(save.SchemaVersion).IsEqualTo(FamilyAggregateSchema.GetVersion(expected.Key));
      await Assert.That(save.Command.FamilyId).IsEqualTo(familyId);
      await Assert.That(save.Command.UpdatedBy).IsEqualTo(guardianId);
      await Assert.That(save.Command.ExpectedRevision).IsEqualTo(0);
    }

    var profile = savesByType["family-profile"].Command.State;
    var guardian = profile.GetProperty("members").EnumerateArray().Single();
    await Assert.That(guardian.GetProperty("id").GetString()).IsEqualTo(guardianId.Value);
    await Assert.That(guardian.GetProperty("name").GetString()).IsEqualTo("Guardian");
    await Assert.That(guardian.GetProperty("guardianAccess").GetString()).IsEqualTo("admin");

    var savings = savesByType["savings"].Command.State;
    await Assert.That(savings.GetProperty("appliedOperationIds").GetArrayLength()).IsEqualTo(0);
    await Assert.That(savings.GetProperty("ledger").GetArrayLength()).IsEqualTo(0);
    await Assert.That(savings.GetProperty("settledInterestPeriods").GetArrayLength()).IsEqualTo(0);
    await Assert.That(savings.GetProperty("vouchers").GetArrayLength()).IsEqualTo(0);

    var contributions = savesByType["contributions"].Command.State;
    await Assert.That(contributions.GetProperty("usedContributionIds").GetArrayLength()).IsEqualTo(0);

    var progression = savesByType["family-progression"].Command.State;
    await Assert.That(progression.GetProperty("closedWeekOutcomes").EnumerateObject().Count()).IsEqualTo(0);
    await Assert.That(progression.GetProperty("weeklyEvaluations").EnumerateObject().Count()).IsEqualTo(0);
  }
}
