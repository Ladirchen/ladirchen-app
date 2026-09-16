import type { AggregateDraft, FamilyAggregateType, SaveVersionedAggregateCommand, VersionedAggregateSnapshot } from '@/application/contracts/versioned-aggregate-contract';
import type { VersionedAggregateRepository } from '@/application/ports/versioned-aggregate-repository';
import type { FamilyId } from '@/domain/types';

interface AggregateSaveState<TState> {
  pendingDraft: AggregateDraft<TState> | null;
  revision: number;
  saveQueue: Promise<void>;
  saveTimer: ReturnType<typeof setTimeout> | null;
}

const cloneSerializableDraft = <TState>(draft: AggregateDraft<TState>): AggregateDraft<TState> => {
  // JSON serialization is intentional at this boundary: Pinia exposes nested
  // state through reactive proxies, which structuredClone cannot clone. The
  // aggregate contracts contain JSON data only, matching both persistence
  // adapters and the future HTTP transport.
  const serialized = JSON.stringify(draft);
  const clone: unknown = JSON.parse(serialized);
  return clone as AggregateDraft<TState>;
};

export class VersionedAggregateService<
  TAggregateType extends FamilyAggregateType,
  TSchemaVersion extends number,
  TState,
> {
  private readonly saveStates = new Map<FamilyId, AggregateSaveState<TState>>();

  public constructor(
    private readonly aggregateType: TAggregateType,
    private readonly repository: VersionedAggregateRepository<TAggregateType, TSchemaVersion, TState>,
    private readonly debounceMilliseconds = 180,
  ) {}

  public async load(familyId: FamilyId): Promise<VersionedAggregateSnapshot<TAggregateType, TSchemaVersion, TState> | null> {
    const snapshot = await this.repository.load(familyId);
    this.saveStateFor(familyId).revision = snapshot?.revision ?? 0;
    return snapshot;
  }

  public scheduleSave(draft: AggregateDraft<TState>, onError: (error: unknown) => void): void {
    const saveState = this.saveStateFor(draft.familyId);
    saveState.pendingDraft = cloneSerializableDraft(draft);
    if (saveState.saveTimer !== null) {
      clearTimeout(saveState.saveTimer);
    }
    saveState.saveTimer = setTimeout(() => {
      saveState.saveTimer = null;
      this.enqueuePendingSave(draft.familyId, onError);
    }, this.debounceMilliseconds);
  }

  private enqueuePendingSave(familyId: FamilyId, onError: (error: unknown) => void): void {
    const saveState = this.saveStateFor(familyId);
    saveState.saveQueue = saveState.saveQueue
      .then(() => this.flushPendingSave(familyId))
      .catch(onError);
  }

  private async flushPendingSave(familyId: FamilyId): Promise<void> {
    const saveState = this.saveStateFor(familyId);
    while (saveState.pendingDraft !== null) {
      const draft = saveState.pendingDraft;
      saveState.pendingDraft = null;
      const command: SaveVersionedAggregateCommand<TAggregateType, TState> = {
        ...draft,
        aggregateType: this.aggregateType,
        expectedRevision: saveState.revision,
      };
      try {
        const saved = await this.repository.save(command);
        saveState.revision = saved.revision;
      } catch (error) {
        saveState.pendingDraft ??= draft;
        throw error;
      }
    }
  }

  private saveStateFor(familyId: FamilyId): AggregateSaveState<TState> {
    const current = this.saveStates.get(familyId);
    if (current) {
      return current;
    }
    const created: AggregateSaveState<TState> = {
      pendingDraft: null,
      revision: 0,
      saveQueue: Promise.resolve(),
      saveTimer: null,
    };
    this.saveStates.set(familyId, created);
    return created;
  }
}
