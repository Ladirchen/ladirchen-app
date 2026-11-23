<template>
  <BrandedCard class="settings-card pa-4 mt-5" tone="profile">
    <div class="d-flex align-center justify-space-between flex-wrap ga-2">
      <div>
        <strong>{{ t("profile.schabi.title") }}</strong>
        <p class="text-caption text-medium-emphasis mt-1">{{ t("profile.schabi.description") }}</p>
      </div>
      <div class="d-flex align-center flex-wrap ga-2">
        <v-btn
          color="info"
          :disabled="syncableDrafts.length === 0"
          :loading="syncingAll"
          prepend-icon="i-mdi:sync"
          size="small"
          variant="tonal"
          @click="synchronizeAll"
        >
          {{ t("profile.schabi.syncAll") }}
        </v-btn>
        <v-chip color="info" prepend-icon="i-mdi:school-outline" size="small" variant="tonal">
          {{ t("profile.schabi.schedule") }}
        </v-chip>
      </div>
    </div>

    <v-progress-linear v-if="loading" class="mt-4" color="primary" indeterminate rounded />
    <v-alert v-else-if="loadError" class="mt-4" density="compact" type="error" variant="tonal">
      {{ t("profile.schabi.loadError") }}
    </v-alert>

    <div v-else class="d-flex flex-column ga-4 mt-4">
      <section v-for="draft in drafts" :key="draft.memberId" class="schabi-child pa-4">
        <div class="d-flex align-center justify-space-between ga-3">
          <div class="d-flex align-center ga-2">
            <span class="text-h6" aria-hidden="true">{{ memberAvatar(draft.memberId) }}</span>
            <div>
              <strong>{{ memberName(draft.memberId) }}</strong>
              <small class="d-block text-medium-emphasis">
                {{ configurationStatus(draft) }}
              </small>
            </div>
          </div>
          <div class="d-flex align-center ga-2">
            <v-btn
              v-if="draft.configured"
              :aria-label="editLockLabel(draft)"
              color="info"
              :icon="editLockIcon(draft)"
              size="small"
              :title="editLockLabel(draft)"
              variant="text"
              @click="toggleEditing(draft)"
            />
            <v-switch
              v-model="draft.enabled"
              color="primary"
              density="compact"
              :disabled="!draft.editing"
              hide-details
              inset
            />
          </div>
        </div>

        <div class="d-grid ga-3 mt-4">
          <v-text-field
            v-model="draft.username"
            autocomplete="off"
            density="compact"
            :label="t('profile.schabi.username')"
            :readonly="!draft.editing"
            variant="outlined"
          />
          <v-text-field
            v-model="draft.calendarUrl"
            autocomplete="off"
            density="compact"
            :hint="calendarHint(draft)"
            :label="t('profile.schabi.calendarUrl')"
            persistent-hint
            :placeholder="secretPlaceholder(draft.configured, draft.editing)"
            :prepend-inner-icon="secretIcon(draft.configured)"
            :readonly="!draft.editing"
            :type="secretFieldType(draft.editing)"
            variant="outlined"
          />
          <v-text-field
            v-model="draft.appointmentsCalendarUrl"
            autocomplete="off"
            density="compact"
            :hint="appointmentsCalendarHint(draft)"
            :label="t('profile.schabi.appointmentsCalendarUrl')"
            persistent-hint
            :placeholder="secretPlaceholder(draft.appointmentsConfigured, draft.editing)"
            :prepend-inner-icon="secretIcon(draft.appointmentsConfigured)"
            :readonly="!draft.editing"
            :type="secretFieldType(draft.editing)"
            variant="outlined"
          />
          <div class="schedule-grid">
            <v-select
              v-model="draft.syncDayOfWeek"
              density="compact"
              item-title="title"
              item-value="value"
              :items="weekdayOptions"
              :label="t('profile.schabi.syncDay')"
              :readonly="!draft.editing"
              variant="outlined"
            />
            <v-text-field
              v-model="draft.syncTimeLocal"
              density="compact"
              :label="t('profile.schabi.syncTime')"
              :readonly="!draft.editing"
              type="time"
              variant="outlined"
            />
            <v-text-field
              v-model.number="draft.syncHorizonDays"
              density="compact"
              :label="t('profile.schabi.syncDays')"
              :max="90"
              :min="1"
              :readonly="!draft.editing"
              type="number"
              variant="outlined"
            />
          </div>
        </div>

        <v-alert v-if="draft.lastError" class="mt-3" density="compact" type="warning" variant="tonal">
          {{ draft.lastError }}
        </v-alert>
        <small v-else-if="draft.lastSuccessfulSyncAt" class="d-block text-medium-emphasis mt-2">
          {{ t("profile.schabi.lastSync", { value: formatDate(draft.lastSuccessfulSyncAt) }) }}
        </small>

        <div class="d-flex justify-end ga-2 mt-3">
          <v-btn
            v-if="draft.configured"
            color="info"
            :disabled="draft.busy || draft.editing || !draft.enabled"
            :loading="draft.syncing"
            prepend-icon="i-mdi:sync"
            size="small"
            variant="tonal"
            @click="synchronize(draft)"
          >
            {{ t("profile.schabi.syncNow") }}
          </v-btn>
          <v-btn
            v-if="draft.configured && draft.editing"
            color="error"
            :disabled="draft.busy || draft.syncing"
            size="small"
            variant="text"
            @click="remove(draft)"
          >
            {{ t("common.delete") }}
          </v-btn>
          <v-btn
            v-if="draft.editing"
            color="primary"
            :disabled="!canSave(draft)"
            :loading="draft.busy"
            size="small"
            variant="flat"
            @click="save(draft)"
          >
            {{ t("common.save") }}
          </v-btn>
        </div>
      </section>
    </div>
  </BrandedCard>
</template>

<script lang="ts" setup>
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import type { FamilyMemberId } from "@/domain/shared/identifiers";
import type { SchabiIntegration } from "@/application/ports/schabi-integration-gateway";
import BrandedCard from "@/shared/components/ui/BrandedCard.vue";
import { familyContext, schabiIntegrationGateway } from "@/app/composition-root";
import { useFamilyWorldStore } from "@/stores/family-world";

interface SchabiIntegrationDraft extends SchabiIntegration {
  appointmentsCalendarUrl: string;
  busy: boolean;
  calendarUrl: string;
  editing: boolean;
  persisted: SchabiIntegration;
  syncing: boolean;
}

const store = useFamilyWorldStore();
const { locale, t } = useI18n();
const drafts = ref<SchabiIntegrationDraft[]>([]);
const loading = ref(true);
const loadError = ref(false);
const syncingAll = ref(false);
const syncableDrafts = computed(() =>
  drafts.value.filter((draft) => draft.configured && draft.enabled && !draft.busy && !draft.editing),
);
const weekdayOptions = computed(() =>
  Array.from({ length: 7 }, (_, value) => ({
    title: new Intl.DateTimeFormat(locale.value, { weekday: "long" }).format(new Date(Date.UTC(2024, 0, 7 + value))),
    value,
  })),
);

function canSave(draft: SchabiIntegrationDraft): boolean {
  return (
    draft.editing &&
    !draft.busy &&
    !draft.syncing &&
    Number.isInteger(draft.syncDayOfWeek) &&
    Number.isInteger(draft.syncHorizonDays) &&
    draft.syncHorizonDays >= 1 &&
    draft.syncHorizonDays <= 90 &&
    /^(?:[01]\d|2[0-3]):[0-5]\d$/u.test(draft.syncTimeLocal) &&
    draft.username.trim().length > 0 &&
    (draft.configured || draft.calendarUrl.trim().length > 0)
  );
}

function calendarHint(draft: SchabiIntegrationDraft): string {
  return draft.configured ? t("profile.schabi.calendarUrlConfigured") : t("profile.schabi.calendarUrlHint");
}

function appointmentsCalendarHint(draft: SchabiIntegrationDraft): string {
  return draft.appointmentsConfigured
    ? t("profile.schabi.appointmentsCalendarUrlConfigured")
    : t("profile.schabi.appointmentsCalendarUrlHint");
}

function configurationStatus(draft: SchabiIntegrationDraft): string {
  if (!draft.configured) {
    return t("profile.schabi.notConfigured");
  }

  return draft.editing ? t("profile.schabi.configuredEditing") : t("profile.schabi.configuredLocked");
}

function editLockLabel(draft: SchabiIntegrationDraft): string {
  return draft.editing ? t("profile.schabi.cancelEditing") : t("profile.schabi.unlockEditing");
}

function editLockIcon(draft: SchabiIntegrationDraft): string {
  return draft.editing ? "i-mdi:lock-open-variant-outline" : "i-mdi:lock-outline";
}

function formatDate(value: string): string {
  return new Intl.DateTimeFormat(locale.value, { dateStyle: "medium", timeStyle: "short" }).format(new Date(value));
}

function memberAvatar(memberId: FamilyMemberId): string {
  return store.members.find((member) => member.id === memberId)?.avatar ?? "🎒";
}

function memberName(memberId: FamilyMemberId): string {
  return store.members.find((member) => member.id === memberId)?.name ?? memberId;
}

function secretPlaceholder(configured: boolean, editing: boolean): string | undefined {
  return configured && !editing ? t("profile.schabi.storedSecret") : undefined;
}

function secretIcon(configured: boolean): string | undefined {
  return configured ? "i-mdi:lock-outline" : undefined;
}

function secretFieldType(editing: boolean): "password" | "text" {
  return editing ? "password" : "text";
}

function toDraft(integration: SchabiIntegration): SchabiIntegrationDraft {
  return {
    ...integration,
    appointmentsCalendarUrl: "",
    busy: false,
    calendarUrl: "",
    editing: !integration.configured,
    persisted: { ...integration },
    syncing: false,
  };
}

function toggleEditing(draft: SchabiIntegrationDraft): void {
  if (!draft.editing) {
    draft.editing = true;

    return;
  }

  Object.assign(draft, draft.persisted, {
    appointmentsCalendarUrl: "",
    calendarUrl: "",
    editing: false,
  });
}

async function load(): Promise<void> {
  loading.value = true;
  loadError.value = false;

  try {
    const integrations = await schabiIntegrationGateway.list(familyContext.activeFamilyId);
    drafts.value = integrations.map(toDraft);
  } catch {
    loadError.value = true;
  } finally {
    loading.value = false;
  }
}

async function remove(draft: SchabiIntegrationDraft): Promise<void> {
  draft.busy = true;

  try {
    await schabiIntegrationGateway.remove(familyContext.activeFamilyId, draft.memberId);
    Object.assign(draft, {
      appointmentsCalendarUrl: "",
      appointmentsConfigured: false,
      calendarUrl: "",
      configured: false,
      editing: true,
      enabled: false,
      lastError: undefined,
      lastSuccessfulSyncAt: undefined,
      syncDayOfWeek: 4,
      syncHorizonDays: 14,
      syncTimeLocal: "06:00",
      username: "",
    });
  } finally {
    draft.busy = false;
  }
}

async function save(draft: SchabiIntegrationDraft): Promise<void> {
  draft.busy = true;

  try {
    const saved = await schabiIntegrationGateway.save(familyContext.activeFamilyId, draft.memberId, {
      appointmentsCalendarUrl: draft.appointmentsCalendarUrl.trim(),
      calendarUrl: draft.calendarUrl.trim(),
      enabled: draft.enabled,
      syncDayOfWeek: draft.syncDayOfWeek,
      syncHorizonDays: draft.syncHorizonDays,
      syncTimeLocal: draft.syncTimeLocal,
      username: draft.username.trim(),
    });
    Object.assign(draft, saved, {
      appointmentsCalendarUrl: "",
      calendarUrl: "",
      configured: true,
      editing: false,
      persisted: { ...saved, configured: true },
    });
  } finally {
    draft.busy = false;
  }
}

async function synchronize(draft: SchabiIntegrationDraft): Promise<void> {
  draft.syncing = true;

  try {
    const synchronized = await schabiIntegrationGateway.synchronize(familyContext.activeFamilyId, draft.memberId);
    Object.assign(draft, synchronized, { persisted: { ...synchronized } });
  } finally {
    draft.syncing = false;
  }
}

async function synchronizeAll(): Promise<void> {
  syncingAll.value = true;

  try {
    await Promise.allSettled(syncableDrafts.value.map(synchronize));
  } finally {
    syncingAll.value = false;
  }
}

onMounted(load);
</script>

<style lang="scss" scoped>
.schabi-child {
  border: 1px solid color-mix(in srgb, var(--lad-color-info) 18%, transparent);
  border-radius: 1rem;
  background: color-mix(in srgb, var(--lad-surface-raised) 75%, transparent);
}

.schedule-grid {
  display: grid;
  grid-template-columns: minmax(0, 1fr) repeat(2, minmax(8rem, 0.6fr));
  gap: 0.75rem;
}

@media (max-width: 35rem) {
  .schedule-grid {
    grid-template-columns: 1fr;
  }
}
</style>
