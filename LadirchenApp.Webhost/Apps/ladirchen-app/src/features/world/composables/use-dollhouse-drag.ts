import { computed } from 'vue';
import type { ComputedRef } from 'vue';

import { isHouseZoneId } from '@/application/contracts/family-aggregate-validation';
import { characterCollidesWithFurniture, furnitureVisualDefinitionFor } from '@/domain/house';
import type { HouseAccessory, HouseLayoutPlacement, HouseZoneId } from '@/domain/house';
import type { HouseLayoutPlacementId } from '@/domain/shared/identifiers';
import { ENTITY_PLACEMENT_CONFIG } from '../entity-visual-config';
import { useEntityDrag } from './use-entity-drag';

interface DollhouseDragProps {
  accessories: HouseAccessory[];
  editable: boolean;
  placements: HouseLayoutPlacement[];
  storageOpen: boolean;
}

interface DollhouseDragEmit {
  (event: 'drag-state', entityType: HouseLayoutPlacement['entityType'] | null): void;
  (event: 'move', placementId: HouseLayoutPlacementId, zoneId: HouseZoneId, x: number, y: number): void;
  (event: 'reset', placementId: HouseLayoutPlacementId): void;
  (event: 'store', accessoryId: HouseAccessory['id']): void;
}

const isLadiPerchDrop = (
  placement: HouseLayoutPlacement | undefined,
  zoneId: HouseZoneId,
  x: number,
  y: number,
): boolean => placement?.entityType === 'ladi' && zoneId === 'living-room' &&
  x >= ENTITY_PLACEMENT_CONFIG.perch.minimumX && x <= ENTITY_PLACEMENT_CONFIG.perch.maximumX &&
  y >= ENTITY_PLACEMENT_CONFIG.perch.minimumY && y <= ENTITY_PLACEMENT_CONFIG.perch.maximumY;

export const useDollhouseDrag = (
  props: DollhouseDragProps,
  emit: DollhouseDragEmit,
  unlockedZones: ComputedRef<HouseZoneId[]>,
  accessoryFor: (placement: HouseLayoutPlacement) => HouseAccessory | undefined,
) => {
  const finishStorageDrop = (
    activeDrag: { placementId: HouseLayoutPlacementId },
    placement: HouseLayoutPlacement | undefined,
    elementsAtDropPoint: Element[],
  ): boolean => {
    if (!elementsAtDropPoint.some(element => element.closest('[data-furniture-storage]'))) {return false;}
    try {
      if (placement?.entityType === 'furniture') {emit('store', placement.entityId);}
      else {emit('reset', activeDrag.placementId);}
    } finally {
      clearDrag();
      emit('drag-state', null);
    }
    return true;
  };
  const commitDrag = (clientX: number, clientY: number) => {
    const activeDrag = drag.value;
    if (!activeDrag) {return;}
    const elementsAtDropPoint = document.elementsFromPoint(clientX, clientY);
    const placement = props.placements.find(item => item.id === activeDrag.placementId);
    if (finishStorageDrop(activeDrag, placement, elementsAtDropPoint)) {return;}
    const zone = elementsAtDropPoint
      .map(element => element.closest<HTMLElement>('[data-zone-id]'))
      .find(element => isHouseZoneId(element?.dataset.zoneId) && unlockedZones.value.includes(element.dataset.zoneId));
    try {
      const zoneId = zone?.dataset.zoneId;
      if (!zone || !isHouseZoneId(zoneId)) {
        emit('reset', activeDrag.placementId);
        return;
      }
      const bounds = zone.getBoundingClientRect();
      const isFloorEntity = placement?.entityType === 'member' || placement?.entityType === 'pet' || placement?.entityType === 'ladi';
      const accessory = placement ? accessoryFor(placement) : undefined;
      const visualDefinition = accessory?.visual ? furnitureVisualDefinitionFor(accessory.visual) : undefined;
      const rawX = ((clientX - bounds.left) / bounds.width) * 100;
      const rawY = ((clientY - bounds.top) / bounds.height) * 100;
      const snapsToLadiPerch = isLadiPerchDrop(placement, zoneId, rawX, rawY);
      const horizontalInset = isFloorEntity ? ENTITY_PLACEMENT_CONFIG.floorHorizontalInset : ENTITY_PLACEMENT_CONFIG.furnitureHorizontalInset;
      const minimumY = isFloorEntity && !snapsToLadiPerch ? ENTITY_PLACEMENT_CONFIG.floorMinimumY : visualDefinition?.minimumY ?? ENTITY_PLACEMENT_CONFIG.furnitureMinimumY;
      const maximumY = visualDefinition?.maximumY ?? ENTITY_PLACEMENT_CONFIG.maximumY;
      const x = snapsToLadiPerch ? ENTITY_PLACEMENT_CONFIG.perch.x : Math.min(100 - horizontalInset, Math.max(horizontalInset, rawX));
      const y = snapsToLadiPerch ? ENTITY_PLACEMENT_CONFIG.perch.y : Math.min(maximumY, Math.max(minimumY, rawY));
      const collidesWithFurniture = isFloorEntity && !snapsToLadiPerch && placement
        ? characterCollidesWithFurniture(placement.id, zoneId, x, y, props.placements, props.accessories)
        : false;
      if (!collidesWithFurniture) {emit('move', activeDrag.placementId, zoneId, x, y);}
    } finally {
      clearDrag();
      emit('drag-state', null);
    }
  };
  const {
    cancel: cancelDrag, clear: clearDrag, drag, finish: finishDrag,
    finishAtLastPosition: finishDragAtLastPosition, start: startEntityDrag, track: trackDrag,
  } = useEntityDrag({
    commit: commitDrag,
    reset: (placementId) => { emit('drag-state', null); emit('reset', placementId); },
  });
  const draggingFurniture = computed(() => {
    const placement = props.placements.find(item => item.id === drag.value?.placementId);
    return placement?.entityType === 'furniture';
  });
  const dragOffset = (placementId: HouseLayoutPlacementId) => drag.value?.placementId === placementId
    ? { x: drag.value.offsetX, y: drag.value.offsetY }
    : undefined;
  const startDrag = (event: PointerEvent, placement: HouseLayoutPlacement) => {
    if (!props.editable && !(props.storageOpen && placement.entityType === 'furniture')) {return;}
    if (placement.entityType === 'furniture' && accessoryFor(placement)?.mobility === 'fixed') {return;}
    if (startEntityDrag(event, placement.id)) {emit('drag-state', placement.entityType);}
  };

  return { cancelDrag, dragOffset, draggingFurniture, finishDrag, finishDragAtLastPosition, startDrag, trackDrag };
};
