import type { Component } from 'vue';
import type { FurnitureVisualId } from '@/domain/house';
import RugVisual from './RugVisual.vue';
import SofaVisual from './SofaVisual.vue';
import BookshelfVisual from './BookshelfVisual.vue';
import LampVisual from './LampVisual.vue';
import BunkBedVisual from './BunkBedVisual.vue';
import TableVisual from './TableVisual.vue';
import PlantVisual from './PlantVisual.vue';
import WallArtVisual from './WallArtVisual.vue';
import CatTreeVisual from './CatTreeVisual.vue';
import PetBedVisual from './PetBedVisual.vue';
import DogBlanketVisual from './DogBlanketVisual.vue';
import FoodBowlVisual from './FoodBowlVisual.vue';
import KitchenCounterVisual from './KitchenCounterVisual.vue';
import RetroFridgeVisual from './RetroFridgeVisual.vue';
import DiningTableVisual from './DiningTableVisual.vue';
import DoubleBedVisual from './DoubleBedVisual.vue';
import WardrobeVisual from './WardrobeVisual.vue';
import BedsideTableVisual from './BedsideTableVisual.vue';
import ArtDeskVisual from './ArtDeskVisual.vue';
import StorageCabinetVisual from './StorageCabinetVisual.vue';
import PoolVisual from './PoolVisual.vue';
import GardenChairVisual from './GardenChairVisual.vue';
import GardenTableVisual from './GardenTableVisual.vue';
import TrampolineVisual from './TrampolineVisual.vue';
import SunshadeVisual from './SunshadeVisual.vue';
import FlowerBoxVisual from './FlowerBoxVisual.vue';
import StringLightsVisual from './StringLightsVisual.vue';
import HammockVisual from './HammockVisual.vue';
import TelescopeVisual from './TelescopeVisual.vue';
import PumpkinArchVisual from './PumpkinArchVisual.vue';
import BatGarlandVisual from './BatGarlandVisual.vue';
import CandyBushVisual from './CandyBushVisual.vue';
import CandyFenceVisual from './CandyFenceVisual.vue';
import FamilyFlagVisual from './FamilyFlagVisual.vue';

export const furnitureVisualComponents = {
  'rug': RugVisual,
  'sofa': SofaVisual,
  'bookshelf': BookshelfVisual,
  'lamp': LampVisual,
  'bunk-bed': BunkBedVisual,
  'table': TableVisual,
  'plant': PlantVisual,
  'wall-art': WallArtVisual,
  'cat-tree': CatTreeVisual,
  'pet-bed': PetBedVisual,
  'dog-blanket': DogBlanketVisual,
  'food-bowl': FoodBowlVisual,
  'kitchen-counter': KitchenCounterVisual,
  'retro-fridge': RetroFridgeVisual,
  'dining-table': DiningTableVisual,
  'double-bed': DoubleBedVisual,
  'wardrobe': WardrobeVisual,
  'bedside-table': BedsideTableVisual,
  'art-desk': ArtDeskVisual,
  'storage-cabinet': StorageCabinetVisual,
  'pool': PoolVisual,
  'garden-chair': GardenChairVisual,
  'garden-table': GardenTableVisual,
  'trampoline': TrampolineVisual,
  'sunshade': SunshadeVisual,
  'flower-box': FlowerBoxVisual,
  'string-lights': StringLightsVisual,
  'hammock': HammockVisual,
  'telescope': TelescopeVisual,
  'pumpkin-arch': PumpkinArchVisual,
  'bat-garland': BatGarlandVisual,
  'candy-bush': CandyBushVisual,
  'candy-fence': CandyFenceVisual,
  'family-flag': FamilyFlagVisual,
} satisfies Record<FurnitureVisualId, Component>;
