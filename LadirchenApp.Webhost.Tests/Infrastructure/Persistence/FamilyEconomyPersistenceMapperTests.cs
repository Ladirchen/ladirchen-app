using System;
using System.Linq;
using System.Threading.Tasks;

using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Infrastructure.Persistence.Typed;
using LadirchenApp.Webhost.Infrastructure.Persistence.Typed.Entities;

namespace LadirchenApp.Webhost.Tests.Infrastructure.Persistence;

public sealed class FamilyEconomyPersistenceMapperTests
{
  [Test]
  public async Task IgnoresStoredWalletsForMembersNoLongerInTheFamily()
  {
    var familyId = new FamilyId(Guid.Parse("f8b4dce8-59ac-49ec-aa06-c8c979d8d047"));
    var administratorId = new FamilyMemberId("administrator");
    var family = new Family(
      familyId,
      administratorId,
      "Administrator",
      new IanaTimeZone("Europe/Zurich")
    );
    var entity = new FamilyEconomyRootEntity
    {
      FamilyId = familyId.Value,
      Wallets =
      {
        new FamilyEconomyWalletEntity
        {
          FamilyId = familyId.Value,
          MemberId = administratorId.Value,
          Balance = 7,
        },
        new FamilyEconomyWalletEntity
        {
          FamilyId = familyId.Value,
          MemberId = "removed-member",
          Balance = 0,
        },
      },
    };

    var economy = FamilyEconomyPersistenceMapper.ToDomain(entity, family);
    var wallets = economy.CaptureState().Wallets;

    await Assert.That(wallets).HasSingleItem();
    await Assert.That(wallets.Single().MemberId).IsEqualTo(administratorId);
    await Assert.That(wallets.Single().Balance).IsEqualTo(7);
  }
}
