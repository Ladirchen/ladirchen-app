<template>
  <v-dialog :model-value="modelValue" max-width="500" scrollable @update:model-value="emit('update:modelValue', $event)">
    <v-card class="avatar-builder" rounded="xl">
      <header class="studio-header px-5 pt-4">
        <div><p class="eyebrow mb-0">{{ t('avatar.builder.eyebrow') }}</p><h2>{{ t('avatar.builder.title', { name: userName }) }}</h2></div>
        <v-btn :aria-label="t('avatar.builder.close')" icon="i-mdi:close" size="small" variant="text" @click="close" />
      </header>

      <section class="studio-preview mx-5 mt-3" :aria-label="t('avatar.builder.previewAria')">
        <div class="preview-decoration preview-star-one" /><div class="preview-decoration preview-star-two" />
        <AvatarFigure :appearance="draft" :size="190" />
        <div class="preview-tools">
          <strong>{{ userName }}</strong><span>{{ t('avatar.builder.previewHint') }}</span>
          <div class="random-actions"><button type="button" @click="randomLook(false)">{{ t('avatar.builder.random') }}</button><button class="fun-random" type="button" @click="randomLook(true)">{{ t('avatar.builder.funRandom') }}</button></div>
        </div>
      </section>

      <nav class="category-rail mt-3" :aria-label="t('avatar.builder.partsAria')">
        <button v-for="category in categories" :key="category.value" :aria-current="section === category.value ? 'page' : undefined" :class="{ active: section === category.value }" type="button" @click="selectSection(category.value)">
          <AvatarCategoryIcon :name="category.value" /><span>{{ category.label }}</span>
        </button>
      </nav>

      <v-card-text ref="optionsPanel" class="builder-options px-5 pt-4">
        <div class="section-intro"><div><p>{{ activeCategory.kicker }}</p><h3>{{ activeCategory.title }}</h3></div><span>{{ activeCategory.hint }}</span></div>

        <div v-if="section === 'base'" class="skin-studio mt-4">
          <button v-for="tone in skinToneOptions" :key="tone.id" :aria-label="t(tone.id)" :aria-pressed="draft.skinToneId === tone.value" :class="{ active: draft.skinToneId === tone.value }" class="skin-choice" type="button" @click="draft.skinToneId = tone.value">
            <span :style="{ '--swatch-color': tone.color }" /><strong>{{ t(tone.id) }}</strong>
          </button>
        </div>
        <div v-if="section === 'base' && profileRole === 'guardian'" class="guardian-presets mt-5">
          <p class="mini-section-label">{{ t('avatar.builder.profileType') }}</p>
          <div class="guardian-preset-grid mt-3">
            <button v-for="preset in guardianPresets" :key="preset.value" :aria-pressed="activeGuardianPreset === preset.value" :class="{ active: activeGuardianPreset === preset.value }" type="button" @click="selectGuardianPreset(preset.value)">
              <AvatarFigure :appearance="guardianPresetAppearance(preset.value)" :size="68" />
              <strong>{{ preset.label }}</strong>
              <span>{{ preset.description }}</span>
            </button>
          </div>
        </div>

        <template v-else-if="section === 'face'">
          <p class="mini-section-label mt-4">{{ t('avatar.builder.faceShape') }}</p>
          <OptionGrid v-model="draft.faceShape" :options="faceShapeOptions" preview-kind="faceShape" />
          <p class="mini-section-label mt-5">{{ t('avatar.builder.expression') }}</p>
          <OptionGrid v-model="draft.face" :options="faceOptions" preview-kind="face" />
        </template>
        <template v-else-if="section === 'hair'">
          <OptionGrid v-model="draft.hair" :options="visibleHairOptions" preview-kind="hair" /><ColorPicker v-model="draft.hairColorId" :options="visibleHairColorOptions" :label="t('avatar.builder.hairColor')" />
        </template>
        <template v-else-if="section === 'outfit'">
          <OptionGrid v-model="draft.outfit" :options="visibleOutfitOptions" preview-kind="outfit" /><ColorPicker v-model="draft.outfitColorId" :options="outfitColorOptions" :label="t('avatar.builder.outfitColor')" />
        </template>
        <OptionGrid v-else-if="section === 'extras'" v-model="draft.accessoryId" :options="accessoryOptions" preview-kind="accessory" />
        <OptionGrid v-else-if="section === 'fun'" v-model="draft.funAccessoryId" :options="funOptions" preview-kind="fun" />
        <OptionGrid v-else v-model="draft.seasonalAccessoryId" :options="seasonOptions" preview-kind="season" />
      </v-card-text>

      <v-card-actions class="builder-actions px-5 py-3">
        <v-btn class="studio-save-button" color="primary" rounded="lg" variant="flat" @click="save">
          <v-icon class="studio-save-icon" icon="i-mdi:check-circle-outline" />
          <span>{{ t('avatar.builder.save') }}</span>
          <v-icon class="studio-save-arrow" icon="i-mdi:arrow-right" />
          <i class="studio-save-shine" aria-hidden="true" />
        </v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script lang="ts" setup>
import { computed, defineComponent, h, nextTick, reactive, ref, watch } from 'vue';
import { useI18n } from 'vue-i18n';

import AvatarCategoryIcon from './AvatarCategoryIcon.vue';
import AvatarFigure from '@/shared/components/avatar/AvatarFigure.vue';
import { accessoryOptions, adultHairColorOptions, adultHairOptions, adultOutfitOptions, faceOptions, faceShapeOptions, funOptions, hairColorOptions, hairOptions, outfitColorOptions, outfitOptions, seasonOptions, skinToneOptions } from './data/avatar-options';
import { createDefaultAvatarAppearance, createGuardianAvatarAppearance } from '@/domain/avatar';
import type { AvatarAppearance, GuardianAvatarPreset } from '@/domain/avatar';
import type { ViewerRole } from '@/domain/family/types';
import type { AvatarCatalogItemId, AvatarColorOption } from './data/avatar-options';

type Section = 'base' | 'face' | 'hair' | 'outfit' | 'extras' | 'fun' | 'season';
type PreviewKind = 'face' | 'faceShape' | 'hair' | 'outfit' | 'accessory' | 'fun' | 'season';

const props = withDefaults(defineProps<{ modelValue: boolean; userName: string; initialAppearance?: AvatarAppearance; profileRole?: ViewerRole }>(), { profileRole: 'child' });
const emit = defineEmits<{ 'update:modelValue': [value: boolean]; save: [appearance: AvatarAppearance] }>();
const { t } = useI18n();
const section = ref<Section>('base');
const optionsPanel = ref<HTMLElement | { $el?: HTMLElement }>();
const draft = reactive<AvatarAppearance>(createDefaultAvatarAppearance());
const categories = computed<Array<{ value: Section; label: string; kicker: string; title: string; hint: string }>>(() => [
  { value: 'base', label: t('avatar.builder.categories.base.label'), kicker: t('avatar.builder.categories.base.kicker'), title: t(`avatar.builder.categories.base.${props.profileRole}Title`), hint: t('avatar.builder.categories.base.hint') },
  { value: 'face', label: t('avatar.builder.categories.face.label'), kicker: t('avatar.builder.categories.face.kicker'), title: t('avatar.builder.categories.face.title'), hint: t('avatar.builder.categories.face.hint') },
  { value: 'hair', label: t('avatar.builder.categories.hair.label'), kicker: t('avatar.builder.categories.hair.kicker'), title: t('avatar.builder.categories.hair.title'), hint: t('avatar.builder.categories.hair.hint') },
  { value: 'outfit', label: t(`avatar.builder.categories.outfit.${props.profileRole}Label`), kicker: t(`avatar.builder.categories.outfit.${props.profileRole}Kicker`), title: t(`avatar.builder.categories.outfit.${props.profileRole}Title`), hint: t(`avatar.builder.categories.outfit.${props.profileRole}Hint`) },
  { value: 'extras', label: t('avatar.builder.categories.extras.label'), kicker: t('avatar.builder.categories.extras.kicker'), title: t('avatar.builder.categories.extras.title'), hint: t('avatar.builder.categories.extras.hint') },
  { value: 'fun', label: t('avatar.builder.categories.fun.label'), kicker: t('avatar.builder.categories.fun.kicker'), title: t('avatar.builder.categories.fun.title'), hint: t('avatar.builder.categories.fun.hint') },
  { value: 'season', label: t('avatar.builder.categories.season.label'), kicker: t('avatar.builder.categories.season.kicker'), title: t('avatar.builder.categories.season.title'), hint: t('avatar.builder.categories.season.hint') },
]);
const activeCategory = computed(() => categories.value.find(category => category.value === section.value) ?? categories.value[0]!);
const baseAppearance = () => props.profileRole === 'guardian' ? createGuardianAvatarAppearance() : createDefaultAvatarAppearance();
const visibleHairOptions = computed(() => props.profileRole === 'guardian' ? adultHairOptions : hairOptions);
const visibleHairColorOptions = computed(() => props.profileRole === 'guardian' ? adultHairColorOptions : hairColorOptions);
const visibleOutfitOptions = computed(() => props.profileRole === 'guardian' ? adultOutfitOptions : outfitOptions);
const guardianPresets = computed<Array<{ value: GuardianAvatarPreset; label: string; description: string }>>(() => [
  { value: 'adult', label: t('avatar.builder.presets.adult.label'), description: t('avatar.builder.presets.adult.description') },
  { value: 'grandma', label: t('avatar.builder.presets.grandma.label'), description: t('avatar.builder.presets.grandma.description') },
  { value: 'grandpa', label: t('avatar.builder.presets.grandpa.label'), description: t('avatar.builder.presets.grandpa.description') },
]);
const activeGuardianPreset = computed<GuardianAvatarPreset>(() => draft.age !== 'senior' ? 'adult' : draft.outfit === 'cardigan' ? 'grandma' : 'grandpa');
const guardianPresetAppearance = (preset: GuardianAvatarPreset) => ({ ...createGuardianAvatarAppearance(preset), skinToneId: draft.skinToneId });
const selectGuardianPreset = (preset: GuardianAvatarPreset) => Object.assign(draft, guardianPresetAppearance(preset));
const previewField: Record<PreviewKind, keyof AvatarAppearance> = {
  accessory: 'accessoryId',
  face: 'face',
  faceShape: 'faceShape',
  fun: 'funAccessoryId',
  hair: 'hair',
  outfit: 'outfit',
  season: 'seasonalAccessoryId',
};
const previewAppearance = (kind: PreviewKind, value: string): AvatarAppearance => ({ ...baseAppearance(), age: draft.age, skinToneId: draft.skinToneId, hairColorId: draft.hairColorId, outfitColorId: draft.outfitColorId, [previewField[kind]]: value });

const OptionGrid = defineComponent({
  props: { modelValue: { type: String, required: true }, options: { type: Array as () => Array<{ id: AvatarCatalogItemId; value: string }>, required: true }, previewKind: { type: String as () => PreviewKind, required: true } },
  emits: ['update:modelValue'],
  setup: (gridProps, { emit: gridEmit }) => () => h(
    'div',
    { class: 'option-grid mt-4' },
    gridProps.options.map((option) => h(
      'button',
      {
        class: ['option-choice', { active: gridProps.modelValue === option.value }],
        type: 'button',
        'data-catalog-id': option.id,
        'data-preview-kind': gridProps.previewKind,
        'aria-label': t('avatar.builder.selectAria', { label: t(option.id) }),
        'aria-pressed': gridProps.modelValue === option.value,
        onClick: () => gridEmit('update:modelValue', option.value),
      },
      {
        default: () => [
          h('span', { class: 'option-art', 'aria-hidden': 'true' }, [h(AvatarFigure, { appearance: previewAppearance(gridProps.previewKind, option.value), size: 72 })]),
          h('strong', t(option.id)),
          gridProps.modelValue === option.value ? h('span', { class: 'option-check', 'aria-hidden': 'true' }, '✓') : null,
        ],
      },
    )),
  ),
});

const ColorPicker = defineComponent({
  props: { modelValue: { type: String, required: true }, options: { type: Array as () => AvatarColorOption<string>[], required: true }, label: { type: String, required: true } }, emits: ['update:modelValue'],
  setup: (colorProps, { emit: colorEmit }) => () => h('div', { class: 'compact-colors' }, [h('strong', colorProps.label), h('div', { class: 'color-row' }, colorProps.options.map(option => h('button', { class: ['color-choice', { active: colorProps.modelValue === option.value }], type: 'button', 'data-catalog-id': option.id, 'aria-label': t(option.id), 'aria-pressed': colorProps.modelValue === option.value, style: { background: option.color }, onClick: () => colorEmit('update:modelValue', option.value) }))) ]),
});

const randomItem = <T,>(items: T[]): T => {
  const item = items[Math.floor(Math.random() * items.length)];
  if (item === undefined) {throw new RangeError('Cannot select a random item from an empty list.');}
  return item;
};
const resetDraft = () => Object.assign(draft, baseAppearance(), props.initialAppearance ?? {});
const scrollOptionsToTop = () => void nextTick(() => { const panel = optionsPanel.value; const element = panel instanceof HTMLElement ? panel : panel?.$el; element?.scrollTo({ top: 0, behavior: 'smooth' }); });
const selectSection = (value: Section) => { section.value = value; scrollOptionsToTop(); };
const randomLook = (funny: boolean) => {
  const funnyParts = funOptions.filter(option => option.value !== 'none');
  Object.assign(draft, { skinToneId: randomItem(skinToneOptions).value, faceShape: randomItem(faceShapeOptions).value, face: randomItem(faceOptions).value, hair: randomItem(visibleHairOptions.value).value, hairColorId: randomItem(visibleHairColorOptions.value).value, outfit: randomItem(visibleOutfitOptions.value).value, outfitColorId: randomItem(outfitColorOptions).value, accessoryId: randomItem(accessoryOptions).value, funAccessoryId: funny ? randomItem(funnyParts).value : 'none', seasonalAccessoryId: funny && Math.random() > .55 ? randomItem(seasonOptions).value : 'none' });
};
const close = () => emit('update:modelValue', false);
const save = () => { emit('save', { ...draft }); close(); };
watch(() => props.modelValue, (isOpen) => { if (!isOpen) return; resetDraft(); section.value = 'base'; scrollOptionsToTop(); }, { immediate: true });
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.avatar-builder {
  height: min(660px, calc(100dvh - 28px));
  max-height: min(660px, calc(100dvh - 28px));
  @apply d-flex flex-column overflow-hidden;
  background: var(--lad-surface);
}
.studio-header {
  @apply d-flex flex-shrink-0 align-center justify-space-between;
}
.studio-header h2 {
  margin: 1px 0 0;
  font-size: rem(23);
  letter-spacing: -0.035em;
}
.studio-preview {
  min-height: 196px;
  @apply position-relative d-grid flex-shrink-0;
  grid-template-columns: 210px 1fr;
  @apply align-center overflow-hidden;
  border: 2px solid
    color-mix(in srgb, var(--lad-color-primary-muted) 15%, transparent);
  border-radius: 28px;
  background:
    radial-gradient(
      circle at 20% 18%,
      color-mix(in srgb, var(--lad-surface-raised) 95%, transparent) 0 42px,
      transparent 43px
    ),
    linear-gradient(
      145deg,
      var(--lad-surface-soft),
      var(--lad-color-reward-soft)
    );
  box-shadow:
    inset 0 -10px 0
      color-mix(in srgb, var(--lad-color-accent-warm-deep) 5%, transparent),
    0 5px 0
      color-mix(in srgb, var(--lad-color-primary-supporting) 8%, transparent);
}
.studio-preview::after {
  content: "";
  height: 38px;
  @apply position-absolute right-0 bottom-0 left-0;
  background: color-mix(
    in srgb,
    var(--lad-color-accent-warm-soft) 25%,
    transparent
  );
  clip-path: polygon(
    0 55%,
    25% 20%,
    53% 62%,
    78% 10%,
    100% 48%,
    100% 100%,
    0 100%
  );
}
.studio-preview :deep(.avatar-figure) {
  @apply position-relative;
  z-index: 2;
  @apply justify-self-center;
  filter: drop-shadow(
    0 9px 7px color-mix(in srgb, var(--lad-text-strong) 15%, transparent)
  );
}
.preview-tools {
  @apply position-relative;
  z-index: 2;
  padding-right: 18px;
  @apply d-flex flex-column align-start;
}
.preview-tools > strong {
  font-size: rem(22);
}
.preview-tools > span {
  max-width: 270px;
  margin-top: 2px;
  color: var(--lad-muted);
  font-size: 0.75rem;
  line-height: 1.35;
}
.random-actions {
  margin-top: 14px;
  @apply d-flex flex-wrap ga-2;
}
.random-actions button {
  min-height: 36px;
  padding: 0 13px;
  color: var(--lad-text-strong);
  border: 1px solid color-mix(in srgb, var(--lad-text-strong) 18%, transparent);
  border-radius: 12px;
  background: color-mix(in srgb, var(--lad-surface-raised) 80%, transparent);
  box-shadow: 0 3px 0
    color-mix(in srgb, var(--lad-text-strong) 10%, transparent);
  font: inherit;
  font-size: rem(11);
  @apply font-weight-black cursor-pointer;
}
.random-actions .fun-random {
  color: var(--lad-color-accent-pink-strong);
  background: var(--lad-surface-soft);
}
.preview-decoration {
  width: 12px;
  height: 12px;
  @apply position-absolute;
  z-index: 1;
  background: var(--lad-color-reward);
  clip-path: polygon(
    50% 0,
    61% 38%,
    100% 50%,
    61% 62%,
    50% 100%,
    39% 62%,
    0 50%,
    39% 38%
  );
  animation: studio-twinkle 4.8s ease-in-out infinite;
}
.preview-star-one {
  top: 30px;
  right: 34px;
}
.preview-star-two {
  bottom: 45px;
  left: 29px;
  animation-delay: -2.1s;
}
.category-rail {
  padding: 2px 16px 8px;
  @apply d-flex flex-shrink-0;
  gap: 5px;
  @apply overflow-x-auto;
  scrollbar-width: none;
  border-bottom: 1px solid var(--lad-border);
}
.category-rail::-webkit-scrollbar {
  @apply d-none;
}
.category-rail button {
  min-width: 78px;
  height: 68px;
  @apply position-relative;
  padding: 4px 6px 6px;
  @apply d-flex flex-column align-center justify-center;
  gap: 1px;
  color: var(--lad-muted);
  border: 0;
  border-radius: 18px;
  background: transparent;
  font: inherit;
  @apply cursor-pointer;
  transition:
    transform 150ms ease,
    color 150ms ease,
    background 150ms ease;
}
.category-rail button:hover {
  transform: translateY(-2px);
  background: var(--lad-surface-soft);
}
.category-rail button.active {
  color: var(--lad-color-primary-deep);
  background: linear-gradient(
    145deg,
    var(--lad-surface-soft),
    var(--lad-color-reward-soft)
  );
  box-shadow: inset 0 0 0 2px
    color-mix(in srgb, var(--lad-color-primary) 20%, transparent);
}
.category-rail button.active::after {
  content: "";
  width: 24px;
  height: 3px;
  @apply position-absolute;
  bottom: 3px;
  border-radius: var(--lad-radius-pill);
  background: var(--lad-mint);
}
.category-rail span {
  font-size: rem(10);
  @apply font-weight-black;
}
.builder-options {
  min-height: 0;
  flex: 1 1 auto;
  @apply overflow-y-auto;
  padding-bottom: 34px;
  scroll-padding-bottom: 34px;
}
.section-intro {
  @apply d-flex;
  align-items: end;
  @apply justify-space-between ga-4;
}
.section-intro p {
  margin: 0 0 1px;
  color: var(--lad-color-primary-strong);
  font-size: rem(9);
  font-weight: var(--lad-font-weight-black);
  letter-spacing: 0.11em;
  @apply text-uppercase;
}
.section-intro h3 {
  @apply ma-0;
  font-size: rem(18);
}
.section-intro > span {
  color: var(--lad-muted);
  font-size: rem(10);
  @apply text-right;
}
.mini-section-label {
  margin-bottom: -8px;
  color: var(--lad-text-strong);
  font-size: rem(10);
  font-weight: var(--lad-font-weight-black);
  letter-spacing: 0.06em;
  @apply text-uppercase;
}
.guardian-preset-grid {
  @apply d-grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 9px;
}
.guardian-preset-grid button {
  @apply min-w-0;
  padding: 10px 6px;
  @apply d-flex flex-column align-center;
  gap: 3px;
  color: var(--lad-text);
  border: 1px solid var(--lad-border);
  border-radius: 18px;
  background: var(--lad-surface);
  box-shadow: 0 3px 0 color-mix(in srgb, var(--lad-text-strong) 8%, transparent);
  font: inherit;
  @apply cursor-pointer;
}
.guardian-preset-grid button.active {
  border: 2px solid var(--lad-mint);
  background: linear-gradient(
    155deg,
    var(--lad-surface-soft),
    var(--lad-color-reward-soft)
  );
}
.guardian-preset-grid strong {
  font-size: rem(11);
}
.guardian-preset-grid span {
  color: var(--lad-muted);
  font-size: 0.5rem;
  line-height: 1.2;
  @apply text-center;
}
.skin-studio {
  @apply d-grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 10px;
}
.skin-choice {
  min-height: 126px;
  padding: 13px 6px 10px;
  @apply d-flex flex-column align-center justify-center;
  gap: 9px;
  color: var(--lad-text);
  border: 1px solid var(--lad-border);
  border-radius: 22px;
  background: linear-gradient(155deg, var(--lad-surface), var(--lad-surface));
  box-shadow: 0 4px 0 color-mix(in srgb, var(--lad-text-strong) 8%, transparent);
  font: inherit;
  @apply cursor-pointer;
}
.skin-choice > span {
  width: 68px;
  height: 72px;
  @apply position-relative;
  border: 3px solid var(--lad-border-on-accent);
  border-radius: 48% 48% 44% 44%;
  background: var(--swatch-color);
  box-shadow: 0 3px 0 color-mix(in srgb, var(--lad-text-warm) 15%, transparent);
}
.skin-choice > span::before {
  content: "";
  width: 20px;
  height: 10px;
  @apply position-absolute;
  top: 12px;
  left: 12px;
  border-radius: 50%;
  background: color-mix(in srgb, var(--lad-surface-raised) 30%, transparent);
  transform: rotate(-25deg);
}
.skin-choice strong {
  font-size: rem(11);
}
.skin-choice.active {
  border: 2px solid var(--lad-mint);
  background: linear-gradient(
    155deg,
    var(--lad-surface-soft),
    var(--lad-color-reward-soft)
  );
  box-shadow: 0 5px 0
    color-mix(in srgb, var(--lad-color-primary) 25%, transparent);
  transform: translateY(-2px);
}
:deep(.option-grid) {
  @apply d-grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 10px;
}
:deep(.option-choice) {
  min-height: 116px;
  @apply position-relative;
  padding: 6px 5px 9px;
  @apply d-flex flex-column align-center justify-center ga-1 overflow-hidden;
  color: var(--lad-text);
  border: 1px solid var(--lad-border);
  border-radius: 21px;
  background: linear-gradient(
    155deg,
    var(--lad-surface-raised),
    var(--lad-surface)
  );
  box-shadow: 0 4px 0 color-mix(in srgb, var(--lad-text-strong) 8%, transparent);
  font: inherit;
  @apply cursor-pointer;
}
:deep(.option-choice:nth-child(3n + 2)) {
  background: linear-gradient(
    155deg,
    var(--lad-surface-soft),
    var(--lad-surface)
  );
}
:deep(.option-choice:nth-child(3n + 3)) {
  background: linear-gradient(
    155deg,
    var(--lad-surface),
    var(--lad-surface-soft)
  );
}
:deep(.option-choice.active) {
  border: 2px solid var(--lad-mint);
  background: linear-gradient(
    155deg,
    var(--lad-surface-soft),
    var(--lad-color-reward-soft)
  );
  box-shadow: 0 5px 0
    color-mix(in srgb, var(--lad-color-primary) 25%, transparent);
}
:deep(.option-choice strong) {
  max-width: 100%;
  font-size: rem(10);
  line-height: 1.05;
  @apply text-center;
}
:deep(.option-art) {
  width: 78px;
  height: 78px;
  @apply d-grid place-center overflow-hidden;
  border: 2px solid
    color-mix(in srgb, var(--lad-border-on-accent) 85%, transparent);
  border-radius: 40% 40% 34% 34%;
  background: radial-gradient(
    circle at 50% 37%,
    var(--lad-surface) 0 42%,
    var(--lad-surface-soft) 43% 69%,
    var(--lad-color-reward-muted) 70%
  );
  filter: drop-shadow(
    0 4px 3px color-mix(in srgb, var(--lad-text) 12%, transparent)
  );
}
:deep(.option-art .avatar-figure) {
  border: 0;
  border-radius: 30%;
  background: transparent;
  box-shadow: none;
}
:deep(.option-art .avatar-figure *) {
  animation: none;
}
:deep(.option-choice[data-preview-kind="face"] .avatar-figure),
:deep(.option-choice[data-preview-kind="faceShape"] .avatar-figure),
:deep(.option-choice[data-preview-kind="accessory"] .avatar-figure),
:deep(.option-choice[data-preview-kind="fun"] .avatar-figure),
:deep(.option-choice[data-preview-kind="season"] .avatar-figure) {
  transform: translateY(11px) scale(1.28);
  transform-origin: center top;
}
:deep(.option-choice[data-preview-kind="hair"] .avatar-figure) {
  transform: translateY(4px) scale(1.12);
  transform-origin: center top;
}
:deep(.option-choice[data-preview-kind="outfit"] .avatar-figure) {
  transform: translateY(4px) scale(1.02);
  transform-origin: center top;
}
:deep(.option-check) {
  width: 20px;
  height: 20px;
  @apply position-absolute;
  top: 5px;
  right: 5px;
  @apply d-grid place-center;
  color: var(--lad-text-inverse);
  border: 2px solid var(--lad-border-on-accent);
  border-radius: 50%;
  background: var(--lad-mint);
  box-shadow: 0 2px 5px
    color-mix(in srgb, var(--lad-color-primary-deep) 20%, transparent);
  font-size: rem(11);
  font-weight: var(--lad-font-weight-black);
}
:deep(.compact-colors) {
  margin-top: 18px;
  padding: 12px 14px;
  @apply d-flex align-center justify-space-between ga-3;
  border: 1px solid var(--lad-border);
  border-radius: 18px;
  background: var(--lad-surface);
}
:deep(.compact-colors > strong) {
  font-size: rem(11);
}
:deep(.color-row) {
  @apply d-flex flex-wrap justify-end ga-2;
}
:deep(.color-choice) {
  width: 34px;
  height: 34px;
  @apply position-relative;
  border: 3px solid var(--lad-border-on-accent);
  border-radius: 43% 43% 48% 48%;
  box-shadow:
    0 2px 0 color-mix(in srgb, var(--lad-text-warm) 15%, transparent),
    0 0 0 1px var(--lad-border);
  @apply cursor-pointer;
}
:deep(.color-choice::before) {
  content: "";
  width: 8px;
  height: 5px;
  @apply position-absolute;
  top: 5px;
  left: 6px;
  border-radius: 50%;
  background: color-mix(in srgb, var(--lad-surface-raised) 35%, transparent);
  transform: rotate(-28deg);
}
:deep(.color-choice.active) {
  transform: translateY(-2px) scale(1.08);
  box-shadow:
    0 4px 0 color-mix(in srgb, var(--lad-color-primary) 20%, transparent),
    0 0 0 3px var(--lad-mint);
}
.builder-actions {
  @apply position-relative justify-center;
  z-index: 3;
  @apply flex-shrink-0;
  border-top: 1px solid var(--lad-border);
  background: var(--lad-surface);
  box-shadow: 0 -10px 22px color-mix(in srgb, var(--lad-text) 5%, transparent);
}
.studio-save-button {
  min-width: 250px;
  min-height: 50px;
  padding-inline: 18px;
  @apply position-relative overflow-hidden;
  border: 2px solid
    color-mix(in srgb, var(--lad-border-on-accent) 90%, transparent);
  border-radius: 17px;
  background: linear-gradient(
    135deg,
    var(--lad-color-info-subtle),
    var(--lad-color-primary-muted)
  );
  box-shadow:
    0 4px 0 color-mix(in srgb, var(--lad-color-primary-deep) 70%, transparent),
    0 9px 18px
      color-mix(in srgb, var(--lad-color-primary-deep) 15%, transparent);
  font-size: rem(14);
  font-weight: var(--lad-font-weight-heavy);
  text-transform: none;
  letter-spacing: 0;
}
.studio-save-button :deep(.v-btn__content) {
  gap: 10px;
}
.studio-save-icon {
  font-size: rem(23);
}
.studio-save-arrow {
  font-size: rem(19);
  transition: transform 0.18s ease;
}
.studio-save-button:hover .studio-save-arrow {
  transform: translateX(4px);
}
.studio-save-shine {
  width: 45px;
  height: 160%;
  @apply position-absolute pointer-events-none;
  top: -30%;
  left: -65px;
  transform: rotate(17deg);
  background: linear-gradient(
    90deg,
    transparent,
    color-mix(in srgb, var(--lad-surface-raised) 60%, transparent),
    transparent
  );
  animation: studio-save-shine 3.8s ease-in-out infinite;
}
@keyframes studio-twinkle {
  0%,
  100% {
    opacity: 0.45;
    transform: rotate(0) scale(0.75);
  }
  50% {
    opacity: 1;
    transform: rotate(90deg) scale(1.18);
  }
}
@keyframes studio-save-shine {
  0%,
  55% {
    left: -65px;
    opacity: 0;
  }
  67% {
    opacity: 0.8;
  }
  82%,
  100% {
    left: 115%;
    opacity: 0;
  }
}
@include respond-down(studio) {
  .studio-preview {
    min-height: 185px;
    grid-template-columns: 170px 1fr;
  }
  .studio-preview :deep(.avatar-figure) {
    width: 158px;
    height: 158px;
  }
  .preview-tools > strong {
    font-size: rem(18);
  }
  .category-rail button {
    min-width: 72px;
  }
  .skin-studio {
    grid-template-columns: repeat(2, 1fr);
  }
  :deep(.option-grid) {
    grid-template-columns: repeat(2, 1fr);
  }
}
@include respond-down(narrow) {
  .studio-preview {
    grid-template-columns: 1fr;
    @apply pa-2;
  }
  .studio-preview :deep(.avatar-figure) {
    width: 140px;
    height: 140px;
  }
  .preview-tools {
    @apply d-none;
  }
}
@include reduced-motion {
  .preview-decoration,
  .studio-save-shine {
    animation: none;
  }
}
</style>
