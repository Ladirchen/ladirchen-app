using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Application.Commands;
using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Economy;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.House;
using LadirchenApp.Domain.House.Shop;
using LadirchenApp.Domain.Ladi;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Infrastructure.Persistence;
using LadirchenApp.Webhost.Tests.Application.Commands;

using Microsoft.EntityFrameworkCore;

using Testcontainers.PostgreSql;

namespace LadirchenApp.Webhost.Tests.Infrastructure.Persistence;

public sealed class TypedAggregateRepositoryTests
{
  [Test]
  public async Task ExistingChildCanBeUpdatedAndReloadedAfterRestart()
  {
    await using var postgres = await StartPostgreSqlAsync();
    var options = CreateOptions(postgres.GetConnectionString());
    var familyId = new FamilyId(Guid.NewGuid());
    var administratorId = new FamilyMemberId("guardian");
    var childId = new FamilyMemberId("child");
    var now = DateTimeOffset.Parse("2026-10-29T12:00:00Z");

    await using (var database = new LadirchenDbContext(options))
    {
      await database.Database.MigrateAsync();
      var unitOfWork = new DatabaseTypedAggregateUnitOfWork(database, TimeProvider.System);
      var repository = new DatabaseFamilyRepository(database, unitOfWork);
      var family = new Family(familyId, administratorId, "Guardian", new IanaTimeZone("Europe/Zurich"));
      family.AddMember(
        administratorId,
        childId,
        "Before",
        MemberRole.Child,
        GuardianAccess.Supporter,
        false,
        now
      );
      await repository.SaveAsync(
        family,
        0,
        administratorId,
        CancellationToken.None
      );
      var loaded = await repository.LoadAsync(familyId, CancellationToken.None);
      await Assert.That(loaded).IsNotNull();
      loaded!.Root.RenameMember(administratorId, childId, "After", now.AddMinutes(1));

      var updated = await repository.SaveAsync(
        loaded.Root,
        loaded.Revision,
        administratorId,
        CancellationToken.None
      );

      await Assert.That(updated.Revision).IsEqualTo(2);
    }

    await using (var restartedDatabase = new LadirchenDbContext(options))
    {
      var unitOfWork = new DatabaseTypedAggregateUnitOfWork(
        restartedDatabase,
        TimeProvider.System
      );
      var repository = new DatabaseFamilyRepository(restartedDatabase, unitOfWork);
      var loaded = await repository.LoadAsync(familyId, CancellationToken.None);
      await Assert.That(loaded).IsNotNull();
      await Assert.That(loaded!.Revision).IsEqualTo(2);
      await Assert.That(loaded.Root.Members.Single(item => item.Id == childId).Name)
        .IsEqualTo("After");
    }
  }

  [Test]
  public async Task EconomyChildrenAndDerivedReservationSurviveUpdateAndRestart()
  {
    await using var postgres = await StartPostgreSqlAsync();
    var options = CreateOptions(postgres.GetConnectionString());
    var familyId = new FamilyId(Guid.NewGuid());
    var administratorId = new FamilyMemberId("guardian");
    var childId = new FamilyMemberId("child");
    var contributionId = new ContributionId("contribution");
    var rewardId = new ShopRewardId("reward");
    var now = DateTimeOffset.Parse("2026-10-29T12:00:00Z");

    await using (var database = new LadirchenDbContext(options))
    {
      await database.Database.MigrateAsync();
      var unitOfWork = new DatabaseTypedAggregateUnitOfWork(database, TimeProvider.System);
      var familyRepository = new DatabaseFamilyRepository(database, unitOfWork);
      var economyRepository = new DatabaseFamilyEconomyRepository(database, unitOfWork);
      var family = new Family(familyId, administratorId, "Guardian", new IanaTimeZone("Europe/Zurich"));
      family.AddMember(
        administratorId,
        childId,
        "Child",
        MemberRole.Child,
        GuardianAccess.Supporter,
        false,
        now
      );
      await familyRepository.SaveAsync(family, 0, administratorId, CancellationToken.None);
      var economy = new FamilyEconomy(family);
      economy.GiveSpecialGift(
        administratorId,
        childId,
        new LadirchenAmount(20),
        "Initial balance",
        new TransactionId("gift-operation"),
        now
      );
      economy.AddContribution(
        administratorId,
        contributionId,
        new ContributionDefinition(
          "Help",
          ContributionKind.Basic,
          new LadirchenAmount(5),
          new Percentage(10),
          childId
        ),
        now
      );
      economy.AddShopReward(
        administratorId,
        rewardId,
        new ShopRewardDefinition(
          "Reward",
          ShopRewardCategory.Activity,
          new LadirchenAmount(10),
          1,
          true,
          new AvailabilityWindow()
        ),
        now
      );
      economy.RequestShopReward(
        childId,
        rewardId,
        new RedemptionRequestId("request"),
        new TransactionId("request-operation"),
        now
      );
      var presentation = new FamilyEconomyPresentationState(
        [new ContributionPresentationState(contributionId, "Description", "icon", false)],
        [],
        [new ShopRewardPresentationState(rewardId, "Details", "reward-icon", "Conditions")],
        []
      );
      await economyRepository.SaveAsync(
        economy,
        presentation,
        0,
        administratorId,
        CancellationToken.None
      );
      var staleGuardedBatch = new TypedAggregateSaveBatch(
        familyId,
        administratorId,
        familyEconomy: new PendingFamilyEconomySave(economy, presentation, 1),
        revisionGuards:
        [
          new TypedAggregateRevisionGuard(TypedAggregateKind.Family, 0),
        ]
      );
      await Assert.That(async () =>
        await unitOfWork.SaveAsync(staleGuardedBatch, CancellationToken.None)
      ).Throws<FamilyAggregateConflictException>();
      var loadedFamily = await familyRepository.LoadAsync(familyId, CancellationToken.None);
      await Assert.That(loadedFamily).IsNotNull();
      var loadedEconomy = await economyRepository.LoadAsync(
        loadedFamily!.Root,
        CancellationToken.None
      );
      await Assert.That(loadedEconomy).IsNotNull();
      await Assert.That(loadedEconomy!.Root.WalletFor(childId).Reserved).IsEqualTo(10);
      await Assert.That(loadedEconomy.Root.WalletFor(childId).Available).IsEqualTo(10);
      loadedEconomy.Root.SubmitContribution(childId, contributionId, now.AddMinutes(1));
      await economyRepository.SaveAsync(
        loadedEconomy.Root,
        loadedEconomy.Presentation,
        loadedEconomy.Revision,
        administratorId,
        CancellationToken.None
      );
    }

    await using (var restartedDatabase = new LadirchenDbContext(options))
    {
      var unitOfWork = new DatabaseTypedAggregateUnitOfWork(
        restartedDatabase,
        TimeProvider.System
      );
      var familyRepository = new DatabaseFamilyRepository(restartedDatabase, unitOfWork);
      var economyRepository = new DatabaseFamilyEconomyRepository(restartedDatabase, unitOfWork);
      var loadedFamily = await familyRepository.LoadAsync(familyId, CancellationToken.None);
      await Assert.That(loadedFamily).IsNotNull();
      var loadedEconomy = await economyRepository.LoadAsync(
        loadedFamily!.Root,
        CancellationToken.None
      );
      await Assert.That(loadedEconomy).IsNotNull();
      await Assert.That(loadedEconomy!.Revision).IsEqualTo(2);
      await Assert.That(loadedEconomy.Root.WalletFor(childId).Reserved).IsEqualTo(10);
      await Assert.That(loadedEconomy.Root.WalletFor(childId).Available).IsEqualTo(10);
      await Assert.That(loadedEconomy.Root.Contributions.Single().Status)
        .IsEqualTo(ContributionStatus.Pending);
      await Assert.That(loadedEconomy.Root.RedemptionRequests.Single().Status)
        .IsEqualTo(RedemptionStatus.Pending);
      await Assert.That(loadedEconomy.Presentation.Contributions.Single().Description)
        .IsEqualTo("Description");
      await Assert.That(loadedEconomy.Presentation.ShopRewards.Single().Conditions)
        .IsEqualTo("Conditions");
    }
  }

  [Test]
  public async Task ProgressionAndHouseCustomizationSurviveOneAtomicRoundtrip()
  {
    await using var postgres = await StartPostgreSqlAsync();
    var options = CreateOptions(postgres.GetConnectionString());
    var familyId = new FamilyId(Guid.NewGuid());
    var administratorId = new FamilyMemberId("guardian");
    var childId = new FamilyMemberId("child");
    var now = DateTimeOffset.Parse("2026-10-29T12:00:00Z");
    var period = new IsoWeekPeriod(2026, 44);

    await using (var database = new LadirchenDbContext(options))
    {
      await database.Database.MigrateAsync();
      var unitOfWork = new DatabaseTypedAggregateUnitOfWork(database, TimeProvider.System);
      var familyRepository = new DatabaseFamilyRepository(database, unitOfWork);
      var family = new Family(
        familyId,
        administratorId,
        "Guardian",
        new IanaTimeZone("Europe/Zurich")
      );
      family.AddMember(
        administratorId,
        childId,
        "Child",
        MemberRole.Child,
        GuardianAccess.Supporter,
        false,
        now
      );
      await familyRepository.SaveAsync(family, 0, administratorId, CancellationToken.None);
      var progression = FamilyHouse.CreateInitial(family);
      var evaluation = new PersonalLadiEvaluation(
        childId,
        period.Year,
        period.Week,
        new PersonalParticipation(7, 100, 5),
        new ParticipationValue(10.5m, 8),
        LadiStage.AtPosition(6)
      );
      var closed = progression.CloseSuccessfulWeek(
        period,
        HouseEnergyPolicy.MinimumSuccessfulWeekEnergy,
        [evaluation],
        now
      );
      await Assert.That(closed).IsTrue();
      var house = FamilyHouseShop.CreateInitial(
        family,
        progression.StoredLevel,
        HouseSubscriptionTier.Pro
      );
      await Assert.That(house.PurchaseTheme("halloween-night", true, 1_000)).IsTrue();
      await Assert.That(house.MoveAccessory("flower-boxes", "garden", 35, 55)).IsTrue();

      var saved = await unitOfWork.SaveAsync(
        new TypedAggregateSaveBatch(
          familyId,
          administratorId,
          familyProgression: new PendingFamilyProgressionSave(progression, 0),
          houseCustomization: new PendingHouseCustomizationSave(house, 0),
          revisionGuards:
          [
            new TypedAggregateRevisionGuard(TypedAggregateKind.Family, 1),
          ]
        ),
        CancellationToken.None
      );

      await Assert.That(saved.FamilyProgression?.Revision).IsEqualTo(1);
      await Assert.That(saved.HouseCustomization?.Revision).IsEqualTo(1);
    }

    await using (var restartedDatabase = new LadirchenDbContext(options))
    {
      var unitOfWork = new DatabaseTypedAggregateUnitOfWork(
        restartedDatabase,
        TimeProvider.System
      );
      var familyRepository = new DatabaseFamilyRepository(restartedDatabase, unitOfWork);
      var progressionRepository = new DatabaseFamilyProgressionRepository(
        restartedDatabase,
        unitOfWork
      );
      var houseRepository = new DatabaseHouseCustomizationRepository(
        restartedDatabase,
        unitOfWork
      );
      var loadedFamily = await familyRepository.LoadAsync(familyId, CancellationToken.None);
      await Assert.That(loadedFamily).IsNotNull();
      var loadedProgression = await progressionRepository.LoadAsync(
        loadedFamily!.Root,
        CancellationToken.None
      );
      await Assert.That(loadedProgression).IsNotNull();
      var loadedHouse = await houseRepository.LoadAsync(
        loadedFamily.Root,
        loadedProgression!.Root.StoredLevel,
        CancellationToken.None
      );
      await Assert.That(loadedHouse).IsNotNull();
      await Assert.That(loadedProgression.Revision).IsEqualTo(1);
      await Assert.That(loadedProgression.Root.StoredLevel).IsEqualTo(1);
      await Assert.That(loadedProgression.Root.CompletedWeeklyStreak).IsEqualTo(1);
      await Assert.That(loadedProgression.Root.StoredWeeklyStreakFor(childId)).IsEqualTo(1);
      var closedWeek = loadedProgression.Root.ClosedWeekFor(period);
      await Assert.That(closedWeek.Outcome).IsEqualTo(FamilyWeekOutcome.Completed);
      await Assert.That(closedWeek.MemberEvaluations[childId].Stage.Id)
        .IsEqualTo("super-ladi");
      await Assert.That(loadedHouse!.Revision).IsEqualTo(1);
      await Assert.That(loadedHouse.Root.SubscriptionTier).IsEqualTo(HouseSubscriptionTier.Pro);
      await Assert.That(loadedHouse.Root.SelectedThemeId).IsEqualTo("halloween-night");
      await Assert.That(
        loadedHouse.Root.RoomDesigns.Single(item => item.Id == "garden-halloween-night").Selected
      ).IsTrue();
      var flowerBoxes = loadedHouse.Root.Accessories.Single(item => item.Id == "flower-boxes");
      await Assert.That(flowerBoxes.Placement.Area).IsEqualTo("garden");
      await Assert.That(flowerBoxes.Placement.X).IsEqualTo(35m);
      await Assert.That(flowerBoxes.Placement.Y).IsEqualTo(55m);
    }
  }

  [Test]
  public async Task EconomyCommandsKeepPresentationRowsAlignedWithDomainState()
  {
    await using var postgres = await StartPostgreSqlAsync();
    var options = CreateOptions(postgres.GetConnectionString());
    var familyId = new FamilyId(Guid.NewGuid());
    var administratorId = new FamilyMemberId("guardian");
    var childId = new FamilyMemberId("child");
    var now = DateTimeOffset.Parse("2026-10-29T12:00:00Z");

    await using var database = new LadirchenDbContext(options);
    await database.Database.MigrateAsync();
    var timeProvider = new FixedTimeProvider(now);
    var unitOfWork = new DatabaseTypedAggregateUnitOfWork(database, timeProvider);
    var familyRepository = new DatabaseFamilyRepository(database, unitOfWork);
    var economyRepository = new DatabaseFamilyEconomyRepository(database, unitOfWork);
    var progressionRepository = new DatabaseFamilyProgressionRepository(database, unitOfWork);
    var houseRepository = new DatabaseHouseCustomizationRepository(database, unitOfWork);
    var family = new Family(
      familyId,
      administratorId,
      "Guardian",
      new IanaTimeZone("Europe/Zurich")
    );
    family.AddMember(
      administratorId,
      childId,
      "Child",
      MemberRole.Child,
      GuardianAccess.Supporter,
      false,
      now
    );
    var savedFamily = await familyRepository.SaveAsync(
      family,
      0,
      administratorId,
      CancellationToken.None
    );
    var economy = new FamilyEconomy(family);
    var progression = FamilyHouse.CreateInitial(family);
    var house = FamilyHouseShop.CreateInitial(
      family,
      progression.StoredLevel,
      HouseSubscriptionTier.Free
    );
    _ = await unitOfWork.SaveAsync(
      new TypedAggregateSaveBatch(
        familyId,
        administratorId,
        familyEconomy: new PendingFamilyEconomySave(
          economy,
          FamilyEconomyPresentationState.Empty,
          0
        ),
        familyProgression: new PendingFamilyProgressionSave(progression, 0),
        houseCustomization: new PendingHouseCustomizationSave(house, 0),
        revisionGuards:
        [
          new TypedAggregateRevisionGuard(TypedAggregateKind.Family, savedFamily.Revision),
        ]
      ),
      CancellationToken.None
    );
    var handler = new ExecuteFamilyCommandHandler(
      familyRepository,
      economyRepository,
      progressionRepository,
      houseRepository,
      unitOfWork,
      new ImmediateFamilyCommandExecutionCoordinator(),
      timeProvider
    );

    await ExecuteAsync(
      handler,
      familyId,
      administratorId,
      "contributions.add",
      new
      {
        id = "help",
        title = "Help",
        description = "Description",
        icon = "hands",
        kind = "basic",
        reward = 10,
        energy = 10,
        assigneeId = childId.Value,
      }
    );
    await ExecuteAsync(
      handler,
      familyId,
      childId,
      "contributions.submit",
      new { id = "help" }
    );
    await ExecuteAsync(
      handler,
      familyId,
      administratorId,
      "contributions.approve",
      new { id = "help", stars = 5 }
    );
    await ExecuteAsync(
      handler,
      familyId,
      childId,
      "contributions.acknowledge-reward",
      new { id = "help" }
    );
    var loadedFamily = await familyRepository.LoadAsync(familyId, CancellationToken.None);
    await Assert.That(loadedFamily).IsNotNull();
    var loadedEconomy = await economyRepository.LoadAsync(
      loadedFamily!.Root,
      CancellationToken.None
    );
    await Assert.That(loadedEconomy).IsNotNull();
    await Assert.That(loadedEconomy!.Presentation.Contributions.Single().RewardCelebrated)
      .IsTrue();
    await ExecuteAsync(
      handler,
      familyId,
      administratorId,
      "contributions.delete",
      new { id = "help" }
    );

    await ExecuteAsync(
      handler,
      familyId,
      administratorId,
      "reward-shop.add",
      new
      {
        id = "cinema",
        title = "Cinema",
        description = "Watch a film",
        conditions = "On Saturday",
        icon = "film",
        category = "activity",
        price = 5,
        quantity = 1,
        isVisible = true,
      }
    );
    await ExecuteAsync(
      handler,
      familyId,
      administratorId,
      "reward-shop.delete",
      new { id = "cinema" }
    );

    await ExecuteAsync(
      handler,
      familyId,
      childId,
      "savings.add-goal",
      new
      {
        id = "bike",
        ownerId = childId.Value,
        title = "Bike",
        target = 100,
        visibility = "private",
        icon = "old-icon",
      }
    );
    await ExecuteAsync(
      handler,
      familyId,
      childId,
      "savings.update-goal",
      new
      {
        id = "bike",
        title = "New bike",
        target = 100,
        visibility = "private",
        icon = "new-icon",
      }
    );
    loadedFamily = await familyRepository.LoadAsync(familyId, CancellationToken.None);
    loadedEconomy = await economyRepository.LoadAsync(
      loadedFamily!.Root,
      CancellationToken.None
    );
    await Assert.That(loadedEconomy!.Presentation.SavingGoals.Single().Icon)
      .IsEqualTo("new-icon");
    await ExecuteAsync(
      handler,
      familyId,
      childId,
      "savings.cancel-goal",
      new { id = "bike" }
    );

    await ExecuteAsync(
      handler,
      familyId,
      administratorId,
      "savings.special-gift",
      new
      {
        giftId = "gift",
        childId = childId.Value,
        amount = 5,
        reason = "Bonus",
      }
    );
    await ExecuteAsync(
      handler,
      familyId,
      childId,
      "savings.consume-guardian-gift",
      new { giftId = "gift" }
    );
    loadedFamily = await familyRepository.LoadAsync(familyId, CancellationToken.None);
    loadedEconomy = await economyRepository.LoadAsync(
      loadedFamily!.Root,
      CancellationToken.None
    );
    await Assert.That(loadedEconomy).IsNotNull();
    await Assert.That(loadedEconomy!.Presentation.Contributions).IsEmpty();
    await Assert.That(loadedEconomy.Presentation.ShopRewards).IsEmpty();
    await Assert.That(loadedEconomy.Presentation.SavingGoals).IsEmpty();
    await Assert.That(loadedEconomy.Presentation.GuardianGifts).IsEmpty();
  }

  private static DbContextOptions<LadirchenDbContext> CreateOptions(string connectionString)
  {
    return new DbContextOptionsBuilder<LadirchenDbContext>()
      .UseNpgsql(connectionString)
      .Options;
  }

  private static async Task<PostgreSqlContainer> StartPostgreSqlAsync()
  {
    var postgres = new PostgreSqlBuilder("postgres:18-alpine").Build();
    await postgres.StartAsync();

    return postgres;
  }

  private static async Task ExecuteAsync(
    ExecuteFamilyCommandHandler handler,
    FamilyId familyId,
    FamilyMemberId actorId,
    string commandType,
    object payload
  )
  {
    await handler.HandleAsync(
      new FamilyCommand(
        Guid.NewGuid(),
        familyId,
        actorId,
        commandType,
        JsonSerializer.SerializeToElement(payload),
        actorId
      ),
      CancellationToken.None
    );
  }
}
