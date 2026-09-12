import type { HouseAccessoryState, HouseEditionState, HomeCustomizationSnapshot, SaveHomeCustomizationCommand } from '@/application/contracts/home-customization-contract';
import type { HomeCustomizationRepository } from '@/application/ports/home-customization-repository';
import type { HouseThemeId } from '@/domain/house';
import type { FamilyId, FamilyMemberId, HouseLayoutPlacement } from '@/domain/types';

export interface HomeCustomizationDraft {
  readonly familyId: FamilyId;
  readonly updatedBy: FamilyMemberId;
  readonly accessories: ReadonlyArray<HouseAccessoryState>;
  readonly editions: ReadonlyArray<HouseEditionState>;
  readonly placements: ReadonlyArray<HouseLayoutPlacement>;
  readonly selectedEditionId: HouseThemeId;
}

export class HomeCustomizationService {
  private pendingDraft: HomeCustomizationDraft | null = null;
  private revision = 0;
  private saveTimer: ReturnType<typeof setTimeout> | null = null;
  private saveQueue: Promise<void> = Promise.resolve();

  public constructor(private readonly repository: HomeCustomizationRepository) {}

  public async load(familyId: FamilyId): Promise<HomeCustomizationSnapshot | null> {
    const snapshot = await this.repository.load(familyId);
    this.revision = snapshot?.revision ?? 0;
    return snapshot;
  }

  public scheduleSave(draft: HomeCustomizationDraft, onError: (error: unknown) => void): void {
    this.pendingDraft = draft;
    if (this.saveTimer !== null) {
      clearTimeout(this.saveTimer);
    }
    this.saveTimer = setTimeout(() => {
      this.saveTimer = null;
      this.enqueuePendingSave(onError);
    }, 180);
  }

  private enqueuePendingSave(onError: (error: unknown) => void): void {
    this.saveQueue = this.saveQueue
      .then(() => this.flushPendingSave())
      .catch(onError);
  }

  private async flushPendingSave(): Promise<void> {
    while (this.pendingDraft !== null) {
      const draft = this.pendingDraft;
      this.pendingDraft = null;
      const command: SaveHomeCustomizationCommand = {
        ...draft,
        expectedRevision: this.revision,
      };
      try {
        const saved = await this.repository.save(command);
        this.revision = saved.revision;
      } catch (error) {
        this.pendingDraft ??= draft;
        throw error;
      }
    }
  }
}
