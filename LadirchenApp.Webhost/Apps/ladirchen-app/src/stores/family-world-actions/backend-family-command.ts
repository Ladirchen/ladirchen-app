import type { FamilyCommandPayload, FamilyCommandType } from "@/application/ports/family-command-gateway";
import { FamilyCommandHttpError } from "@/infrastructure/http/http-family-command-gateway";
import type { FamilyWorldStoreContext } from "@/stores/family-world-store-context";

function isAuthenticationFailure(error: unknown): error is FamilyCommandHttpError {
  return (
    error instanceof FamilyCommandHttpError &&
    (error.status === 401 || (error.status === 403 && error.message === "Family command failed with HTTP 403."))
  );
}

export async function executeBackendFamilyCommand<TCommandType extends FamilyCommandType>(
  store: FamilyWorldStoreContext,
  commandType: TCommandType,
  payload: FamilyCommandPayload<NoInfer<TCommandType>>,
  failureNotification:
    | "notifications.save.contributions"
    | "notifications.save.profile"
    | "notifications.save.savings"
    | "notifications.save.shop"
    | "notifications.save.home",
  onSuccess?: () => void,
): Promise<void> {
  const { familyCommands, familyContext } = store.$familyWorld;

  try {
    const changedReadModelTypes = await familyCommands.execute(
      familyContext.activeFamilyId,
      store.signedInMemberId,
      commandType,
      payload,
    );
    await Promise.all(changedReadModelTypes.map((readModelType) => store.refreshFamilyReadModel(readModelType)));
    onSuccess?.();
  } catch (error: unknown) {
    if (isAuthenticationFailure(error)) {
      const previousFamilyId = familyContext.activeFamilyId;
      const previousMemberId = store.signedInMemberId;
      const restored = await store.restoreAuthenticatedSession();

      if (
        restored &&
        (familyContext.activeFamilyId !== previousFamilyId || store.signedInMemberId !== previousMemberId)
      ) {
        store.notify("notifications.auth.sessionChanged");

        return;
      }

      if (!restored) {
        store.notify("notifications.auth.sessionExpired");

        return;
      }
    }

    store.notify(failureNotification);
  }
}
