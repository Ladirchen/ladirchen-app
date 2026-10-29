import type { FamilyCommandPayload } from "@/application/ports/family-command-gateway";
import type { FamilyWorldStoreContext } from "@/stores/family-world-store-context";

export async function executeBackendFamilyCommand(
  store: FamilyWorldStoreContext,
  commandType: string,
  payload: FamilyCommandPayload,
  failureNotification:
    | "notifications.save.contributions"
    | "notifications.save.profile"
    | "notifications.save.savings"
    | "notifications.save.shop"
    | "notifications.save.home",
  onSuccess?: () => void,
): Promise<boolean> {
  const { familyCommands, familyContext } = store.$familyWorld;

  if (!familyCommands.backendDriven) {
    return false;
  }

  try {
    const changedAggregates = await familyCommands.execute(
      familyContext.activeFamilyId,
      store.signedInMemberId,
      commandType,
      payload,
    );
    await Promise.all(changedAggregates.map((aggregateType) => store.refreshFamilyAggregate(aggregateType)));
    onSuccess?.();
  } catch {
    store.notify(failureNotification);
  }

  return true;
}
