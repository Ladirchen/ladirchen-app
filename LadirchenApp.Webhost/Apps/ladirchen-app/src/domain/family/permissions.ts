/** Capabilities derived from the signed-in family member. */
export interface FamilyPermissions {
  canInviteMembers: boolean;
  canManageContent: boolean;
  canManageChildTasks: boolean;
  canManageFamily: boolean;
  canManageGoals: boolean;
  canSupportChildGoals: boolean;
  canViewFamilyGoals: boolean;
  canViewGuardianGoals: boolean;
  canReviewChildTasks: boolean;
  canViewTaskAssignments: boolean;
  canViewHouse: boolean;
}
export const NO_FAMILY_PERMISSIONS: FamilyPermissions = {
  canInviteMembers: false,
  canManageContent: false,
  canManageChildTasks: false,
  canManageFamily: false,
  canManageGoals: false,
  canSupportChildGoals: false,
  canViewFamilyGoals: false,
  canViewGuardianGoals: false,
  canReviewChildTasks: false,
  canViewTaskAssignments: false,
  canViewHouse: false,
};
