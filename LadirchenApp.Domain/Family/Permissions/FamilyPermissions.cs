namespace LadirchenApp.Domain.Family.Permissions;

public sealed record FamilyPermissions(
  bool InviteMembers,
  bool ManageGoals,
  bool ManageContent,
  bool ManageFamily,
  bool SupportGoals,
  bool ViewFamilyGoals,
  bool ViewGuardianGoals,
  bool ManageChildTasks,
  bool ReviewChildTasks,
  bool ViewTaskAssignments,
  bool ViewHouse,
  bool PurchaseHouseContentWithoutBalance,
  bool ManageShop,
  bool ReceivePushNotifications
);
