<template>
  <div class="page page-padding profile-page">
    <PageHeader
      description="Dein Profil, deine Fortschritte und deine Einstellungen."
      eyebrow="Persönlicher Bereich"
      icon="mdi-account-circle-outline"
      title="Ich"
      tone="blue"
    />

    <v-card class="profile-hero pa-5" elevation="0" rounded="xl">
      <div class="d-flex align-center ga-4">
        <AvatarFigure v-if="store.viewerRole === 'child'" :appearance="activeAppearance" :size="100" />
        <div v-else class="guardian-avatar" :style="{ background: `${store.signedInMember.color}24` }">{{ store.signedInMember.avatar }}</div>
        <div class="flex-grow-1 min-w-0">
          <p class="eyebrow mb-1">{{ store.viewerRole === 'child' ? 'Mein Ladirchen-Profil' : 'Bezugsperson' }}</p>
          <h2>{{ store.displayNameFor(store.signedInMemberId) }}</h2>
          <p class="text-caption text-medium-emphasis mt-1">{{ store.viewerRole === 'child' ? 'So sehen dich alle in eurer Familienwelt.' : 'Du begleitest die Kinder und verwaltest die Familienwelt.' }}</p>
        </div>
      </div>
      <v-btn v-if="store.viewerRole === 'child'" class="mt-4" color="primary" prepend-icon="mdi-palette-outline" rounded="lg" variant="flat" width="100%" @click="avatarBuilderOpen = true">Aussehen gestalten</v-btn>
      <v-btn v-else class="mt-4" color="info" prepend-icon="mdi-account-group-outline" rounded="lg" variant="tonal" width="100%" @click="store.openFamilySetup">Familie verwalten</v-btn>
    </v-card>

    <template v-if="store.viewerRole === 'child'">
      <v-card class="ladi-card pa-4 mt-5" elevation="0" rounded="xl">
        <div class="d-flex align-center ga-3">
          <LadiMascot :score="store.averageTaskRating" :size="78" />
          <div><p class="eyebrow mb-1">Mein Ladi-Level</p><strong>{{ ladiStage.name }}</strong><p class="text-caption text-medium-emphasis mt-1">{{ ladiStage.description }}</p></div>
        </div>
      </v-card>

      <v-card class="nickname-card pa-4 mt-4" elevation="0" rounded="xl">
        <div class="d-flex align-start ga-3">
          <v-avatar color="secondary" size="40" variant="tonal">✨</v-avatar>
          <div class="flex-grow-1 min-w-0">
            <strong>Mein Spitzname</strong>
            <p class="text-caption text-medium-emphasis mt-1">So dürfen dich die anderen in der Familienwelt nennen.</p>
          </div>
        </div>
        <div class="nickname-actions mt-3">
          <v-text-field v-model="nicknameDraft" counter="18" density="compact" hide-details label="Spitzname" maxlength="18" variant="outlined" />
          <v-btn color="secondary" :disabled="!nicknameChanged" rounded="lg" variant="flat" @click="saveNickname">Speichern</v-btn>
        </div>
      </v-card>

      <v-card class="settings-card pa-4 mt-4" elevation="0" rounded="xl">
        <strong>Meine Einstellungen</strong>
        <div class="setting-row mt-3"><span><v-icon size="19">mdi-bell-outline</v-icon> Erinnerungen</span><v-switch color="primary" density="compact" hide-details inset model-value /></div>
        <div class="setting-row"><span><v-icon size="19">mdi-eye-outline</v-icon> Familienziele sehen</span><v-switch color="primary" density="compact" hide-details inset model-value /></div>
      </v-card>
    </template>

    <v-card v-else class="settings-card pa-4 mt-5" elevation="0" rounded="xl">
      <strong>Meine Berechtigungen</strong>
      <div class="permission-list mt-3"><span>✓ Beiträge erstellen und bewerten</span><span>✓ Sparziele unterstützen</span><span>✓ Shop-Elemente verwalten</span><span>✓ Bonusaktionen planen</span></div>
    </v-card>

    <AvatarBuilderDialog v-if="store.viewerRole === 'child'" v-model="avatarBuilderOpen" :initial-appearance="activeAppearance" :user-name="store.displayNameFor(store.activeChildId)" @save="store.saveOwnAppearance" />
  </div>
</template>

<script lang="ts" setup>
import { computed, ref, watch } from 'vue';

import AvatarBuilderDialog from '../components/AvatarBuilderDialog.vue';
import AvatarFigure from '../components/AvatarFigure.vue';
import LadiMascot from '../components/LadiMascot.vue';
import PageHeader from '../components/ui/PageHeader.vue';
import { createDefaultAvatarAppearance } from '../domain/avatar';
import { getLadiStage } from '../domain/ladi';
import { useFamilyWorldStore } from '../stores/family-world';

const store = useFamilyWorldStore();
const avatarBuilderOpen = ref(false);
const nicknameDraft = ref(store.activeChild.nickname ?? '');
const activeAppearance = computed(() => store.activeChild.appearance ?? createDefaultAvatarAppearance());
const ladiStage = computed(() => getLadiStage(store.averageTaskRating));
const nicknameChanged = computed(() => nicknameDraft.value.trim() !== (store.activeChild.nickname ?? ''));
const saveNickname = () => store.setOwnNickname(nicknameDraft.value);
watch(() => store.activeChildId, () => { nicknameDraft.value = store.activeChild.nickname ?? ''; });
</script>

<style scoped>
.profile-hero {
  border: 1px solid rgba(62, 188, 140, 0.22);
  background: linear-gradient(145deg, #eaf9f2, #fff3d5);
  box-shadow: 0 5px 0 rgba(62, 188, 140, 0.13) !important;
}
.profile-hero h2 {
  margin: 0;
  font-size: 24px;
  letter-spacing: -0.035em;
}
.guardian-avatar {
  width: 92px;
  height: 92px;
  display: grid;
  place-items: center;
  flex: 0 0 auto;
  border: 3px solid white;
  border-radius: 30px;
  box-shadow: 0 6px 16px rgba(45, 78, 65, 0.12);
  font-size: 45px;
}
.ladi-card,
.nickname-card,
.settings-card {
  border: 1px solid var(--lad-border);
  box-shadow: 0 4px 0 var(--lad-border) !important;
}
.ladi-card {
  background: linear-gradient(145deg, #fffaf0, #edf9f4);
}
.nickname-card {
  background: linear-gradient(145deg, #fff8fc, #f3efff);
}
.nickname-actions {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto;
  align-items: center;
  gap: 8px;
}
.setting-row {
  min-height: 50px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  border-top: 1px solid var(--lad-border);
}
.setting-row span {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 13px;
  font-weight: 700;
}
.permission-list {
  display: grid;
  gap: 10px;
  color: var(--lad-muted);
  font-size: 13px;
}
</style>
