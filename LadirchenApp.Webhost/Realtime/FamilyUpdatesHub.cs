using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.SignalR;

namespace LadirchenApp.Webhost.Realtime;

public sealed class FamilyUpdatesHub : Hub
{
  public Task SubscribeToFamily(Guid familyId)
  {
    if (familyId == Guid.Empty)
    {
      throw new HubException("A family ID cannot be empty.");
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
