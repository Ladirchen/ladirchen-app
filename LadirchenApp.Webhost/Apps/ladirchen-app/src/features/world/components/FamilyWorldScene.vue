<template>
  <div class="world-scene-wrap" :class="[{ 'is-inside': view === 'inside', 'is-garden': view === 'garden', 'is-storage-open': storageOpen, 'is-dragging-furniture': draggingEntityType === 'furniture' }, houseEnergyClass, `theme-${activeThemeId}`]" :style="sceneStyle">
    <motion.div
      class="scene-drag-layer"
      :drag="false"
      :drag-constraints="{ left: 0, right: 0 }"
      :drag-elastic=".16"
      :drag-momentum="false"
      :transition="{ type: 'spring', stiffness: 390, damping: 30 }"
    >
      <svg class="world-scene" :class="`theme-${activeThemeId}`" viewBox="10 8 440 325" role="img" :aria-label="ariaLabel">
        <defs>
          <linearGradient id="meadowGround" x1="0" x2="0" y1="0" y2="1">
            <stop offset="0" stop-color="var(--lad-palette-green-250)" />
            <stop offset=".48" stop-color="var(--lad-palette-green-250)" />
            <stop offset="1" stop-color="var(--lad-palette-amber-100)" />
          </linearGradient>
          <linearGradient id="insideFloor" x1="0" x2="1" y1="0" y2="1">
            <stop offset="0" stop-color="var(--lad-palette-amber-200)" />
            <stop offset="1" stop-color="var(--lad-palette-orange-350)" />
          </linearGradient>
          <linearGradient id="insideWall" x1="0" x2="0" y1="0" y2="1">
            <stop offset="0" stop-color="var(--lad-palette-amber-100)" />
            <stop offset="1" stop-color="var(--lad-palette-amber-150)" />
          </linearGradient>
          <pattern id="cottonRoofPattern" width="24" height="24" patternUnits="userSpaceOnUse" patternTransform="rotate(-8)">
            <rect width="24" height="24" fill="var(--lad-palette-rose-200)" />
            <path d="M-6 6 6-6m0 24L30-6M18 30 30 18" stroke="var(--lad-palette-surface)" stroke-width="8" />
            <circle cx="7" cy="7" r="2.5" fill="var(--lad-palette-blue-250)" />
          </pattern>
          <filter id="sceneShadow" height="160%" width="160%" x="-30%" y="-30%">
            <feDropShadow dx="0" dy="12" flood-color="var(--lad-palette-muted-700)" flood-opacity=".2" stdDeviation="8" />
          </filter>
          <clipPath id="frontDoorOpeningClip">
            <path d="M210 195q0-13 13-13h15q13 0 13 13v60h-41Z" />
          </clipPath>
        </defs>

        <g class="scene-sun" :style="{ opacity: sunOpacity }" aria-hidden="true">
          <circle class="sun-halo" cx="375" cy="48" r="39" />
          <circle class="sun-core" cx="375" cy="48" r="23" />
          <path class="sun-rays" d="M375 10V1m0 94v-9m38-38h9m-94 0h9m65-27 7-7m-67 67 7-7m53 7 7 7m-67-67 7 7" />
        </g>
        <g class="cloud cloud-a" :class="{ heavy: energy < 45 }"><ellipse cx="75" cy="79" rx="38" ry="12" /><circle cx="60" cy="70" r="15" /><circle cx="81" cy="65" r="21" /></g>
        <g class="cloud cloud-b" :class="{ heavy: energy < 60 }"><ellipse cx="381" cy="91" rx="32" ry="10" /><circle cx="370" cy="84" r="13" /><circle cx="388" cy="80" r="17" /></g>
        <g v-if="energy < 40" class="rain-cloud" aria-hidden="true">
          <ellipse cx="229" cy="68" rx="48" ry="15" /><circle cx="209" cy="57" r="20" /><circle cx="237" cy="52" r="25" /><circle cx="258" cy="62" r="17" />
          <path class="rain-drops" d="M205 86l-6 12m31-12-6 12m31-12-6 12" />
        </g>

        <g class="far-landscape" aria-hidden="true">
          <g class="mountains">
            <path class="mountain-layer" d="M0 180V145l48-48 35 34 47-60 48 58 43-42 57 61 47-65 62 53 43-38 50 40v42Z" />
            <path class="mountain-snow" d="m31 114 17-17 14 14 21 20-17-7-10 7-11-10-14 5Zm74-12 25-31 25 30-15-8-10 10-9-9Zm96 5 20-20 21 23-12-5-9 8-9-9Zm105 9 26-33 28 24-17-6-11 11-11-9Zm101-2 23-16 18 15-11-3-8 7-10-7Z" />
          </g>
          <path class="hill-layer hill-back" d="M0 194v-38q59-34 116 11 60-48 124 0 53-39 104-1 59-34 116 2v26Z" />
          <path class="hill-layer hill-front" d="M0 207v-29q58-37 128 7 68-43 136 1 71-42 128-2 37-21 68-3v26Z" />
          <path class="meadow-ground" d="M0 192q60-18 123 4 70-28 139 0 71-27 131-4 35-12 67 1v147H0Z" />
          <path class="meadow-highlight" d="M-5 231q86-28 165 2t157 1q78-27 148 1M11 281q74-19 145 3t147 0q76-21 151 0" />
          <g class="distant-pines">
            <path d="m23 165 10-23 10 23h-6l8 16H21l8-16Zm367 1 10-25 10 25h-6l8 16h-24l8-16Zm37 4 8-19 8 19h-5l7 14h-20l7-14Z" />
          </g>
        </g>

        <g :key="revealVersion" :class="['island', 'world-reveal', `plot-level-${houseLevel}`]" filter="url(#sceneShadow)">
          <g v-if="houseLevel >= 1" class="plot-upgrade plot-flower-border" aria-hidden="true">
            <path d="M82 215q18 16 40 25m256-25q-17 16-38 25" />
            <circle cx="91" cy="220" r="4" /><circle cx="103" cy="230" r="3" /><circle cx="115" cy="237" r="4" />
            <circle cx="369" cy="222" r="4" /><circle cx="357" cy="231" r="3" /><circle cx="345" cy="238" r="4" />
          </g>
          <g v-if="houseLevel >= 2" class="plot-upgrade plot-path" aria-hidden="true">
            <ellipse cx="232" cy="274" rx="13" ry="6" /><ellipse cx="232" cy="291" rx="10" ry="5" />
          </g>
          <g v-if="houseLevel >= 3" class="plot-upgrade plot-garden-bed" aria-hidden="true">
            <path d="m319 246 42-18 13 13-42 19Z" />
            <path d="m327 245 32-13m-23 17 30-13" />
            <path class="plot-sprouts" d="M337 239q-7-8-10 1m10-1q4-9 10-3m5 7q-4-8-10-1m10 1q5-7 10-2" />
          </g>

          <g class="tree" :class="{ subdued: !hasEffect('garden') }" transform="translate(94 139)">
            <g class="tree-motion">
              <path d="m35 61 7 37-13 6-2-41z" fill="var(--lad-palette-orange-650)" />
              <g class="tree-crown"><circle cx="31" cy="35" fill="var(--lad-palette-teal-550)" r="27" /><circle cx="14" cy="48" fill="var(--lad-palette-mint-450)" r="21" /><circle cx="51" cy="49" fill="var(--lad-palette-mint-strong)" r="23" /><circle cx="35" cy="21" fill="var(--lad-palette-teal-400)" r="22" /></g>
            </g>
          </g>

          <g :key="view" class="house-view">
            <g v-if="view === 'front'" class="house house-front" :class="`house-level-${houseLevel}`">
              <ellipse class="toy-house-shadow" cx="232" cy="271" rx="125" ry="21" />
              <rect class="toy-house-body" x="137" :y="houseBodyY" width="188" :height="houseBodyHeight" rx="10" />
              <g v-if="houseLevel >= 1" class="house-addon toy-upper-floor">
                <rect x="163" y="87" width="136" height="69" rx="10" />
                <path d="M163 147h136" />
                <g class="toy-window upper-window"><rect x="205" y="101" width="52" height="37" rx="8" /><path d="M231 105v29m-22-15h44" /></g>
              </g>

              <g class="toy-roof">
                <path :d="roofPath" />
                <path class="roof-highlight" :d="roofHighlightPath" />
                <path class="roof-detail" :d="roofDetailPath" />
              </g>

              <g class="toy-door">
                <path class="doorway-back" d="M210 195q0-13 13-13h15q13 0 13 13v60h-41Z" />
                <g class="doorway-glimpse" clip-path="url(#frontDoorOpeningClip)">
                  <rect x="210" y="181" width="42" height="75" fill="var(--lad-palette-amber-150)" />
                  <path d="m211 232 40-7v31h-40Z" fill="var(--lad-palette-orange-350)" />
                  <path d="M231 182v31m-12 42 5-24m23 24-10-27" fill="none" stroke="var(--lad-palette-orange-500)" stroke-width="2" />
                  <path d="M218 218h25v12h-25Z" fill="var(--lad-palette-indigo-350)" stroke="var(--lad-palette-blue-600)" stroke-width="2" />
                  <circle cx="230" cy="202" r="7" fill="var(--lad-palette-amber-250)" opacity=".8" />
                  <path d="M230 181v15" stroke="var(--lad-palette-orange-650)" stroke-width="2" />
                </g>
                <g class="door-leaf">
                  <path class="door-panel" d="M210 195q0-13 13-13h15q13 0 13 13v60h-41Z" />
                  <g class="door-handle">
                    <circle class="door-knob" cx="241" cy="220" r="4" />
                    <path d="M241 220h8" />
                  </g>
                  <path class="door-panel-detail" d="M217 205h27v17h-27Zm0 25h27v18h-27Z" />
                </g>
                <path class="door-threshold" d="M203 255h55" />
              </g>
              <g class="toy-window left-window"><rect x="157" y="174" width="43" height="39" rx="9" /><path d="M178 178v31m-17-15h35" /></g>
              <g class="toy-window right-window"><rect x="263" y="174" width="43" height="39" rx="9" /><path d="M284 178v31m-17-15h35" /></g>
              <g class="front-porch-detail" aria-hidden="true"><path d="M194 185v67m73-67v67M188 185h85" /><circle cx="188" cy="183" r="5" /><circle cx="273" cy="183" r="5" /></g>

              <g v-if="houseLevel >= 2" class="house-addon toy-extension">
                <rect x="90" y="196" width="55" height="58" rx="10" />
                <path d="m80 199 38-32 38 32-9 12-29-23-29 23Z" />
                <rect x="104" y="211" width="28" height="25" rx="6" />
              </g>
              <g v-if="houseLevel >= 3" class="house-addon toy-tower">
                <rect x="303" y="112" width="54" height="143" rx="9" />
                <path d="m294 115 36-42 36 42-10 11-26-30-27 30Z" />
                <rect x="316" y="137" width="28" height="31" rx="8" /><rect x="316" y="187" width="28" height="31" rx="8" />
              </g>
              <g v-if="houseLevel >= 4" class="house-addon toy-balcony-front">
                <path d="M170 144h120v18H170Z" /><path d="M179 147v27m25-27v27m26-27v27m26-27v27m25-27v27" />
              </g>

              <g class="front-step"><path d="M195 255h71l15 15h-101Z" /><path d="M188 265h86l12 12H176Z" /></g>
              <g class="toy-bush bush-left"><circle cx="135" cy="242" r="19" /><circle cx="118" cy="250" r="15" /></g>
              <g class="toy-bush bush-right"><circle cx="327" cy="241" r="18" /><circle cx="343" cy="250" r="14" /></g>
              <g class="front-planters" aria-hidden="true"><path d="M151 216h54l-5 14h-44Zm105 0h54l-5 14h-44Z" /><circle cx="164" cy="214" r="5" /><circle cx="178" cy="212" r="6" /><circle cx="193" cy="214" r="5" /><circle cx="270" cy="214" r="5" /><circle cx="284" cy="212" r="6" /><circle cx="298" cy="214" r="5" /></g>
            </g>

          </g>

          <HouseThemeDecoration
            v-if="view === 'front'"
            :label="activeThemeLabel"
            :theme-id="activeThemeId"
          />

          <g v-if="view === 'front' && hasEffect('smoke')" class="smoke"><path d="m281 132 7-3v17l-7 3z" fill="var(--lad-palette-orange-600)" /><circle cx="288" cy="122" fill="var(--lad-palette-white)" opacity=".55" r="8" /><circle cx="296" cy="110" fill="var(--lad-palette-white)" opacity=".38" r="11" /></g>
          <g class="garden" :class="{ subdued: !hasEffect('flowers') }" transform="translate(137 244)"><ellipse fill="var(--lad-palette-teal-600)" rx="20" ry="8" /><circle cx="-9" cy="-8" fill="var(--lad-palette-coral)" r="6" /><circle cx="4" cy="-11" fill="var(--lad-palette-yellow)" r="6" /><circle cx="13" cy="-5" fill="var(--lad-palette-rose-250)" r="5" /></g>
        </g>
      </svg>

      <div v-if="view === 'front' && frontExterior" class="front-scene-art" aria-hidden="true">
        <img class="front-scene-art__background" :src="frontExterior.backgroundUrl" alt="">
        <i class="front-scene-sun" />
        <i class="front-scene-cloud front-scene-cloud--left" />
        <i class="front-scene-cloud front-scene-cloud--right" />
        <span class="front-scene-art__house">
          <img :src="frontExterior.houseUrl" alt="">
        </span>
      </div>

      <div
        v-if="view !== 'front'"
        :key="view"
        class="scene-world-shell"
        :class="{ 'garden-view': view === 'garden' }"
        @pointerdown.stop
      >
        <DollhouseInterior
          :accessories="accessories"
          compact
          contextual-neighbors
          :editable="canArrangeHouse"
          :energy="energy"
          include-garden
          :members="members"
          :pets="pets"
          :placements="houseLayout"
          :room-designs="roomDesigns"
          :rooms="rooms"
          :score="ladiScore"
          :selected-zone-id="selectedRoomView"
          :show-room-labels="false"
          :storage-open="storageOpen"
          :viewer-member-id="viewerMemberId"
          @drag-state="draggingEntityType = $event"
          @move="forwardEntityMove"
          @reset="forwardEntityReset"
          @select-zone="selectZone"
          @store="storeAccessory"
        />
      </div>

      <FurnitureStoragePanel
        v-if="view !== 'front'"
        v-model:open="storageOpen"
        :accessories="storageAccessories"
        :dragging="draggingEntityType === 'furniture'"
        @place="placeStoredAccessory"
      />

      <div v-if="view === 'front'" class="world-family" :class="{ 'guardian-active': activeFamilyMember?.role === 'guardian' }" :aria-label="t('world.scene.familyAria')">
        <div class="world-family-background" :aria-label="t('world.scene.backgroundFamilyAria')">
          <div v-for="group in backgroundMemberGroups" :key="group.side" :class="['world-family-side', `world-family-side--${group.side}`]">
            <div
              v-for="(member, index) in group.members"
              :key="member.id"
              :class="['world-family-member', 'is-background-member', memberRoleClass(member)]"
            >
              <Transition name="member-name">
                <span v-if="speakingMemberId === member.id" class="member-name-bubble" role="status">{{ worldMemberName(member) }}</span>
              </Transition>
              <AvatarFigure
                :appearance="appearanceFor(member, index)"
                calm
                full-body
                :size="backgroundMemberSize(member)"
                @interact="showMemberName(member.id)"
              />
            </div>
          </div>
        </div>
        <div v-if="activeFamilyMember" class="world-family-member is-active-member">
          <Transition name="member-name">
            <span v-if="speakingMemberId === activeFamilyMember.id" class="member-name-bubble" role="status">{{ worldMemberName(activeFamilyMember) }}</span>
          </Transition>
          <AvatarFigure
            :appearance="appearanceFor(activeFamilyMember, 0)"
            calm
            full-body
            :size="58"
            @interact="showMemberName(activeFamilyMember.id)"
          />
        </div>
      </div>

      <div v-if="view === 'front'" class="world-pets" :aria-label="t('world.scene.petsAria')">
        <div v-for="pet in pets" :key="pet.id" class="world-pet">
          <Transition name="member-name">
            <span v-if="activePetId === pet.id" class="pet-name-bubble" role="status">{{ pet.name }}</span>
          </Transition>
          <AnimatedPet :pet="pet" :size="48" @interact="showPetName(pet.id)" />
        </div>
      </div>
    </motion.div>

    <div class="scene-actions">
      <template v-if="view === 'front'">
        <button class="scene-action" :aria-label="t('world.scene.enterAria')" type="button" @click.stop="enterHouse">
          <v-icon icon="i-mdi:door-open" size="20" />
          <span><strong>{{ t('world.scene.enter') }}</strong><small>{{ t('world.scene.viewRooms') }}</small></span>
        </button>
      </template>
      <button v-else class="scene-action" :aria-label="t('world.scene.leaveAria')" type="button" @click.stop="leaveScene">
        <v-icon icon="i-mdi:arrow-left" size="20" />
        <span><strong>{{ t('world.scene.outside') }}</strong><small>{{ t('world.scene.frontGarden') }}</small></span>
      </button>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { motion } from "motion-v";
import { computed } from "vue";
import { useI18n } from "vue-i18n";

import AvatarFigure from "@/shared/components/avatar/AvatarFigure.vue";
import AnimatedPet from "@/shared/components/family/AnimatedPet.vue";
import DollhouseInterior from "./DollhouseInterior.vue";
import FurnitureStoragePanel from "./FurnitureStoragePanel.vue";
import HouseThemeDecoration from "./HouseThemeDecoration.vue";
import { useFamilyWorldScene } from "@/features/world/composables/use-family-world-scene";
import type { FamilyWorldSceneEmit, FamilyWorldSceneProps } from "@/features/world/composables/use-family-world-scene";
import type { FamilyMember } from "@/domain/family/types";

const props = defineProps<FamilyWorldSceneProps>();
const emit = defineEmits<FamilyWorldSceneEmit>();
const { t } = useI18n();
const {
  activeFamilyMember, activePetId, activeThemeId, activeThemeLabel, appearanceFor,
  ariaLabel, backgroundMemberGroups, draggingEntityType, enterHouse, forwardEntityMove,
  forwardEntityReset, frontExterior, hasEffect, houseEnergyClass, leaveScene, placeStoredAccessory,
  sceneStyle, selectedRoomView, selectZone, showMemberName, showPetName, speakingMemberId, storageAccessories,
  storageOpen, storeAccessory, sunOpacity, view, worldMemberName,
} = useFamilyWorldScene(props, emit);
const houseHasUpperFloor = computed(() => props.houseLevel >= 1);
const houseBodyY = computed(() => houseHasUpperFloor.value ? 145 : 151);
const houseBodyHeight = computed(() => houseHasUpperFloor.value ? 110 : 104);
const roofPath = computed(() => houseHasUpperFloor.value
  ? "M143 91 230 36l88 55-13 17-75-45-74 45Z"
  : "M112 155 230 78l119 77-15 18-104-66-103 66Z");
const roofHighlightPath = computed(() => houseHasUpperFloor.value
  ? "M230 36l88 55-7 9-81-49Z"
  : "M230 78l119 77-8 10-111-70Z");
const roofDetailPath = computed(() => houseHasUpperFloor.value
  ? "M170 82l60-33 61 34M184 89l46-25 47 25"
  : "M143 145l87-51 88 51M160 151l70-41 71 41");
const memberRoleClass = (member: FamilyMember) => member.role === "child" ? "is-child" : "is-guardian";
const backgroundMemberSize = (member: FamilyMember) => member.role === "child" ? 42 : 55;
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;
.world-scene-wrap {
  --world-furniture-overview-size: 60px;
  --world-furniture-room-size: 96px;
  --world-furniture-focused-size: 116px;
  --uno: w-100 position-relative overflow-hidden select-none;
  margin-top: 10px;
  padding-bottom: 8px;

  container: world-scene / inline-size;
  touch-action: pan-y;

}
.scene-world-shell :deep(.layout-entity.entity-furniture) {
  --world-entity-responsive-scale: 0.8;
}
.scene-world-shell :deep(.room-furniture--kitchen-counter) {
  transform: translateX(-16px);
}
.scene-world-shell :deep(.room-furniture--retro-fridge) {
  transform: translateX(16px);
}
@container world-scene (min-width: 42rem) {
  .scene-world-shell :deep(.layout-entity) {
    --world-entity-responsive-scale: 1.2;
  }
  .scene-world-shell :deep(.layout-entity.entity-furniture) {
    --world-entity-responsive-scale: 1.2;
  }
}
@container world-scene (min-width: 50rem) {
  .scene-world-shell :deep(.layout-entity) {
    --world-entity-responsive-scale: 1.35;
  }
  .scene-world-shell :deep(.layout-entity.entity-furniture) {
    --world-entity-responsive-scale: 1.35;
  }
}
.scene-drag-layer {
  --uno: position-relative;
  touch-action: pan-y;
}
.world-scene {
  --uno: w-100 overflow-visible;
  height: auto;

}
.front-scene-art {
  --uno: position-absolute overflow-hidden;
  inset: 0;
  z-index: 1;
  border-radius: 1rem 1rem 0.75rem 0.75rem;
  pointer-events: none;
}
.front-scene-art::after {
  content: "";
  --uno: position-absolute;
  inset: 0;
  z-index: 2;
  background:
    linear-gradient(
      180deg,
      var(--lad-palette-background) 0,
      color-mix(in srgb, var(--lad-palette-background) 78%, transparent) 5%,
      transparent 18% 76%,
      color-mix(in srgb, var(--lad-palette-amber-100) 94%, transparent) 100%
    ),
    linear-gradient(
      90deg,
      color-mix(in srgb, var(--lad-palette-background) 64%, transparent),
      transparent 12% 88%,
      color-mix(in srgb, var(--lad-palette-background) 58%, transparent)
    );
  box-shadow: inset 0 0 1.75rem
    color-mix(in srgb, var(--lad-palette-background) 32%, transparent);
}
.front-scene-art img {
  --uno: position-absolute w-100 h-100;
  inset: 0;
  object-fit: cover;
}
.front-scene-art__background {
  filter: saturate(var(--world-saturation)) brightness(var(--world-brightness));
}
.front-scene-art__house {
  width: 92%;
  aspect-ratio: 1.598;
  --uno: position-absolute;
  top: 8.5%;
  left: 50%;
  z-index: 2;
  transform: translateX(-50%);
  transform-origin: 50% 92%;
  will-change: transform;
  animation: exterior-house-idle 4.8s cubic-bezier(0.45, 0, 0.25, 1) infinite;
}
.front-scene-art__house::before {
  width: 68%;
  height: 7%;
  content: "";
  --uno: position-absolute;
  right: 16%;
  bottom: 6%;
  left: 16%;
  z-index: 0;
  border-radius: 50%;
  background: color-mix(in srgb, var(--lad-palette-muted-750) 22%, transparent);
  filter: blur(0.35rem);
}
.front-scene-art__house > img {
  z-index: 1;
  object-fit: contain;
}
.front-scene-sun {
  width: 3.4rem;
  aspect-ratio: 1;
  --uno: position-absolute;
  top: 4%;
  right: 10%;
  z-index: 1;
  border-radius: 50%;
  background: var(--lad-palette-amber-250);
  box-shadow:
    0 0 0 0.7rem
      color-mix(in srgb, var(--lad-palette-amber-250) 22%, transparent),
    0 0 1.4rem color-mix(in srgb, var(--lad-palette-yellow) 32%, transparent);
  animation: sun-glow 3.6s ease-in-out infinite;
}
.front-scene-sun::before {
  content: "";
  --uno: position-absolute;
  inset: -1.15rem;
  background: repeating-conic-gradient(
    from 0deg,
    var(--lad-palette-amber-250) 0 6deg,
    transparent 6deg 45deg
  );
  mask: radial-gradient(
    circle,
    transparent 0 46%,
    var(--lad-palette-black) 48% 57%,
    transparent 59%
  );
  animation: sun-rays-pulse 3.6s ease-in-out infinite;
}
.theme-halloween-night .front-scene-sun,
.theme-starlight-palace .front-scene-sun {
  background: var(--lad-palette-amber-100);
  box-shadow:
    inset -0.65rem -0.25rem 0
      color-mix(in srgb, var(--lad-palette-blue-350) 28%, transparent),
    0 0 1.4rem color-mix(in srgb, var(--lad-palette-white) 32%, transparent);
}
.theme-halloween-night .front-scene-sun::before,
.theme-starlight-palace .front-scene-sun::before {
  --uno: d-none;
}
.front-scene-cloud {
  width: 4.5rem;
  height: 1.25rem;
  --uno: position-absolute;
  z-index: 1;
  border-radius: 50%;
  background: color-mix(in srgb, var(--lad-palette-white) 86%, transparent);
  box-shadow:
    1.2rem -0.45rem 0 0.15rem
      color-mix(in srgb, var(--lad-palette-white) 86%, transparent),
    2.25rem 0 0 color-mix(in srgb, var(--lad-palette-white) 86%, transparent);
  opacity: 0.78;
  filter: blur(0.02rem);
  animation: front-cloud-drift 11s ease-in-out infinite alternate;
}
.front-scene-cloud--left {
  top: 21%;
  left: 6%;
}
.front-scene-cloud--right {
  top: 18%;
  right: 20%;
  animation-delay: -5.5s;
  animation-direction: alternate-reverse;
  transform: scale(0.78);
}
.world-scene-wrap.energy-tired .garden,
.world-scene-wrap.energy-tired .plot-flower-border {
  filter: saturate(0.82);
}
.world-scene-wrap.energy-low .garden,
.world-scene-wrap.energy-low .plot-flower-border,
.world-scene-wrap.energy-low .plot-garden-bed {
  filter: saturate(0.58) brightness(0.92);
}
.world-scene-wrap.energy-critical .garden,
.world-scene-wrap.energy-critical .plot-flower-border,
.world-scene-wrap.energy-critical .plot-garden-bed {
  filter: saturate(0.28) brightness(0.78);
}
.world-scene-wrap.energy-low .garden circle,
.world-scene-wrap.energy-low .plot-flower-border circle {
  transform-box: fill-box;
  transform-origin: bottom center;
  transform: translateY(4px) rotate(14deg);
}
.world-scene-wrap.energy-critical .garden circle,
.world-scene-wrap.energy-critical .plot-flower-border circle {
  transform-box: fill-box;
  transform-origin: bottom center;
  transform: translateY(8px) rotate(30deg) scaleY(0.78);
}
.scene-world-shell {
  --uno: position-absolute overflow-hidden;
  top: 19%;
  right: 0;
  bottom: 2%;
  left: 0;
  z-index: 5;
  pointer-events: auto;
  animation: enter-house 420ms cubic-bezier(0.18, 0.78, 0.22, 1) both;
  transition: right 0.2s ease;
}
.world-scene-wrap.is-storage-open .scene-world-shell {
  right: auto;
  width: calc(100% - 134px);
}
.world-scene-wrap.is-storage-open.is-dragging-furniture .scene-world-shell {
  --uno: overflow-visible;
  z-index: 30;
}
.world-scene-wrap.is-inside .scene-world-shell {
  top: 0;
  background: linear-gradient(
    180deg,
    var(--lad-palette-surface),
    var(--lad-palette-amber-100)
  );
}
.world-scene-wrap.is-garden .scene-world-shell {
  top: 0;
}
.world-scene-wrap.is-inside .scene-sun,
.world-scene-wrap.is-inside .cloud,
.world-scene-wrap.is-inside .rain-cloud,
.world-scene-wrap.is-inside .far-landscape,
.world-scene-wrap.is-inside .island {
  opacity: 0;
}
.scene-world-shell :deep(.dollhouse-layout) {
  width: 100%;
  height: 100%;
  min-height: 229px;
  --uno: pa-0;
  gap: 0;
  border: 0;
  border-radius: 0;
  background: transparent;
  box-shadow: none;
}
.scene-world-shell :deep(.contextual-zone .dollhouse-room) {
  min-height: 100%;
}
.scene-world-shell :deep(.dollhouse-room) {
  border: 0;
  border-radius: 0;
  box-shadow: none;
}
.scene-world-shell :deep(.compact.single-zone .dollhouse-room),
.scene-world-shell
  :deep(.compact.room-count-1:not(:has(.garden-zone)) .dollhouse-room),
.scene-world-shell :deep(.compact.single-zone .garden-zone) {
  min-height: 229px;
}
.scene-world-shell :deep(.compact:not(.single-zone) .entity-furniture) {
  width: var(--world-furniture-overview-size);
  height: var(--world-furniture-overview-size);
}
.scene-world-shell :deep(.compact:not(.single-zone) .entity-member) {
  width: 42px;
  height: 56px;
}
.scene-world-shell :deep(.compact:not(.single-zone) .entity-pet) {
  width: 40px;
  height: 40px;
}
.scene-world-shell :deep(.compact:not(.single-zone) .entity-ladi) {
  width: 52px;
  height: 52px;
}
.scene-world-shell :deep(.compact.single-zone .entity-furniture) {
  width: var(--world-furniture-room-size);
  height: var(--world-furniture-room-size);
}
.scene-world-shell :deep(.compact.single-zone .entity-member) {
  width: 61px;
  height: 82px;
}
.scene-world-shell :deep(.compact.single-zone .entity-pet) {
  width: 57px;
  height: 57px;
}
.scene-world-shell :deep(.compact.single-zone .entity-ladi) {
  width: 78px;
  height: 78px;
}
.scene-world-shell
  :deep(.compact.contextual-zone .dollhouse-room.is-focused .entity-furniture) {
  width: var(--world-furniture-focused-size);
  height: var(--world-furniture-focused-size);
}
.scene-world-shell
  :deep(.compact.contextual-zone .dollhouse-room.is-focused .entity-member) {
  width: 67px;
  height: 90px;
}
.scene-world-shell
  :deep(.compact.contextual-zone .dollhouse-room.is-focused .entity-pet) {
  width: 62px;
  height: 62px;
}
.scene-world-shell
  :deep(.compact.contextual-zone .dollhouse-room.is-focused .entity-ladi) {
  width: 86px;
  height: 86px;
}
.scene-world-shell.garden-view :deep(.dollhouse-layout),
.scene-world-shell.garden-view :deep(.garden-zone) {
  background: transparent;
}
.scene-world-shell.garden-view :deep(.garden-zone) {
  min-height: 229px;
}
.world-scene-wrap.is-storage-open .scene-actions {
  right: 134px;
}
.world-scene-wrap.is-inside .house-inside {
  opacity: 0;
}
.doorway-glimpse {
  opacity: 0.35;
}
.island {
  transform-origin: 230px 210px;
}
.world-reveal {
  animation: house-reveal 850ms var(--lad-easing-pop) both;
}
.plot-upgrade {
  animation: garden-grow 700ms var(--lad-easing-pop);
}
.plot-flower-border > path {
  fill: none;
  stroke: var(--lad-palette-teal-600);
  stroke-linecap: round;
  stroke-width: 3;
}
.plot-flower-border circle:nth-of-type(3n + 1) {
  fill: var(--lad-palette-red-300);
}
.plot-flower-border circle:nth-of-type(3n + 2) {
  fill: var(--lad-palette-yellow);
}
.plot-flower-border circle:nth-of-type(3n) {
  fill: var(--lad-palette-rose-250);
}
.plot-path ellipse {
  fill: var(--lad-palette-orange-350);
  stroke: color-mix(in srgb, var(--lad-palette-orange-600) 30%, transparent);
  stroke-width: 2;
}
.plot-garden-bed > path:first-child {
  fill: var(--lad-palette-orange-600);
  stroke: var(--lad-palette-orange-650);
  stroke-linejoin: round;
  stroke-width: 2;
}
.plot-garden-bed > path:nth-child(2) {
  fill: none;
  stroke: var(--lad-palette-orange-400-2);
  stroke-linecap: round;
  stroke-width: 2;
}
.plot-garden-bed .plot-sprouts {
  fill: none;
  stroke: var(--lad-palette-teal-550);
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-width: 3;
}
.house-view {
  transform-origin: 235px 205px;
  filter: saturate(var(--world-saturation)) brightness(var(--world-brightness));
  animation: house-turn 420ms cubic-bezier(0.2, 0.8, 0.2, 1);
  transition: filter 650ms ease;
}
.house {
  transform-box: fill-box;
  transform-origin: center bottom;
  will-change: transform;
  animation: house-idle 4.8s cubic-bezier(0.45, 0, 0.25, 1) infinite;
}
.dollhouse {
  filter: drop-shadow(
    0 8px 6px
      color-mix(in srgb, var(--lad-palette-muted-750-2) 15%, transparent)
  );
}
.room-wall {
  stroke: var(--house-trim);
  stroke-width: 7;
}
.room-side {
  stroke: var(--house-trim);
  stroke-linejoin: round;
  stroke-width: 6;
}
.room-floor {
  stroke: var(--house-trim);
  stroke-linejoin: round;
  stroke-width: 6;
}
.room-frame {
  fill: none;
  stroke: var(--house-trim);
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-width: 7;
}
.floorboards {
  fill: none;
  stroke: var(--lad-palette-orange-400-2);
  stroke-width: 2;
  opacity: 0.38;
}
.baseboard {
  fill: none;
  stroke: var(--lad-palette-orange-350);
  stroke-width: 7;
}
.wide-window path:not(.curtain) {
  fill: none;
  stroke: var(--lad-palette-white);
  stroke-width: 3;
}
.wide-window .curtain {
  fill: none;
  stroke: var(--lad-palette-rose-250);
  stroke-linecap: round;
  stroke-width: 7;
}
.starter-shelf rect:first-child {
  fill: var(--lad-palette-orange-500);
}
.starter-shelf circle {
  fill: var(--lad-palette-yellow);
}
.starter-shelf path {
  fill: var(--lad-palette-mint-450);
  stroke: var(--lad-palette-teal-700);
  stroke-width: 2;
}
.starter-shelf rect:last-child {
  fill: var(--lad-palette-indigo-350);
}
.bookshelf path {
  stroke: var(--lad-palette-yellow);
}
.floor-lamp circle {
  --uno: pointer-events-none;
}
.dollhouse-roof {
  transform-origin: 230px 75px;
  animation: roof-breathe 4s ease-in-out infinite;
}
.upper-dollhouse-room {
  stroke: var(--lad-palette-orange-500);
  stroke-linejoin: round;
  stroke-width: 6;
}
.upper-dollhouse-room > path:nth-child(2) {
  stroke: none;
}
.upper-room-toys {
  stroke: none;
}
.toy-house-shadow {
  fill: color-mix(in srgb, var(--lad-palette-muted-750-2) 20%, transparent);
}
.toy-house-body,
.toy-upper-floor rect:first-child {
  fill: var(--house-wall);
  stroke: var(--house-trim);
  stroke-width: 7;
}
.toy-upper-floor rect:first-child {
  fill: var(--house-wall-upper);
}
.toy-upper-floor > path {
  fill: none;
  stroke: var(--lad-palette-orange-350);
  stroke-width: 6;
}
.toy-roof path:first-child,
.toy-side-roof {
  fill: var(--house-roof);
  stroke: var(--house-trim);
  stroke-linejoin: round;
  stroke-width: 6;
}
.toy-roof .roof-highlight {
  fill: var(--house-roof-shade);
  stroke: none;
}
.toy-roof .roof-detail {
  fill: none;
  stroke: color-mix(in srgb, var(--lad-palette-white) 35%, transparent);
  stroke-linecap: round;
  stroke-width: 3;
}
.toy-door .doorway-back {
  fill: var(--lad-palette-orange-650);
  stroke: var(--house-trim);
  stroke-width: 5;
}
.toy-door .door-panel,
.side-door path:first-child {
  fill: var(--house-door);
  stroke: var(--house-trim);
  stroke-width: 5;
}
.toy-door .door-knob,
.side-door circle {
  fill: var(--lad-palette-yellow);
  stroke: var(--lad-palette-amber-650);
  stroke-width: 2;
}
.toy-door .door-handle path {
  fill: none;
  stroke: var(--lad-palette-amber-650);
  stroke-linecap: round;
  stroke-width: 3;
}
.toy-door .door-panel-detail {
  fill: none;
  stroke: color-mix(in srgb, var(--lad-palette-white) 25%, transparent);
  stroke-linejoin: round;
  stroke-width: 2;
}
.toy-door .door-threshold {
  fill: none;
  stroke: var(--house-trim);
  stroke-linecap: round;
  stroke-width: 8;
}
.toy-window rect,
.side-sunroom rect {
  fill: var(--house-window);
  stroke: var(--lad-palette-white);
  stroke-width: 6;
}
.toy-window path {
  fill: none;
  stroke: var(--lad-palette-white);
  stroke-width: 3;
}
.front-step path {
  fill: var(--lad-palette-orange-350);
  stroke: var(--lad-palette-orange-600);
  stroke-linejoin: round;
  stroke-width: 3;
}
.toy-bush {
  fill: var(--lad-palette-mint-450);
  stroke: var(--lad-palette-teal-600);
  stroke-width: 3;
}
.toy-extension rect:first-child,
.toy-tower rect:first-child,
.side-upper rect:first-child,
.side-sunroom rect:first-child {
  fill: var(--house-wall-upper);
  stroke: var(--house-trim);
  stroke-width: 5;
}
.toy-extension > path,
.toy-tower > path {
  fill: var(--house-roof);
  stroke: var(--house-trim);
  stroke-linejoin: round;
  stroke-width: 4;
}
.toy-extension rect:last-child,
.toy-tower rect:not(:first-child) {
  fill: var(--lad-palette-blue-250);
  stroke: var(--lad-palette-white);
  stroke-width: 4;
}
.toy-balcony-front {
  fill: var(--lad-palette-amber-250);
  stroke: var(--lad-palette-orange-500);
  stroke-width: 4;
}
.toy-balcony-front path:not(:first-child) {
  fill: none;
}
.front-porch-detail {
  fill: var(--house-trim);
}
.front-porch-detail path {
  fill: none;
  stroke: var(--house-trim);
  stroke-linecap: round;
  stroke-width: 4;
  opacity: 0.72;
}
.front-planters path {
  fill: var(--lad-palette-orange-600);
  stroke: var(--lad-palette-orange-650);
  stroke-width: 2;
}
.front-planters circle {
  fill: var(--landscape-accent);
  stroke: color-mix(in srgb, var(--lad-palette-white) 65%, transparent);
  stroke-width: 1.5;
}
.toy-side-body {
  fill: var(--house-wall);
  stroke: var(--house-trim);
  stroke-linejoin: round;
  stroke-width: 7;
}
.toy-side-wall {
  fill: var(--lad-palette-amber-200);
  stroke: var(--house-trim);
  stroke-linejoin: round;
  stroke-width: 6;
}
.side-upper .toy-window rect {
  stroke-width: 4;
}
.side-sunroom > path {
  fill: none;
  stroke: var(--house-roof);
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-width: 11;
}
.side-deck {
  fill: var(--lad-palette-orange-350);
  stroke: var(--lad-palette-orange-600);
  stroke-width: 4;
}
.side-pool ellipse:first-child {
  fill: var(--lad-palette-amber-250);
  stroke: var(--lad-palette-orange-600);
  stroke-width: 4;
}
.side-pool ellipse:last-child {
  fill: var(--lad-palette-blue-250);
  stroke: none;
}
.side-detail path {
  fill: none;
  stroke: var(--house-trim);
  stroke-linecap: round;
  stroke-width: 3;
  opacity: 0.5;
}
.side-detail circle {
  fill: var(--landscape-accent);
}
.exterior-pumpkin-arch,
.exterior-bat-garland,
.exterior-candy-bushes,
.exterior-candy-fence {
  pointer-events: none;
}
.theme-halloween-night .far-landscape {
  filter: hue-rotate(34deg) saturate(0.7) brightness(0.82);
}
.theme-halloween-night .scene-sun {
  filter: grayscale(0.7);
}
.theme-halloween-night .toy-window rect {
  filter: drop-shadow(0 0 7px var(--lad-palette-yellow));
}
.theme-cotton-candy-dream .far-landscape {
  filter: hue-rotate(310deg) saturate(0.75) brightness(1.08);
}
.theme-cotton-candy-dream .toy-roof path:first-child {
  fill: url(#cottonRoofPattern);
}
.theme-cotton-candy-dream .toy-house-body {
  filter: drop-shadow(
    0 0 7px color-mix(in srgb, var(--lad-palette-white) 80%, transparent)
  );
}
.theme-cotton-candy-dream .toy-window rect {
  filter: drop-shadow(
    0 0 5px color-mix(in srgb, var(--lad-palette-amber-250) 75%, transparent)
  );
}
.theme-starlight-palace .far-landscape {
  filter: hue-rotate(12deg) saturate(0.75) brightness(1.04);
}
.theme-starlight-palace .toy-roof path:first-child {
  fill: var(--lad-palette-blue-550);
  stroke: var(--lad-palette-indigo-750);
}
.theme-starlight-palace .toy-house-body {
  fill: var(--lad-palette-surface);
  stroke: var(--lad-palette-amber-550);
  filter: drop-shadow(
    0 0 8px color-mix(in srgb, var(--lad-palette-yellow) 30%, transparent)
  );
}
.theme-starlight-palace .toy-window rect {
  fill: var(--lad-palette-amber-250);
  filter: drop-shadow(
    0 0 6px color-mix(in srgb, var(--lad-palette-yellow) 80%, transparent)
  );
}
.exterior-pumpkin-arch path {
  fill: none;
  stroke: var(--lad-palette-muted-600-2);
  stroke-width: 7;
}
.exterior-pumpkin-arch circle {
  fill: var(--lad-palette-orange-400);
  stroke: var(--lad-palette-orange-650);
  stroke-width: 2;
}
.exterior-bat-garland > path:first-child {
  fill: none;
  stroke: var(--lad-palette-muted-750-2);
  stroke-width: 2;
}
.exterior-bat-garland > path:last-child {
  fill: var(--lad-palette-muted-750-2);
}
.exterior-candy-bushes {
  fill: var(--lad-palette-rose-200);
  stroke: var(--lad-palette-white);
  stroke-width: 2;
}
.exterior-candy-bushes circle:nth-child(n + 3) {
  fill: var(--lad-palette-blue-250);
}
.exterior-candy-fence {
  fill: none;
  stroke: var(--lad-palette-rose-250);
  stroke-dasharray: 6 4;
  stroke-linecap: round;
  stroke-width: 6;
}
.world-family {
  width: 215px;
  height: 108px;
  --uno: position-absolute left-0 d-grid place-end-center pointer-events-none;
  bottom: 3.5%;
  z-index: 3;

  filter: drop-shadow(
    0 6px 5px color-mix(in srgb, var(--lad-palette-muted-750) 15%, transparent)
  );
}
.world-family::before {
  content: "";
  width: 145px;
  height: 20px;
  --uno: position-absolute;
  left: 50%;
  bottom: -2px;
  z-index: 0;
  transform: translateX(-50%);
  border-radius: 50%;
  background: color-mix(in srgb, var(--lad-palette-muted-700) 12%, transparent);
  filter: blur(2px);
}
.world-family.inside {
  bottom: 4.5%;
}
.world-family-background {
  --uno: w-100 d-flex align-end justify-space-between;
  grid-area: 1 / 1;
  z-index: 1;

  transform: translateY(-17px);
}
.world-family-side {
  width: 86px;
  --uno: d-flex align-end justify-center;
}
.world-family-side .world-family-member {
  margin-inline: -5px;
}
.world-family-side .world-family-member.is-guardian {
  margin-inline: -7px;
}
.world-family-member {
  --uno: position-relative;
  z-index: 2;
  margin-inline: -8px;
  transform-origin: center bottom;
}
.world-family-member :deep(.avatar-figure) {
  pointer-events: auto;
  --uno: cursor-pointer;
}
.member-name-bubble,
.pet-name-bubble {
  min-width: max-content;
  padding: 5px 8px;
  --uno: position-absolute font-weight-black pointer-events-none;
  left: 50%;
  bottom: calc(100% + 4px);
  z-index: 7;
  transform: translateX(-50%);
  color: var(--lad-palette-muted-700);
  border: 2px solid
    color-mix(in srgb, var(--lad-palette-white) 95%, transparent);
  border-radius: 11px;
  background: var(--lad-palette-surface);
  box-shadow: 0 4px 10px
    color-mix(in srgb, var(--lad-palette-muted-750-2) 18%, transparent);
  font-size: rem(9);

  line-height: 1;

}
.member-name-bubble::after,
.pet-name-bubble::after {
  content: "";
  width: 8px;
  height: 8px;
  --uno: position-absolute;
  left: 50%;
  bottom: -5px;
  transform: translateX(-50%) rotate(45deg);
  border-right: 2px solid
    color-mix(in srgb, var(--lad-palette-white) 95%, transparent);
  border-bottom: 2px solid
    color-mix(in srgb, var(--lad-palette-white) 95%, transparent);
  background: var(--lad-palette-surface);
}
.member-name-enter-active,
.member-name-leave-active {
  transition:
    opacity 160ms ease,
    transform 160ms ease;
}
.member-name-enter-from,
.member-name-leave-to {
  opacity: 0;
  transform: translateX(-50%) translateY(5px) scale(0.9);
}
.world-family-member.is-guardian {
  margin-inline: -12px;
  opacity: 0.98;
}
.world-family-member.is-child {
  translate: 0 3px;
}
.world-family-member.is-background-member {
  z-index: 1;
  opacity: 1;
}
.world-family > .world-family-member.is-active-member {
  grid-area: 1 / 1;
  z-index: 6;
  --uno: ma-0;
  translate: 0 8px;
  opacity: 1;
}
.world-family-member:nth-child(2) {
  animation-delay: -1.1s;
}
.world-family-member:nth-child(3) {
  animation-delay: -2.2s;
}
.world-family-member:nth-child(4) {
  animation-delay: -3.3s;
}
.world-family-member:nth-child(5) {
  animation-delay: -4.4s;
}
.world-pets {
  width: 108px;
  --uno: position-absolute d-flex align-end justify-center pointer-events-none;
  right: 3%;
  bottom: 13%;
  z-index: 4;

  gap: 2px;

}
.world-pets.inside {
  right: 3%;
  bottom: 13%;
}
.world-pet {
  --uno: position-relative d-flex align-end;
  z-index: 1;

  pointer-events: auto;
}
.world-pet:nth-child(2) :deep(.animated-pet) {
  animation-delay: -1.7s;
}
.world-pets :deep(.animated-pet) {
  pointer-events: auto;
  --uno: cursor-pointer;
}
.far-landscape {
  opacity: 0.92;
  transition: opacity 650ms ease;
}
.meadow-ground {
  fill: url(#meadowGround);
}
.meadow-highlight {
  fill: none;
  stroke: color-mix(in srgb, var(--lad-palette-white) 25%, transparent);
  stroke-linecap: round;
  stroke-width: 3;
}
.mountains {
  transform-box: fill-box;
  transform-origin: center bottom;
  will-change: transform;
  animation: mountain-breathe 15s ease-in-out infinite;
}
.mountain-layer {
  fill: var(--lad-palette-muted-250);
}
.mountain-snow {
  fill: var(--lad-palette-background);
  opacity: 0.82;
}
.hill-layer {
  transform-box: fill-box;
  transform-origin: center bottom;
}
.hill-back {
  fill: var(--lad-palette-green-250);
  animation: hill-drift-back 15s ease-in-out infinite alternate;
}
.hill-front {
  fill: var(--lad-palette-mint-450);
  animation: hill-drift-front 12s ease-in-out infinite alternate;
}
.hill-layer,
.distant-pines,
.scene-sun,
.cloud {
  will-change: transform;
}
.distant-pines {
  fill: var(--lad-palette-mint-450);
  opacity: 0.72;
  transform-box: fill-box;
  transform-origin: center bottom;
  animation: pine-breeze 7s ease-in-out infinite;
}
.scene-sun {
  transform-origin: 375px 48px;
  animation: sun-smile 8s ease-in-out infinite;
  transition: opacity 650ms ease;
}
.sun-halo {
  fill: var(--lad-palette-amber-150);
  opacity: 0.35;
}
.sun-core {
  fill: var(--lad-palette-yellow);
  transform-box: fill-box;
  transform-origin: center;
  animation: sun-glow 3.6s ease-in-out infinite;
}
.sun-rays {
  fill: none;
  stroke: var(--lad-palette-yellow);
  stroke-linecap: round;
  stroke-width: 5;
  transform-box: fill-box;
  transform-origin: center;
  animation: sun-rays-pulse 3.6s ease-in-out infinite;
}
.cloud {
  fill: var(--lad-palette-white);
  opacity: 0.65;
  transition:
    fill 650ms ease,
    opacity 650ms ease;
}
.cloud.heavy {
  fill: var(--lad-palette-muted-250);
  opacity: 0.92;
}
.cloud-a {
  animation: cloud-drift 8s ease-in-out infinite alternate;
}
.cloud-b {
  animation: cloud-drift 10s ease-in-out infinite alternate-reverse;
}
.rain-cloud {
  fill: var(--lad-palette-muted-350);
  animation: rain-cloud-drift 4s ease-in-out infinite alternate;
}
.rain-drops {
  fill: none;
  stroke: var(--lad-palette-blue-350);
  stroke-dasharray: 7 8;
  stroke-linecap: round;
  stroke-width: 5;
  animation: rain-fall 750ms linear infinite;
}
.tree-crown {
  transform-box: fill-box;
  transform-origin: center bottom;
  animation: tree-breathe 3.8s ease-in-out infinite;
}
.tree-motion {
  transform-box: fill-box;
  transform-origin: center bottom;
  will-change: transform;
  animation: tree-sway 6.7s -1.3s ease-in-out infinite;
}
.tree,
.garden {
  transition:
    opacity 400ms ease,
    filter 400ms ease;
}
.subdued {
  opacity: 0.42;
  filter: saturate(0.45);
}
.window {
  transition: fill 450ms ease;
}
.smoke {
  animation: smoke-rise 2.6s ease-in-out infinite;
}
.accessory,
.interior-item {
  animation: accessory-pop 450ms var(--lad-easing-pop);
}
.house-addon {
  animation: accessory-pop 650ms var(--lad-easing-pop);
}
.scene-actions {
  --uno: position-absolute d-flex justify-center;
  right: 7px;
  bottom: 3px;
  left: 7px;
  z-index: 8;
  gap: 7px;
  pointer-events: none;
}
.scene-action {
  min-width: 104px;
  height: 42px;
  padding: 5px 11px;
  --uno: d-flex align-center justify-center cursor-pointer;
  gap: 7px;
  color: var(--lad-palette-muted-700);
  border: 1px solid
    color-mix(in srgb, var(--lad-palette-muted-700) 15%, transparent);
  border-radius: 15px;
  background: color-mix(
    in srgb,
    var(--lad-palette-background) 95%,
    transparent
  );
  box-shadow:
    0 4px 0 var(--lad-palette-teal-150),
    0 8px 18px color-mix(in srgb, var(--lad-palette-text) 15%, transparent);

  font: inherit;
  pointer-events: auto;
  backdrop-filter: blur(10px);
  transition:
    transform 150ms ease,
    box-shadow 150ms ease;
}
.scene-action svg {
  width: 19px;
  height: 19px;
  fill: none;
  stroke: currentColor;
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-width: 2.3;
}
.scene-action span,
.scene-action strong,
.scene-action small {
  --uno: d-block;
}
.scene-action span {
  --uno: text-left;
  line-height: 1.05;
}
.scene-action strong {
  font-size: rem(10);
}
.scene-action small {
  margin-top: 2px;
  color: var(--lad-palette-muted);
  font-size: 0.5rem;
}
.scene-action:active {
  transform: translateY(2px);
  box-shadow: 0 2px 0 var(--lad-palette-teal-150);
}
.scene-action:focus-visible {
  @include focus-ring(
    color-mix(in srgb, var(--lad-palette-teal-550) 30%, transparent)
  );
}
.scene-action:disabled {
  opacity: 0.76;
  cursor: wait;
}

@keyframes garden-grow {
  from {
    opacity: 0;
    transform: translateY(8px) scale(0.75);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}
@keyframes cloud-drift {
  0% {
    transform: translate(-10px, 2px);
  }
  45% {
    transform: translate(2px, -4px);
  }
  100% {
    transform: translate(15px, 1px);
  }
}
@keyframes sun-smile {
  0%,
  100% {
    transform: translateY(2px) rotate(-3deg) scale(0.97);
  }
  50% {
    transform: translateY(-5px) rotate(4deg) scale(1.06);
  }
}
@keyframes sun-glow {
  0%,
  100% {
    filter: drop-shadow(0 0 0 transparent);
    transform: scale(0.98);
  }
  50% {
    filter: drop-shadow(
      0 0 9px color-mix(in srgb, var(--lad-palette-yellow) 70%, transparent)
    );
    transform: scale(1.05);
  }
}
@keyframes sun-rays-pulse {
  0%,
  100% {
    opacity: 0.65;
    transform: rotate(-3deg) scale(0.92);
  }
  50% {
    opacity: 1;
    transform: rotate(4deg) scale(1.08);
  }
}
@keyframes mountain-breathe {
  0%,
  100% {
    transform: translateY(1px);
  }
  50% {
    transform: translateY(-2px);
  }
}
@keyframes hill-drift-back {
  from {
    transform: translateX(-4px);
  }
  to {
    transform: translateX(5px);
  }
}
@keyframes hill-drift-front {
  from {
    transform: translateX(5px);
  }
  to {
    transform: translateX(-4px);
  }
}
@keyframes pine-breeze {
  0%,
  100% {
    transform: skewX(-0.5deg);
  }
  50% {
    transform: skewX(1.5deg);
  }
}
@keyframes rain-cloud-drift {
  from {
    transform: translateX(-7px);
  }
  to {
    transform: translateX(8px);
  }
}
@keyframes rain-fall {
  to {
    stroke-dashoffset: -15;
  }
}
@keyframes tree-breathe {
  0%,
  100% {
    transform: rotate(-1.5deg);
  }
  50% {
    transform: rotate(2deg);
  }
}
@keyframes tree-sway {
  0%,
  100% {
    transform: rotate(-1.1deg) translateY(0);
  }
  50% {
    transform: rotate(1.4deg) translateY(-1px);
  }
}
@keyframes smoke-rise {
  0%,
  100% {
    transform: translateY(4px);
    opacity: 0.5;
  }
  50% {
    transform: translateY(-5px);
    opacity: 0.85;
  }
}
@keyframes accessory-pop {
  from {
    opacity: 0;
    transform: scale(0.4);
  }
  to {
    opacity: 1;
    transform: scale(1);
  }
}
@keyframes enter-house {
  from {
    opacity: 0.3;
    transform: scale(0.82) translateY(18px);
  }
  to {
    opacity: 1;
    transform: scale(1) translateY(0);
  }
}
@keyframes house-turn {
  from {
    opacity: 0.25;
    transform: scaleX(0.2) translateY(5px);
  }
  to {
    opacity: 1;
    transform: scaleX(1) translateY(0);
  }
}
@keyframes house-idle {
  0%,
  100% {
    transform: translateY(1px) rotate(-0.2deg) scale(1, 0.998);
  }
  38% {
    transform: translateY(-6px) rotate(0.45deg) scale(1.008, 0.992);
  }
  62% {
    transform: translateY(-3px) rotate(-0.3deg) scale(0.997, 1.004);
  }
  78% {
    transform: translateY(-5px) rotate(0.18deg) scale(1.003, 0.997);
  }
}
@keyframes exterior-house-idle {
  0%,
  100% {
    transform: translateX(-50%) translateY(0.08rem) rotate(-0.12deg)
      scale(1, 0.998);
  }
  42% {
    transform: translateX(-50%) translateY(-0.28rem) rotate(0.2deg)
      scale(1.005, 0.995);
  }
  72% {
    transform: translateX(-50%) translateY(-0.15rem) rotate(-0.12deg)
      scale(0.998, 1.003);
  }
}
@keyframes front-cloud-drift {
  from {
    translate: -0.75rem 0;
  }
  to {
    translate: 1rem -0.2rem;
  }
}
@keyframes house-reveal {
  0% {
    opacity: 0;
    transform: translateY(24px) scale(0.58) rotate(-4deg);
  }
  65% {
    opacity: 1;
    transform: translateY(-9px) scale(1.07) rotate(1deg);
  }
  100% {
    transform: translateY(3px) scale(1);
  }
}
@keyframes roof-breathe {
  0%,
  100% {
    transform: translateY(0);
  }
  50% {
    transform: translateY(-2px);
  }
}

@include reduced-motion {
  .world-reveal,
  .plot-upgrade,
  .house-view,
  .house,
  .front-scene-art__house,
  .front-scene-cloud,
  .front-scene-sun,
  .front-scene-sun::before,
  .scene-sun,
  .sun-core,
  .sun-rays,
  .cloud,
  .mountains,
  .hill-layer,
  .distant-pines,
  .rain-cloud,
  .rain-drops,
  .tree-motion,
  .tree-crown,
  .smoke,
  .palace-lights circle,
  .palace-crown,
  .palace-stars path,
  .accessory,
  .interior-item,
  .house-addon,
  .dollhouse-roof,
  .interior-family :deep(.avatar-figure) {
    animation: none;
  }
}
</style>
