/** Capabilities derived from the signed-in family member. */
export interface FamilyPermissions {
  canInviteMembers: boolean;
  canManageContent: boolean;
  canManageFamily: boolean;
  canManageGoals: boolean;
  canSupportChildGoals: boolean;
  canViewFamilyGoals: boolean;
  canViewGuardianGoals: boolean;
}
export const NO_FAMILY_PERMISSIONS: FamilyPermissions = {
  canInviteMembers: false,
  canManageContent: false,
  canManageFamily: false,
  canManageGoals: false,
  canSupportChildGoals: false,
  canViewFamilyGoals: false,
  canViewGuardianGoals: false,
};
