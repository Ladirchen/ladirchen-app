using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Domain.Shared;
using LadirchenApp.Webhost.Tests.Application.Commands;

namespace LadirchenApp.Webhost.Tests.Application.Queries;

public sealed class FamilyProfileQueryHandlerTests
{
  private static readonly FamilyId _familyId = new(Guid.Parse("81c4fb5e-22f5-49d0-88a2-ce3e519c752f"));
  private static readonly FamilyMemberId _guardianId = new("guardian");
  private static readonly FamilyMemberId _childId = new("child");

  [Test]
  public async Task ChildProfileOmitsPendingInvitationsAndOtherMemberEmailAddresses()
  {
    var repository = new TestFamilyAggregateSnapshotRepository();
    repository.Seed(new FamilyAggregateSnapshot(
      _familyId,
      "family-profile",
      2,
      1,
      JsonSerializer.SerializeToElement(new
      {
        timeZone = "Europe/Zurich",
        pets = Array.Empty<object>(),
        members = new object[]
        {
          new
          {
            id = _guardianId.Value,
            name = "Guardian",
            role = "guardian",
            guardianAccess = "admin",
            email = "guardian@example.test",
            participatesInWeeklyGoal = false,
          },
          new
          {
            id = _childId.Value,
            name = "Child",
            role = "child",
            participatesInWeeklyGoal = true,
          },
          new
          {
            id = "pending-guardian",
            name = "Pending guardian",
            role = "guardian",
            guardianAccess = "supporter",
            email = "pending@example.test",
            invitationPending = true,
            participatesInWeeklyGoal = false,
          },
        },
      }),
      _guardianId,
      DateTimeOffset.UnixEpoch
    ));
    var handler = new GetFamilyAggregateQueryHandler(repository, TimeProvider.System);

    var result = await handler.HandleAsync(
      new GetFamilyAggregateQuery(_familyId, "family-profile", _childId),
      CancellationToken.None
    );
    var members = result?.State.GetProperty("members").EnumerateArray().ToArray()
      ?? throw new InvalidOperationException("The family profile result is required.");
    var guardian = members.Single(member => member.GetProperty("id").GetString() == _guardianId.Value);
    var child = members.Single(member => member.GetProperty("id").GetString() == _childId.Value);

    await Assert.That(members.Any(member => member.GetProperty("id").GetString() == "pending-guardian"))
      .IsFalse();
    await Assert.That(guardian.TryGetProperty("email", out _)).IsFalse();
    await Assert.That(guardian.TryGetProperty("permissions", out _)).IsFalse();
    await Assert.That(child.GetProperty("permissions").GetProperty("canManageFamily").GetBoolean())
      .IsFalse();
  }
}
