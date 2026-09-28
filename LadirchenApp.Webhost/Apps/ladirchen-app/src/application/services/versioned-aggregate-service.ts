import type { AggregateDraft, FamilyAggregateType, SaveVersionedAggregateCommand, VersionedAggregateSnapshot } from "@/application/contracts/versioned-aggregate-contract";
import { AggregateConflictError } from "@/application/ports/versioned-aggregate-repository";
import type { VersionedAggregateRepository } from "@/application/ports/versioned-aggregate-repository";
import type { FamilyId } from "@/domain/shared/identifiers";
import { mergeSerializableAggregateState } from "./three-way-aggregate-merge";
import type { AggregateConflictResolver } from "./three-way-aggregate-merge";

interface AggregateSaveState<TState> {
  baseState: TState | null;
  pendingDraft: AggregateDraft<TState> | null;
  revision: number;
  saveQueue: Promise<void>;
  saveTimer: ReturnType<typeof setTimeout> | null;
}

const cloneSerializableDraft = <TState>(draft: AggregateDraft<TState>): AggregateDraft<TState> => {
  return JSON.parse(JSON.stringify(draft)) as AggregateDraft<TState>;
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
    private readonly resolveConflict: AggregateConflictResolver<TState> = mergeSerializableAggregateState,
    private readonly maximumConflictRetries = 2,
  ) {}

  public async load(familyId: FamilyId): Promise<VersionedAggregateSnapshot<TAggregateType, TSchemaVersion, TState> | null> {
    const snapshot = await this.repository.load(familyId);
    const saveState = this.saveStateFor(familyId);
    saveState.revision = snapshot?.revision ?? 0;
    saveState.baseState = snapshot ? structuredClone(snapshot.state) : null;
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
      let draft = saveState.pendingDraft;
      saveState.pendingDraft = null;
      let conflictAttempts = 0;
      while (true) {
        const command: SaveVersionedAggregateCommand<TAggregateType, TState> = {
          ...draft,
          aggregateType: this.aggregateType,
          expectedRevision: saveState.revision,
        };
        try {
          const saved = await this.repository.save(command);
          saveState.revision = saved.revision;
          saveState.baseState = structuredClone(saved.state);
          break;
        } catch (error) {
          if (error instanceof AggregateConflictError && conflictAttempts < this.maximumConflictRetries) {
            try {
              const remote = await this.repository.load(familyId);
              conflictAttempts += 1;
              saveState.revision = remote?.revision ?? 0;
              if (remote && saveState.baseState) {
                draft = {
                  ...draft,
                  state: this.resolveConflict(saveState.baseState, draft.state, remote.state),
                };
              }
              saveState.baseState = remote ? structuredClone(remote.state) : null;
            } catch (reloadError) {
              saveState.pendingDraft ??= draft;
              throw reloadError;
            }
            continue;
          }
          saveState.pendingDraft ??= draft;
          throw error;
        }
      }
    }
  }

  private saveStateFor(familyId: FamilyId): AggregateSaveState<TState> {
    const current = this.saveStates.get(familyId);
    if (current) {
      return current;
    }
    const created: AggregateSaveState<TState> = {
      baseState: null,
      pendingDraft: null,
      revision: 0,
      saveQueue: Promise.resolve(),
      saveTimer: null,
    };
    this.saveStates.set(familyId, created);
    return created;
  }
}
