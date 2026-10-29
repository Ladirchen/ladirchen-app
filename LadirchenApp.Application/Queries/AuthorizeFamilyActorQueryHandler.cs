using System;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

using LadirchenApp.Application.Aggregates;
using LadirchenApp.Domain.Shared;

namespace LadirchenApp.Application.Queries;

public sealed class AuthorizeFamilyActorQueryHandler
{
  private readonly IFamilyAggregateSnapshotRepository _repository;

  public AuthorizeFamilyActorQueryHandler(IFamilyAggregateSnapshotRepository repository)
  {
    _repository = repository;
  }

  public async Task<bool> HandleAsync(
    FamilyId familyId,
    FamilyMemberId authenticatedMemberId,
    FamilyMemberId requestedActorId,
    CancellationToken cancellationToken
  )
  {
    if (authenticatedMemberId == requestedActorId)
    {
      return true;
    }

    var profile = await _repository.GetAsync(familyId, "family-profile", cancellationToken);

    if (profile is null)
    {
      return false;
    }

    var state = JsonNode.Parse(profile.State.GetRawText()) as JsonObject;
    var member = (state?["members"] as JsonArray)?
      .OfType<JsonObject>()
      .SingleOrDefault(item => item["id"]?.GetValue<string>() == authenticatedMemberId.Value);

    return member is not null && FamilyMemberPermissionResolver.Resolve(member).ManageFamily;
  }
}
