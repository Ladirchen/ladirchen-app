<template>
  <v-dialog :model-value="modelValue" max-width="680" scrollable @update:model-value="emit('update:modelValue', $event)">
    <v-card class="avatar-builder" rounded="xl">
      <header class="studio-header px-5 pt-4">
        <div><p class="eyebrow mb-0">Mein Profil</p><h2>{{ userName }}s Figurenstudio</h2></div>
        <v-btn aria-label="Figurenstudio schließen" icon="mdi-close" size="small" variant="text" @click="close" />
      </header>

      <section class="studio-preview mx-5 mt-3" aria-label="Live-Vorschau des Avatars">
        <div class="preview-decoration preview-star-one" /><div class="preview-decoration preview-star-two" />
        <AvatarFigure :appearance="draft" :size="190" />
        <div class="preview-tools">
          <strong>{{ userName }}</strong><span>Tippe unten auf ein Teil – du siehst es sofort.</span>
          <div class="random-actions"><button type="button" @click="randomLook(false)">Überraschung</button><button class="fun-random" type="button" @click="randomLook(true)">Quatschmix</button></div>
        </div>
      </section>

      <nav class="category-rail mt-3" aria-label="Figurenteile">
        <button v-for="category in categories" :key="category.value" :aria-current="section === category.value ? 'page' : undefined" :class="{ active: section === category.value }" type="button" @click="selectSection(category.value)">
          <AvatarCategoryIcon :name="category.value" /><span>{{ category.label }}</span>
        </button>
      </nav>

      <v-card-text ref="optionsPanel" class="builder-options px-5 pt-4">
        <div class="section-intro"><div><p>{{ activeCategory.kicker }}</p><h3>{{ activeCategory.title }}</h3></div><span>{{ activeCategory.hint }}</span></div>

        <div v-if="section === 'base'" class="skin-studio mt-4">
          <button v-for="(color, index) in skinColors" :key="color" :aria-label="`Hautton ${index + 1}`" :aria-pressed="draft.skinColor === color" :class="{ active: draft.skinColor === color }" class="skin-choice" type="button" @click="draft.skinColor = color">
            <span :style="{ '--swatch-color': color }" /><strong>Ton {{ index + 1 }}</strong>
          </button>
        </div>

        <template v-else-if="section === 'face'">
          <p class="mini-section-label mt-4">Gesichtsform</p>
          <OptionGrid v-model="draft.faceShape" :options="faceShapeOptions" preview-kind="faceShape" />
          <p class="mini-section-label mt-5">Augen & Ausdruck</p>
          <OptionGrid v-model="draft.face" :options="faceOptions" preview-kind="face" />
        </template>
        <template v-else-if="section === 'hair'">
          <OptionGrid v-model="draft.hair" :options="hairOptions" preview-kind="hair" /><ColorPicker v-model="draft.hairColor" :colors="hairColors" label="Haarfarbe" />
        </template>
        <template v-else-if="section === 'outfit'">
          <OptionGrid v-model="draft.outfit" :options="outfitOptions" preview-kind="outfit" /><ColorPicker v-model="draft.outfitColor" :colors="outfitColors" label="Outfitfarbe" />
        </template>
        <OptionGrid v-else-if="section === 'extras'" v-model="draft.accessory" :options="accessoryOptions" preview-kind="accessory" />
        <OptionGrid v-else-if="section === 'fun'" v-model="draft.fun" :options="funOptions" preview-kind="fun" />
        <OptionGrid v-else v-model="draft.season" :options="seasonOptions" preview-kind="season" />
      </v-card-text>

      <v-card-actions class="builder-actions px-5 py-3">
        <v-btn rounded="lg" variant="text" @click="close">Abbrechen</v-btn><v-spacer />
        <v-btn class="raised-button" color="primary" prepend-icon="mdi-check" rounded="lg" variant="flat" @click="save">Profil speichern</v-btn>
      </v-card-actions>
    </v-card>
  </v-dialog>
</template>

<script lang="ts" setup>
import { computed, defineComponent, h, nextTick, reactive, ref, watch } from 'vue';
import { motion } from 'motion-v';

import AvatarCategoryIcon from './AvatarCategoryIcon.vue';
import AvatarFigure from './AvatarFigure.vue';
import { accessoryOptions, faceOptions, faceShapeOptions, funOptions, hairColors, hairOptions, outfitColors, outfitOptions, seasonOptions, skinColors } from '../data/avatar-options';
import { createDefaultAvatarAppearance } from '../domain/avatar';
import type { AvatarAppearance, AvatarFun } from '../domain/avatar';

type Section = 'base' | 'face' | 'hair' | 'outfit' | 'extras' | 'fun' | 'season';
type PreviewKind = 'face' | 'faceShape' | 'hair' | 'outfit' | 'accessory' | 'fun' | 'season';

const props = defineProps<{ modelValue: boolean; userName: string; initialAppearance?: AvatarAppearance }>();
const emit = defineEmits<{ 'update:modelValue': [value: boolean]; save: [appearance: AvatarAppearance] }>();
const section = ref<Section>('base');
const optionsPanel = ref<HTMLElement | { $el?: HTMLElement }>();
const draft = reactive<AvatarAppearance>(createDefaultAvatarAppearance());
const categories: Array<{ value: Section; label: string; kicker: string; title: string; hint: string }> = [
  { value: 'base', label: 'Ich', kicker: 'Grundlage', title: 'Mein Hautton', hint: 'Was passt zu dir?' },
  { value: 'face', label: 'Gesicht', kicker: 'Mimik', title: 'So schaue ich', hint: 'Auch Zwinkern ist erlaubt.' },
  { value: 'hair', label: 'Haare', kicker: 'Frisur', title: 'Haare & Farbe', hint: 'Natürlich, verspielt oder mutig.' },
  { value: 'outfit', label: 'Kostüme', kicker: 'Verkleiden', title: 'Wer will ich heute sein?', hint: 'Kostüm, Maske oder Lieblingslook.' },
  { value: 'extras', label: 'Extras', kicker: 'Accessoires', title: 'Mein Lieblingsdetail', hint: 'Cool, süß oder königlich.' },
  { value: 'fun', label: 'Quatsch', kicker: 'Lustige Teile', title: 'Heute ein bisschen anders', hint: 'Clown, Pirat oder Monster?' },
  { value: 'season', label: 'Saison', kicker: 'Sammlung', title: 'Für besondere Tage', hint: 'Feiern geht das ganze Jahr.' },
];
const activeCategory = computed(() => categories.find(category => category.value === section.value) ?? categories[0]!);
const previewAppearance = (kind: PreviewKind, value: string): AvatarAppearance => ({ ...createDefaultAvatarAppearance(), skinColor: draft.skinColor, hairColor: draft.hairColor, outfitColor: draft.outfitColor, [kind]: value }) as AvatarAppearance;

const OptionGrid = defineComponent({
  props: { modelValue: { type: String, required: true }, options: { type: Array as () => Array<{ value: string; label: string }>, required: true }, previewKind: { type: String as () => PreviewKind, required: true } },
  emits: ['update:modelValue'],
  setup: (gridProps, { emit: gridEmit }) => () => h(
    'div',
    { class: 'option-grid mt-4' },
    gridProps.options.map((option, index) => h(
      motion.button,
      {
        class: ['option-choice', { active: gridProps.modelValue === option.value }],
        type: 'button',
        'data-preview-kind': gridProps.previewKind,
        'aria-label': `${option.label} auswählen`,
        'aria-pressed': gridProps.modelValue === option.value,
        initial: { opacity: 0, y: 10, scale: .95 },
        animate: { opacity: 1, y: 0, scale: 1 },
        transition: { type: 'spring', stiffness: 430, damping: 30, delay: index * .022 },
        whileHover: { y: -3, scale: 1.02 },
        whilePress: { scale: .96 },
        onClick: () => gridEmit('update:modelValue', option.value),
      },
      {
        default: () => [
          h('span', { class: 'option-art', 'aria-hidden': 'true' }, [h(AvatarFigure, { appearance: previewAppearance(gridProps.previewKind, option.value), size: 72 })]),
          h('strong', option.label),
          gridProps.modelValue === option.value ? h('span', { class: 'option-check', 'aria-hidden': 'true' }, '✓') : null,
        ],
      },
    )),
  ),
});

const ColorPicker = defineComponent({
  props: { modelValue: { type: String, required: true }, colors: { type: Array as () => string[], required: true }, label: { type: String, required: true } }, emits: ['update:modelValue'],
  setup: (colorProps, { emit: colorEmit }) => () => h('div', { class: 'compact-colors' }, [h('strong', colorProps.label), h('div', { class: 'color-row' }, colorProps.colors.map((color, index) => h('button', { class: ['color-choice', { active: colorProps.modelValue === color }], type: 'button', 'aria-label': `${colorProps.label} ${index + 1}`, 'aria-pressed': colorProps.modelValue === color, style: { background: color }, onClick: () => colorEmit('update:modelValue', color) }))) ]),
});

const randomItem = <T,>(items: T[]): T => items[Math.floor(Math.random() * items.length)] as T;
const resetDraft = () => Object.assign(draft, createDefaultAvatarAppearance(), props.initialAppearance ?? {});
const scrollOptionsToTop = () => void nextTick(() => { const panel = optionsPanel.value; const element = panel instanceof HTMLElement ? panel : panel?.$el; element?.scrollTo({ top: 0, behavior: 'smooth' }); });
const selectSection = (value: Section) => { section.value = value; scrollOptionsToTop(); };
const randomLook = (funny: boolean) => {
  const funnyParts = funOptions.filter(option => option.value !== 'none') as Array<{ value: AvatarFun; label: string }>;
  Object.assign(draft, { skinColor: randomItem(skinColors), faceShape: randomItem(faceShapeOptions).value, face: randomItem(faceOptions).value, hair: randomItem(hairOptions).value, hairColor: randomItem(hairColors), outfit: randomItem(outfitOptions).value, outfitColor: randomItem(outfitColors), accessory: randomItem(accessoryOptions).value, fun: funny ? randomItem(funnyParts).value : 'none', season: funny && Math.random() > .55 ? randomItem(seasonOptions).value : 'none' });
};
const close = () => emit('update:modelValue', false);
const save = () => { emit('save', { ...draft }); close(); };
watch(() => props.modelValue, (isOpen) => { if (!isOpen) return; resetDraft(); section.value = 'base'; scrollOptionsToTop(); }, { immediate: true });
</script>

<style scoped>
.avatar-builder {
  height: min(850px, 94dvh);
  max-height: min(850px, 94dvh);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  background: #fffdf9;
}
.studio-header {
  display: flex;
  flex-shrink: 0;
  align-items: center;
  justify-content: space-between;
}
.studio-header h2 {
  margin: 1px 0 0;
  font-size: 23px;
  letter-spacing: -0.035em;
}
.studio-preview {
  min-height: 196px;
  position: relative;
  display: grid;
  flex-shrink: 0;
  grid-template-columns: 210px 1fr;
  align-items: center;
  overflow: hidden;
  border: 2px solid rgba(58, 141, 114, 0.14);
  border-radius: 28px;
  background:
    radial-gradient(
      circle at 20% 18%,
      rgba(255, 255, 255, 0.96) 0 42px,
      transparent 43px
    ),
    linear-gradient(145deg, #dff7ee, #fff2c9);
  box-shadow:
    inset 0 -10px 0 rgba(112, 85, 44, 0.05),
    0 5px 0 rgba(54, 122, 96, 0.08);
}
.studio-preview::after {
  content: "";
  height: 38px;
  position: absolute;
  right: 0;
  bottom: 0;
  left: 0;
  background: rgba(229, 191, 116, 0.25);
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
  position: relative;
  z-index: 2;
  justify-self: center;
  filter: drop-shadow(0 9px 7px rgba(48, 86, 70, 0.16));
}
.preview-tools {
  position: relative;
  z-index: 2;
  padding-right: 18px;
  display: flex;
  flex-direction: column;
  align-items: flex-start;
}
.preview-tools > strong {
  font-size: 22px;
}
.preview-tools > span {
  max-width: 270px;
  margin-top: 2px;
  color: var(--lad-muted);
  font-size: 12px;
  line-height: 1.35;
}
.random-actions {
  margin-top: 14px;
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}
.random-actions button {
  min-height: 36px;
  padding: 0 13px;
  color: #31594b;
  border: 1px solid rgba(49, 89, 75, 0.18);
  border-radius: 12px;
  background: rgba(255, 255, 255, 0.82);
  box-shadow: 0 3px 0 rgba(49, 89, 75, 0.1);
  font: inherit;
  font-size: 11px;
  font-weight: 900;
  cursor: pointer;
}
.random-actions .fun-random {
  color: #663c7b;
  background: #f7e9ff;
}
.preview-decoration {
  width: 12px;
  height: 12px;
  position: absolute;
  z-index: 1;
  background: #ffd45f;
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
  display: flex;
  flex-shrink: 0;
  gap: 5px;
  overflow-x: auto;
  scrollbar-width: none;
  border-bottom: 1px solid var(--lad-border);
}
.category-rail::-webkit-scrollbar {
  display: none;
}
.category-rail button {
  min-width: 78px;
  height: 68px;
  position: relative;
  padding: 4px 6px 6px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 1px;
  color: var(--lad-muted);
  border: 0;
  border-radius: 18px;
  background: transparent;
  font: inherit;
  cursor: pointer;
  transition:
    transform 150ms ease,
    color 150ms ease,
    background 150ms ease;
}
.category-rail button:hover {
  transform: translateY(-2px);
  background: #f1faf6;
}
.category-rail button.active {
  color: #237b5e;
  background: linear-gradient(145deg, #e1f8ee, #fff5cf);
  box-shadow: inset 0 0 0 2px rgba(62, 188, 140, 0.2);
}
.category-rail button.active::after {
  content: "";
  width: 24px;
  height: 3px;
  position: absolute;
  bottom: 3px;
  border-radius: 999px;
  background: var(--lad-mint);
}
.category-rail span {
  font-size: 10px;
  font-weight: 900;
}
.builder-options {
  min-height: 0;
  flex: 1 1 auto;
  overflow-y: auto;
  padding-bottom: 34px !important;
  scroll-padding-bottom: 34px;
}
.section-intro {
  display: flex;
  align-items: end;
  justify-content: space-between;
  gap: 16px;
}
.section-intro p {
  margin: 0 0 1px;
  color: #26906d;
  font-size: 9px;
  font-weight: 950;
  letter-spacing: 0.11em;
  text-transform: uppercase;
}
.section-intro h3 {
  margin: 0;
  font-size: 18px;
}
.section-intro > span {
  color: var(--lad-muted);
  font-size: 10px;
  text-align: right;
}
.mini-section-label {
  margin-bottom: -8px;
  color: #526660;
  font-size: 10px;
  font-weight: 950;
  letter-spacing: 0.06em;
  text-transform: uppercase;
}
.skin-studio {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 10px;
}
.skin-choice {
  min-height: 126px;
  padding: 13px 6px 10px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 9px;
  color: var(--lad-text);
  border: 1px solid var(--lad-border);
  border-radius: 22px;
  background: linear-gradient(155deg, #f4fffa, #fff8e9);
  box-shadow: 0 4px 0 rgba(55, 95, 79, 0.09);
  font: inherit;
  cursor: pointer;
}
.skin-choice > span {
  width: 68px;
  height: 72px;
  position: relative;
  border: 3px solid white;
  border-radius: 48% 48% 44% 44%;
  background: var(--swatch-color);
  box-shadow: 0 3px 0 rgba(65, 70, 68, 0.15);
}
.skin-choice > span::before {
  content: "";
  width: 20px;
  height: 10px;
  position: absolute;
  top: 12px;
  left: 12px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.28);
  transform: rotate(-25deg);
}
.skin-choice strong {
  font-size: 11px;
}
.skin-choice.active {
  border: 2px solid var(--lad-mint);
  background: linear-gradient(155deg, #e1f8ed, #fff4c9);
  box-shadow: 0 5px 0 rgba(62, 188, 140, 0.24);
  transform: translateY(-2px);
}
:deep(.option-grid) {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 10px;
}
:deep(.option-choice) {
  min-height: 116px;
  position: relative;
  padding: 6px 5px 9px;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 4px;
  overflow: hidden;
  color: var(--lad-text);
  border: 1px solid var(--lad-border);
  border-radius: 21px;
  background: linear-gradient(155deg, #f8fffc, #fff7e7);
  box-shadow: 0 4px 0 rgba(55, 95, 79, 0.09);
  font: inherit;
  cursor: pointer;
}
:deep(.option-choice:nth-child(3n + 2)) {
  background: linear-gradient(155deg, #f2f7ff, #fff8ed);
}
:deep(.option-choice:nth-child(3n + 3)) {
  background: linear-gradient(155deg, #fff3f5, #f2fbf7);
}
:deep(.option-choice.active) {
  border: 2px solid var(--lad-mint);
  background: linear-gradient(155deg, #e1f8ed, #fff4c9);
  box-shadow: 0 5px 0 rgba(62, 188, 140, 0.24);
}
:deep(.option-choice strong) {
  max-width: 100%;
  font-size: 10px;
  line-height: 1.05;
  text-align: center;
}
:deep(.option-art) {
  width: 78px;
  height: 78px;
  display: grid;
  place-items: center;
  overflow: hidden;
  border: 2px solid rgba(255, 255, 255, 0.86);
  border-radius: 40% 40% 34% 34%;
  background: radial-gradient(
    circle at 50% 37%,
    #f5fffb 0 42%,
    #dff3ec 43% 69%,
    #f4dca8 70%
  );
  filter: drop-shadow(0 4px 3px rgba(44, 72, 61, 0.12));
}
:deep(.option-art .avatar-figure) {
  border: 0;
  border-radius: 30%;
  background: transparent;
  box-shadow: none;
}
:deep(.option-art .avatar-figure *) {
  animation: none !important;
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
  position: absolute;
  top: 5px;
  right: 5px;
  display: grid;
  place-items: center;
  color: white;
  border: 2px solid white;
  border-radius: 50%;
  background: var(--lad-mint);
  box-shadow: 0 2px 5px rgba(35, 98, 74, 0.2);
  font-size: 11px;
  font-weight: 950;
}
:deep(.compact-colors) {
  margin-top: 18px;
  padding: 12px 14px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  border: 1px solid var(--lad-border);
  border-radius: 18px;
  background: #f7fbf9;
}
:deep(.compact-colors > strong) {
  font-size: 11px;
}
:deep(.color-row) {
  display: flex;
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: 8px;
}
:deep(.color-choice) {
  width: 34px;
  height: 34px;
  position: relative;
  border: 3px solid white;
  border-radius: 43% 43% 48% 48%;
  box-shadow:
    0 2px 0 rgba(65, 70, 68, 0.14),
    0 0 0 1px var(--lad-border);
  cursor: pointer;
}
:deep(.color-choice::before) {
  content: "";
  width: 8px;
  height: 5px;
  position: absolute;
  top: 5px;
  left: 6px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.34);
  transform: rotate(-28deg);
}
:deep(.color-choice.active) {
  transform: translateY(-2px) scale(1.08);
  box-shadow:
    0 4px 0 rgba(62, 188, 140, 0.22),
    0 0 0 3px var(--lad-mint);
}
.builder-actions {
  position: relative;
  z-index: 3;
  flex-shrink: 0;
  border-top: 1px solid var(--lad-border);
  background: #fffdf8;
  box-shadow: 0 -10px 22px rgba(42, 69, 59, 0.06);
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
@media (max-width: 560px) {
  .studio-preview {
    min-height: 185px;
    grid-template-columns: 170px 1fr;
  }
  .studio-preview :deep(.avatar-figure) {
    width: 158px !important;
    height: 158px !important;
  }
  .preview-tools > strong {
    font-size: 18px;
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
@media (max-width: 390px) {
  .studio-preview {
    grid-template-columns: 1fr;
    padding: 8px;
  }
  .studio-preview :deep(.avatar-figure) {
    width: 140px !important;
    height: 140px !important;
  }
  .preview-tools {
    display: none;
  }
}
@media (prefers-reduced-motion: reduce) {
  .preview-decoration {
    animation: none;
  }
}
</style>
