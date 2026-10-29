<template>
  <div>
    <div v-if="invitation" class="invitation-result">
      <v-alert color="success" icon="i-mdi:account-check-outline" variant="tonal">
        <strong>{{ t("family.memberAccount.created") }}</strong>
        <p class="mt-1 mb-0">{{ t("family.memberAccount.shareCode", { email: invitation.email }) }}</p>
      </v-alert>
      <div class="activation-code mt-4">
        <span>{{ t("family.memberAccount.activationCode") }}</span>
        <code>{{ invitation.activationCode }}</code>
        <small>{{ t("family.memberAccount.expiresAt", { expiresAt: formattedExpiry }) }}</small>
      </div>
      <p v-if="copyFeedback" class="copy-feedback mt-2" :class="`copy-feedback--${copyFeedback}`" role="status">
        {{ t(`family.memberAccount.copy.${copyFeedback}`) }}
      </p>
      <div class="result-actions mt-4">
        <v-btn color="primary" prepend-icon="i-mdi:content-copy" variant="flat" @click="copyActivationCode">
          {{ t("family.memberAccount.copy.action") }}
        </v-btn>
        <v-btn color="primary" prepend-icon="i-mdi:account-plus-outline" variant="tonal" @click="reset">
          {{ t("family.memberAccount.inviteAnother") }}
        </v-btn>
      </div>
    </div>

    <form v-else @submit.prevent="inviteMember">
      <v-text-field
        v-model="name"
        autocomplete="name"
        :label="t('family.memberAccount.name')"
        prepend-inner-icon="i-mdi:account-outline"
        variant="outlined"
      />
      <v-text-field
        v-model="email"
        autocomplete="email"
        :maxlength="EMAIL_INPUT_MAXIMUM_LENGTH"
        :label="t('family.memberAccount.email')"
        prepend-inner-icon="i-mdi:email-outline"
        type="email"
        variant="outlined"
      />
      <v-select
        v-model="role"
        :items="roleOptions"
        item-title="title"
        item-value="value"
        :label="t('family.memberAccount.role')"
        prepend-inner-icon="i-mdi:account-group-outline"
        variant="outlined"
      />
      <v-select
        v-if="role === 'guardian'"
        v-model="guardianAccess"
        :items="guardianAccessOptions"
        item-title="title"
        item-value="value"
        :label="t('family.permissions.label')"
        variant="outlined"
      />
      <v-alert class="mb-4" color="info" density="compact" variant="tonal">
        {{ t("family.memberAccount.activationNotice") }}
      </v-alert>
      <p v-if="errorMessage" class="form-error mb-4" role="alert">{{ errorMessage }}</p>
      <v-btn block color="primary" :disabled="submitIsDisabled" :loading="submitting" rounded="lg" type="submit">
        {{ t("family.memberAccount.submit") }}
      </v-btn>
    </form>
  </div>
</template>

<script lang="ts" setup>
import { computed, ref } from "vue";
import { useI18n } from "vue-i18n";
import { authenticationGateway } from "@/app/composition-root";
import type { MemberAccountInvitation } from "@/application/ports/authentication-gateway";
import type { GuardianAccessLevel, ViewerRole } from "@/domain/family/types";
import { EMAIL_INPUT_MAXIMUM_LENGTH, isEmailAddressInput } from "@/shared/validation/input-validation";
import { copyTextToClipboard } from "@/shared/services/clipboard";
import { useFamilyWorldStore } from "@/stores/family-world";

export interface Emits {
  invited: [invitation: MemberAccountInvitation];
}

const emit = defineEmits<Emits>();
const store = useFamilyWorldStore();
const { t } = useI18n();
const name = ref("");
const email = ref("");
const role = ref<ViewerRole>("child");
const guardianAccess = ref<GuardianAccessLevel>("supporter");
const submitting = ref(false);
const errorMessage = ref("");
const invitation = ref<MemberAccountInvitation>();
const copyFeedback = ref<"error" | "success">();
const roleOptions = computed(() => [
  { title: t("family.roles.child"), value: "child" satisfies ViewerRole },
  { title: t("family.roles.guardian"), value: "guardian" satisfies ViewerRole },
]);
const guardianAccessOptions = computed(() => [
  { title: t("family.permissions.supporter"), value: "supporter" satisfies GuardianAccessLevel },
  { title: t("family.permissions.admin"), value: "admin" satisfies GuardianAccessLevel },
]);
const emailIsValid = computed(() => isEmailAddressInput(email.value));
const submitIsDisabled = computed(
  () => !store.canInviteFamilyMembers || submitting.value || name.value.trim().length < 2 || !emailIsValid.value,
);
const formattedExpiry = computed(() => {
  if (!invitation.value) {
    return "";
  }

  return new Date(invitation.value.expiresAt).toLocaleString();
});

function reset() {
  name.value = "";
  email.value = "";
  role.value = "child";
  guardianAccess.value = "supporter";
  errorMessage.value = "";
  invitation.value = undefined;
  copyFeedback.value = undefined;
}

async function copyActivationCode() {
  if (!invitation.value) {
    return;
  }

  copyFeedback.value = (await copyTextToClipboard(invitation.value.activationCode)) ? "success" : "error";
}

async function inviteMember() {
  if (submitIsDisabled.value) {
    return;
  }

  submitting.value = true;
  errorMessage.value = "";

  try {
    const createdInvitation = await authenticationGateway.inviteMember({
      email: email.value.trim(),
      guardianAccess: role.value === "guardian" ? guardianAccess.value : undefined,
      name: name.value.trim(),
      role: role.value,
    });
    invitation.value = createdInvitation;
    await store.refreshFamilyReadModel("family-profile");
    emit("invited", createdInvitation);
  } catch {
    errorMessage.value = t("family.memberAccount.createError");
  } finally {
    submitting.value = false;
  }
}
</script>

<style lang="scss" scoped>
@use "@/styles/mixins" as *;

.activation-code {
  padding: 1rem;
  border: rem(1) solid var(--lad-border);
  border-radius: rem(14);
  background: var(--lad-surface-soft);
}

.activation-code span,
.activation-code code,
.activation-code small {
  --uno: d-block;
}

.activation-code span,
.activation-code small {
  color: var(--lad-muted);
  font-size: rem(10);
}

.activation-code code {
  margin: 0.5rem 0;
  color: var(--lad-color-primary-deep);
  font-size: rem(20);
  font-weight: var(--lad-font-weight-black);
  letter-spacing: 0.08em;
  overflow-wrap: anywhere;
}

.form-error {
  color: var(--lad-color-danger-strong);
  font-size: rem(11);
}

.result-actions {
  --uno: d-flex flex-wrap;
  gap: 0.5rem;
}

.copy-feedback {
  font-size: rem(10);
}

.copy-feedback--success {
  color: var(--lad-color-primary-deep);
}

.copy-feedback--error {
  color: var(--lad-color-danger-strong);
}
</style>
