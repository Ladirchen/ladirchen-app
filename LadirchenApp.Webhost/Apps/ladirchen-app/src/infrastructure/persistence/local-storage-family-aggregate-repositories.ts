import { CONTRIBUTIONS_AGGREGATE_TYPE, CONTRIBUTIONS_SCHEMA_VERSION } from '@/application/contracts/contributions-contract';
import type { ContributionsState } from '@/application/contracts/contributions-contract';
import { FAMILY_PROFILE_AGGREGATE_TYPE, FAMILY_PROFILE_SCHEMA_VERSION } from '@/application/contracts/family-profile-contract';
import type { FamilyProfileState } from '@/application/contracts/family-profile-contract';
import { FAMILY_PROGRESSION_AGGREGATE_TYPE, FAMILY_PROGRESSION_SCHEMA_VERSION } from '@/application/contracts/family-progression-contract';
import type { FamilyProgressionState } from '@/application/contracts/family-progression-contract';
import { REWARD_SHOP_AGGREGATE_TYPE, REWARD_SHOP_SCHEMA_VERSION } from '@/application/contracts/reward-shop-contract';
import type { RewardShopState } from '@/application/contracts/reward-shop-contract';
import { SAVINGS_AGGREGATE_TYPE, SAVINGS_SCHEMA_VERSION } from '@/application/contracts/savings-contract';
import type { SavingsState } from '@/application/contracts/savings-contract';
import { isContributionsState, isFamilyProfileState, isFamilyProgressionState, isRecord, isRewardShopState, isSavingsState } from '@/application/contracts/family-aggregate-validation';
import type { FamilyAggregateRepositories } from '@/application/ports/family-aggregate-repositories';
import { isPersistedAvatarAppearance, normalizeAvatarAppearance } from '@/domain/avatar';
import type { FamilyId } from '@/domain/types';
import { createFamilyBalances, createFamilyMembers, createFamilyPets, createSavingGoals } from '@/infrastructure/fixtures/family-world-fixtures';
import { normalizeLegacyFamilyPetKind } from '@/infrastructure/persistence/legacy-value-migrations';

import { LocalStorageVersionedAggregateRepository, parseLocalStorageValue } from './local-storage-versioned-aggregate-repository';
import { LocalStorageHomeCustomizationRepository } from './local-storage-home-customization-repository';

const LEGACY_KEYS = {
  exchangeRate: 'ladirchen-exchange-rate',
  familyCurrency: 'ladirchen-family-currency',
  familyMembers: 'ladirchen-family-members',
  familyPets: 'ladirchen-family-pets',
  familySetup: 'ladirchen-family-setup',
  pendingGifts: 'ladirchen-pending-gifts',
} as const;

const readLegacyArray = (key: string): unknown[] | null => {
  const value = parseLocalStorageValue(key);
  return Array.isArray(value) ? value : null;
};
const readLegacyText = (key: string): string | null => {
  try {
    return localStorage.getItem(key);
  } catch {
    return null;
  }
};

const normalizeLegacyMembers = (value: unknown): unknown => {
  if (!Array.isArray(value)) {return value;}
  return value.map((member) => {
    if (!isRecord(member) || member.appearance === undefined || !isPersistedAvatarAppearance(member.appearance)) {
      return member;
    }
    return { ...member, appearance: normalizeAvatarAppearance(member.appearance) };
  });
};

const migrateFamilyProfile = (stored: unknown, _familyId: FamilyId): FamilyProfileState | null => {
  if (isRecord(stored) && isRecord(stored.state)) {
    const storedCandidate: unknown = {
      ...stored.state,
      members: normalizeLegacyMembers(stored.state.members),
    };
    if (isFamilyProfileState(storedCandidate)) {return storedCandidate;}
  }
  const members = readLegacyArray(LEGACY_KEYS.familyMembers);
  const legacyPets = readLegacyArray(LEGACY_KEYS.familyPets);
  const pets = legacyPets?.map(pet => isRecord(pet)
    ? { ...pet, kind: normalizeLegacyFamilyPetKind(pet.kind) }
    : pet) ?? null;
  const onboardingCompleted = readLegacyText(LEGACY_KEYS.familySetup) === 'completed';
  if (members === null && pets === null && !onboardingCompleted) {
    return null;
  }
  const candidate: unknown = {
    members: normalizeLegacyMembers(members ?? createFamilyMembers()),
    onboardingCompleted,
    pets: pets ?? createFamilyPets(),
    subscriptionTier: 'pro',
  };
  return isFamilyProfileState(candidate) ? candidate : null;
};

const migrateSavings = (_stored: unknown, _familyId: FamilyId): SavingsState | null => {
  const currency = readLegacyText(LEGACY_KEYS.familyCurrency);
  const exchangeRateValue = readLegacyText(LEGACY_KEYS.exchangeRate);
  const exchangeRate = Number(exchangeRateValue);
  const pendingGuardianGifts = readLegacyArray(LEGACY_KEYS.pendingGifts);
  if (currency === null && exchangeRateValue === null && pendingGuardianGifts === null) {
    return null;
  }
  const candidate: unknown = {
    balances: createFamilyBalances(),
    familyCurrencyCode: currency === 'EUR' || currency === 'HUF' ? currency : 'CHF',
    goals: createSavingGoals(),
    ladirchenPerCurrencyUnit: Number.isFinite(exchangeRate) && exchangeRate >= 1 ? exchangeRate : 10,
    pendingGuardianGifts: pendingGuardianGifts ?? [],
  };
  return isSavingsState(candidate) ? candidate : null;
};

export class LocalStorageFamilyProfileRepository extends LocalStorageVersionedAggregateRepository<
  typeof FAMILY_PROFILE_AGGREGATE_TYPE,
  typeof FAMILY_PROFILE_SCHEMA_VERSION,
  FamilyProfileState
> {
  public constructor() {
    super({ aggregateType: FAMILY_PROFILE_AGGREGATE_TYPE, isState: isFamilyProfileState, migrate: migrateFamilyProfile, schemaVersion: FAMILY_PROFILE_SCHEMA_VERSION });
  }
}

export class LocalStorageContributionsRepository extends LocalStorageVersionedAggregateRepository<
  typeof CONTRIBUTIONS_AGGREGATE_TYPE,
  typeof CONTRIBUTIONS_SCHEMA_VERSION,
  ContributionsState
> {
  public constructor() {
    super({ aggregateType: CONTRIBUTIONS_AGGREGATE_TYPE, isState: isContributionsState, schemaVersion: CONTRIBUTIONS_SCHEMA_VERSION });
  }
}

export class LocalStorageSavingsRepository extends LocalStorageVersionedAggregateRepository<
  typeof SAVINGS_AGGREGATE_TYPE,
  typeof SAVINGS_SCHEMA_VERSION,
  SavingsState
> {
  public constructor() {
    super({ aggregateType: SAVINGS_AGGREGATE_TYPE, isState: isSavingsState, migrate: migrateSavings, schemaVersion: SAVINGS_SCHEMA_VERSION });
  }
}

export class LocalStorageRewardShopRepository extends LocalStorageVersionedAggregateRepository<
  typeof REWARD_SHOP_AGGREGATE_TYPE,
  typeof REWARD_SHOP_SCHEMA_VERSION,
  RewardShopState
> {
  public constructor() {
    super({ aggregateType: REWARD_SHOP_AGGREGATE_TYPE, isState: isRewardShopState, schemaVersion: REWARD_SHOP_SCHEMA_VERSION });
  }
}

export class LocalStorageFamilyProgressionRepository extends LocalStorageVersionedAggregateRepository<
  typeof FAMILY_PROGRESSION_AGGREGATE_TYPE,
  typeof FAMILY_PROGRESSION_SCHEMA_VERSION,
  FamilyProgressionState
> {
  public constructor() {
    super({ aggregateType: FAMILY_PROGRESSION_AGGREGATE_TYPE, isState: isFamilyProgressionState, schemaVersion: FAMILY_PROGRESSION_SCHEMA_VERSION });
  }
}

export const createLocalStorageFamilyAggregateRepositories = (): FamilyAggregateRepositories => ({
  contributions: new LocalStorageContributionsRepository(),
  familyProfile: new LocalStorageFamilyProfileRepository(),
  familyProgression: new LocalStorageFamilyProgressionRepository(),
  homeCustomization: new LocalStorageHomeCustomizationRepository(),
  rewardShop: new LocalStorageRewardShopRepository(),
  savings: new LocalStorageSavingsRepository(),
});
