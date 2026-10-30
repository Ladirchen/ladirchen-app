using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Persistence;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Infrastructure;
using LadirchenApp.Webhost.Infrastructure.Persistence;
using LadirchenApp.Webhost.Models;
using LadirchenApp.Webhost.Tests.Application.Commands;

using Microsoft.EntityFrameworkCore;

using Testcontainers.PostgreSql;

namespace LadirchenApp.Webhost.Tests.Infrastructure.Authentication;

public sealed class MemberAccountServiceTests
{
  [Test]
  public async Task ChildInvitationIsHashedActivatedOnceAndSynchronizedAcrossAggregates()
  {
    await using var postgres = await StartPostgreSqlAsync();
    var options = CreateOptions(postgres.GetConnectionString());
    await using var database = new LadirchenDbContext(options);
    await database.Database.MigrateAsync();
    var timeProvider = TimeProvider.System;
    var unitOfWork = new DatabaseTypedAggregateUnitOfWork(database, timeProvider);
    var familyRepository = new DatabaseFamilyRepository(database, unitOfWork);
    var economyRepository = new DatabaseFamilyEconomyRepository(database, unitOfWork);
    var progressionRepository = new DatabaseFamilyProgressionRepository(database, unitOfWork);
    var houseRepository = new DatabaseHouseCustomizationRepository(database, unitOfWork);
    var outboxWriter = new FamilyMembershipOutboxWriter(database, timeProvider);
    var authentication = new AuthenticationService(
      database,
      unitOfWork,
      timeProvider,
      outboxWriter
    );
    var memberAccounts = new MemberAccountService(
      database,
      familyRepository,
      economyRepository,
      progressionRepository,
      houseRepository,
      unitOfWork,
      outboxWriter,
      timeProvider
    );
    var familyId = Guid.NewGuid();
    var typedFamilyId = new FamilyId(familyId);
    var administratorId = new FamilyMemberId("guardian-1");
    await authentication.RegisterAsync(
      Registration(familyId),
      CancellationToken.None
    );

    var invitation = await memberAccounts.CreateInvitationAsync(
      typedFamilyId,
      administratorId,
      new CreateMemberInvitationRequest(
        "child@example.com",
        null,
        "Child",
        "child"
      ),
      CancellationToken.None
    );
    var persistedInvitation = await database.MemberAccountInvitations
      .AsNoTracking()
      .SingleAsync();
    var pendingFamily = await familyRepository.LoadAsync(typedFamilyId, CancellationToken.None)
      ?? throw new InvalidOperationException();
    var pendingMember = pendingFamily.Root.Members.Single(item => item.Id.Value == invitation.MemberId);
    var pendingEconomy = await economyRepository.LoadAsync(
      pendingFamily.Root,
      CancellationToken.None
    ) ?? throw new InvalidOperationException();
    var pendingProgression = await progressionRepository.LoadAsync(
      pendingFamily.Root,
      CancellationToken.None
    ) ?? throw new InvalidOperationException();
    var pendingHouse = await houseRepository.LoadAsync(
      pendingFamily.Root,
      pendingProgression.Root.StoredLevel,
      CancellationToken.None
    ) ?? throw new InvalidOperationException();
    database.FamilyAggregateOutboxMessages.Add(
      new FamilyAggregateOutboxMessageEntity
      {
        AggregateType = "savings",
        FamilyId = familyId,
        MessageId = Guid.NewGuid(),
        OccurredAt = DateTimeOffset.UtcNow,
        Revision = checked(pendingEconomy.Revision + 1),
        UpdatedAt = DateTimeOffset.UtcNow,
        UpdatedBy = administratorId.Value,
      }
    );
    await database.SaveChangesAsync();
    var rejected = await memberAccounts.ActivateAsync(
      new ActivateMemberAccountRequest(
        "incorrect-code",
        invitation.Email,
        "member password",
        "child-user"
      ),
      CancellationToken.None
    );

    var activated = await memberAccounts.ActivateAsync(
      new ActivateMemberAccountRequest(
        invitation.ActivationCode.ToLowerInvariant(),
        invitation.Email.ToUpperInvariant(),
        "member password",
        "Child-User"
      ),
      CancellationToken.None
    );
    var activeFamily = await familyRepository.LoadAsync(typedFamilyId, CancellationToken.None)
      ?? throw new InvalidOperationException();
    var activeMember = activeFamily.Root.Members.Single(item => item.Id.Value == invitation.MemberId);
    var activeEconomy = await economyRepository.LoadAsync(
      activeFamily.Root,
      CancellationToken.None
    ) ?? throw new InvalidOperationException();
    var activeProgression = await progressionRepository.LoadAsync(
      activeFamily.Root,
      CancellationToken.None
    ) ?? throw new InvalidOperationException();
    var activeHouse = await houseRepository.LoadAsync(
      activeFamily.Root,
      activeProgression.Root.StoredLevel,
      CancellationToken.None
    ) ?? throw new InvalidOperationException();
    var secondActivation = await memberAccounts.ActivateAsync(
      new ActivateMemberAccountRequest(
        invitation.ActivationCode,
        invitation.Email,
        "member password",
        "another-user"
      ),
      CancellationToken.None
    );
    var usernameSession = await authentication.AuthenticateAsync(
      new AuthenticateAccountRequest("CHILD-USER", "member password"),
      CancellationToken.None
    );
    var emailSession = await authentication.AuthenticateAsync(
      new AuthenticateAccountRequest("CHILD@EXAMPLE.COM", "member password"),
      CancellationToken.None
    );

    await Assert.That(persistedInvitation.ActivationCodeHash.SequenceEqual(
      Encoding.UTF8.GetBytes(invitation.ActivationCode)
    )).IsFalse();
    await Assert.That(pendingMember.InvitationPending).IsTrue();
    await Assert.That(pendingMember.ParticipatesInWeeklyGoal).IsFalse();
    await Assert.That(pendingEconomy.Root.Wallets.Any(item => item.Id == pendingMember.Id)).IsFalse();
    await Assert.That(pendingHouse.Root.Placements.Any(item =>
      item.EntityType == "member" && item.EntityId == invitation.MemberId
    )).IsFalse();
    await Assert.That(rejected).IsNull();
    await Assert.That(activated?.MemberId).IsEqualTo(invitation.MemberId);
    await Assert.That(activeMember.InvitationPending).IsFalse();
    await Assert.That(activeMember.ParticipatesInWeeklyGoal).IsTrue();
    await Assert.That(activeFamily.Root.ActiveChildId).IsEqualTo(activeMember.Id);
    await Assert.That(activeFamily.Root.OnboardingCompleted).IsTrue();
    await Assert.That(activeEconomy.Root.Wallets.Any(item => item.Id == activeMember.Id)).IsTrue();
    await Assert.That(activeHouse.Root.Placements.Any(item =>
      item.EntityType == "member" && item.EntityId == invitation.MemberId
    )).IsTrue();
    await Assert.That(secondActivation).IsNull();
    await Assert.That(usernameSession?.MemberId).IsEqualTo(invitation.MemberId);
    await Assert.That(emailSession?.MemberId).IsEqualTo(invitation.MemberId);
  }

  [Test]
  public async Task OnlyAdministratorCanInviteAndDuplicateIdentityValuesAreRejected()
  {
    await using var postgres = await StartPostgreSqlAsync();
    var options = CreateOptions(postgres.GetConnectionString());
    await using var database = new LadirchenDbContext(options);
    await database.Database.MigrateAsync();
    var timeProvider = TimeProvider.System;
    var unitOfWork = new DatabaseTypedAggregateUnitOfWork(database, timeProvider);
    var familyRepository = new DatabaseFamilyRepository(database, unitOfWork);
    var economyRepository = new DatabaseFamilyEconomyRepository(database, unitOfWork);
    var progressionRepository = new DatabaseFamilyProgressionRepository(database, unitOfWork);
    var houseRepository = new DatabaseHouseCustomizationRepository(database, unitOfWork);
    var outboxWriter = new FamilyMembershipOutboxWriter(database, timeProvider);
    var authentication = new AuthenticationService(
      database,
      unitOfWork,
      timeProvider,
      outboxWriter
    );
    var memberAccounts = new MemberAccountService(
      database,
      familyRepository,
      economyRepository,
      progressionRepository,
      houseRepository,
      unitOfWork,
      outboxWriter,
      timeProvider
    );
    var familyId = Guid.NewGuid();
    var typedFamilyId = new FamilyId(familyId);
    var administratorId = new FamilyMemberId("guardian-1");
    await authentication.RegisterAsync(Registration(familyId), CancellationToken.None);
    var supporterInvitation = await memberAccounts.CreateInvitationAsync(
      typedFamilyId,
      administratorId,
      new CreateMemberInvitationRequest(
        "supporter@example.com",
        "supporter",
        "Supporter",
        "guardian"
      ),
      CancellationToken.None
    );
    await memberAccounts.ActivateAsync(
      new ActivateMemberAccountRequest(
        supporterInvitation.ActivationCode,
        supporterInvitation.Email,
        "supporter password",
        "supporter"
      ),
      CancellationToken.None
    );
    var supporterId = new FamilyMemberId(supporterInvitation.MemberId);

    await Assert.That(async () => await memberAccounts.CreateInvitationAsync(
      typedFamilyId,
      supporterId,
      new CreateMemberInvitationRequest("new@example.com", null, "New child", "child"),
      CancellationToken.None
    )).Throws<DomainRuleException>();
    var firstChildRequest = new CreateMemberInvitationRequest(
      "first@example.com",
      null,
      "First",
      "child"
    );
    var firstChildInvitation = await memberAccounts.CreateInvitationAsync(
      typedFamilyId,
      administratorId,
      firstChildRequest,
      CancellationToken.None
    );
    var retriedFirstChildInvitation = await memberAccounts.CreateInvitationAsync(
      typedFamilyId,
      administratorId,
      firstChildRequest,
      CancellationToken.None
    );
    var replacedCodeActivation = await memberAccounts.ActivateAsync(
      new ActivateMemberAccountRequest(
        firstChildInvitation.ActivationCode,
        firstChildInvitation.Email,
        "first password",
        "shared-name"
      ),
      CancellationToken.None
    );
    await Assert.That(async () => await memberAccounts.CreateInvitationAsync(
      typedFamilyId,
      administratorId,
      new CreateMemberInvitationRequest("FIRST@example.com", null, "Duplicate", "child"),
      CancellationToken.None
    )).Throws<InvalidOperationException>();
    await memberAccounts.ActivateAsync(
      new ActivateMemberAccountRequest(
        retriedFirstChildInvitation.ActivationCode,
        retriedFirstChildInvitation.Email,
        "first password",
        "shared-name"
      ),
      CancellationToken.None
    );
    var secondChildInvitation = await memberAccounts.CreateInvitationAsync(
      typedFamilyId,
      administratorId,
      new CreateMemberInvitationRequest("second@example.com", null, "Second", "child"),
      CancellationToken.None
    );
    await Assert.That(async () => await memberAccounts.ActivateAsync(
      new ActivateMemberAccountRequest(
        secondChildInvitation.ActivationCode,
        secondChildInvitation.Email,
        "second password",
        "SHARED-NAME"
      ),
      CancellationToken.None
    )).Throws<InvalidOperationException>();
    var family = await familyRepository.LoadAsync(typedFamilyId, CancellationToken.None)
      ?? throw new InvalidOperationException();
    await Assert.That(retriedFirstChildInvitation.MemberId).IsEqualTo(
      firstChildInvitation.MemberId
    );
    await Assert.That(retriedFirstChildInvitation.ActivationCode).IsNotEqualTo(
      firstChildInvitation.ActivationCode
    );
    await Assert.That(replacedCodeActivation).IsNull();
    await Assert.That(family.Root.Members.Single(
      item => item.Id.Value == secondChildInvitation.MemberId
    ).InvitationPending).IsTrue();
  }

  [Test]
  public async Task ExpiredInvitationCanBeReissuedWithoutCreatingAnotherFamilyMember()
  {
    await using var postgres = await StartPostgreSqlAsync();
    var options = CreateOptions(postgres.GetConnectionString());
    await using var database = new LadirchenDbContext(options);
    await database.Database.MigrateAsync();
    var initialTime = new DateTimeOffset(2026, 10, 29, 12, 0, 0, TimeSpan.Zero);
    var initialTimeProvider = new FixedTimeProvider(initialTime);
    var initialUnitOfWork = new DatabaseTypedAggregateUnitOfWork(database, initialTimeProvider);
    var initialFamilyRepository = new DatabaseFamilyRepository(database, initialUnitOfWork);
    var initialEconomyRepository = new DatabaseFamilyEconomyRepository(database, initialUnitOfWork);
    var initialProgressionRepository = new DatabaseFamilyProgressionRepository(
      database,
      initialUnitOfWork
    );
    var initialHouseRepository = new DatabaseHouseCustomizationRepository(
      database,
      initialUnitOfWork
    );
    var initialOutboxWriter = new FamilyMembershipOutboxWriter(database, initialTimeProvider);
    var authentication = new AuthenticationService(
      database,
      initialUnitOfWork,
      initialTimeProvider,
      initialOutboxWriter
    );
    var initialMemberAccounts = new MemberAccountService(
      database,
      initialFamilyRepository,
      initialEconomyRepository,
      initialProgressionRepository,
      initialHouseRepository,
      initialUnitOfWork,
      initialOutboxWriter,
      initialTimeProvider
    );
    var familyId = Guid.NewGuid();
    var typedFamilyId = new FamilyId(familyId);
    var administratorId = new FamilyMemberId("guardian-1");
    var invitationRequest = new CreateMemberInvitationRequest(
      "child@example.com",
      null,
      "Child",
      "child"
    );
    await authentication.RegisterAsync(Registration(familyId), CancellationToken.None);
    var initialInvitation = await initialMemberAccounts.CreateInvitationAsync(
      typedFamilyId,
      administratorId,
      invitationRequest,
      CancellationToken.None
    );

    var afterExpiry = initialTime
      .Add(AuthenticationSecurityPolicy.MemberInvitationLifetime)
      .AddMinutes(1);
    var laterTimeProvider = new FixedTimeProvider(afterExpiry);
    var laterUnitOfWork = new DatabaseTypedAggregateUnitOfWork(database, laterTimeProvider);
    var laterFamilyRepository = new DatabaseFamilyRepository(database, laterUnitOfWork);
    var laterEconomyRepository = new DatabaseFamilyEconomyRepository(database, laterUnitOfWork);
    var laterProgressionRepository = new DatabaseFamilyProgressionRepository(
      database,
      laterUnitOfWork
    );
    var laterHouseRepository = new DatabaseHouseCustomizationRepository(database, laterUnitOfWork);
    var laterOutboxWriter = new FamilyMembershipOutboxWriter(database, laterTimeProvider);
    var laterMemberAccounts = new MemberAccountService(
      database,
      laterFamilyRepository,
      laterEconomyRepository,
      laterProgressionRepository,
      laterHouseRepository,
      laterUnitOfWork,
      laterOutboxWriter,
      laterTimeProvider
    );
    var expiredActivation = await laterMemberAccounts.ActivateAsync(
      new ActivateMemberAccountRequest(
        initialInvitation.ActivationCode,
        initialInvitation.Email,
        "member password",
        "child-user"
      ),
      CancellationToken.None
    );
    var reissuedInvitation = await laterMemberAccounts.CreateInvitationAsync(
      typedFamilyId,
      administratorId,
      invitationRequest,
      CancellationToken.None
    );
    var familyAfterReissue = await laterFamilyRepository.LoadAsync(
      typedFamilyId,
      CancellationToken.None
    ) ?? throw new InvalidOperationException();
    var rotatedCodeRejected = await laterMemberAccounts.ActivateAsync(
      new ActivateMemberAccountRequest(
        initialInvitation.ActivationCode,
        initialInvitation.Email,
        "member password",
        "child-user"
      ),
      CancellationToken.None
    );
    var activated = await laterMemberAccounts.ActivateAsync(
      new ActivateMemberAccountRequest(
        reissuedInvitation.ActivationCode,
        reissuedInvitation.Email,
        "member password",
        "child-user"
      ),
      CancellationToken.None
    );

    await Assert.That(expiredActivation).IsNull();
    await Assert.That(reissuedInvitation.MemberId).IsEqualTo(initialInvitation.MemberId);
    await Assert.That(reissuedInvitation.ActivationCode).IsNotEqualTo(
      initialInvitation.ActivationCode
    );
    await Assert.That(reissuedInvitation.ExpiresAt).IsEqualTo(
      afterExpiry.Add(AuthenticationSecurityPolicy.MemberInvitationLifetime)
    );
    await Assert.That(await database.MemberAccountInvitations.CountAsync()).IsEqualTo(1);
    await Assert.That(familyAfterReissue.Root.Members.Count).IsEqualTo(2);
    await Assert.That(rotatedCodeRejected).IsNull();
    await Assert.That(activated?.MemberId).IsEqualTo(initialInvitation.MemberId);
  }

  private static RegisterAccountRequest Registration(Guid familyId)
  {
    return new RegisterAccountRequest(
      familyId,
      "admin@example.com",
      "Sun Family",
      "Administrator",
      "guardian-1",
      "administrator password",
      "administrator"
    );
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
}
