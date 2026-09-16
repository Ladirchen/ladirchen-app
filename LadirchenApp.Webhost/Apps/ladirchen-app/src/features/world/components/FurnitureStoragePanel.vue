<template>
  <button
    v-if="!open"
    class="furniture-storage-trigger"
    :aria-expanded="open"
    :aria-label="t('world.scene.storage.open')"
    type="button"
    @click.stop="emit('update:open', true)"
  >
    <v-icon icon="i-mdi:sofa-outline" size="21" />
    <v-icon class="storage-plus" icon="i-mdi:plus" size="12" />
  </button>
  <Transition name="storage-tray">
    <aside v-if="open" class="furniture-storage" :class="{ dragging }" data-furniture-storage :aria-label="t('world.scene.storage.title')">
      <header>
        <div class="storage-title">
          <span class="storage-title-icon"><v-icon icon="i-mdi:archive-star-outline" size="19" /></span>
          <span><strong>{{ t('world.scene.storage.title') }}</strong><small>{{ t('world.scene.storage.count', { count: accessories.length }) }}</small></span>
        </div>
        <button :aria-label="t('world.scene.storage.close')" type="button" @click="emit('update:open', false)"><v-icon icon="i-mdi:close" size="18" /></button>
      </header>
      <div class="storage-items">
        <p v-if="accessories.length === 0" class="storage-empty"><span><v-icon icon="i-mdi:inbox-arrow-down-outline" size="25" /></span><strong>{{ t('world.scene.storage.emptyTitle') }}</strong><small>{{ t('world.scene.storage.emptyDescription') }}</small></p>
        <button
          v-for="accessory in accessories"
          :key="accessory.id"
          :aria-label="t('world.scene.storage.placeAria', { title: accessory.title })"
          class="stored"
          type="button"
          @click="emit('place', accessory)"
        >
          <span class="stored-preview"><RoomFurniture :item="accessory" /></span>
          <strong>{{ accessory.title }}</strong>
          <span class="stored-action"><v-icon icon="i-mdi:plus" size="10" />{{ t('world.scene.storage.place') }}</span>
        </button>
        <p v-if="accessories.length > 0" class="storage-drop-hint"><v-icon icon="i-mdi:inbox-arrow-down-outline" size="15" />{{ t('world.scene.storage.dropMore') }}</p>
      </div>
    </aside>
  </Transition>
</template>

<script lang="ts" setup>
import { useI18n } from 'vue-i18n';

import type { HouseAccessory } from '@/domain/house';
import RoomFurniture from '@/shared/components/house/RoomFurniture.vue';

defineProps<{ accessories: ReadonlyArray<HouseAccessory>; dragging: boolean; open: boolean }>();
const emit = defineEmits<{ place: [accessory: HouseAccessory]; 'update:open': [open: boolean] }>();
const { t } = useI18n();
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;

.furniture-storage-trigger {
  width: 43px;
  height: 43px;
  @apply position-absolute d-grid place-center cursor-pointer;
  top: 8px;
  right: 8px;
  z-index: 25;
  color: var(--lad-palette-mint-strong);
  border: 3px solid var(--lad-palette-white);
  border-radius: 14px;
  background: var(--lad-palette-background);
  box-shadow:
    0 4px 0 var(--lad-palette-muted-250),
    0 8px 16px color-mix(in srgb, var(--lad-palette-muted-700) 18%, transparent);
}
.storage-plus {
  width: 17px;
  height: 17px;
  @apply position-absolute d-grid place-center;
  right: -5px;
  bottom: -5px;
  color: var(--lad-palette-white);
  border: 2px solid var(--lad-palette-white);
  border-radius: 50%;
  background: var(--lad-palette-mint);
}
.furniture-storage {
  width: 126px;
  @apply position-absolute overflow-hidden;
  top: 0;
  right: 0;
  bottom: 2%;
  z-index: 24;
  padding: 8px 7px 10px;
  border: 2px solid var(--lad-palette-muted-250);
  border-radius: 22px 0 0 22px;
  background:
    radial-gradient(
      circle at 92% 8%,
      color-mix(in srgb, var(--lad-palette-yellow) 40%, transparent) 0 25px,
      transparent 26px
    ),
    radial-gradient(
      circle at 8% 82%,
      color-mix(in srgb, var(--lad-palette-blue-350) 25%, transparent) 0 35px,
      transparent 36px
    ),
    linear-gradient(
      165deg,
      var(--lad-palette-background),
      var(--lad-palette-amber-100) 52%,
      var(--lad-palette-background)
    );
  box-shadow:
    -7px 0 0 color-mix(in srgb, var(--lad-palette-teal-400) 30%, transparent),
    -13px 0 25px
      color-mix(in srgb, var(--lad-palette-muted-750-2) 15%, transparent);
  backdrop-filter: blur(12px);
}
.furniture-storage::after {
  content: "";
  width: 56px;
  height: 56px;
  @apply position-absolute;
  right: -34px;
  bottom: -26px;
  z-index: -1;
  border: 9px solid color-mix(in srgb, var(--lad-palette-mint) 10%, transparent);
  border-radius: 50%;
}
.furniture-storage > header {
  @apply d-flex align-start justify-space-between;
  gap: 4px;
  margin: -3px -2px 8px;
  padding: 6px 5px 8px;
  border-bottom: 1px dashed
    color-mix(in srgb, var(--lad-palette-teal-600) 25%, transparent);
}
.storage-title {
  min-width: 0;
  @apply d-flex align-center;
  gap: 5px;
}
.storage-title > span:last-child {
  min-width: 0;
  @apply d-flex flex-column;
  color: var(--lad-palette-muted-700);
  line-height: 1.08;
}
.storage-title strong {
  font-size: rem(10);
}
.storage-title small {
  margin-top: 2px;
  @apply overflow-hidden;
  color: var(--lad-palette-muted);
  font-size: rem(6);
  text-overflow: ellipsis;
  white-space: nowrap;
}
.storage-title-icon {
  width: 29px;
  height: 29px;
  @apply d-grid place-center flex-shrink-0;
  color: var(--lad-palette-white);
  border: 2px solid var(--lad-palette-white);
  border-radius: 10px 10px 8px 8px;
  background: linear-gradient(
    145deg,
    var(--lad-palette-teal-400),
    var(--lad-palette-teal-550)
  );
  box-shadow: 0 3px 0 var(--lad-palette-teal-700);
  transform: rotate(-4deg);
}
.furniture-storage > header button {
  width: 24px;
  height: 24px;
  @apply d-grid place-center flex-shrink-0 cursor-pointer;
  border: 2px solid var(--lad-palette-white);
  border-radius: 50%;
  background: var(--lad-palette-amber-100);
  color: var(--lad-palette-amber-650);
  box-shadow: 0 3px 0 var(--lad-palette-amber-200);
}
.storage-items {
  @apply d-grid overflow-y-auto;
  grid-template-columns: 1fr;
  gap: 7px;
  max-height: calc(100% - 47px);
  padding: 2px 3px 6px 1px;
  scrollbar-width: thin;
}
.storage-empty {
  min-height: 128px;
  @apply position-relative d-flex flex-column align-center justify-center overflow-hidden text-center;
  gap: 4px;
  @apply ma-0;
  padding: 8px;
  color: var(--lad-palette-teal-600);
  border: 2px dashed var(--lad-palette-teal-400);
  border-radius: 17px;
  background:
    radial-gradient(
      circle at 82% 16%,
      color-mix(in srgb, var(--lad-palette-yellow) 40%, transparent),
      transparent 25%
    ),
    linear-gradient(
      145deg,
      color-mix(in srgb, var(--lad-palette-background) 95%, transparent),
      color-mix(in srgb, var(--lad-palette-background) 90%, transparent)
    );
  box-shadow: inset 0 0 0 4px
    color-mix(in srgb, var(--lad-palette-white) 40%, transparent);
  font-weight: 750;
}
.storage-empty::after {
  content: "✦";
  @apply position-absolute;
  top: 8px;
  right: 10px;
  color: var(--lad-palette-amber-500);
  font-size: rem(9);
}
.storage-empty > span {
  width: 43px;
  height: 43px;
  @apply d-grid place-center;
  margin-bottom: 3px;
  color: var(--lad-palette-white);
  border: 3px solid var(--lad-palette-white);
  border-radius: 15px;
  background: linear-gradient(
    145deg,
    var(--lad-palette-teal-400),
    var(--lad-palette-blue)
  );
  box-shadow: 0 4px 0 var(--lad-palette-blue-550);
}
.storage-empty strong {
  font-size: 0.5rem;
}
.storage-empty small {
  max-width: 82px;
  font-size: rem(6);
  line-height: 1.3;
}
.storage-items > button.stored {
  min-height: 112px;
  @apply position-relative d-flex flex-column align-center justify-center cursor-pointer;
  gap: 2px;
  padding: 7px 4px 6px;
  color: var(--lad-palette-muted-700);
  border: 1px solid var(--lad-palette-teal-150);
  border-radius: 17px;
  background: linear-gradient(
    155deg,
    var(--lad-palette-white),
    var(--lad-palette-background)
  );
  box-shadow:
    0 4px 0 var(--lad-palette-teal-150),
    0 8px 12px color-mix(in srgb, var(--lad-palette-muted-700) 8%, transparent);
  transition:
    transform 0.16s ease,
    box-shadow 0.16s ease;
}
.storage-items > button.stored:hover {
  transform: translateY(-2px);
  box-shadow:
    0 6px 0 var(--lad-palette-teal-150),
    0 10px 14px
      color-mix(in srgb, var(--lad-palette-muted-700) 10%, transparent);
}
.storage-items > button.stored > strong {
  max-width: 92px;
  @apply overflow-hidden;
  font-size: rem(7);
  line-height: 1.15;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.stored-preview {
  width: 62px;
  height: 57px;
  @apply d-grid place-center;
  margin-bottom: 1px;
  border-radius: 18px;
  background:
    radial-gradient(circle, var(--lad-palette-white) 0 48%, transparent 49%),
    linear-gradient(
      145deg,
      var(--lad-palette-background),
      var(--lad-palette-amber-100)
    );
}
.stored-preview :deep(.room-furniture) {
  width: 62px;
  height: 62px;
}
.stored-action {
  @apply d-inline-flex align-center;
  gap: 1px;
  margin-top: 3px;
  padding: 4px 7px;
  color: var(--lad-palette-white);
  border-radius: var(--lad-radius-pill);
  background: linear-gradient(
    145deg,
    var(--lad-palette-teal-400),
    var(--lad-palette-mint-strong)
  );
  box-shadow: 0 3px 0 var(--lad-palette-teal-700);
  font-size: rem(6);
  font-weight: var(--lad-font-weight-strong);
}
.storage-drop-hint {
  @apply d-flex align-center justify-center text-center;
  gap: 3px;
  margin: 1px 0 0;
  padding: 6px 4px;
  color: var(--lad-palette-teal-600);
  border: 1px dashed var(--lad-palette-muted-250);
  border-radius: 11px;
  background: color-mix(
    in srgb,
    var(--lad-palette-background) 70%,
    transparent
  );
  font-size: rem(6);
  font-weight: 800;
}
.storage-tray-enter-active,
.storage-tray-leave-active {
  transition:
    opacity 0.18s ease,
    transform 0.18s ease;
}
.storage-tray-enter-from,
.storage-tray-leave-to {
  opacity: 0;
  transform: translateX(12px) scale(0.97);
}
.furniture-storage.dragging {
  border-color: var(--lad-palette-mint);
  background: var(--lad-palette-surface);
  box-shadow:
    inset 0 0 0 3px color-mix(in srgb, var(--lad-palette-mint) 12%, transparent),
    -7px 0 18px
      color-mix(in srgb, var(--lad-palette-muted-750-2) 15%, transparent);
}
.furniture-storage.dragging .storage-empty {
  color: var(--lad-palette-mint-strong);
  border-color: var(--lad-palette-mint);
  background: color-mix(in srgb, var(--lad-palette-mint) 8%, transparent);
}
</style>
