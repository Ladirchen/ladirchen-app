using System;
using System.Threading.Tasks;

using LadirchenApp.Webhost.Infrastructure;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LadirchenApp.Webhost.Realtime;

[Authorize(AuthenticationSchemes = FamilySessionIdentity.AuthenticationScheme)]
public sealed class FamilyUpdatesHub : Hub
{
  public Task SubscribeToFamily(Guid familyId)
  {
    if (familyId == Guid.Empty)
    {
      throw new HubException("A family ID cannot be empty.");
    }

    if (!FamilySessionIdentity.BelongsToFamily(Context.User!, familyId))
    {
      throw new HubException("The authenticated session does not belong to this family.");
    }

    return Groups.AddToGroupAsync(Context.ConnectionId, GroupName(familyId));
  }

  public Task UnsubscribeFromFamily(Guid familyId)
  {
    return Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(familyId));
  }

  public static string GroupName(Guid familyId)
  {
    return $"family:{familyId:D}";
  }
}
