using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Domain.Family;
using LadirchenApp.Domain.House;
using LadirchenApp.Domain.House.Shop;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Models.ReadModels;
using LadirchenApp.Webhost.Tests.Application.Commands;

namespace LadirchenApp.Webhost.Tests.Application.Queries;

public sealed class FamilyProfileQueryHandlerTests
{
  private static readonly FamilyId _familyId = new(
    Guid.Parse("81c4fb5e-22f5-49d0-88a2-ce3e519c752f")
  );
  private static readonly FamilyMemberId _guardianId = new("guardian");
  private static readonly FamilyMemberId _childId = new("child");

  [Test]
  public async Task ChildProfileOmitsPendingInvitationsAndOtherMemberEmailAddresses()
  {
    var family = Family.Reconstitute(new FamilyState(
      _familyId,
      new IanaTimeZone("Europe/Zurich"),
      FamilyCurrency.CHF,
      new ExchangeRate(1),
      _childId,
      false,
      [
        new FamilyMemberState(
          _guardianId,
          "Guardian",
          MemberRole.Guardian,
          GuardianAccess.Admin,
          false,
          null,
          "guardian@example.test",
          false
        ),
        new FamilyMemberState(
          _childId,
          "Child",
          MemberRole.Child,
          GuardianAccess.Supporter,
          false,
          null,
          null,
          true
        ),
        new FamilyMemberState(
          new FamilyMemberId("pending-guardian"),
          "Pending guardian",
          MemberRole.Guardian,
          GuardianAccess.Supporter,
          true,
          null,
          "pending@example.test",
          false
        ),
      ],
      []
    ));
    var progression = FamilyHouse.Reconstitute(
      family,
      new FamilyHouseState(
        _familyId,
        0,
        0,
        [new ParticipantProgressState(_childId, 3)],
        []
      )
    );
    var house = FamilyHouseShop.CreateInitial(
      family,
      progression.StoredLevel,
      HouseSubscriptionTier.Pro
    );
    var store = new TestTypedAggregateStore(TimeProvider.System);
    store.Seed(family, progression: progression, house: house);
    var handler = new GetFamilyAggregateQueryHandler(
      store,
      store,
      store,
      store,
      TimeProvider.System
    );

    var result = await handler.HandleAsync(
      new GetFamilyAggregateQuery(_familyId, "family-profile", _childId),
      CancellationToken.None
    ) as FamilyProfileReadModelProjection
      ?? throw new InvalidOperationException("The family profile result is required.");
    var response = FamilyProfileReadModelResponse.FromProjection(result);
    var guardian = response.State.Members.Single(member => member.Id == _guardianId.Value);
    var child = response.State.Members.Single(member => member.Id == _childId.Value);

    await Assert.That(result.Members.Any(member => member.Id.Value == "pending-guardian"))
      .IsFalse();
    await Assert.That(result.EmailVisibleMemberIds.Contains(_guardianId)).IsFalse();
    await Assert.That(result.Permissions.ContainsKey(_guardianId)).IsFalse();
    await Assert.That(result.Permissions[_childId].ManageFamily).IsFalse();
    await Assert.That(result.WeeklyStreaks[_childId]).IsEqualTo(3);
    await Assert.That(result.SubscriptionTier).IsEqualTo("pro");
    await Assert.That(response.State.SubscriptionTier).IsEqualTo("pro");
    await Assert.That(guardian.Avatar).IsEqualTo("🧑");
    await Assert.That(guardian.Color).IsEqualTo("#7e8db8");
    await Assert.That(child.Avatar).IsEqualTo("🧒");
    await Assert.That(child.Color).IsEqualTo("#58aa82");
    await Assert.That(child.WeeklyStreak).IsEqualTo(3);
  }
}
