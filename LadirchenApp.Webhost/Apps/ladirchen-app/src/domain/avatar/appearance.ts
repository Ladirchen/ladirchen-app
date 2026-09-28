import type { AvatarAccessoryId, AvatarFunAccessoryId, AvatarSeasonalAccessoryId } from "./accessory-catalog";
import type { AvatarHairColorId, AvatarOutfitColorId, AvatarSkinToneId } from "./color-catalog";
import type { AvatarFace, AvatarFaceShape } from "./face-catalog";
import type { AvatarHair } from "./hair-catalog";
import type { AvatarOutfit } from "./outfit-catalog";
import type { FamilyMember } from "@/domain/family/types";

export const AVATAR_AGES = ["child", "adult", "senior"] as const;
export type AvatarAge = typeof AVATAR_AGES[number];

export interface AvatarAppearance {
  age: AvatarAge;
  skinToneId: AvatarSkinToneId;
  faceShape: AvatarFaceShape;
  face: AvatarFace;
  hair: AvatarHair;
  hairColorId: AvatarHairColorId;
  outfit: AvatarOutfit;
  outfitColorId: AvatarOutfitColorId;
  accessoryId: AvatarAccessoryId;
  funAccessoryId: AvatarFunAccessoryId;
  seasonalAccessoryId: AvatarSeasonalAccessoryId;
}

export const DEFAULT_AVATAR_APPEARANCE = {
  age: "child",
  skinToneId: "skin-medium",
  faceShape: "soft",
  face: "happy",
  hair: "ponytail",
  hairColorId: "hair-brown",
  outfit: "hoodie",
  outfitColorId: "outfit-blue",
  accessoryId: "none",
  funAccessoryId: "none",
  seasonalAccessoryId: "none",
} as const satisfies AvatarAppearance;

export const GUARDIAN_AVATAR_PRESETS = {
  adult: {
    age: "adult",
    face: "happy",
    faceShape: "soft",
    hair: "short",
    hairColorId: "hair-brown",
    outfit: "shirt",
    outfitColorId: "outfit-emerald",
    accessoryId: "none",
  },
  grandma: {
    age: "senior",
    face: "confident",
    faceShape: "soft",
    hair: "bun",
    hairColorId: "hair-white",
    outfit: "cardigan",
    outfitColorId: "outfit-lavender",
    accessoryId: "glasses",
  },
  grandpa: {
    age: "senior",
    face: "confident",
    faceShape: "angular",
    hair: "short",
    hairColorId: "hair-silver",
    outfit: "blazer",
    outfitColorId: "outfit-slate",
    accessoryId: "glasses",
  },
} as const satisfies Record<string, Partial<AvatarAppearance>>;

export type GuardianAvatarPreset = keyof typeof GUARDIAN_AVATAR_PRESETS;

export const createDefaultAvatarAppearance = (): AvatarAppearance => ({ ...DEFAULT_AVATAR_APPEARANCE });

export const normalizeAvatarAppearance = (
  appearance?: AvatarAppearance,
  fallback: AvatarAppearance = createDefaultAvatarAppearance(),
): AvatarAppearance => ({ ...(appearance ?? fallback) });

export const createGuardianAvatarAppearance = (preset: GuardianAvatarPreset = "adult"): AvatarAppearance => ({
  ...DEFAULT_AVATAR_APPEARANCE,
  ...GUARDIAN_AVATAR_PRESETS[preset],
});

const CHILD_AVATAR_VARIANTS: ReadonlyArray<Partial<AvatarAppearance>> = [
  { hair: "ponytail", outfitColorId: "outfit-blue" },
  { hair: "short", hairColorId: "hair-black", outfit: "overalls", outfitColorId: "outfit-gold" },
  { hair: "curls", hairColorId: "hair-brown", outfit: "space", outfitColorId: "outfit-ocean" },
];

export const resolveChildAvatarAppearance = (
  appearance: AvatarAppearance | undefined,
  childIndex: number,
): AvatarAppearance => {
  if (appearance) {return normalizeAvatarAppearance(appearance);}
  const safeIndex = Math.max(0, childIndex);
  return {
    ...createDefaultAvatarAppearance(),
    ...(CHILD_AVATAR_VARIANTS[safeIndex % CHILD_AVATAR_VARIANTS.length] ?? {}),
  };
};

export const resolveFamilyMemberAvatarAppearance = (
  member: Pick<FamilyMember, "appearance" | "id" | "role">,
  familyMembers: ReadonlyArray<Pick<FamilyMember, "id" | "role">>,
): AvatarAppearance => {
  const roleIndex = Math.max(0, familyMembers
    .filter(candidate => candidate.role === member.role)
    .findIndex(candidate => candidate.id === member.id));
  if (member.role === "guardian") {
    return normalizeAvatarAppearance(
      member.appearance,
      createGuardianAvatarAppearance(roleIndex % 2 === 0 ? "adult" : "grandpa"),
    );
  }
  return resolveChildAvatarAppearance(member.appearance, roleIndex);
};
