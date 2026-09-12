<template>
  <v-menu location="top end">
    <template #activator="{ props: menuProps }">
      <v-btn v-bind="menuProps" class="session-switcher" color="deep-purple" prepend-icon="mdi-test-tube" rounded="xl" size="small" variant="flat">
        Demo: {{ store.signedInMember.name }}
      </v-btn>
    </template>
    <v-card min-width="270" rounded="xl">
      <div class="pa-4 pb-2">
        <p class="eyebrow mb-1">Nur Entwicklungsmodus</p>
        <strong>Anmeldung simulieren</strong>
        <p class="text-caption text-medium-emphasis mt-1">Dieser Schalter gehört nicht zur Kinderoberfläche.</p>
      </div>
      <v-list density="compact" nav>
        <v-list-item
          v-for="member in store.members"
          :key="member.id"
          :active="member.id === store.signedInMemberId"
          :prepend-icon="member.role === 'guardian' ? 'mdi-shield-account-outline' : 'mdi-account-outline'"
          :subtitle="member.role === 'guardian' ? 'Bezugsperson' : 'Kind'"
          :title="member.name"
          @click="store.switchSession(member.id)"
        />
      </v-list>
    </v-card>
  </v-menu>
</template>

<script lang="ts" setup>
import { useFamilyWorldStore } from '../stores/family-world';

const store = useFamilyWorldStore();
</script>

<style scoped>
.session-switcher {
  position: fixed;
  right: 18px;
  bottom: 18px;
  z-index: 90;
  box-shadow: 0 8px 24px rgba(58, 41, 99, 0.22);
}
@media (max-width: 650px) {
  .session-switcher {
    right: 10px;
    bottom: 82px;
    min-width: 42px;
    padding-inline: 10px;
  }
}
</style>
