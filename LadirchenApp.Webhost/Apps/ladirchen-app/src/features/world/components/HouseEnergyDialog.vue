<template>
  <v-dialog :model-value="modelValue" max-width="500" scrollable @update:model-value="emit('update:modelValue', $event)">
    <v-card class="energy-dialog" rounded="xl">
      <div class="energy-dialog-header pa-5">
        <div class="energy-title-row">
          <div>
            <p class="eyebrow mb-1">Unsere Familienwelt</p>
            <h2>Hausenergie heute</h2>
            <p class="energy-subtitle mt-1">Jeder Grundbeitrag bringt Licht und Leben in euer Zuhause.</p>
          </div>
          <AnimatedHouseEnergy class="energy-mascot" :size="86" />
          <v-btn class="close-button" aria-label="Hausenergie schließen" icon="mdi-close" size="small" variant="text" @click="close" />
        </div>

        <div class="summary-grid mt-4">
          <div class="summary-tile summary-tile--energy" :class="{ 'is-achieved': store.houseMeetsMinimumEnergy }">
            <div class="energy-orb" :style="{ '--energy': `${store.familyEnergy * 3.6}deg` }">
              <span>{{ store.familyEnergy }}<small>%</small></span>
            </div>
            <div>
              <span>Gemeinsame Hausenergie</span>
              <strong class="energy-state-title">{{ energyState.title }}<span v-if="store.houseMeetsMinimumEnergy" class="energy-state-spark" aria-hidden="true">✦</span></strong>
              <small>{{ energyState.copy }}</small>
            </div>
          </div>
          <div class="summary-tile summary-tile--goal">
            <div class="summary-icon" :class="{ 'is-achieved': store.houseMeetsMinimumEnergy }" aria-hidden="true">
              <AnimatedEnergyStar v-if="store.houseMeetsMinimumEnergy" :size="38" />
              <v-icon v-else size="25">mdi-progress-clock</v-icon>
            </div>
            <div>
              <span>Heutiges Tagesziel</span>
              <strong>{{ store.houseMeetsMinimumEnergy ? 'Erreicht' : `Noch ${60 - store.familyEnergy} %` }}</strong>
              <small>Ab 60 % zählt der Tag.</small>
            </div>
          </div>
        </div>
      </div>

      <v-card-text class="energy-content pa-5">
        <div class="threshold-card mb-5">
          <div class="threshold-heading">
            <div><span class="section-kicker">HEUTIGES GEMEINSAMES ZIEL</span><strong>Das Haus mit Energie versorgen</strong></div>
            <span>mindestens 60 %</span>
          </div>
          <div class="progress-wrap my-3">
            <v-progress-linear :color="store.houseMeetsMinimumEnergy ? 'primary' : 'warning'" height="11" :model-value="store.familyEnergy" rounded />
            <span v-if="store.houseMeetsMinimumEnergy" class="progress-glint" aria-hidden="true" />
          </div>
          <p><v-icon :color="store.houseMeetsMinimumEnergy ? 'primary' : 'warning'" size="16">{{ store.houseMeetsMinimumEnergy ? 'mdi-check-circle' : 'mdi-progress-clock' }}</v-icon>{{ store.houseMeetsMinimumEnergy ? 'Geschafft – dieser Tag zählt für eure Serie!' : `Noch ${60 - store.familyEnergy} Prozentpunkte, damit dieser Tag für eure Serie zählt.` }}</p>
        </div>

        <HouseProgressPanel
          class="mb-5"
          :completed-weeks="store.completedWeeklyStreak"
          :house-level="store.houseLevel"
        />

        <div class="children-heading mb-3">
          <div><span class="section-kicker">GEMEINSAM GESAMMELT</span><h3>So setzt sie sich zusammen</h3></div>
          <span class="average-label">Durchschnitt</span>
        </div>
        <div class="child-energy-list">
          <div v-for="(child, childIndex) in children" :key="child.id" class="child-energy-row" :style="{ '--avatar-color': child.color }">
            <div class="child-avatar">
              <AvatarFigure :appearance="childAppearance(child, childIndex)" calm :size="54" />
            </div>
            <div class="child-energy-copy">
              <div class="child-energy-title"><strong>{{ child.name }}</strong><b>{{ store.contributionProgress(child.id) }} %</b></div>
              <v-progress-linear class="mt-2" color="primary" height="8" :model-value="store.contributionProgress(child.id)" rounded />
              <p class="child-energy-description"><span>{{ approvedCount(child.id) }} von {{ baseCount(child.id) }}</span> Grundbeiträgen bestätigt</p>
            </div>
          </div>
        </div>

        <div class="calculation-note mt-5">
          <div class="calculation-icon"><v-icon size="23">mdi-calculator-variant-outline</v-icon></div>
          <p><strong>Ganz einfach gerechnet</strong><span>Die persönlichen Energie-Prozente aller Kinder werden addiert und durch die Anzahl der Kinder geteilt. Spezialaufgaben zählen hier nicht mit.</span></p>
        </div>

        <v-btn class="understood-button mt-5" color="primary" rounded="lg" size="large" variant="flat" width="100%" @click="close">Verstanden</v-btn>
      </v-card-text>
    </v-card>
  </v-dialog>
</template>

<script lang="ts" setup>
import { computed } from 'vue';

import AvatarFigure from '@/features/avatar/components/AvatarFigure.vue';
import { createDefaultAvatarAppearance } from '@/domain/avatar';
import type { AvatarAppearance } from '@/domain/avatar';
import type { FamilyMember, FamilyMemberId } from '@/domain/types';
import { useFamilyWorldStore } from '@/stores/family-world';

import AnimatedEnergyStar from './AnimatedEnergyStar.vue';
import AnimatedHouseEnergy from './AnimatedHouseEnergy.vue';
import HouseProgressPanel from './HouseProgressPanel.vue';

defineProps<{ modelValue: boolean }>();
const emit = defineEmits<{ 'update:modelValue': [value: boolean] }>();
const store = useFamilyWorldStore();
const children = computed(() => store.members.filter((member) => member.role === 'child'));
const energyState = computed(() => {
  if (store.familyEnergy === 100) return { title: 'Unser Haus strahlt!', copy: 'Alle Grundbeiträge der Familie sind geschafft.' };
  if (store.familyEnergy >= 60) return { title: 'Gut versorgt', copy: 'Gemeinsam habt ihr das Tagesziel erreicht.' };
  return { title: 'Braucht noch Energie', copy: 'Weitere Grundbeiträge bringen Licht und Leben zurück.' };
});
const baseContributions = (childId: FamilyMemberId) => store.contributions.filter((item) => item.kind === 'basic' && item.assigneeId === childId);
const baseCount = (childId: FamilyMemberId) => baseContributions(childId).length;
const approvedCount = (childId: FamilyMemberId) => baseContributions(childId).filter((item) => item.status === 'approved').length;
const childAppearance = (child: FamilyMember, index: number): AvatarAppearance => {
  if (child.appearance) return child.appearance;
  const appearance = createDefaultAvatarAppearance();
  const variants: ReadonlyArray<Partial<AvatarAppearance>> = [
    { hair: 'ponytail', outfitColorId: 'outfit-blue' },
    { hair: 'short', hairColorId: 'hair-black', outfit: 'overalls', outfitColorId: 'outfit-gold' },
    { hair: 'curls', hairColorId: 'hair-brown', outfit: 'space', outfitColorId: 'outfit-ocean' },
  ];
  return { ...appearance, ...(variants[index % variants.length] ?? {}) };
};
const close = () => emit('update:modelValue', false);
</script>

<style scoped>
.energy-dialog {
  max-height: min(820px, 94dvh);
  @apply overflow-hidden;
  color: #253843;
  border: 2px solid rgba(224, 142, 99, 0.2);
  background: #fffdf8 !important;
  box-shadow:
    0 10px 0 rgba(119, 85, 47, 0.12),
    0 28px 70px rgba(62, 85, 75, 0.22) !important;
}
.energy-dialog-header {
  @apply position-relative overflow-hidden;
  flex: 0 0 auto;
  border-bottom: 1px solid rgba(181, 107, 107, 0.14);
  background: linear-gradient(145deg, #ffe3e9 0%, #ffedda 48%, #fff4c9 100%);
}
.energy-dialog-header::before,
.energy-dialog-header::after {
  content: "";
  @apply position-absolute;
  border-radius: 50%;
  @apply pointer-events-none;
}
.energy-dialog-header::before {
  width: 190px;
  height: 190px;
  top: -113px;
  right: -34px;
  background: rgba(255, 255, 255, 0.34);
  box-shadow: 0 0 0 22px rgba(255, 255, 255, 0.15);
}
.energy-dialog-header::after {
  width: 105px;
  height: 35px;
  right: 76px;
  bottom: -21px;
  background: rgba(255, 255, 255, 0.32);
  filter: blur(2px);
}
.energy-title-row {
  min-height: 108px;
  @apply position-relative;
  z-index: 1;
  @apply d-flex align-start;
}
.energy-title-row > div:first-child {
  max-width: 280px;
}
.energy-dialog-header h2 {
  @apply ma-0;
  font-size: 27px;
  letter-spacing: -0.04em;
}
.energy-subtitle {
  max-width: 260px;
  color: #65766f;
  font-size: 12px;
  line-height: 1.4;
}
.close-button {
  @apply position-absolute;
  top: -7px;
  right: -8px;
  z-index: 4;
}
.energy-mascot {
  @apply position-absolute;
  top: 10px;
  right: 19px;
}
.summary-grid {
  @apply position-relative;
  z-index: 1;
  @apply d-grid;
  grid-template-columns: 1.2fr 0.8fr;
  gap: 10px;
}
.summary-tile {
  @apply min-w-0;
  padding: 11px;
  @apply d-flex align-center;
  gap: 10px;
  border: 2px solid rgba(110, 82, 72, 0.11);
  border-radius: 18px;
  background: rgba(255, 255, 255, 0.76);
  box-shadow:
    0 5px 0 rgba(126, 88, 57, 0.08),
    inset 0 1px 0 rgba(255, 255, 255, 0.72);
}
.summary-tile--energy {
  @apply position-relative overflow-hidden;
}
.summary-tile--energy::after {
  width: 48px;
  height: 130%;
  content: "";
  @apply position-absolute pointer-events-none;
  top: -15%;
  left: -70px;
  background: linear-gradient(
    90deg,
    transparent,
    rgba(255, 255, 255, 0.78),
    transparent
  );
  transform: skewX(-18deg);
}
.summary-tile--energy.is-achieved::after {
  animation: energy-card-shimmer 4s 1s ease-in-out infinite;
}
.summary-tile > div:last-child {
  @apply min-w-0;
}
.summary-tile span,
.summary-tile strong,
.summary-tile small {
  @apply d-block;
}
.summary-tile span {
  color: var(--lad-muted);
  font-size: 9px;
}
.summary-tile strong {
  margin-top: 1px;
  font-size: 15px;
  line-height: 1.2;
}
.summary-tile small {
  margin-top: 3px;
  @apply overflow-hidden;
  color: #64756e;
  font-size: 8px;
  line-height: 1.3;
}
.energy-state-title {
  @apply d-flex align-center;
  gap: 4px;
}
.energy-state-spark {
  color: #efa827 !important;
  font-size: 12px !important;
  animation: energy-state-spark 2.4s ease-in-out infinite;
}
.energy-orb {
  width: 60px;
  height: 60px;
  @apply d-grid place-center;
  flex: 0 0 60px;
  border-radius: 50%;
  background: conic-gradient(
    var(--lad-mint) var(--energy),
    rgba(77, 149, 119, 0.12) 0
  );
  box-shadow:
    0 4px 0 rgba(43, 143, 106, 0.13),
    0 0 0 5px rgba(255, 255, 255, 0.52);
  animation: energy-orb-breathe 2.9s ease-in-out infinite;
}
.energy-orb::before {
  width: 46px;
  height: 46px;
  content: "";
  grid-area: 1 / 1;
  border-radius: 50%;
  background: #fffdf8;
}
.energy-orb span {
  z-index: 1;
  grid-area: 1 / 1;
  color: var(--lad-mint-dark);
  font-size: 20px;
  font-weight: 950;
}
.energy-orb small {
  @apply d-inline;
  font-size: 9px;
}
.summary-icon {
  width: 44px;
  height: 44px;
  @apply d-grid place-center;
  flex: 0 0 44px;
  color: #b27518;
  border: 2px solid rgba(255, 255, 255, 0.86);
  border-radius: 14px;
  background: #ffedb5;
  box-shadow: 0 4px 0 rgba(181, 111, 18, 0.13);
  transform: rotate(-4deg);
}
.summary-icon.is-achieved {
  background: linear-gradient(145deg, #fff8cf, #ffe59b);
  box-shadow: 0 4px 0 rgba(181, 111, 18, 0.13);
}
.energy-content {
  min-height: 0;
  flex: 1 1 auto;
  @apply overflow-y-auto;
  background: linear-gradient(180deg, #fffdf8, #f1faf5);
}
.threshold-card {
  padding: 15px;
  border: 2px solid rgba(242, 175, 66, 0.25);
  border-radius: 20px;
  background:
    radial-gradient(
      circle at 92% 12%,
      rgba(255, 255, 255, 0.65),
      transparent 25%
    ),
    linear-gradient(135deg, #fff9ed, #fff3d6 62%, #effaf4);
  box-shadow:
    0 6px 0 rgba(179, 126, 45, 0.1),
    0 12px 24px rgba(97, 91, 60, 0.06);
}
.threshold-heading {
  @apply d-flex align-end justify-space-between ga-3;
}
.threshold-heading > div span,
.threshold-heading > div strong {
  @apply d-block;
}
.threshold-heading > div strong {
  margin-top: 2px;
  font-size: 13px;
}
.threshold-heading > span {
  padding: 5px 8px;
  color: #865a14;
  border-radius: 999px;
  background: #ffe8ad;
  font-size: 8px;
  @apply font-weight-black text-no-wrap;
}
.section-kicker {
  color: #278568;
  font-size: 8px;
  font-weight: 950;
  letter-spacing: 0.1em;
}
.progress-wrap {
  @apply position-relative overflow-hidden;
  border-radius: 999px;
}
.progress-glint {
  width: 52px;
  height: 24px;
  @apply position-absolute;
  top: -6px;
  left: -58px;
  background: linear-gradient(
    90deg,
    transparent,
    rgba(255, 255, 255, 0.86),
    transparent
  );
  transform: skewX(-20deg);
  animation: progress-glint 3.2s ease-in-out infinite;
}
.threshold-card p {
  @apply ma-0 d-flex align-center;
  gap: 5px;
  color: #52655d;
  font-size: 9px;
  font-weight: 750;
  line-height: 1.4;
}
.children-heading {
  @apply d-flex align-end justify-space-between ga-3;
}
.energy-dialog h3 {
  margin: 1px 0 0;
  font-size: 17px;
}
.average-label {
  color: var(--lad-muted);
  font-size: 9px;
  font-weight: 800;
}
.child-energy-list {
  @apply d-flex flex-column;
  gap: 10px;
}
.child-energy-row {
  min-height: 78px;
  padding: 11px 12px;
  @apply d-flex align-center;
  gap: 12px;
  border: 2px solid rgba(74, 145, 114, 0.16);
  border-radius: 18px;
  background: linear-gradient(
    135deg,
    rgba(255, 255, 255, 0.94),
    color-mix(in srgb, var(--avatar-color, #6f8df5) 7%, white)
  );
  box-shadow:
    0 5px 0 rgba(57, 137, 106, 0.09),
    0 10px 20px rgba(71, 107, 92, 0.06);
  transition:
    transform 0.2s ease,
    border-color 0.2s ease;
}
.child-energy-row:hover {
  border-color: rgba(62, 188, 140, 0.34);
  transform: translateY(-2px);
}
.child-avatar {
  width: 58px;
  height: 58px;
  @apply d-grid place-center overflow-hidden;
  flex: 0 0 58px;
  border: 2px solid rgba(255, 255, 255, 0.9);
  border-radius: 19px;
  background: color-mix(in srgb, var(--avatar-color, #6f8df5) 18%, white);
  box-shadow: 0 4px 0
    color-mix(in srgb, var(--avatar-color, #6f8df5) 18%, transparent);
  transform: rotate(-2deg);
}
.child-energy-copy {
  @apply min-w-0;
  flex: 1;
}
.child-energy-title {
  @apply d-flex align-center justify-space-between ga-2;
}
.child-energy-row strong {
  font-size: 13px;
}
.child-energy-row b {
  padding: 4px 8px;
  color: var(--lad-mint-dark);
  border-radius: 999px;
  background: #e4f6ec;
  font-size: 11px;
}
.child-energy-description {
  @apply ma-0 mt-2;
  color: #62766d;
  font-size: 10px;
  font-weight: 650;
  line-height: 1.35;
}
.child-energy-description span {
  @apply d-inline;
  color: #278568;
  font-weight: 900;
}
.calculation-note {
  padding: 13px;
  @apply d-flex align-start;
  gap: 11px;
  border: 1px solid rgba(87, 153, 202, 0.1);
  border-radius: 18px;
  background: linear-gradient(135deg, #edf7ff, #f4fbff);
}
.calculation-icon {
  width: 40px;
  height: 40px;
  @apply d-grid place-center;
  flex: 0 0 40px;
  color: #3e88c5;
  border-radius: 13px;
  background: #dcefff;
}
.calculation-note p,
.calculation-note strong,
.calculation-note span {
  @apply d-block;
}
.calculation-note p {
  @apply ma-0;
}
.calculation-note strong {
  font-size: 11px;
}
.calculation-note span {
  margin-top: 2px;
  color: var(--lad-muted);
  font-size: 9px;
  line-height: 1.45;
}
.understood-button {
  box-shadow: 0 4px 0 #238463;
}
@keyframes energy-orb-breathe {
  0%,
  100% {
    transform: scale(1);
  }
  50% {
    transform: scale(1.035);
  }
}
@keyframes energy-card-shimmer {
  0%,
  35% {
    left: -70px;
  }
  68%,
  100% {
    left: 120%;
  }
}
@keyframes energy-state-spark {
  0%,
  30%,
  100% {
    opacity: 0.42;
    transform: rotate(0) scale(0.72);
  }
  48%,
  62% {
    opacity: 1;
    transform: rotate(25deg) scale(1.2);
  }
}
@keyframes progress-glint {
  0%,
  35% {
    left: -58px;
  }
  72%,
  100% {
    left: 110%;
  }
}
@media (max-width: 420px) {
  .summary-grid {
    grid-template-columns: 1fr;
  }
  .energy-title-row > div:first-child {
    max-width: 235px;
  }
  .energy-mascot {
    right: 12px;
    transform: scale(0.9);
    transform-origin: right top;
  }
}
@media (prefers-reduced-motion: reduce) {
  .energy-orb,
  .summary-tile--energy::after,
  .energy-state-spark,
  .progress-glint {
    animation: none;
  }
  .child-energy-row {
    transition: none;
  }
}
</style>
