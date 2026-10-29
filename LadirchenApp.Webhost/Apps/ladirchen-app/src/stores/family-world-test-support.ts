import { createPinia, setActivePinia } from "pinia";
import { createApp } from "vue";

import type { FamilyWorldDependencies } from "@/application/ports/family-world-dependencies";
import type { FamilyWorldInitialDataFactory } from "@/application/ports/family-world-initial-data";
import { createEmptyFamilyWorldInitialData } from "@/infrastructure/empty-family-world-initial-data";
import { createDomainId } from "@/domain/shared/identifiers";
import { createFamilyWorldStoreDefinition } from "@/stores/family-world";

export function createFamilyWorldTestDependencies(
  overrides: Partial<FamilyWorldDependencies> = {},
): FamilyWorldDependencies {
  let activeFamilyId = createDomainId.family("c776a9b6-c37d-4a51-91ba-269a67274478");
  const emptyReadModelService = { load: async () => null };

  return {
    authentication: {
      authenticate: async () => null,
      register: async () => undefined,
      signOut: async () => undefined,
    },
    clientStorage: {
      getItem: () => null,
      removeItem: () => undefined,
      setItem: () => undefined,
    },
    contributionsService: emptyReadModelService,
    familyAggregateUpdates: {
      connect: async () => undefined,
      disconnect: async () => undefined,
    },
    familyCommands: {
      execute: async () => [],
    },
    familyContext: {
      get activeFamilyId() {
        return activeFamilyId;
      },
      setActiveFamilyId(familyId) {
        activeFamilyId = familyId;
      },
    },
    familyProfileService: emptyReadModelService,
    familyProgressionService: emptyReadModelService,
    homeCustomizationService: emptyReadModelService,
    initialDataFactory: { create: createEmptyFamilyWorldInitialData },
    rewardShopService: emptyReadModelService,
    savingsService: emptyReadModelService,
    scheduler: {
      cancel: (handle) => clearTimeout(handle),
      schedule: (callback, delayMilliseconds) => setTimeout(callback, delayMilliseconds),
    },
    ...overrides,
  };
}

export function createFamilyWorldTestStore(
  dependencies: FamilyWorldDependencies,
  initialDataFactory: FamilyWorldInitialDataFactory = { create: createEmptyFamilyWorldInitialData },
) {
  const pinia = createPinia();
  pinia.use(() => ({ $familyWorld: dependencies }));
  createApp({}).use(pinia);
  setActivePinia(pinia);

  return createFamilyWorldStoreDefinition(initialDataFactory)();
}
