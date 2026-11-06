<template>
  <div class="application-version text-caption text-medium-emphasis" aria-live="polite">
    <span v-if="loading">{{ t("common.version.loading") }}</span>
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
import { inject, onMounted } from "vue";
import { useI18n } from "vue-i18n";
import { APPLICATION_INFO_GATEWAY, useApplicationInfo } from "@/shared/composables/use-application-info";

const applicationInfoGateway = inject(APPLICATION_INFO_GATEWAY);

if (!applicationInfoGateway) {
  throw new Error("The application information gateway has not been registered.");
}

const { t } = useI18n();
const { info, loading, failed, refresh } = useApplicationInfo(applicationInfoGateway);

onMounted(refresh);
</script>

<style lang="scss" scoped>
.application-version {
  min-width: 0;
  overflow-wrap: anywhere;
}
</style>
