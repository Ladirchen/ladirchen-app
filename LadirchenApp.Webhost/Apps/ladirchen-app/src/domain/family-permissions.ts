import type { FamilyMember } from './types';

export interface FamilyPermissions {
  canInviteMembers: boolean;
  canManageContent: boolean;
  canManageFamily: boolean;
  canManageGoals: boolean;
  canSupportChildGoals: boolean;
  canViewFamilyGoals: boolean;
  canViewGuardianGoals: boolean;
}

const noPermissions: FamilyPermissions = {
  canInviteMembers: false,
  canManageContent: false,
  canManageFamily: false,
  canManageGoals: false,
  canSupportChildGoals: false,
  canViewFamilyGoals: false,
  canViewGuardianGoals: false,
};

export const resolveFamilyPermissions = (member?: FamilyMember): FamilyPermissions => {
  if (!member) {return noPermissions;}
  if (member.role === 'child') {
    return { ...noPermissions, canViewFamilyGoals: true };
  }
  if (member.guardianAccess === 'admin') {
    return {
      canInviteMembers: true,
      canManageContent: true,
      canManageFamily: true,
      canManageGoals: true,
      canSupportChildGoals: true,
      canViewFamilyGoals: true,
      canViewGuardianGoals: true,
    };
  }
  return { ...noPermissions, canSupportChildGoals: true };
};
