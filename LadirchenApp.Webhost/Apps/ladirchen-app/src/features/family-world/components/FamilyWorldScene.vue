<template>
  <div class="world-scene-wrap" :style="sceneStyle">
    <motion.div
      class="scene-drag-layer"
      drag="x"
      :drag-constraints="{ left: 0, right: 0 }"
      :drag-elastic=".16"
      :drag-momentum="false"
      :while-drag="reducedMotion ? undefined : { scale: .985, rotate: dragDirection * 1.2 }"
      :transition="{ type: 'spring', stiffness: 390, damping: 30 }"
      :on-drag="trackDrag"
      :on-drag-end="finishDrag"
    >
      <PixiWorldFoundation :energy="energy" :view="view" />
      <svg class="world-scene" viewBox="10 8 440 325" role="img" :aria-label="ariaLabel">
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
          <filter id="sceneShadow" height="160%" width="160%" x="-30%" y="-30%">
            <feDropShadow dx="0" dy="12" flood-color="#335443" flood-opacity=".2" stdDeviation="8" />
          </filter>
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
              </g>

              <g class="toy-door">
                <path d="M210 195q0-13 13-13h15q13 0 13 13v60h-41Z" />
                <circle cx="241" cy="220" r="4" />
                <path d="M203 255h55" />
              </g>
              <g class="toy-window left-window"><rect x="157" y="174" width="43" height="39" rx="9" /><path d="M178 178v31m-17-15h35" /></g>
              <g class="toy-window right-window"><rect x="263" y="174" width="43" height="39" rx="9" /><path d="M284 178v31m-17-15h35" /></g>

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
            </g>

            <g v-else-if="view === 'side'" class="house house-side">
              <ellipse class="toy-house-shadow" cx="232" cy="271" rx="126" ry="21" />
              <path class="toy-side-body" d="M139 148h151l45 28v80H139Z" />
              <path class="toy-side-wall" d="m290 148 45 28v80l-45-13Z" />
              <path class="toy-side-roof" d="M112 151 218 74l106 72 37 28-17 16-48-31-78-56-91 66Z" />
              <g class="toy-window side-wide"><rect x="178" y="171" width="68" height="47" rx="11" /><path d="M212 176v37m-29-18h58" /></g>
              <g class="side-door"><path d="M266 194q0-10 10-10h22q10 0 10 10v61h-42Z" /><circle cx="296" cy="220" r="4" /></g>
              <g v-if="houseLevel >= 1" class="house-addon side-upper"><rect x="177" y="103" width="83" height="50" rx="9" /><g class="toy-window"><rect x="199" y="113" width="39" height="29" rx="7" /></g></g>
              <g v-if="houseLevel >= 2" class="house-addon side-sunroom"><rect x="87" y="197" width="59" height="58" rx="12" /><path d="m80 199 36-30 37 30" /><rect x="99" y="210" width="34" height="29" rx="8" /></g>
              <g v-if="houseLevel >= 3" class="house-addon side-deck"><path d="M314 235h66v13h-66Z" /><path d="M326 247v25m42-25v25" /></g>
              <g v-if="houseLevel >= 4" class="house-addon side-pool"><ellipse cx="355" cy="260" rx="36" ry="13" /><ellipse cx="355" cy="257" rx="27" ry="8" /></g>
              <g class="toy-bush bush-left"><circle cx="129" cy="239" r="19" /><circle cx="111" cy="249" r="14" /></g>
            </g>

            <g v-else class="house house-inside dollhouse">
              <ellipse class="room-shadow" cx="232" cy="279" fill="#456052" opacity=".2" rx="145" ry="20" />

              <g class="ground-room">
                <path class="room-wall" d="M111 126Q111 116 122 116H338Q349 116 349 127V230H111Z" fill="var(--house-wall)" />
                <path class="room-side" d="m349 127 24 15v102l-24-14Z" fill="#e6c08a" />
                <path class="room-floor" d="M111 230h238l24 15-63 37H89Z" fill="var(--house-floor)" />
                <path class="floorboards" d="m111 244 224 1m-197-15-18 39m72-39-7 52m61-52 8 47m46-47 21 29" />
                <path class="baseboard" d="M113 225h234" />
                <path class="room-frame" d="M111 230V126Q111 116 122 116h216q11 0 11 11v103m0 0 24 15M111 230l-22 52h221l63-37" />

                <g class="wide-window">
                  <rect x="205" y="137" width="54" height="45" rx="7" fill="#fff" />
                  <rect x="211" y="143" width="42" height="33" rx="4" :fill="hasEffect('lights') ? '#ffe27c' : '#96d8e8'" />
                  <path d="M232 143v33m-21-16h42" />
                  <path class="curtain" d="M204 137c-9 14-7 35 0 47m56-47c9 14 7 35 0 47" />
                </g>

                <g class="starter-shelf">
                  <rect x="287" y="151" width="38" height="8" rx="4" />
                  <circle cx="297" cy="143" r="8" />
                  <path d="M297 136c-6-8 4-13 6-4 5-6 10 3 2 7" />
                  <rect x="311" y="137" width="8" height="14" rx="2" />
                </g>

                <HouseFurniture v-for="item in equippedInteriorItems" :key="item.id" :item="item" />
              </g>

              <g v-if="houseLevel >= 1" class="house-addon upper-dollhouse-room">
                <path d="M139 76q0-8 9-8h164q9 0 9 9v49H139Z" fill="var(--house-wall-upper)" />
                <path d="M139 121h182v9H139Z" fill="#a86c59" />
                <rect x="205" y="83" width="49" height="30" rx="6" fill="#fff" />
                <rect x="211" y="89" width="37" height="18" rx="3" fill="#9bd9e6" />
                <path d="M230 89v18" stroke="#fff" stroke-width="3" />
                <g class="upper-room-toys"><path d="M158 104h31v17h-31Z" fill="#80a8e4" /><circle cx="166" cy="101" fill="#f3b44f" r="7" /><path d="m283 106 8-14 8 14v15h-16Z" fill="#7dc895" /></g>
              </g>

              <g class="dollhouse-roof" :class="{ raised: houseLevel >= 1 }">
                <path :d="houseLevel >= 1 ? 'M121 68 230 18l111 50-15 15-96-42-95 42Z' : 'M91 116 230 53l139 63-17 18-122-55-122 55Z'" fill="var(--house-roof)" />
                <path :d="houseLevel >= 1 ? 'm230 18 111 50-9 9-102-44Z' : 'm230 53 139 63-10 11-129-57Z'" fill="var(--house-roof-shade)" />
              </g>

              <g v-if="houseLevel >= 2" class="house-addon attic-star"><circle cx="230" cy="58" r="17" fill="#ffe27b" /><path d="m230 46 4 8 9 1-7 6 2 9-8-5-8 5 2-9-7-6 9-1Z" fill="#fff7cd" /></g>
              <g v-if="houseLevel >= 3" class="house-addon toy-balcony"><path d="M321 119h43v56h-43Z" fill="#fff0ce" /><path d="M316 119h54l-7-13h-40Z" fill="#6f8edb" /><path d="M326 145h33m-28-14v39m23-39v39" stroke="#d47a63" stroke-width="4" /></g>
              <g v-if="houseLevel >= 4" class="house-addon rooftop-flag"><path d="M231 18V1m0 1 30 9-30 9" fill="#ffd05b" stroke="#96602c" stroke-linejoin="round" stroke-width="3" /></g>
            </g>
          </g>

          <g v-if="view !== 'inside' && hasEffect('smoke')" class="smoke"><path d="m281 132 7-3v17l-7 3z" fill="#8d5d49" /><circle cx="288" cy="122" fill="#fff" opacity=".55" r="8" /><circle cx="296" cy="110" fill="#fff" opacity=".38" r="11" /></g>
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

          <g v-if="isEquipped('flower-boxes') && view !== 'inside'" class="accessory flowers"><rect x="250" y="213" width="24" height="5" rx="2" fill="#9c684b" /><circle cx="255" cy="210" fill="#ff8a83" r="4" /><circle cx="263" cy="208" fill="#ffd264" r="4" /><circle cx="271" cy="210" fill="#e98db0" r="4" /></g>
          <g v-if="isEquipped('garden-lights')" class="accessory lights"><path d="m123 235 29 15m-22-11v15m13-10v15" stroke="#6b624c" stroke-width="2" /><circle cx="130" cy="240" fill="#ffe270" r="4" /><circle cx="143" cy="246" fill="#ffe270" r="4" /></g>
          <g v-if="isEquipped('hammock')" class="accessory hammock"><path d="m111 205 57 29" stroke="#e87363" stroke-width="5" /><path d="m112 198-8 41m65-14 8 42" stroke="#705441" stroke-width="3" /></g>
          <g v-if="isEquipped('telescope')" class="accessory telescope"><path d="m350 192 18-8" stroke="#496272" stroke-width="7" /><path d="m358 190-8 25m8-25 9 20" stroke="#496272" stroke-width="3" /></g>
        </g>
      </svg>

      <div class="world-family" :class="{ inside: view === 'inside', 'guardian-active': activeFamilyMember?.role === 'guardian' }" aria-label="Familienmitglieder in der Familienwelt">
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

      <div class="world-pets" :class="{ inside: view === 'inside' }" aria-label="Haustiere in der Familienwelt">
        <div v-for="pet in pets" :key="pet.id" class="world-pet">
          <Transition name="member-name">
            <span v-if="activePetId === pet.id" class="pet-name-bubble" role="status">{{ pet.name }}</span>
          </Transition>
          <AnimatedPet :pet="pet" :size="view === 'inside' ? 52 : 48" @interact="showPetName(pet.id)" />
        </div>
      </div>
    </motion.div>

    <button class="scene-rotate" :aria-label="`Haus drehen, aktuelle Ansicht ${viewLabel}`" type="button" @click.stop="changeView(1)">
      <svg viewBox="0 0 24 24" aria-hidden="true"><path d="M18.7 8.2A7.5 7.5 0 1 0 19 15m-.3-6.8V4.5m0 3.7H15" /></svg>
      <span><strong>Drehen</strong><small>{{ viewLabel }}</small></span>
    </button>
  </div>
</template>

<script lang="ts" setup>
import { computed, onUnmounted, ref } from 'vue';
import { motion, useReducedMotion } from 'motion-v';
import type { PanInfo } from 'motion-v';

import PixiWorldFoundation from '@/components/PixiWorldFoundation.vue';

import AvatarFigure from './AvatarFigure.vue';
import AnimatedPet from './AnimatedPet.vue';
import HouseFurniture from './HouseFurniture.vue';
import { HOUSE_THEMES } from '../data/house-catalog';
import { createDefaultAvatarAppearance } from '../domain/avatar';
import type { AvatarAppearance } from '../domain/avatar';
import type { FamilyMember, FamilyPet, HouseAccessory } from '../domain/types';

type HouseView = 'front' | 'side' | 'inside';

const props = defineProps<{
  energy: number;
  houseLevel: number;
  houseThemeId?: string;
  effects: string[];
  accessories: HouseAccessory[];
  members: FamilyMember[];
  pets: FamilyPet[];
  revealVersion: number;
  viewerMemberId: string;
}>();

const views: HouseView[] = ['front', 'side', 'inside'];
const viewIndex = ref(0);
const dragDirection = ref(0);
const speakingMemberId = ref<string | null>(null);
const activePetId = ref<string | null>(null);
let memberNameTimer: number | undefined;
let petNameTimer: number | undefined;
const reducedMotion = useReducedMotion();
const view = computed(() => views[viewIndex.value]);
const viewLabel = computed(() => ({ front: 'Vorne', side: 'Seite', inside: 'Innen' })[view.value]);
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
const sceneStyle = computed(() => ({
  '--house-wall': activeTheme.value?.wall ?? '#fff8df',
  '--house-wall-upper': activeTheme.value?.wallUpper ?? '#f2dcf2',
  '--house-floor': activeTheme.value?.floor ?? '#f8d89b',
  '--house-roof': activeTheme.value?.roof ?? '#ec6e66',
  '--house-roof-shade': activeTheme.value?.roofShade ?? '#c64f56',
  '--house-trim': activeTheme.value?.trim ?? '#a96855',
  '--world-saturation': `${.55 + props.energy / 200}`,
  '--world-brightness': `${.82 + props.energy / 550}`,
}));
const equippedInteriorItems = computed(() => props.accessories.filter(
  (accessory) => accessory.placement === 'inside' && accessory.equipped && accessory.visual && accessory.scene,
));
const ariaLabel = computed(
  () => `Drehbare Familienwelt, Hausstufe ${props.houseLevel + 1}, ${viewLabel.value}, ${props.energy} Prozent Hausenergie, ${weatherLabel.value}`,
);

const hasEffect = (effect: string) => props.effects.includes(effect);
const isEquipped = (id: string) => props.accessories.some((accessory) => accessory.id === id && accessory.equipped);
const appearanceFor = (member: FamilyMember, index: number): AvatarAppearance => {
  if (member.appearance) return member.appearance;
  const appearance = createDefaultAvatarAppearance();
  if (member.role === 'guardian') {
    const guardianIndex = familyGuardians.value.findIndex((guardian) => guardian.id === member.id);
    const guardianVariants: Array<Partial<AvatarAppearance>> = [
      { faceShape: 'soft', hair: 'waves', hairColor: '#6b4535', outfit: 'explorer', outfitColor: member.color },
      { faceShape: 'angular', hair: 'short', hairColor: '#3f3029', outfit: 'hoodie', outfitColor: member.color },
    ];
    return { ...appearance, ...(guardianVariants[guardianIndex % guardianVariants.length] ?? {}) };
  }
  const childIndex = familyChildren.value.findIndex((child) => child.id === member.id);
  const variants: Array<Partial<AvatarAppearance>> = [
    { hair: 'ponytail', outfitColor: '#6f8df5' },
    { hair: 'short', hairColor: '#33251f', outfit: 'overalls', outfitColor: '#e6a83f' },
    { hair: 'curls', hairColor: '#69432b', outfit: 'space', outfitColor: '#3b8aaa' },
  ];
  return { ...appearance, ...(variants[(childIndex >= 0 ? childIndex : index) % variants.length] ?? {}) };
};
const showMemberName = (memberId: string) => {
  speakingMemberId.value = memberId;
  if (memberNameTimer !== undefined) window.clearTimeout(memberNameTimer);
  memberNameTimer = window.setTimeout(() => { speakingMemberId.value = null; }, 1900);
};
const showPetName = (petId: string) => {
  activePetId.value = petId;
  if (petNameTimer !== undefined) window.clearTimeout(petNameTimer);
  petNameTimer = window.setTimeout(() => { activePetId.value = null; }, 1900);
};
const changeView = (direction: number) => {
  viewIndex.value = (viewIndex.value + direction + views.length) % views.length;
};
const trackDrag = (_event: PointerEvent, info: PanInfo) => {
  dragDirection.value = Math.sign(info.offset.x);
};
const finishDrag = (_event: PointerEvent, info: PanInfo) => {
  if (Math.abs(info.offset.x) >= 35 || Math.abs(info.velocity.x) >= 450) changeView(info.offset.x < 0 ? 1 : -1);
  dragDirection.value = 0;
};
onUnmounted(() => {
  if (memberNameTimer !== undefined) window.clearTimeout(memberNameTimer);
  if (petNameTimer !== undefined) window.clearTimeout(petNameTimer);
});
</script>

<style scoped>
.world-scene-wrap {
  width: 100%;
  padding-bottom: 8px;
  position: relative;
  overflow: hidden;
  touch-action: pan-y;
  user-select: none;
}
.scene-drag-layer {
  position: relative;
  cursor: grab;
  touch-action: pan-y;
}
.scene-drag-layer:active {
  cursor: grabbing;
}
.world-scene {
  width: 100%;
  height: auto;
  position: relative;
  z-index: 1;
  overflow: visible;
  opacity: 0.16;
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
  pointer-events: none;
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
.toy-door path:first-child,
.side-door path:first-child {
  fill: #75a982;
  stroke: #426e59;
  stroke-width: 5;
}
.toy-door circle,
.side-door circle {
  fill: #ffd45f;
  stroke: #986621;
  stroke-width: 2;
}
.toy-door path:last-child {
  fill: none;
  stroke: #a86851;
  stroke-linecap: round;
  stroke-width: 8;
}
.toy-window rect,
.side-sunroom rect {
  fill: #8ed5e6;
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
.world-family {
  width: 215px;
  height: 108px;
  position: absolute;
  left: 0;
  bottom: 3.5%;
  z-index: 3;
  display: grid;
  place-items: end center;
  pointer-events: none;
  filter: drop-shadow(0 6px 5px rgba(67, 58, 48, 0.16));
}
.world-family::before {
  content: "";
  width: 145px;
  height: 20px;
  position: absolute;
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
  width: 100%;
  grid-area: 1 / 1;
  z-index: 1;
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  transform: translateY(-17px);
}
.world-family-side {
  width: 86px;
  display: flex;
  align-items: flex-end;
  justify-content: center;
}
.world-family-side .world-family-member {
  margin-inline: -5px;
}
.world-family-side .world-family-member.is-guardian {
  margin-inline: -7px;
}
.world-family-member {
  position: relative;
  z-index: 2;
  margin-inline: -8px;
  transform-origin: center bottom;
}
.world-family-member :deep(.avatar-figure) {
  pointer-events: auto;
  cursor: pointer;
}
.member-name-bubble,
.pet-name-bubble {
  min-width: max-content;
  padding: 5px 8px;
  position: absolute;
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
  font-weight: 900;
  line-height: 1;
  pointer-events: none;
}
.member-name-bubble::after,
.pet-name-bubble::after {
  content: "";
  width: 8px;
  height: 8px;
  position: absolute;
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
  margin: 0;
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
  position: absolute;
  right: 3%;
  bottom: 13%;
  z-index: 4;
  display: flex;
  align-items: flex-end;
  justify-content: center;
  gap: 2px;
  pointer-events: none;
}
.world-pets.inside {
  right: 3%;
  bottom: 13%;
}
.world-pet {
  position: relative;
  z-index: 1;
  display: flex;
  align-items: flex-end;
  pointer-events: auto;
}
.world-pet:nth-child(2) :deep(.animated-pet) {
  animation-delay: -1.7s;
}
.world-pets :deep(.animated-pet) {
  pointer-events: auto;
  cursor: pointer;
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
.scene-rotate {
  min-width: 92px;
  height: 42px;
  padding: 5px 11px;
  position: absolute;
  right: 50%;
  bottom: 3px;
  z-index: 4;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 7px;
  transform: translateX(50%);
  color: #315f51;
  border: 1px solid rgba(44, 79, 65, 0.16);
  border-radius: 15px;
  background: rgba(239, 251, 245, 0.94);
  box-shadow:
    0 4px 0 #c1e4d2,
    0 8px 18px rgba(45, 76, 64, 0.14);
  cursor: pointer;
  font: inherit;
  backdrop-filter: blur(10px);
  transition:
    transform 150ms ease,
    box-shadow 150ms ease;
}
.scene-rotate svg {
  width: 19px;
  height: 19px;
  fill: none;
  stroke: currentColor;
  stroke-linecap: round;
  stroke-linejoin: round;
  stroke-width: 2.3;
}
.scene-rotate span,
.scene-rotate strong,
.scene-rotate small {
  display: block;
}
.scene-rotate span {
  text-align: left;
  line-height: 1.05;
}
.scene-rotate strong {
  font-size: 10px;
}
.scene-rotate small {
  margin-top: 2px;
  color: #71857c;
  font-size: 8px;
}
.scene-rotate:active {
  transform: translateX(50%) translateY(2px);
  box-shadow: 0 2px 0 #c1e4d2;
}
.scene-rotate:focus-visible {
  outline: 3px solid rgba(69, 158, 124, 0.3);
  outline-offset: 2px;
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
