import type { FamilyCommandPayload, FamilyCommandType } from "@/application/ports/family-command-gateway";
import type { FamilyWorldStoreContext } from "@/stores/family-world-store-context";

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
  } catch {
    store.notify(failureNotification);
  }
}
