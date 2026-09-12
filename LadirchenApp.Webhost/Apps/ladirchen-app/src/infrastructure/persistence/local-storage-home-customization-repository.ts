import { HOME_CUSTOMIZATION_AGGREGATE_TYPE, HOME_CUSTOMIZATION_SCHEMA_VERSION } from '@/application/contracts/home-customization-contract';
import type { HomeCustomizationState } from '@/application/contracts/home-customization-contract';
import { isHomeCustomizationState } from '@/application/contracts/family-aggregate-validation';

import { LocalStorageVersionedAggregateRepository } from './local-storage-versioned-aggregate-repository';

export class LocalStorageHomeCustomizationRepository extends LocalStorageVersionedAggregateRepository<
  typeof HOME_CUSTOMIZATION_AGGREGATE_TYPE,
  typeof HOME_CUSTOMIZATION_SCHEMA_VERSION,
  HomeCustomizationState
> {
  public constructor() {
    super({ aggregateType: HOME_CUSTOMIZATION_AGGREGATE_TYPE, isState: isHomeCustomizationState, schemaVersion: HOME_CUSTOMIZATION_SCHEMA_VERSION });
  }
}
