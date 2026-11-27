<template>
  <v-dialog
    v-model="store.accountStatementOpen"
    content-class="account-statement-dialog-frame"
    max-width="900"
    scrollable
  >
    <v-card class="account-statement-dialog" rounded="xl">
      <div class="account-statement-toolbar">
        <v-btn
          :aria-label="t('common.close')"
          icon="i-mdi:close"
          size="small"
          variant="text"
          @click="store.accountStatementOpen = false"
        />
      </div>
      <v-card-text class="pa-3 pt-0 pa-sm-5 pt-sm-0">
        <AccountStatement
          :balances="store.balances"
          :entries="store.accountStatement"
          :member-ids="store.accountStatementMemberIds"
          :members="store.members"
          :time-zone="store.familyTimeZone"
        />
      </v-card-text>
    </v-card>
  </v-dialog>
</template>

<script lang="ts" setup>
import { useI18n } from "vue-i18n";
import AccountStatement from "./AccountStatement.vue";
import { useFamilyWorldStore } from "@/stores/family-world";

const store = useFamilyWorldStore();
const { t } = useI18n();
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;

.account-statement-dialog {
  max-height: calc(100dvh - 28px);
  overflow: hidden;
  @include dialog-frame;
}

.account-statement-dialog :deep(.v-card-text) {
  overflow-y: auto;
}

.account-statement-toolbar {
  min-height: 48px;
  padding: 8px 10px 0;
  display: flex;
  justify-content: flex-end;
  flex: 0 0 auto;
}
</style>
