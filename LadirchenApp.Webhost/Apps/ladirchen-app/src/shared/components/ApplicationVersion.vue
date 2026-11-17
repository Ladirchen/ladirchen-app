<template>
  <div
    v-if="!props.informationalOnly || info"
    class="application-version"
    :class="{ 'text-caption text-medium-emphasis': !props.showDetails }"
    aria-live="polite"
  >
    <span v-if="loading">{{ t("common.version.loading") }}</span>
    <small v-else-if="info && props.informationalOnly">
      {{ t("common.version.label", { version: info.informationalVersion }) }}
    </small>
    <v-dialog
      v-else-if="info && props.showDetails"
      v-model="detailsOpen"
      :aria-labelledby="detailsTitleId"
      max-width="560"
      scrollable
    >
      <template #activator="{ props: activatorProps }">
        <button
          v-bind="activatorProps"
          class="application-version-trigger d-flex align-center ga-1 cursor-pointer"
          :title="info.informationalVersion"
          type="button"
        >
          <span class="i-mdi:information-outline flex-shrink-0" aria-hidden="true"></span>
          <span>{{ t("common.version.label", { version: info.semVer }) }}</span>
        </button>
      </template>
      <v-card rounded="xl">
        <v-card-title :id="detailsTitleId" tag="h2">{{ t("common.version.dialogTitle") }}</v-card-title>
        <v-card-text>
          <dl>
            <div v-for="detail in details" :key="detail.field" class="mb-3">
              <dt class="text-caption text-medium-emphasis">{{ detail.label }}</dt>
              <dd class="version-detail text-body-2">{{ detail.value }}</dd>
            </div>
          </dl>
        </v-card-text>
        <v-card-actions class="justify-end">
          <v-btn @click="detailsOpen = false">{{ t("common.close") }}</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
    <span
      v-else-if="info"
      class="application-version-value"
      :aria-label="t('common.version.label', { version: info.informationalVersion })"
      :title="info.informationalVersion"
    >
      {{ t("common.version.label", { version: info.semVer }) }}
    </span>
    <span v-else-if="failed">
      {{ t("common.version.unavailable") }}
      <button class="text-decoration-underline" type="button" @click="refresh">
        {{ t("common.version.retry") }}
      </button>
    </span>
  </div>
</template>

<script lang="ts" setup>
import { computed, inject, onMounted, ref, useId } from "vue";
import { useI18n } from "vue-i18n";
import { applicationDetailFields } from "@/application/ports/application-info-gateway";
import { APPLICATION_INFO_GATEWAY, useApplicationInfo } from "@/shared/composables/use-application-info";

export interface Props {
  informationalOnly?: boolean;
  showDetails?: boolean;
}

const props = defineProps<Props>();
const applicationInfoGateway = inject(APPLICATION_INFO_GATEWAY);

if (!applicationInfoGateway) {
  throw new Error("The application information gateway has not been registered.");
}

const { t } = useI18n();
const { info, loading, failed, refresh } = useApplicationInfo(applicationInfoGateway);
const detailsTitleId = useId();
const detailsOpen = ref(false);
const details = computed(() => {
  const metadata = info.value;

  if (!metadata) {
    return [];
  }

  const labels = {
    branch: t("common.version.details.branch"),
    company: t("common.version.details.company"),
    configuration: t("common.version.details.configuration"),
    copyright: t("common.version.details.copyright"),
    description: t("common.version.details.description"),
    fileVersion: t("common.version.details.fileVersion"),
    informationalVersion: t("common.version.details.informationalVersion"),
    name: t("common.version.details.name"),
    product: t("common.version.details.product"),
    semVer: t("common.version.details.semVer"),
    sha: t("common.version.details.sha"),
    title: t("common.version.details.title"),
    version: t("common.version.details.version"),
  };

  return (["semVer", "informationalVersion", ...applicationDetailFields] as const).flatMap((field) => {
    const value = metadata[field];

    return value === undefined ? [] : [{ field, label: labels[field], value }];
  });
});

onMounted(refresh);
</script>

<style lang="scss" scoped>
.application-version {
  min-width: 0;
  overflow-wrap: anywhere;
}

.application-version-trigger {
  padding: 0;
  border: 0;
  border-radius: 0;
  appearance: none;
  background: transparent;
  color: inherit;
  font: inherit;
  text-align: start;
}

.application-version-trigger:hover {
  text-decoration: underline;
}

.application-version-trigger:focus-visible {
  outline: 2px solid currentColor;
  outline-offset: 2px;
}

.version-detail {
  overflow-wrap: anywhere;
}
</style>
