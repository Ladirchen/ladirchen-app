import type { FamilyAggregateType, SaveVersionedAggregateCommand, VersionedAggregateSnapshot } from '@/application/contracts/versioned-aggregate-contract';
import { isVersionedAggregateSnapshot } from '@/application/contracts/family-aggregate-validation';
import { AggregateConflictError } from '@/application/ports/versioned-aggregate-repository';
import type { VersionedAggregateRepository } from '@/application/ports/versioned-aggregate-repository';
import { createDomainId } from '@/domain/types';
import type { FamilyId } from '@/domain/types';

export const parseLocalStorageValue = (key: string): unknown => {
  try {
    const serialized = localStorage.getItem(key);
    if (serialized === null) {
      return null;
    }
    const parsed: unknown = JSON.parse(serialized);
    return parsed;
  } catch {
    return null;
  }
};

interface LocalStorageAggregateOptions<
  TAggregateType extends FamilyAggregateType,
  TSchemaVersion extends number,
  TState,
> {
  readonly aggregateType: TAggregateType;
  readonly isState: (value: unknown) => value is TState;
  readonly migrate?: (stored: unknown, familyId: FamilyId) => TState | null;
  readonly schemaVersion: TSchemaVersion;
}

const storageAvailable = (): boolean => typeof localStorage !== 'undefined';

export class LocalStorageVersionedAggregateRepository<
  TAggregateType extends FamilyAggregateType,
  TSchemaVersion extends number,
  TState,
> implements VersionedAggregateRepository<TAggregateType, TSchemaVersion, TState> {
  public constructor(private readonly options: LocalStorageAggregateOptions<TAggregateType, TSchemaVersion, TState>) {}

  public async load(familyId: FamilyId): Promise<VersionedAggregateSnapshot<TAggregateType, TSchemaVersion, TState> | null> {
    if (!storageAvailable()) {
      return null;
    }
    const stored = parseLocalStorageValue(this.storageKey(familyId));
    if (this.isSnapshot(stored) && stored.familyId === familyId) {
      return structuredClone(stored);
    }
    const migratedState = this.options.migrate?.(stored, familyId) ?? null;
    return migratedState === null || !this.options.isState(migratedState)
      ? null
      : this.createMigratedSnapshot(familyId, migratedState);
  }

  public async save(command: SaveVersionedAggregateCommand<TAggregateType, TState>): Promise<VersionedAggregateSnapshot<TAggregateType, TSchemaVersion, TState>> {
    if (!this.options.isState(command.state)) {
      throw new TypeError(`The ${this.options.aggregateType} aggregate contains invalid state.`);
    }
    if (storageAvailable()) {
      const current = parseLocalStorageValue(this.storageKey(command.familyId));
      if (this.isSnapshot(current) && current.revision !== command.expectedRevision) {
        throw new AggregateConflictError(this.options.aggregateType);
      }
    }
    const snapshot: VersionedAggregateSnapshot<TAggregateType, TSchemaVersion, TState> = {
      aggregateType: this.options.aggregateType,
      familyId: command.familyId,
      revision: command.expectedRevision + 1,
      schemaVersion: this.options.schemaVersion,
      state: structuredClone(command.state),
      updatedAt: new Date().toISOString(),
      updatedBy: command.updatedBy,
    };
    if (storageAvailable()) {
      localStorage.setItem(this.storageKey(command.familyId), JSON.stringify(snapshot));
    }
    return snapshot;
  }

  private createMigratedSnapshot(familyId: FamilyId, state: TState): VersionedAggregateSnapshot<TAggregateType, TSchemaVersion, TState> {
    return {
      aggregateType: this.options.aggregateType,
      familyId,
      revision: 0,
      schemaVersion: this.options.schemaVersion,
      state: structuredClone(state),
      updatedAt: new Date(0).toISOString(),
      updatedBy: createDomainId.familyMember('legacy-local-user'),
    };
  }

  private isSnapshot(value: unknown): value is VersionedAggregateSnapshot<TAggregateType, TSchemaVersion, TState> {
    return isVersionedAggregateSnapshot(
      value,
      this.options.aggregateType,
      this.options.schemaVersion,
      this.options.isState,
    );
  }

  private storageKey(familyId: FamilyId): string {
    return `ladirchen:${this.options.aggregateType}:${familyId}`;
  }
}
