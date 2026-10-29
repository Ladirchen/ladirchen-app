import { describe, expect, it, vi } from "vite-plus/test";

import { createEmptyFamilyWorldInitialData } from "@/infrastructure/empty-family-world-initial-data";
import { createDomainId } from "@/domain/shared/identifiers";
import { createFamilyWorldState } from "@/stores/family-world-state";
import type { FamilyWorldStoreContext } from "@/stores/family-world-store-context";
import { savingsActions } from "./savings-actions";

function createSavingsActionContext() {
  const familyCommands = { execute: vi.fn().mockResolvedValue(["savings"]) };
  const notify = vi.fn();
  const refreshFamilyReadModel = vi.fn().mockResolvedValue(undefined);
  const context = Object.assign(createFamilyWorldState(createEmptyFamilyWorldInitialData()), {
    $familyWorld: {
      familyCommands,
      familyContext: {
        activeFamilyId: createDomainId.family("c776a9b6-c37d-4a51-91ba-269a67274478"),
      },
    },
    notify,
    refreshFamilyReadModel,
  }) as unknown as FamilyWorldStoreContext;

  return { context, familyCommands, notify, refreshFamilyReadModel };
}

describe("family saving-goal cancellation actions", () => {
  it("requests family approval and reports the pending state", async () => {
    const { context, familyCommands, notify, refreshFamilyReadModel } = createSavingsActionContext();
    const goalId = createDomainId.savingGoal("family-goal");

    await savingsActions.requestGoalCancellation.call(context, goalId);

    expect(familyCommands.execute).toHaveBeenCalledWith(
      context.$familyWorld.familyContext.activeFamilyId,
      context.signedInMemberId,
      "savings.request-goal-cancellation",
      { id: goalId },
    );
    expect(refreshFamilyReadModel).toHaveBeenCalledWith("savings");
    expect(notify).toHaveBeenCalledWith("notifications.goals.cancellationRequested");
  });

  it("sends approval and rejection decisions without calculating votes locally", async () => {
    const { context, familyCommands } = createSavingsActionContext();
    const goalId = createDomainId.savingGoal("family-goal");

    await savingsActions.decideGoalCancellation.call(context, goalId, true);
    await savingsActions.decideGoalCancellation.call(context, goalId, false);

    expect(familyCommands.execute).toHaveBeenNthCalledWith(
      1,
      context.$familyWorld.familyContext.activeFamilyId,
      context.signedInMemberId,
      "savings.decide-goal-cancellation",
      { approved: true, id: goalId },
    );
    expect(familyCommands.execute).toHaveBeenNthCalledWith(
      2,
      context.$familyWorld.familyContext.activeFamilyId,
      context.signedInMemberId,
      "savings.decide-goal-cancellation",
      { approved: false, id: goalId },
    );
  });
});
