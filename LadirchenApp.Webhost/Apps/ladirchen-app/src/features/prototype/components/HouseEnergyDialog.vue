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
          <div class="summary-tile summary-tile--energy">
            <div class="energy-orb" :style="{ '--energy': `${store.familyEnergy * 3.6}deg` }">
              <span>{{ store.familyEnergy }}<small>%</small></span>
            </div>
            <div><span>Gemeinsame Hausenergie</span><strong>{{ energyState.title }}</strong><small>{{ energyState.copy }}</small></div>
          </div>
          <div class="summary-tile summary-tile--goal">
            <div class="summary-icon" aria-hidden="true"><v-icon size="25">mdi-lightning-bolt</v-icon></div>
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

        <div class="house-stage-card mb-3">
          <div class="house-stage-visual" aria-hidden="true">
            <span class="house-halo" />
            <span class="house-spark house-spark--one">✦</span>
            <span class="house-spark house-spark--two">✦</span>
            <span class="house-stage-icon">{{ currentStage.icon }}</span>
          </div>
          <div class="house-stage-copy">
            <span class="section-kicker">AKTUELLE HAUSSTUFE</span>
            <div class="house-stage-title">
              <strong>{{ currentStage.name }}</strong>
              <span>Stufe {{ store.houseLevel + 1 }}</span>
            </div>
            <p>{{ store.completedWeeklyStreak }} erfolgreiche Hauswoche{{ store.completedWeeklyStreak === 1 ? '' : 'n' }} abgeschlossen</p>
            <div class="stage-track" :aria-label="`Hausstufe ${store.houseLevel + 1} von ${houseStages.length}`">
              <span
                v-for="stage in houseStages"
                :key="stage.level"
                :class="{ reached: stage.level <= store.houseLevel, current: stage.level === store.houseLevel }"
              />
            </div>
            <b>{{ store.houseLevel < houseStages.length - 1 ? 'Nächste Stufe: Überraschung' : 'Höchste Hausstufe erreicht' }}</b>
          </div>
        </div>

        <div class="house-evolution mb-5">
          <div class="evolution-heading">
            <div class="evolution-icon" aria-hidden="true"><v-icon size="22">mdi-home-switch</v-icon></div>
            <div><span class="section-kicker">WOCHENABSCHLUSS</span><strong>So entwickelt sich das Haus</strong></div>
          </div>
          <div class="evolution-steps mt-3">
            <div><span>60 %</span><p>Erreicht eure gemeinsame Hausenergie täglich mindestens 60 %, zählt der Tag für die Serie.</p></div>
            <div><span>7 Tage</span><p>Am Wochenabschluss wird die nächste Hausstufe enthüllt.</p></div>
            <div><span>Unter 60 %</span><p>Das Haus geht eine Stufe zurück. Ladirchen, Ziele, Möbel und Gartendinge bleiben erhalten.</p></div>
          </div>
          <p class="evolution-note mt-3">Die Hausenergie ist der Durchschnitt des Beitragsfortschritts aller Kinder. Bezugspersonen erstellen und bewerten Beiträge, erledigen sie aber nicht selbst.</p>
        </div>

        <div class="children-heading mb-3">
          <div><span class="section-kicker">GEMEINSAM GESAMMELT</span><h3>So setzt sie sich zusammen</h3></div>
          <span class="average-label">Durchschnitt</span>
        </div>
        <div class="child-energy-list">
          <div v-for="child in children" :key="child.id" class="child-energy-row">
            <div class="child-avatar" :style="{ background: `${child.color}20` }">{{ child.avatar }}</div>
            <div class="child-energy-copy">
              <div class="child-energy-title"><strong>{{ child.name }}</strong><b>{{ store.contributionProgress(child.id) }} %</b></div>
              <v-progress-linear class="mt-2" color="primary" height="8" :model-value="store.contributionProgress(child.id)" rounded />
              <small>{{ approvedCount(child.id) }} von {{ baseCount(child.id) }} Grundbeiträgen bestätigt</small>
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

import AnimatedHouseEnergy from './AnimatedHouseEnergy.vue';
import { HOUSE_STAGES } from '../data/house-catalog';
import { usePrototypeStore } from '../stores/prototype';

defineProps<{ modelValue: boolean }>();
const emit = defineEmits<{ 'update:modelValue': [value: boolean] }>();
const store = usePrototypeStore();
const houseStages = HOUSE_STAGES;
const currentStage = computed(() => houseStages[store.houseLevel] ?? houseStages[0]);
const children = computed(() => store.members.filter((member) => member.role === 'child'));
const energyState = computed(() => {
  if (store.familyEnergy === 100) return { title: 'Unser Haus strahlt!', copy: 'Alle Grundbeiträge der Familie sind geschafft.' };
  if (store.familyEnergy >= 60) return { title: 'Gut versorgt', copy: 'Gemeinsam habt ihr das Tagesziel erreicht.' };
  return { title: 'Braucht noch Energie', copy: 'Weitere Grundbeiträge bringen Licht und Leben zurück.' };
});
const baseContributions = (childId: string) => store.contributions.filter((item) => item.kind === 'basic' && item.assigneeId === childId);
const baseCount = (childId: string) => baseContributions(childId).length;
const approvedCount = (childId: string) => baseContributions(childId).filter((item) => item.status === 'approved').length;
const close = () => emit('update:modelValue', false);
</script>

<style scoped>
.energy-dialog {
  max-height: min(820px, 94dvh);
  overflow: hidden;
  color: #253843;
  background: #fffdf8 !important;
}
.energy-dialog-header {
  position: relative;
  overflow: hidden;
  flex: 0 0 auto;
  border-bottom: 1px solid rgba(181, 107, 107, 0.14);
  background: linear-gradient(145deg, #ffe3e9 0%, #ffedda 48%, #fff4c9 100%);
}
.energy-dialog-header::before,
.energy-dialog-header::after {
  content: "";
  position: absolute;
  border-radius: 50%;
  pointer-events: none;
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
  position: relative;
  z-index: 1;
  display: flex;
  align-items: flex-start;
}
.energy-title-row > div:first-child {
  max-width: 280px;
}
.energy-dialog-header h2 {
  margin: 0;
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
  position: absolute;
  top: -7px;
  right: -8px;
  z-index: 4;
}
.energy-mascot {
  position: absolute;
  top: 10px;
  right: 19px;
}
.summary-grid {
  position: relative;
  z-index: 1;
  display: grid;
  grid-template-columns: 1.2fr 0.8fr;
  gap: 10px;
}
.summary-tile {
  min-width: 0;
  padding: 11px;
  display: flex;
  align-items: center;
  gap: 10px;
  border: 1px solid rgba(110, 82, 72, 0.13);
  border-radius: 18px;
  background: rgba(255, 255, 255, 0.7);
  box-shadow: inset 0 1px 0 rgba(255, 255, 255, 0.72);
}
.summary-tile > div:last-child {
  min-width: 0;
}
.summary-tile span,
.summary-tile strong,
.summary-tile small {
  display: block;
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
  overflow: hidden;
  color: #64756e;
  font-size: 8px;
  line-height: 1.3;
}
.energy-orb {
  width: 60px;
  height: 60px;
  display: grid;
  place-items: center;
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
  display: inline;
  font-size: 9px;
}
.summary-icon {
  width: 44px;
  height: 44px;
  display: grid;
  place-items: center;
  flex: 0 0 44px;
  color: #b27518;
  border-radius: 14px;
  background: #ffedb5;
  animation: energy-bolt-pulse 2.3s ease-in-out infinite;
}
.energy-content {
  min-height: 0;
  flex: 1 1 auto;
  overflow-y: auto;
  background: linear-gradient(180deg, #fffdf8, #f1faf5);
}
.threshold-card {
  padding: 15px;
  border: 1px solid rgba(242, 175, 66, 0.25);
  border-radius: 20px;
  background: linear-gradient(135deg, #fff9ed, #fff3d6 62%, #effaf4);
  box-shadow: 0 4px 0 rgba(179, 126, 45, 0.07);
}
.threshold-heading {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 12px;
}
.threshold-heading > div span,
.threshold-heading > div strong {
  display: block;
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
  font-weight: 900;
  white-space: nowrap;
}
.section-kicker {
  color: #278568;
  font-size: 8px;
  font-weight: 950;
  letter-spacing: 0.1em;
}
.progress-wrap {
  position: relative;
  overflow: hidden;
  border-radius: 999px;
}
.progress-glint {
  width: 52px;
  height: 24px;
  position: absolute;
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
  margin: 0;
  display: flex;
  align-items: center;
  gap: 5px;
  color: #52655d;
  font-size: 9px;
  font-weight: 750;
  line-height: 1.4;
}
.house-stage-card {
  min-height: 126px;
  padding: 14px;
  display: flex;
  align-items: center;
  gap: 13px;
  overflow: hidden;
  border: 1px solid rgba(236, 179, 74, 0.2);
  border-radius: 20px;
  background: linear-gradient(135deg, #fff9ed, #fff1d4 58%, #edf9f2);
  box-shadow: 0 4px 0 rgba(179, 126, 45, 0.08);
}
.house-stage-visual {
  width: 88px;
  height: 88px;
  position: relative;
  display: grid;
  place-items: center;
  flex: 0 0 88px;
}
.house-halo {
  width: 72px;
  height: 72px;
  position: absolute;
  border-radius: 50%;
  background: repeating-conic-gradient(
    from 0deg,
    rgba(244, 179, 64, 0.28) 0 12deg,
    transparent 12deg 25deg
  );
  animation: house-halo-spin 12s linear infinite;
}
.house-stage-icon {
  position: relative;
  z-index: 2;
  font-size: 52px;
  line-height: 1;
  filter: drop-shadow(0 6px 5px rgba(111, 76, 41, 0.18));
  animation: house-stage-bounce 3.2s ease-in-out infinite;
}
.house-spark {
  position: absolute;
  z-index: 3;
  color: #e99a2c;
  font-size: 14px;
  animation: energy-spark 1.8s ease-in-out infinite;
}
.house-spark--one {
  top: 7px;
  right: 7px;
}
.house-spark--two {
  bottom: 7px;
  left: 5px;
  animation-delay: -0.9s;
}
.house-stage-copy {
  min-width: 0;
  flex: 1;
}
.house-stage-title {
  margin: 2px 0;
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 5px;
}
.house-stage-title strong {
  font-size: 14px;
}
.house-stage-title span {
  padding: 3px 7px;
  color: #805716;
  border-radius: 999px;
  background: #ffe8ad;
  font-size: 8px;
  font-weight: 900;
}
.house-stage-copy > p {
  margin: 0;
  color: var(--lad-muted);
  font-size: 9px;
  line-height: 1.35;
}
.house-stage-copy > b {
  display: block;
  margin-top: 5px;
  color: #7b581d;
  font-size: 8px;
}
.stage-track {
  margin-top: 8px;
  display: grid;
  grid-template-columns: repeat(5, 1fr);
  gap: 4px;
}
.stage-track span {
  height: 6px;
  border-radius: 999px;
  background: rgba(93, 126, 107, 0.13);
}
.stage-track span.reached {
  background: #48bf91;
}
.stage-track span.current {
  box-shadow: 0 0 0 3px rgba(72, 191, 145, 0.16);
  animation: stage-pulse 1.8s ease-in-out infinite;
}
.house-evolution {
  padding: 14px;
  border: 1px solid rgba(80, 151, 120, 0.17);
  border-radius: 20px;
  background: rgba(255, 255, 255, 0.78);
}
.evolution-heading {
  display: flex;
  align-items: center;
  gap: 10px;
}
.evolution-heading > div:last-child span,
.evolution-heading > div:last-child strong {
  display: block;
}
.evolution-heading > div:last-child strong {
  margin-top: 1px;
  font-size: 13px;
}
.evolution-icon {
  width: 40px;
  height: 40px;
  display: grid;
  place-items: center;
  flex: 0 0 40px;
  color: #278568;
  border-radius: 13px;
  background: #def4e8;
}
.evolution-steps {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 6px;
}
.evolution-steps > div {
  padding: 8px;
  border-radius: 12px;
  background: #f5faf7;
}
.evolution-steps span {
  color: #238765;
  font-size: 9px;
  font-weight: 950;
}
.evolution-steps p {
  margin: 3px 0 0;
  color: #566960;
  font-size: 7.5px;
  line-height: 1.35;
}
.evolution-note {
  margin-bottom: 0;
  color: var(--lad-muted);
  font-size: 8px;
  line-height: 1.4;
}
.children-heading {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: 12px;
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
  display: flex;
  flex-direction: column;
  gap: 9px;
}
.child-energy-row {
  padding: 11px 12px;
  display: flex;
  align-items: center;
  gap: 11px;
  border: 1px solid rgba(74, 145, 114, 0.14);
  border-radius: 17px;
  background: rgba(255, 255, 255, 0.77);
  box-shadow: 0 3px 0 rgba(57, 137, 106, 0.07);
  transition:
    transform 0.2s ease,
    border-color 0.2s ease;
}
.child-energy-row:hover {
  border-color: rgba(62, 188, 140, 0.34);
  transform: translateY(-1px);
}
.child-avatar {
  width: 44px;
  height: 44px;
  display: grid;
  place-items: center;
  flex: 0 0 44px;
  border-radius: 15px;
  font-size: 24px;
}
.child-energy-copy {
  min-width: 0;
  flex: 1;
}
.child-energy-title {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 8px;
}
.child-energy-row strong,
.child-energy-row b {
  font-size: 11px;
}
.child-energy-row b {
  padding: 3px 7px;
  color: var(--lad-mint-dark);
  border-radius: 999px;
  background: #e4f6ec;
}
.child-energy-row small {
  display: block;
  margin-top: 4px;
  color: var(--lad-muted);
  font-size: 9px;
}
.calculation-note {
  padding: 13px;
  display: flex;
  align-items: flex-start;
  gap: 11px;
  border: 1px solid rgba(87, 153, 202, 0.1);
  border-radius: 18px;
  background: linear-gradient(135deg, #edf7ff, #f4fbff);
}
.calculation-icon {
  width: 40px;
  height: 40px;
  display: grid;
  place-items: center;
  flex: 0 0 40px;
  color: #3e88c5;
  border-radius: 13px;
  background: #dcefff;
}
.calculation-note p,
.calculation-note strong,
.calculation-note span {
  display: block;
}
.calculation-note p {
  margin: 0;
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
@keyframes energy-spark {
  0%,
  100% {
    opacity: 0.3;
    transform: scale(0.7) rotate(0);
  }
  50% {
    opacity: 1;
    transform: scale(1.2) rotate(20deg);
  }
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
@keyframes energy-bolt-pulse {
  0%,
  100% {
    transform: rotate(-3deg) scale(1);
  }
  50% {
    transform: rotate(3deg) scale(1.08);
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
@keyframes house-stage-bounce {
  0%,
  100% {
    transform: translateY(2px) rotate(-2deg);
  }
  50% {
    transform: translateY(-6px) rotate(2deg);
  }
}
@keyframes house-halo-spin {
  to {
    transform: rotate(360deg);
  }
}
@keyframes stage-pulse {
  0%,
  100% {
    transform: scaleY(1);
  }
  50% {
    transform: scaleY(1.5);
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
  .house-stage-visual {
    width: 72px;
    flex-basis: 72px;
  }
  .evolution-steps {
    grid-template-columns: 1fr;
  }
}
@media (prefers-reduced-motion: reduce) {
  .energy-orb,
  .summary-icon,
  .progress-glint,
  .house-halo,
  .house-stage-icon,
  .house-spark,
  .stage-track span.current {
    animation: none;
  }
  .child-energy-row {
    transition: none;
  }
}
</style>
