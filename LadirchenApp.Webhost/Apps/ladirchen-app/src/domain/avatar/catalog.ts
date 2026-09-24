export const AVATAR_PROFILE_ROLES = ["child", "guardian"] as const;

export type AvatarProfileRole = typeof AVATAR_PROFILE_ROLES[number];

export interface AvatarPartDefinition<Id extends string = string> {
  readonly id: Id;
  readonly profiles: readonly AvatarProfileRole[];
}

export const defineAvatarPart = <const Id extends string>(
  id: Id,
  profiles: readonly AvatarProfileRole[] = AVATAR_PROFILE_ROLES,
): AvatarPartDefinition<Id> => ({ id, profiles });

export const avatarPartIds = <const Definitions extends readonly AvatarPartDefinition[]>(
  definitions: Definitions,
): Array<Definitions[number]["id"]> => definitions.map(definition => definition.id);

export const avatarPartsForProfile = <const Definitions extends readonly AvatarPartDefinition[]>(
  definitions: Definitions,
  profile: AvatarProfileRole,
): Definitions[number][] => definitions.filter(definition => definition.profiles.includes(profile));
