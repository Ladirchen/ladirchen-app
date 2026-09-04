import type { HomeCustomizationSnapshot, SaveHomeCustomizationCommand } from '@/application/contracts/home-customization-contract';
import type { FamilyId } from '@/domain/types';

export class HomeCustomizationConflictError extends Error {
  public constructor() {
    super('The home customization was changed by another session.');
    this.name = 'HomeCustomizationConflictError';
  }
}

export interface HomeCustomizationRepository {
  load: (familyId: FamilyId) => Promise<HomeCustomizationSnapshot | null>;
  save: (command: SaveHomeCustomizationCommand) => Promise<HomeCustomizationSnapshot>;
}
