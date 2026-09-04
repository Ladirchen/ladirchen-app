import { HomeCustomizationService } from '@/application/services/home-customization-service';
import { localFamilyContext } from '@/infrastructure/context/local-family-context';
import { LocalStorageHomeCustomizationRepository } from '@/infrastructure/persistence/local-storage-home-customization-repository';

export const familyContext = localFamilyContext;

export const homeCustomizationService = new HomeCustomizationService(
  new LocalStorageHomeCustomizationRepository(),
);
