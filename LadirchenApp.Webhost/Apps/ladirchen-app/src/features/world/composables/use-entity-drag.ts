import { onUnmounted, ref } from 'vue';

import type { HouseLayoutPlacementId } from '@/domain/shared/identifiers';

export interface EntityDragState {
  captureTarget: HTMLElement;
  lastX: number;
  lastY: number;
  offsetX: number;
  offsetY: number;
  placementId: HouseLayoutPlacementId;
  pointerId: number;
  startX: number;
  startY: number;
}

interface EntityDragOptions {
  commit: (clientX: number, clientY: number) => void;
  reset: (placementId: HouseLayoutPlacementId) => void;
}

export const useEntityDrag = ({ commit, reset }: EntityDragOptions) => {
  const drag = ref<EntityDragState | null>(null);

  const track = (event: PointerEvent) => {
    if (!drag.value || event.pointerId !== drag.value.pointerId) {return;}
    event.preventDefault();
    drag.value.lastX = event.clientX;
    drag.value.lastY = event.clientY;
    drag.value.offsetX = event.clientX - drag.value.startX;
    drag.value.offsetY = event.clientY - drag.value.startY;
  };

  const stopTracking = (activeDrag = drag.value) => {
    window.removeEventListener('pointermove', track);
    window.removeEventListener('pointerup', finish);
    window.removeEventListener('pointercancel', cancel);
    window.removeEventListener('blur', cancel);
    if (activeDrag?.captureTarget.hasPointerCapture(activeDrag.pointerId)) {
      activeDrag.captureTarget.releasePointerCapture(activeDrag.pointerId);
    }
  };

  const clear = () => {
    const activeDrag = drag.value;
    drag.value = null;
    stopTracking(activeDrag);
    return activeDrag;
  };

  const finish = (event: PointerEvent) => {
    if (!drag.value || event.pointerId !== drag.value.pointerId) {return;}
    event.preventDefault();
    commit(event.clientX, event.clientY);
  };

  const finishAtLastPosition = (event: PointerEvent) => {
    if (!drag.value || event.pointerId !== drag.value.pointerId) {return;}
    commit(drag.value.lastX, drag.value.lastY);
  };

  const cancel = () => {
    const activeDrag = clear();
    if (activeDrag) {reset(activeDrag.placementId);}
  };

  const start = (event: PointerEvent, placementId: HouseLayoutPlacementId) => {
    const target = event.currentTarget;
    if (!(target instanceof HTMLElement)) {return false;}
    event.preventDefault();
    event.stopPropagation();
    stopTracking();
    drag.value = {
      captureTarget: target,
      lastX: event.clientX,
      lastY: event.clientY,
      offsetX: 0,
      offsetY: 0,
      placementId,
      pointerId: event.pointerId,
      startX: event.clientX,
      startY: event.clientY,
    };
    try {
      target.setPointerCapture(event.pointerId);
    } catch {
      // Window listeners keep mouse and touch drags reliable without capture support.
    }
    window.addEventListener('pointermove', track);
    window.addEventListener('pointerup', finish);
    window.addEventListener('pointercancel', cancel);
    window.addEventListener('blur', cancel);
    return true;
  };

  onUnmounted(stopTracking);

  return { cancel, clear, drag, finish, finishAtLastPosition, start, track };
};
