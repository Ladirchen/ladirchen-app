<template>
  <div class="world-scene-wrap" :class="{ 'is-inside': view === 'inside', 'is-garden': view === 'garden', 'is-entering': entering, 'is-storage-open': storageOpen, 'is-dragging-furniture': draggingEntityType === 'furniture' }" :style="sceneStyle">
    <motion.div
      class="scene-drag-layer"
      :drag="false"
      :drag-constraints="{ left: 0, right: 0 }"
      :drag-elastic=".16"
      :drag-momentum="false"
      :transition="{ type: 'spring', stiffness: 390, damping: 30 }"
    >
      <PixiWorldFoundation :energy="energy" :view="view" />
      <svg class="world-scene" :class="`theme-${activeThemeId}`" viewBox="10 8 440 325" role="img" :aria-label="ariaLabel">
        <defs>
          <linearGradient id="meadowGround" x1="0" x2="0" y1="0" y2="1">
            <stop offset="0" stop-color="#8fca86" />
            <stop offset=".48" stop-color="#b7dda0" />
            <stop offset="1" stop-color="#eef3d7" />
          </linearGradient>
          <linearGradient id="insideFloor" x1="0" x2="1" y1="0" y2="1">
            <stop offset="0" stop-color="#f8d89b" />
            <stop offset="1" stop-color="#e7ae68" />
          </linearGradient>
          <linearGradient id="insideWall" x1="0" x2="0" y1="0" y2="1">
            <stop offset="0" stop-color="#fff8df" />
            <stop offset="1" stop-color="#f8e7bd" />
          </linearGradient>
          <pattern id="cottonRoofPattern" width="24" height="24" patternUnits="userSpaceOnUse" patternTransform="rotate(-8)">
            <rect width="24" height="24" fill="#f5a8ce" />
            <path d="M-6 6 6-6m0 24L30-6M18 30 30 18" stroke="#fff1fa" stroke-width="8" />
            <circle cx="7" cy="7" r="2.5" fill="#8bd8ec" />
          </pattern>
          <filter id="sceneShadow" height="160%" width="160%" x="-30%" y="-30%">
            <feDropShadow dx="0" dy="12" flood-color="#335443" flood-opacity=".2" stdDeviation="8" />
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
              <path d="m35 61 7 37-13 6-2-41z" fill="#76533e" />
              <g class="tree-crown"><circle cx="31" cy="35" fill="#3d9f66" r="27" /><circle cx="14" cy="48" fill="#5bbd79" r="21" /><circle cx="51" cy="49" fill="#2f8a59" r="23" /><circle cx="35" cy="21" fill="#70cf88" r="22" /></g>
            </g>
          </g>

          <g :key="view" class="house-view">
            <g v-if="view === 'front'" class="house house-front" :class="`house-level-${houseLevel}`">
              <ellipse class="toy-house-shadow" cx="232" cy="271" rx="125" ry="21" />
              <rect class="toy-house-body" x="137" :y="houseLevel >= 1 ? 145 : 151" width="188" :height="houseLevel >= 1 ? 110 : 104" rx="10" />
              <g v-if="houseLevel >= 1" class="house-addon toy-upper-floor">
                <rect x="163" y="87" width="136" height="69" rx="10" />
                <path d="M163 147h136" />
                <g class="toy-window upper-window"><rect x="205" y="101" width="52" height="37" rx="8" /><path d="M231 105v29m-22-15h44" /></g>
              </g>

              <g class="toy-roof">
                <path :d="houseLevel >= 1 ? 'M143 91 230 36l88 55-13 17-75-45-74 45Z' : 'M112 155 230 78l119 77-15 18-104-66-103 66Z'" />
                <path class="roof-highlight" :d="houseLevel >= 1 ? 'M230 36l88 55-7 9-81-49Z' : 'M230 78l119 77-8 10-111-70Z'" />
                <path class="roof-detail" :d="houseLevel >= 1 ? 'M170 82l60-33 61 34M184 89l46-25 47 25' : 'M143 145l87-51 88 51M160 151l70-41 71 41'" />
              </g>

              <g class="toy-door">
                <path class="doorway-back" d="M210 195q0-13 13-13h15q13 0 13 13v60h-41Z" />
                <g class="doorway-glimpse" clip-path="url(#frontDoorOpeningClip)">
                  <rect x="210" y="181" width="42" height="75" fill="#f9e7bd" />
                  <path d="m211 232 40-7v31h-40Z" fill="#d79f6d" />
                  <path d="M231 182v31m-12 42 5-24m23 24-10-27" fill="none" stroke="#b47a55" stroke-width="2" />
                  <path d="M218 218h25v12h-25Z" fill="#73a4d6" stroke="#446e96" stroke-width="2" />
                  <circle cx="230" cy="202" r="7" fill="#ffe47c" opacity=".8" />
                  <path d="M230 181v15" stroke="#725844" stroke-width="2" />
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

          <g v-if="view === 'front' && activeThemeId === 'halloween-night'" class="edition-decor halloween-edition" aria-label="Halloween-Nacht Hausedition">
            <path class="bat bat-one" d="M155 111q8-10 16 0 8-10 16 0-8-2-16 10-8-12-16-10Z" /><path class="bat bat-two" d="M317 105q6-8 13 0 6-8 13 0-7-1-13 9-6-10-13-9Z" />
            <circle class="pumpkin" cx="191" cy="255" r="11" /><circle class="pumpkin" cx="275" cy="256" r="10" /><path class="pumpkin-face" d="m186 252 3 3 3-3m-5 8q4 3 8 0m75-8 3 3 3-3m-5 8q4 3 8 0" />
            <path class="spooky-vine" d="M144 213q-20-22 0-43m174 44q20-24 2-45" />
          </g>
          <g v-if="view === 'front' && activeThemeId === 'cotton-candy-dream'" class="edition-decor candy-edition" aria-label="Zuckerwatte-Traum Hausedition">
            <g class="candy-cloud candy-left"><circle cx="119" cy="238" r="17" /><circle cx="135" cy="229" r="20" /><circle cx="151" cy="240" r="16" /></g>
            <g class="candy-cloud candy-right"><circle cx="314" cy="238" r="17" /><circle cx="331" cy="228" r="20" /><circle cx="348" cy="240" r="16" /></g>
            <path class="candy-swirl" d="M217 122q25-23 38 0-8 17-27 6 4-10 14-6" />
            <path class="lollipop" d="M112 208v35m-10-38q10-17 20 0-10 17-20 0Z" />
            <path class="lollipop lollipop-blue" d="M352 206v38m-10-40q10-17 20 0-10 17-20 0Z" />
            <g class="wrapped-candies"><path d="m169 191-9-5v10Zm18 0 9-5v10Z" /><rect x="169" y="184" width="18" height="14" rx="7" /><path d="m273 178-8-5v10Zm17 0 8-5v10Z" /><rect x="273" y="171" width="17" height="14" rx="7" /></g>
            <g class="candy-sprinkles"><circle cx="177" cy="121" r="3" /><circle cx="289" cy="133" r="3" /><circle cx="309" cy="157" r="2.5" /><circle cx="154" cy="153" r="2.5" /></g>
          </g>

          <g v-if="view === 'front' && hasEffect('smoke')" class="smoke"><path d="m281 132 7-3v17l-7 3z" fill="#8d5d49" /><circle cx="288" cy="122" fill="#fff" opacity=".55" r="8" /><circle cx="296" cy="110" fill="#fff" opacity=".38" r="11" /></g>
          <g class="garden" :class="{ subdued: !hasEffect('flowers') }" transform="translate(137 244)"><ellipse fill="#438c58" rx="20" ry="8" /><circle cx="-9" cy="-8" fill="#ff8378" r="6" /><circle cx="4" cy="-11" fill="#ffd161" r="6" /><circle cx="13" cy="-5" fill="#f5a0b9" r="5" /></g>
          <g v-if="energy >= 85 && energy < 100" class="sparkles" fill="#fff4a2">
            <path d="m330 154 4 10 10 4-10 4-4 10-4-10-10-4 10-4z" /><path d="m353 179 3 7 7 3-7 3-3 7-3-7-7-3 7-3z" /><path d="m115 181 3 8 8 3-8 3-3 8-3-8-8-3 8-3Z" />
          </g>
          <g v-if="energy === 100" class="perfect-shimmer" aria-label="Glitzereffekt für 100 Prozent Energie">
            <path class="shimmer-star shimmer-one" d="m352 125 5 13 13 5-13 5-5 13-5-13-13-5 13-5Z" />
            <path class="shimmer-star shimmer-two" d="m397 172 3 8 8 3-8 3-3 8-3-8-8-3 8-3Z" />
            <path class="shimmer-star shimmer-three" d="m111 172 4 10 10 4-10 4-4 10-4-10-10-4 10-4Z" />
            <path class="shimmer-star shimmer-four" d="m176 104 3 7 7 3-7 3-3 7-3-7-7-3 7-3Z" />
            <path class="shimmer-star shimmer-five" d="m289 226 3 8 8 3-8 3-3 8-3-8-8-3 8-3Z" />
            <path class="shimmer-sweep" d="M138 139c54-47 146-62 204-22" />
          </g>

        </g>
      </svg>

      <div v-if="view === 'front'" class="front-garden-items" aria-label="Gartenausstattung im Vorgarten">
        <HouseLayoutEntity
          v-for="placement in frontGardenPlacements"
          :key="placement.id"
          :accessory="accessoryFor(placement)"
          :editable="false"
          :placement="placement"
          :score="ladiScore"
        />
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
          include-garden
          :members="members"
          :pets="pets"
          :placements="houseLayout"
          :rooms="rooms"
          :score="ladiScore"
          :selected-zone-id="selectedRoomView"
          :show-room-labels="false"
          :storage-open="storageOpen"
          @drag-state="draggingEntityType = $event"
          @move="forwardEntityMove"
          @reset="forwardEntityReset"
          @select-zone="selectZone"
          @store="storeAccessory"
        />
      </div>

      <button
        v-if="view !== 'front' && !storageOpen"
        class="furniture-storage-trigger"
        :aria-expanded="storageOpen"
        aria-label="Möbellager öffnen"
        type="button"
        @click.stop="storageOpen = !storageOpen"
      >
        <v-icon icon="mdi-sofa-outline" size="21" />
        <v-icon class="storage-plus" icon="mdi-plus" size="12" />
      </button>
      <Transition name="storage-tray">
        <aside v-if="view !== 'front' && storageOpen" class="furniture-storage" data-furniture-storage aria-label="Möbellager">
          <header>
            <div class="storage-title">
              <span class="storage-title-icon"><v-icon icon="mdi-archive-star-outline" size="19" /></span>
              <span><strong>Möbellager</strong><small>{{ storageAccessories.length === 1 ? '1 Möbelstück verstaut' : `${storageAccessories.length} Möbelstücke verstaut` }}</small></span>
            </div>
            <button aria-label="Möbellager schließen" type="button" @click="storageOpen = false"><v-icon icon="mdi-close" size="18" /></button>
          </header>
          <div class="storage-items">
            <p v-if="storageAccessories.length === 0" class="storage-empty"><span><v-icon icon="mdi-inbox-arrow-down-outline" size="25" /></span><strong>Hier ist noch Platz!</strong><small>Ziehe ein Möbelstück in diese Truhe.</small></p>
            <button
              v-for="accessory in storageAccessories"
              :key="accessory.id"
              :aria-label="`${accessory.title} aufstellen`"
              class="stored"
              type="button"
              @click="placeStoredAccessory(accessory)"
            >
              <span class="stored-preview"><RoomFurniture :item="accessory" /></span>
              <strong>{{ accessory.title }}</strong>
              <span class="stored-action"><v-icon icon="mdi-plus" size="10" />Aufstellen</span>
            </button>
            <p v-if="storageAccessories.length > 0" class="storage-drop-hint"><v-icon icon="mdi-inbox-arrow-down-outline" size="15" />Weitere Möbel hier ablegen</p>
          </div>
        </aside>
      </Transition>

      <div v-if="view === 'front'" class="world-family" :class="{ 'guardian-active': activeFamilyMember?.role === 'guardian' }" aria-label="Familienmitglieder in der Familienwelt">
        <div class="world-family-background" aria-label="Weitere Familienmitglieder im Hintergrund">
          <div v-for="group in backgroundMemberGroups" :key="group.side" :class="['world-family-side', `world-family-side--${group.side}`]">
            <div
              v-for="(member, index) in group.members"
              :key="member.id"
              :class="['world-family-member', 'is-background-member', member.role === 'child' ? 'is-child' : 'is-guardian']"
            >
              <Transition name="member-name">
                <span v-if="speakingMemberId === member.id" class="member-name-bubble" role="status">{{ member.nickname?.trim() || member.name }}</span>
              </Transition>
              <AvatarFigure
                :appearance="appearanceFor(member, index)"
                calm
                full-body
                :size="member.role === 'child' ? 42 : 55"
                @interact="showMemberName(member.id)"
              />
            </div>
          </div>
        </div>
        <div v-if="activeFamilyMember" class="world-family-member is-active-member">
          <Transition name="member-name">
            <span v-if="speakingMemberId === activeFamilyMember.id" class="member-name-bubble" role="status">{{ activeFamilyMember.nickname?.trim() || activeFamilyMember.name }}</span>
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

      <div v-if="view === 'front'" class="world-pets" aria-label="Haustiere in der Familienwelt">
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
        <button class="scene-action" :disabled="entering" aria-label="Haus betreten" type="button" @click.stop="enterHouse">
          <v-icon icon="mdi-door-open" size="20" />
          <span><strong>Eintreten</strong><small>Tür öffnen</small></span>
        </button>
      </template>
      <button v-else class="scene-action" aria-label="Zurück vor das Haus" type="button" @click.stop="leaveScene">
        <v-icon icon="mdi-arrow-left" size="20" />
        <span><strong>Nach draußen</strong><small>Vorgarten</small></span>
      </button>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { computed, onUnmounted, ref } from 'vue';
import { motion } from 'motion-v';

import PixiWorldFoundation from '@/components/PixiWorldFoundation.vue';
import AvatarFigure from '@/features/avatar/components/AvatarFigure.vue';
import AnimatedPet from './AnimatedPet.vue';
import DollhouseInterior from './DollhouseInterior.vue';
import HouseLayoutEntity from './HouseLayoutEntity.vue';
import RoomFurniture from './RoomFurniture.vue';
import { HOUSE_THEMES } from '../data/house-catalog';
import { createDefaultAvatarAppearance, createGuardianAvatarAppearance } from '@/domain/avatar';
import type { AvatarAppearance } from '@/domain/avatar';
import type { HouseAccessoryId, HouseRoomDefinition, HouseStageLevel, HouseThemeId, HouseZoneId } from '@/domain/house';
import type { FamilyMember, FamilyMemberId, FamilyPet, FamilyPetId, HouseAccessory, HouseLayoutPlacement, HouseLayoutPlacementId, WorldEffect } from '@/domain/types';

type HouseView = 'front' | 'inside' | 'garden';

const props = defineProps<{
  energy: number;
  houseLevel: HouseStageLevel;
  houseThemeId?: HouseThemeId;
  houseLayout: HouseLayoutPlacement[];
  rooms: ReadonlyArray<HouseRoomDefinition>;
  ladiScore: number;
  canArrangeHouse: boolean;
  effects: WorldEffect[];
  accessories: HouseAccessory[];
  members: FamilyMember[];
  pets: FamilyPet[];
  revealVersion: number;
  viewerMemberId: FamilyMemberId;
}>();
const emit = defineEmits<{
  'move-entity': [placementId: HouseLayoutPlacementId, zoneId: HouseZoneId, x: number, y: number];
  'place-furniture': [accessoryId: HouseAccessoryId, zoneId: HouseZoneId];
  'reset-entity': [placementId: HouseLayoutPlacementId];
  'store-furniture': [accessoryId: HouseAccessoryId];
}>();

const view = ref<HouseView>('front');
const selectedRoomView = ref<HouseZoneId | 'all'>('living-room');
const entering = ref(false);
const storageOpen = ref(false);
const draggingEntityType = ref<HouseLayoutPlacement['entityType'] | null>(null);
const speakingMemberId = ref<FamilyMemberId | null>(null);
const activePetId = ref<FamilyPetId | null>(null);
let memberNameTimer: number | undefined;
let petNameTimer: number | undefined;
let entranceTimer: number | undefined;
const viewLabels: Record<HouseView, string> = { front: 'Vorne', inside: 'Innen', garden: 'Garten' };
const viewLabel = computed(() => viewLabels[view.value]);
const weatherLabel = computed(() => props.energy >= 70 ? 'sonnig' : props.energy >= 40 ? 'wolkig' : 'regnerisch');
const sunOpacity = computed(() => Math.max(.08, props.energy / 100));
const familyChildren = computed(() => props.members.filter((member) => member.role === 'child'));
const familyGuardians = computed(() => props.members.filter((member) => member.role === 'guardian'));
const familyLineup = computed(() => {
  const guardianSplit = Math.ceil(familyGuardians.value.length / 2);
  const lineup = [
    ...familyGuardians.value.slice(0, guardianSplit),
    ...familyChildren.value,
    ...familyGuardians.value.slice(guardianSplit),
  ];
  const activeMember = lineup.find((member) => member.id === props.viewerMemberId);
  if (!activeMember) return lineup;
  const otherMembers = lineup.filter((member) => member.id !== props.viewerMemberId);
  const centerIndex = Math.ceil(otherMembers.length / 2);
  return [
    ...otherMembers.slice(0, centerIndex),
    activeMember,
    ...otherMembers.slice(centerIndex),
  ];
});
const activeFamilyMember = computed(() => familyLineup.value.find((member) => member.id === props.viewerMemberId));
const backgroundFamilyMembers = computed(() => familyLineup.value.filter((member) => member.id !== props.viewerMemberId));
const backgroundMemberGroups = computed(() => {
  const split = Math.ceil(backgroundFamilyMembers.value.length / 2);
  return [
    { side: 'left', members: backgroundFamilyMembers.value.slice(0, split) },
    { side: 'right', members: backgroundFamilyMembers.value.slice(split) },
  ];
});
const activeTheme = computed(() => HOUSE_THEMES.find((theme) => theme.id === props.houseThemeId) ?? HOUSE_THEMES[0]);
const activeThemeId = computed(() => activeTheme.value?.id ?? 'sunny-dollhouse');
const sceneStyle = computed(() => ({
  '--house-wall': activeTheme.value?.wall ?? '#fff8df',
  '--house-wall-upper': activeTheme.value?.wallUpper ?? '#f2dcf2',
  '--house-floor': activeTheme.value?.floor ?? '#f8d89b',
  '--house-roof': activeTheme.value?.roof ?? '#ec6e66',
  '--house-roof-shade': activeTheme.value?.roofShade ?? '#c64f56',
  '--house-trim': activeTheme.value?.trim ?? '#a96855',
  '--house-door': activeTheme.value?.door ?? '#75a982',
  '--house-window': activeTheme.value?.window ?? '#8ed5e6',
  '--landscape-accent': activeTheme.value?.landscapeAccent ?? '#62bd77',
  '--world-saturation': `${.55 + props.energy / 200}`,
  '--world-brightness': `${.82 + props.energy / 550}`,
}));
const ariaLabel = computed(
  () => `Familienwelt, Hausstufe ${props.houseLevel + 1}, ${viewLabel.value}, ${props.energy} Prozent Hausenergie, ${weatherLabel.value}`,
);

const hasEffect = (effect: WorldEffect) => props.effects.includes(effect);
const accessoryFor = (placement: HouseLayoutPlacement) => placement.entityType === 'furniture'
  ? props.accessories.find((accessory) => accessory.id === placement.entityId)
  : undefined;
const frontGardenPlacements = computed(() => props.houseLayout
  .filter((placement) => placement.zoneId === 'garden' && placement.entityType === 'furniture')
  .filter((placement) => {
    const accessory = accessoryFor(placement);
    return Boolean(accessory?.owned && accessory.equipped);
  }));
const storageAccessories = computed(() => props.accessories.filter((accessory) => accessory.owned && !accessory.equipped));
const storageTargetZone = computed<HouseZoneId>(() => view.value === 'garden'
  ? 'garden'
  : selectedRoomView.value === 'all' || selectedRoomView.value === 'garden'
    ? 'living-room'
    : selectedRoomView.value);
const storeAccessory = (accessoryId: HouseAccessoryId) => emit('store-furniture', accessoryId);
const placeStoredAccessory = (accessory: HouseAccessory) => emit('place-furniture', accessory.id, storageTargetZone.value);
const appearanceFor = (member: FamilyMember, index: number): AvatarAppearance => {
  if (member.appearance) return member.appearance;
  if (member.role === 'guardian') {
    const guardianIndex = familyGuardians.value.findIndex((guardian) => guardian.id === member.id);
    const preset = guardianIndex % 2 === 0 ? 'adult' : 'grandpa';
    return { ...createGuardianAvatarAppearance(preset), outfitColorId: guardianIndex % 2 === 0 ? 'outfit-rose' : 'outfit-mint' };
  }
  const appearance = createDefaultAvatarAppearance();
  const childIndex = familyChildren.value.findIndex((child) => child.id === member.id);
  const variants: Array<Partial<AvatarAppearance>> = [
    { hair: 'ponytail', outfitColorId: 'outfit-blue' },
    { hair: 'short', hairColorId: 'hair-black', outfit: 'overalls', outfitColorId: 'outfit-gold' },
    { hair: 'curls', hairColorId: 'hair-brown', outfit: 'space', outfitColorId: 'outfit-ocean' },
  ];
  return { ...appearance, ...(variants[(childIndex >= 0 ? childIndex : index) % variants.length] ?? {}) };
};
const showMemberName = (memberId: FamilyMemberId) => {
  speakingMemberId.value = memberId;
  if (memberNameTimer !== undefined) window.clearTimeout(memberNameTimer);
  memberNameTimer = window.setTimeout(() => { speakingMemberId.value = null; }, 1900);
};
const showPetName = (petId: FamilyPetId) => {
  activePetId.value = petId;
  if (petNameTimer !== undefined) window.clearTimeout(petNameTimer);
  petNameTimer = window.setTimeout(() => { activePetId.value = null; }, 1900);
};
const forwardEntityMove = (placementId: HouseLayoutPlacementId, zoneId: HouseZoneId, x: number, y: number) => {
  emit('move-entity', placementId, zoneId, x, y);
};
const forwardEntityReset = (placementId: HouseLayoutPlacementId) => emit('reset-entity', placementId);
const enterHouse = () => {
  if (entering.value) return;
  entering.value = true;
  entranceTimer = window.setTimeout(() => {
    selectedRoomView.value = 'living-room';
    view.value = 'inside';
    entering.value = false;
  }, 520);
};
const selectZone = (zoneId: HouseZoneId | 'all') => {
  selectedRoomView.value = zoneId;
  view.value = zoneId === 'garden' ? 'garden' : 'inside';
};
const leaveScene = () => {
  storageOpen.value = false;
  view.value = 'front';
};
onUnmounted(() => {
  if (memberNameTimer !== undefined) window.clearTimeout(memberNameTimer);
  if (petNameTimer !== undefined) window.clearTimeout(petNameTimer);
  if (entranceTimer !== undefined) window.clearTimeout(entranceTimer);
});
</script>

<style scoped>
.world-scene-wrap {
  @apply w-100;
  margin-top: 10px;
  padding-bottom: 8px;
  @apply position-relative overflow-hidden;
  touch-action: pan-y;
  @apply select-none;
}
.scene-drag-layer {
  @apply position-relative;
  touch-action: pan-y;
}
.world-scene {
  @apply w-100 position-relative overflow-visible;
  height: auto;
  z-index: 1;
  opacity: 0.16;
}
.front-garden-items {
  @apply position-absolute pointer-events-none;
  inset: 42% 2% 7%;
  z-index: 2;
}
.front-garden-items :deep(.layout-entity) {
  width: 58px;
  height: 58px;
}
.front-garden-items :deep(.room-furniture) {
  filter: drop-shadow(0 5px 4px rgba(54, 72, 55, 0.18));
}
.scene-world-shell {
  @apply position-absolute overflow-hidden;
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
  overflow: visible;
  z-index: 30;
}
.world-scene-wrap.is-inside .scene-world-shell {
  top: 0;
  background: linear-gradient(180deg, #fff9ea, #f7e7c9);
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
  padding: 0;
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
  width: 68px;
  height: 68px;
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
  width: 112px;
  height: 112px;
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
  width: 136px;
  height: 136px;
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
.furniture-storage-trigger {
  width: 43px;
  height: 43px;
  @apply position-absolute d-grid place-center cursor-pointer;
  top: 8px;
  right: 8px;
  z-index: 25;
  color: #2f8068;
  border: 3px solid #fff;
  border-radius: 14px;
  background: #eefaf4;
  box-shadow:
    0 4px 0 #b9dfce,
    0 8px 16px rgba(42, 83, 68, 0.18);
}
.storage-plus {
  width: 17px;
  height: 17px;
  @apply position-absolute d-grid place-center;
  right: -5px;
  bottom: -5px;
  color: #fff;
  border: 2px solid #fff;
  border-radius: 50%;
  background: #40b78b;
}
.furniture-storage {
  width: 126px;
  @apply position-absolute overflow-hidden;
  top: 0;
  right: 0;
  bottom: 2%;
  z-index: 24;
  padding: 8px 7px 10px;
  border: 2px solid #cce6d9;
  border-radius: 22px 0 0 22px;
  background:
    radial-gradient(
      circle at 92% 9%,
      rgba(255, 214, 116, 0.25) 0 22px,
      transparent 23px
    ),
    linear-gradient(165deg, #fbfff9, #fff8e9 72%);
  box-shadow:
    -7px 0 0 rgba(160, 205, 184, 0.25),
    -13px 0 25px rgba(54, 67, 58, 0.14);
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
  border: 9px solid rgba(85, 184, 143, 0.1);
  border-radius: 50%;
}
.furniture-storage > header {
  @apply d-flex align-start justify-space-between;
  gap: 4px;
  margin: -3px -2px 8px;
  padding: 6px 5px 8px;
  border-bottom: 1px dashed rgba(58, 137, 105, 0.25);
}
.storage-title {
  min-width: 0;
  @apply d-flex align-center;
  gap: 5px;
}
.storage-title > span:last-child {
  min-width: 0;
  @apply d-flex flex-column;
  color: #31594e;
  line-height: 1.08;
}
.storage-title strong {
  font-size: 10px;
}
.storage-title small {
  margin-top: 2px;
  overflow: hidden;
  color: #71817b;
  font-size: 6px;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.storage-title-icon {
  width: 29px;
  height: 29px;
  @apply d-grid place-center flex-shrink-0;
  color: #fff;
  border: 2px solid #fff;
  border-radius: 10px 10px 8px 8px;
  background: linear-gradient(145deg, #66c79f, #369876);
  box-shadow: 0 3px 0 #277a5d;
  transform: rotate(-4deg);
}
.furniture-storage > header button {
  width: 24px;
  height: 24px;
  @apply d-grid place-center flex-shrink-0 cursor-pointer;
  border: 2px solid #fff;
  border-radius: 50%;
  background: #fff0d5;
  color: #a96d25;
  box-shadow: 0 3px 0 #e6cda5;
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
  @apply d-flex flex-column align-center justify-center text-center;
  gap: 4px;
  margin: 0;
  padding: 8px;
  color: #4e7568;
  border: 2px dashed #add7c4;
  border-radius: 17px;
  background: rgba(237, 250, 242, 0.78);
  font-weight: 750;
}
.storage-empty > span {
  width: 43px;
  height: 43px;
  @apply d-grid place-center;
  margin-bottom: 3px;
  color: #398f6e;
  border: 3px solid #fff;
  border-radius: 15px;
  background: #dff5e9;
  box-shadow: 0 4px 0 #bfdccb;
}
.storage-empty strong {
  font-size: 8px;
}
.storage-empty small {
  max-width: 82px;
  font-size: 6px;
  line-height: 1.3;
}
.storage-items > button.stored {
  min-height: 112px;
  @apply position-relative d-flex flex-column align-center justify-center cursor-pointer;
  gap: 2px;
  padding: 7px 4px 6px;
  color: #49645b;
  border: 1px solid #c8e1d5;
  border-radius: 17px;
  background: linear-gradient(155deg, #fff, #edf9f1);
  box-shadow:
    0 4px 0 #d3e6dc,
    0 8px 12px rgba(61, 104, 84, 0.08);
  transition:
    transform 0.16s ease,
    box-shadow 0.16s ease;
}
.storage-items > button.stored:hover {
  transform: translateY(-2px);
  box-shadow:
    0 6px 0 #c7dfd2,
    0 10px 14px rgba(61, 104, 84, 0.11);
}
.storage-items > button.stored > strong {
  max-width: 92px;
  overflow: hidden;
  font-size: 7px;
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
    radial-gradient(circle, #fff 0 48%, transparent 49%),
    linear-gradient(145deg, #e8f6ff, #fff1ce);
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
  color: #fff;
  border-radius: 999px;
  background: linear-gradient(145deg, #58bd94, #318b6c);
  box-shadow: 0 3px 0 #267258;
  font-size: 6px;
  font-weight: 850;
}
.storage-drop-hint {
  @apply d-flex align-center justify-center text-center;
  gap: 3px;
  margin: 1px 0 0;
  padding: 6px 4px;
  color: #598074;
  border: 1px dashed #afd2c2;
  border-radius: 11px;
  background: rgba(239, 250, 243, 0.72);
  font-size: 6px;
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
.world-scene-wrap.is-dragging-furniture .furniture-storage {
  border-color: #55b88f;
  background: #f3fff8;
  box-shadow:
    inset 0 0 0 3px rgba(85, 184, 143, 0.12),
    -7px 0 18px rgba(54, 67, 58, 0.16);
}
.world-scene-wrap.is-dragging-furniture .storage-empty {
  color: #2f8068;
  border-color: #55b88f;
  background: rgba(85, 184, 143, 0.08);
}
.world-scene-wrap.is-storage-open .scene-actions {
  right: 134px;
}
.world-scene-wrap.is-inside .house-inside {
  opacity: 0;
}
.doorway-glimpse {
  opacity: 0.35;
  transition: opacity 0.15s ease;
}
.door-leaf {
  transform-box: view-box;
  transform-origin: 210px 220px;
}
.world-scene-wrap.is-entering .doorway-glimpse {
  opacity: 1;
}
.world-scene-wrap.is-entering .door-leaf {
  animation: front-door-open 460ms cubic-bezier(0.42, 0, 0.2, 1) both;
}
.world-scene-wrap.is-entering .door-handle {
  transform-box: view-box;
  transform-origin: 241px 220px;
  animation: door-knob-turn 460ms ease both;
}
.island {
  transform-origin: 230px 210px;
}
.world-reveal {
  animation: house-reveal 850ms cubic-bezier(0.2, 0.9, 0.2, 1) both;
}
.plot-upgrade {
  animation: garden-grow 700ms cubic-bezier(0.2, 0.9, 0.2, 1);
}
.plot-flower-border > path {
  fill: none;
  stroke: #3f915b;
  stroke-linecap: round;
  stroke-width: 3;
}
.plot-flower-border circle:nth-of-type(3n + 1) {
  fill: #ff8e84;
}
.plot-flower-border circle:nth-of-type(3n + 2) {
  fill: #ffd364;
}
.plot-flower-border circle:nth-of-type(3n) {
  fill: #ee94b5;
}
.plot-path ellipse {
  fill: #e7bb7b;
  stroke: rgba(132, 91, 57, 0.3);
  stroke-width: 2;
}
.plot-garden-bed > path:first-child {
  fill: #8a6243;
  stroke: #69472f;
  stroke-linejoin: round;
  stroke-width: 2;
}
.plot-garden-bed > path:nth-child(2) {
  fill: none;
  stroke: #bf9565;
  stroke-linecap: round;
  stroke-width: 2;
}
.plot-garden-bed .plot-sprouts {
  fill: none;
  stroke: #4e9c5e;
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
  filter: drop-shadow(0 8px 6px rgba(79, 75, 59, 0.16));
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
  stroke: #d79859;
  stroke-width: 2;
  opacity: 0.38;
}
.baseboard {
  fill: none;
  stroke: #dd9e69;
  stroke-width: 7;
}
.wide-window path:not(.curtain) {
  fill: none;
  stroke: #fff;
  stroke-width: 3;
}
.wide-window .curtain {
  fill: none;
  stroke: #e88f9a;
  stroke-linecap: round;
  stroke-width: 7;
}
.starter-shelf rect:first-child {
  fill: #a66d4f;
}
.starter-shelf circle {
  fill: #f1c15b;
}
.starter-shelf path {
  fill: #66b577;
  stroke: #347c50;
  stroke-width: 2;
}
.starter-shelf rect:last-child {
  fill: #6e8fdc;
}
.bookshelf path {
  stroke: #f1bf63;
}
.floor-lamp circle {
  @apply pointer-events-none;
}
.dollhouse-roof {
  transform-origin: 230px 75px;
  animation: roof-breathe 4s ease-in-out infinite;
}
.upper-dollhouse-room {
  stroke: #a96855;
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
  fill: rgba(61, 76, 67, 0.2);
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
  stroke: #dfa06f;
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
  stroke: rgba(255, 255, 255, 0.35);
  stroke-linecap: round;
  stroke-width: 3;
}
.toy-door .doorway-back {
  fill: #654d40;
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
  fill: #ffd45f;
  stroke: #986621;
  stroke-width: 2;
}
.toy-door .door-handle path {
  fill: none;
  stroke: #986621;
  stroke-linecap: round;
  stroke-width: 3;
}
.toy-door .door-panel-detail {
  fill: none;
  stroke: rgba(255, 255, 255, 0.25);
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
  stroke: #fff;
  stroke-width: 6;
}
.toy-window path {
  fill: none;
  stroke: #fff;
  stroke-width: 3;
}
.front-step path {
  fill: #e4ad68;
  stroke: #9b694e;
  stroke-linejoin: round;
  stroke-width: 3;
}
.toy-bush {
  fill: #62bd77;
  stroke: #398659;
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
  fill: #8ed5e6;
  stroke: #fff;
  stroke-width: 4;
}
.toy-balcony-front {
  fill: #ffd576;
  stroke: #a66d4f;
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
  fill: #9a6549;
  stroke: #654536;
  stroke-width: 2;
}
.front-planters circle {
  fill: var(--landscape-accent);
  stroke: rgba(255, 255, 255, 0.65);
  stroke-width: 1.5;
}
.toy-side-body {
  fill: var(--house-wall);
  stroke: var(--house-trim);
  stroke-linejoin: round;
  stroke-width: 7;
}
.toy-side-wall {
  fill: #e7c58e;
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
  fill: #e3ae6f;
  stroke: #94634b;
  stroke-width: 4;
}
.side-pool ellipse:first-child {
  fill: #f0ca83;
  stroke: #96664e;
  stroke-width: 4;
}
.side-pool ellipse:last-child {
  fill: #7bd3e3;
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
.edition-decor,
.exterior-pumpkin-arch,
.exterior-bat-garland,
.exterior-candy-bushes,
.exterior-candy-fence {
  pointer-events: none;
}
.bat {
  fill: #31283f;
  transform-box: fill-box;
  transform-origin: center;
  animation: bat-flutter 1.2s ease-in-out infinite alternate;
}
.bat-two {
  animation-delay: -0.55s;
}
.pumpkin {
  fill: #ef873b;
  stroke: #87452d;
  stroke-width: 3;
}
.pumpkin-face {
  fill: none;
  stroke: #543338;
  stroke-linecap: round;
  stroke-width: 2;
}
.spooky-vine {
  fill: none;
  stroke: #697a4b;
  stroke-linecap: round;
  stroke-width: 5;
}
.theme-halloween-night .far-landscape {
  filter: hue-rotate(34deg) saturate(0.7) brightness(0.82);
}
.theme-halloween-night .scene-sun {
  filter: grayscale(0.7);
}
.theme-halloween-night .toy-window rect {
  filter: drop-shadow(0 0 7px #ffd166);
}
.candy-cloud {
  fill: #f3a9c9;
  stroke: #fff0f7;
  stroke-width: 3;
  filter: drop-shadow(0 4px 2px rgba(168, 104, 151, 0.2));
}
.candy-right {
  fill: #a9dff1;
}
.candy-swirl {
  fill: none;
  stroke: #fff;
  stroke-linecap: round;
  stroke-width: 6;
}
.lollipop {
  fill: #ef91bd;
  stroke: #fff;
  stroke-width: 4;
}
.lollipop-blue {
  fill: #86d6ec;
}
.wrapped-candies {
  fill: #ffd875;
  stroke: #fff;
  stroke-linejoin: round;
  stroke-width: 2;
}
.wrapped-candies rect:last-of-type {
  fill: #88d8e9;
}
.candy-sprinkles circle:nth-child(odd) {
  fill: #ffd56d;
}
.candy-sprinkles circle:nth-child(even) {
  fill: #79d4e7;
}
.theme-cotton-candy-dream .far-landscape {
  filter: hue-rotate(310deg) saturate(0.75) brightness(1.08);
}
.theme-cotton-candy-dream .toy-roof path:first-child {
  fill: url(#cottonRoofPattern);
}
.theme-cotton-candy-dream .toy-house-body {
  filter: drop-shadow(0 0 7px rgba(255, 255, 255, 0.8));
}
.theme-cotton-candy-dream .toy-window rect {
  filter: drop-shadow(0 0 5px rgba(255, 224, 117, 0.75));
}
.exterior-pumpkin-arch path {
  fill: none;
  stroke: #5c7544;
  stroke-width: 7;
}
.exterior-pumpkin-arch circle {
  fill: #ef873b;
  stroke: #88482e;
  stroke-width: 2;
}
.exterior-bat-garland > path:first-child {
  fill: none;
  stroke: #3d314d;
  stroke-width: 2;
}
.exterior-bat-garland > path:last-child {
  fill: #493958;
}
.exterior-candy-bushes {
  fill: #f2a6c9;
  stroke: #fff;
  stroke-width: 2;
}
.exterior-candy-bushes circle:nth-child(n + 3) {
  fill: #a9dff1;
}
.exterior-candy-fence {
  fill: none;
  stroke: #ef8bb8;
  stroke-dasharray: 6 4;
  stroke-linecap: round;
  stroke-width: 6;
}
.world-family {
  width: 215px;
  height: 108px;
  @apply position-absolute left-0;
  bottom: 3.5%;
  z-index: 3;
  @apply d-grid place-end-center pointer-events-none;
  filter: drop-shadow(0 6px 5px rgba(67, 58, 48, 0.16));
}
.world-family::before {
  content: "";
  width: 145px;
  height: 20px;
  @apply position-absolute;
  left: 50%;
  bottom: -2px;
  z-index: 0;
  transform: translateX(-50%);
  border-radius: 50%;
  background: rgba(60, 95, 72, 0.13);
  filter: blur(2px);
}
.world-family.inside {
  bottom: 4.5%;
}
.world-family-background {
  @apply w-100;
  grid-area: 1 / 1;
  z-index: 1;
  @apply d-flex align-end justify-space-between;
  transform: translateY(-17px);
}
.world-family-side {
  width: 86px;
  @apply d-flex align-end justify-center;
}
.world-family-side .world-family-member {
  margin-inline: -5px;
}
.world-family-side .world-family-member.is-guardian {
  margin-inline: -7px;
}
.world-family-member {
  @apply position-relative;
  z-index: 2;
  margin-inline: -8px;
  transform-origin: center bottom;
}
.world-family-member :deep(.avatar-figure) {
  pointer-events: auto;
  @apply cursor-pointer;
}
.member-name-bubble,
.pet-name-bubble {
  min-width: max-content;
  padding: 5px 8px;
  @apply position-absolute;
  left: 50%;
  bottom: calc(100% + 4px);
  z-index: 7;
  transform: translateX(-50%);
  color: #285547;
  border: 2px solid rgba(255, 255, 255, 0.96);
  border-radius: 11px;
  background: #fffdf8;
  box-shadow: 0 4px 10px rgba(54, 78, 66, 0.18);
  font-size: 9px;
  @apply font-weight-black;
  line-height: 1;
  @apply pointer-events-none;
}
.member-name-bubble::after,
.pet-name-bubble::after {
  content: "";
  width: 8px;
  height: 8px;
  @apply position-absolute;
  left: 50%;
  bottom: -5px;
  transform: translateX(-50%) rotate(45deg);
  border-right: 2px solid rgba(255, 255, 255, 0.96);
  border-bottom: 2px solid rgba(255, 255, 255, 0.96);
  background: #fffdf8;
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
  @apply ma-0;
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
  @apply position-absolute;
  right: 3%;
  bottom: 13%;
  z-index: 4;
  @apply d-flex align-end justify-center;
  gap: 2px;
  @apply pointer-events-none;
}
.world-pets.inside {
  right: 3%;
  bottom: 13%;
}
.world-pet {
  @apply position-relative;
  z-index: 1;
  @apply d-flex align-end;
  pointer-events: auto;
}
.world-pet:nth-child(2) :deep(.animated-pet) {
  animation-delay: -1.7s;
}
.world-pets :deep(.animated-pet) {
  pointer-events: auto;
  @apply cursor-pointer;
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
  stroke: rgba(255, 255, 255, 0.24);
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
  fill: #b8cfd0;
}
.mountain-snow {
  fill: #eff7f4;
  opacity: 0.82;
}
.hill-layer {
  transform-box: fill-box;
  transform-origin: center bottom;
}
.hill-back {
  fill: #a8d4a4;
  animation: hill-drift-back 15s ease-in-out infinite alternate;
}
.hill-front {
  fill: #82bf84;
  animation: hill-drift-front 12s ease-in-out infinite alternate;
}
.hill-layer,
.distant-pines,
.scene-sun,
.cloud {
  will-change: transform;
}
.distant-pines {
  fill: #5f9d72;
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
  fill: #fff1a3;
  opacity: 0.35;
}
.sun-core {
  fill: #ffd765;
  transform-box: fill-box;
  transform-origin: center;
  animation: sun-glow 3.6s ease-in-out infinite;
}
.sun-rays {
  fill: none;
  stroke: #ffd765;
  stroke-linecap: round;
  stroke-width: 5;
  transform-box: fill-box;
  transform-origin: center;
  animation: sun-rays-pulse 3.6s ease-in-out infinite;
}
.cloud {
  fill: #fff;
  opacity: 0.65;
  transition:
    fill 650ms ease,
    opacity 650ms ease;
}
.cloud.heavy {
  fill: #afc1c2;
  opacity: 0.92;
}
.cloud-a {
  animation: cloud-drift 8s ease-in-out infinite alternate;
}
.cloud-b {
  animation: cloud-drift 10s ease-in-out infinite alternate-reverse;
}
.rain-cloud {
  fill: #84999d;
  animation: rain-cloud-drift 4s ease-in-out infinite alternate;
}
.rain-drops {
  fill: none;
  stroke: #69b8da;
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
.sparkles {
  transform-origin: center;
  animation: sparkle 2s ease-in-out infinite;
}
.perfect-shimmer {
  filter: drop-shadow(0 0 6px rgba(255, 238, 131, 0.72));
}
.shimmer-star {
  fill: #fff8b3;
  transform-box: fill-box;
  transform-origin: center;
  animation: shimmer-pop 2.8s ease-in-out infinite;
}
.shimmer-two {
  fill: #bdf4ee;
  animation-delay: -0.7s;
}
.shimmer-three {
  fill: #ffe58b;
  animation-delay: -1.4s;
}
.shimmer-four {
  fill: #d5c4ff;
  animation-delay: -2.1s;
}
.shimmer-five {
  fill: #fff;
  animation-delay: -0.35s;
}
.shimmer-sweep {
  fill: none;
  stroke: rgba(255, 255, 255, 0.82);
  stroke-dasharray: 18 170;
  stroke-linecap: round;
  stroke-width: 6;
  animation: shimmer-sweep 3.2s ease-in-out infinite;
}
.accessory,
.interior-item {
  animation: accessory-pop 450ms cubic-bezier(0.2, 0.9, 0.2, 1);
}
.house-addon {
  animation: accessory-pop 650ms cubic-bezier(0.2, 0.9, 0.2, 1);
}
.scene-actions {
  @apply position-absolute d-flex justify-center;
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
  @apply d-flex align-center justify-center;
  gap: 7px;
  color: #315f51;
  border: 1px solid rgba(44, 79, 65, 0.16);
  border-radius: 15px;
  background: rgba(239, 251, 245, 0.94);
  box-shadow:
    0 4px 0 #c1e4d2,
    0 8px 18px rgba(45, 76, 64, 0.14);
  @apply cursor-pointer;
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
  @apply d-block;
}
.scene-action span {
  @apply text-left;
  line-height: 1.05;
}
.scene-action strong {
  font-size: 10px;
}
.scene-action small {
  margin-top: 2px;
  color: #71857c;
  font-size: 8px;
}
.scene-action:active {
  transform: translateY(2px);
  box-shadow: 0 2px 0 #c1e4d2;
}
.scene-action:focus-visible {
  outline: 3px solid rgba(69, 158, 124, 0.3);
  outline-offset: 2px;
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
    filter: drop-shadow(0 0 0 rgba(255, 214, 101, 0));
    transform: scale(0.98);
  }
  50% {
    filter: drop-shadow(0 0 9px rgba(255, 214, 101, 0.68));
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
@keyframes sparkle {
  0%,
  100% {
    opacity: 0.35;
    transform: scale(0.85) rotate(0);
  }
  50% {
    opacity: 1;
    transform: scale(1.12) rotate(20deg);
  }
}
@keyframes shimmer-pop {
  0%,
  70%,
  100% {
    opacity: 0.28;
    transform: scale(0.68) rotate(-8deg);
  }
  35% {
    opacity: 1;
    transform: scale(1.12) rotate(8deg);
  }
}
@keyframes shimmer-sweep {
  0% {
    opacity: 0;
    stroke-dashoffset: 205;
  }
  35%,
  65% {
    opacity: 0.9;
  }
  100% {
    opacity: 0;
    stroke-dashoffset: 0;
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
@keyframes bat-flutter {
  from {
    transform: translateY(0) scaleY(0.82);
  }
  to {
    transform: translateY(-4px) scaleY(1.12);
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
@keyframes front-door-open {
  0%,
  24% {
    transform: scaleX(1) skewY(0);
  }
  65% {
    transform: scaleX(0.48) skewY(-1.5deg);
  }
  100% {
    transform: scaleX(0.13) skewY(-2deg);
    filter: brightness(0.84);
  }
}
@keyframes door-knob-turn {
  0%,
  12% {
    transform: rotate(0);
  }
  28%,
  100% {
    transform: rotate(55deg);
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

@media (prefers-reduced-motion: reduce) {
  .world-reveal,
  .plot-upgrade,
  .house-view,
  .house,
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
  .sparkles,
  .perfect-shimmer *,
  .accessory,
  .interior-item,
  .house-addon,
  .dollhouse-roof,
  .interior-family :deep(.avatar-figure) {
    animation: none;
  }
}
</style>
