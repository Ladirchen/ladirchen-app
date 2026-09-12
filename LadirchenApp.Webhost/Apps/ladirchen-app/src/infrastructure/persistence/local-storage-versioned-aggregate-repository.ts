import type { FamilyAggregateType, SaveVersionedAggregateCommand, VersionedAggregateSnapshot } from '@/application/contracts/versioned-aggregate-contract';
import { isVersionedAggregateSnapshot } from '@/application/contracts/family-aggregate-validation';
import { AggregateConflictError } from '@/application/ports/versioned-aggregate-repository';
import type { VersionedAggregateRepository } from '@/application/ports/versioned-aggregate-repository';
import type { FamilyId } from '@/domain/shared/identifiers';
import type { ClientStorage } from '@/application/ports/client-storage';
import { browserClientStorage } from '@/infrastructure/storage/browser-client-storage';

const parseStoredValue = (storage: ClientStorage, key: string): unknown => {
  try {
    const serialized = storage.getItem(key);
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
  readonly schemaVersion: TSchemaVersion;
}

export class LocalStorageVersionedAggregateRepository<
  TAggregateType extends FamilyAggregateType,
  TSchemaVersion extends number,
  TState,
> implements VersionedAggregateRepository<TAggregateType, TSchemaVersion, TState> {
  public constructor(
    private readonly options: LocalStorageAggregateOptions<TAggregateType, TSchemaVersion, TState>,
    private readonly storage: ClientStorage = browserClientStorage,
  ) {}

  public async load(familyId: FamilyId): Promise<VersionedAggregateSnapshot<TAggregateType, TSchemaVersion, TState> | null> {
    const stored = parseStoredValue(this.storage, this.storageKey(familyId));
    if (this.isSnapshot(stored) && stored.familyId === familyId) {
      return structuredClone(stored);
    }
    return null;
  }

  public async save(command: SaveVersionedAggregateCommand<TAggregateType, TState>): Promise<VersionedAggregateSnapshot<TAggregateType, TSchemaVersion, TState>> {
    if (!this.options.isState(command.state)) {
      throw new TypeError(`The ${this.options.aggregateType} aggregate contains invalid state.`);
    }
    const current = parseStoredValue(this.storage, this.storageKey(command.familyId));
    if (this.isSnapshot(current) && current.revision !== command.expectedRevision) {
      throw new AggregateConflictError(this.options.aggregateType);
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
    this.storage.setItem(this.storageKey(command.familyId), JSON.stringify(snapshot));
    return snapshot;
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
