<template>
  <BrandedCard class="account-statement pa-4 mt-5" tone="profile">
    <div class="d-flex align-start justify-space-between ga-3">
      <div>
        <p class="eyebrow mb-1">{{ t("profile.accountStatement.eyebrow") }}</p>
        <h2>{{ t("profile.accountStatement.title") }}</h2>
        <p class="text-caption text-medium-emphasis mt-1">{{ t("profile.accountStatement.description") }}</p>
      </div>
      <v-avatar color="primary" size="44" variant="tonal">
        <v-icon icon="i-mdi:receipt-text-clock-outline" />
      </v-avatar>
    </div>

    <div class="statement-filters mt-4">
      <v-select
        v-if="memberOptions.length > 1"
        v-model="selectedMemberId"
        density="compact"
        hide-details
        item-title="title"
        item-value="value"
        :items="memberOptions"
        :label="t('profile.accountStatement.filters.person')"
        variant="outlined"
      />
      <v-select
        v-model="selectedTransactionType"
        density="compact"
        hide-details
        item-title="title"
        item-value="value"
        :items="transactionTypeOptions"
        :label="t('profile.accountStatement.filters.type')"
        variant="outlined"
      />
      <v-text-field
        v-model="dateFrom"
        density="compact"
        hide-details
        :label="t('profile.accountStatement.filters.from')"
        type="date"
        variant="outlined"
      />
      <v-text-field
        v-model="dateTo"
        density="compact"
        hide-details
        :label="t('profile.accountStatement.filters.to')"
        type="date"
        variant="outlined"
      />
    </div>

    <div v-if="filteredEntries.length" class="statement-list mt-4">
      <article v-for="entry in filteredEntries" :key="entry.id" class="statement-entry">
        <div class="statement-icon" :class="`statement-icon--${entry.direction}`" aria-hidden="true">
          <v-icon :icon="iconFor(entry.transactionType)" size="21" />
        </div>
        <div class="statement-copy">
          <div class="d-flex align-start justify-space-between ga-3">
            <div class="min-w-0">
              <strong>{{ entry.sourceTitle || typeLabel(entry.transactionType) }}</strong>
              <p>{{ entrySubtitle(entry) }}</p>
              <p v-if="entry.sourceTitle && entry.counterpartyMemberId">{{ counterpartyLabel(entry) }}</p>
            </div>
            <strong class="statement-amount" :class="amountClass(entry.balanceChange)">
              {{ formatAmount(entry) }}
            </strong>
          </div>
          <div class="statement-meta mt-2">
            <span>{{ formatDateTime(entry.occurredAtUtc) }}</span>
            <span>{{
              t("profile.accountStatement.balanceAfter", { amount: formatNumber(entry.resultingBalance) })
            }}</span>
          </div>
        </div>
      </article>
    </div>

    <div v-else class="statement-empty mt-4">
      <v-icon icon="i-mdi:receipt-text-remove-outline" size="30" />
      <span>{{ t("profile.accountStatement.empty") }}</span>
    </div>
  </BrandedCard>
</template>

<script lang="ts" setup>
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import type { FamilyMember } from "@/domain/family/types";
import type { IanaTimeZone } from "@/domain/family/time-zone";
import type { AccountStatementEntry, AccountStatementTransactionType } from "@/domain/savings/types";
import type { FamilyMemberId } from "@/domain/shared/identifiers";
import { filterAccountStatementEntries } from "@/features/profile/account-statement-filter";
import BrandedCard from "@/shared/components/ui/BrandedCard.vue";

const ALL_MEMBERS = "all";
const ALL_TYPES = "all";

export interface Props {
  entries: ReadonlyArray<AccountStatementEntry>;
  memberIds: ReadonlyArray<FamilyMemberId>;
  members: ReadonlyArray<FamilyMember>;
  timeZone: IanaTimeZone;
}

const props = defineProps<Props>();
const { locale, t } = useI18n();
const selectedMemberId = ref<string>(props.memberIds.length > 1 ? ALL_MEMBERS : (props.memberIds[0] ?? ALL_MEMBERS));
const selectedTransactionType = ref<string>(ALL_TYPES);
const dateFrom = ref("");
const dateTo = ref("");
const memberOptions = computed(() => {
  const options = props.memberIds.map((memberId) => ({
    title: displayNameFor(memberId),
    value: memberId,
  }));

  return options.length > 1
    ? [{ title: t("profile.accountStatement.filters.allPeople"), value: ALL_MEMBERS }, ...options]
    : options;
});
const transactionTypeOptions = computed(() => {
  const types = [...new Set(props.entries.map((entry) => entry.transactionType))].sort();

  return [
    { title: t("profile.accountStatement.filters.allTypes"), value: ALL_TYPES },
    ...types.map((type) => ({ title: typeLabel(type), value: type })),
  ];
});
const filteredEntries = computed(() =>
  filterAccountStatementEntries(
    props.entries,
    {
      memberId: selectedMemberId.value === ALL_MEMBERS ? undefined : (selectedMemberId.value as FamilyMemberId),
      transactionType:
        selectedTransactionType.value === ALL_TYPES
          ? undefined
          : (selectedTransactionType.value as AccountStatementTransactionType),
      dateFrom: dateFrom.value || undefined,
      dateTo: dateTo.value || undefined,
    },
    props.timeZone,
  ),
);

function displayNameFor(memberId: FamilyMemberId): string {
  const member = props.members.find((candidate) => candidate.id === memberId);

  return member?.nickname || member?.name || t("profile.accountStatement.unknownMember");
}

function typeLabel(type: AccountStatementTransactionType): string {
  return t(`profile.accountStatement.types.${type}`);
}

function counterpartyLabel(entry: AccountStatementEntry): string {
  return entry.counterpartyMemberId
    ? t("profile.accountStatement.counterparty", { name: displayNameFor(entry.counterpartyMemberId) })
    : typeLabel(entry.transactionType);
}

function entrySubtitle(entry: AccountStatementEntry): string {
  return entry.sourceTitle ? typeLabel(entry.transactionType) : counterpartyLabel(entry);
}

function formatNumber(value: number): string {
  return new Intl.NumberFormat(locale.value, { maximumFractionDigits: 2 }).format(value);
}

function formatAmount(entry: AccountStatementEntry): string {
  if (entry.balanceChange > 0) {
    return `+${formatNumber(entry.balanceChange)}`;
  }

  if (entry.balanceChange < 0) {
    return `−${formatNumber(Math.abs(entry.balanceChange))}`;
  }

  return `↔ ${formatNumber(entry.amount)}`;
}

function formatDateTime(value: string): string {
  return new Intl.DateTimeFormat(locale.value, {
    dateStyle: "medium",
    timeStyle: "short",
    timeZone: props.timeZone,
  }).format(new Date(value));
}

function amountClass(balanceChange: number): string {
  if (balanceChange > 0) {
    return "text-success";
  }

  if (balanceChange < 0) {
    return "text-error";
  }

  return "text-primary";
}

function iconFor(type: AccountStatementTransactionType): string {
  if (type === "task-reward") {
    return "i-mdi:check-circle-outline";
  }

  if (type.includes("gift") || type === "guardian-support") {
    return "i-mdi:gift-outline";
  }

  if (type.startsWith("shop") || type === "house-purchase") {
    return "i-mdi:cart-outline";
  }

  if (type.includes("saving") || type.includes("goal") || type === "interest") {
    return "i-mdi:piggy-bank-outline";
  }

  if (type.startsWith("behavior")) {
    return "i-mdi:traffic-light-outline";
  }

  return "i-mdi:swap-horizontal";
}

watch(
  () => props.memberIds,
  (memberIds) => {
    if (selectedMemberId.value !== ALL_MEMBERS && !memberIds.includes(selectedMemberId.value as FamilyMemberId)) {
      selectedMemberId.value = memberIds.length > 1 ? ALL_MEMBERS : (memberIds[0] ?? ALL_MEMBERS);
    }
  },
);
</script>

<style scoped>
.account-statement h2 {
  font-size: 1.2rem;
}

.statement-filters {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 10px;
}

.statement-list {
  display: grid;
  gap: 10px;
}

.statement-entry {
  display: grid;
  grid-template-columns: auto minmax(0, 1fr);
  gap: 11px;
  padding: 12px;
  border: 1px solid color-mix(in srgb, var(--lad-color-primary-muted) 18%, transparent);
  border-radius: 16px;
  background: var(--lad-surface-raised);
}

.statement-icon {
  width: 38px;
  height: 38px;
  display: grid;
  place-items: center;
  border-radius: 13px;
  color: var(--lad-color-primary-strong);
  background: var(--lad-surface-soft);
}

.statement-icon--credit,
.statement-icon--goal-credit {
  color: var(--lad-color-success-strong);
}

.statement-icon--debit,
.statement-icon--goal-debit {
  color: var(--lad-color-danger-strong);
}

.statement-copy,
.statement-copy > div > div {
  min-width: 0;
}

.statement-copy p,
.statement-meta {
  color: var(--lad-muted);
  font-size: 0.72rem;
}

.statement-copy strong:first-child {
  overflow-wrap: anywhere;
}

.statement-amount {
  white-space: nowrap;
}

.statement-meta {
  display: flex;
  justify-content: space-between;
  gap: 10px;
}

.statement-empty {
  min-height: 88px;
  display: grid;
  place-items: center;
  align-content: center;
  gap: 7px;
  color: var(--lad-muted);
  border: 1px dashed color-mix(in srgb, var(--lad-color-primary-muted) 35%, transparent);
  border-radius: 16px;
}

@media (max-width: 520px) {
  .statement-filters {
    grid-template-columns: 1fr;
  }

  .statement-meta {
    align-items: flex-start;
    flex-direction: column;
    gap: 2px;
  }
}
</style>
