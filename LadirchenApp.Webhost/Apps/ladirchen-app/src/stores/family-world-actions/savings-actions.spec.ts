import { describe, expect, it, vi } from "vite-plus/test";

import { createDomainId } from "@/domain/shared/identifiers";
import { createFamilyWorldTestDependencies, createFamilyWorldTestStore } from "@/stores/family-world-test-support";

function createSavingsActionContext() {
  const execute = vi.fn(async () => ["savings"] as const);
  const context = createFamilyWorldTestStore(createFamilyWorldTestDependencies({ familyCommands: { execute } }));
  const notify = vi.spyOn(context, "notify").mockImplementation(() => undefined);
  const refreshFamilyReadModel = vi.spyOn(context, "refreshFamilyReadModel").mockResolvedValue(undefined);

  return { context, execute, notify, refreshFamilyReadModel };
}

describe("family saving-goal cancellation actions", () => {
  it("requests family approval and reports the pending state", async () => {
    const { context, execute, notify, refreshFamilyReadModel } = createSavingsActionContext();
    const goalId = createDomainId.savingGoal("family-goal");

    await context.requestGoalCancellation(goalId);

    expect(execute).toHaveBeenCalledWith(
      context.$familyWorld.familyContext.activeFamilyId,
      context.signedInMemberId,
      "savings.request-goal-cancellation",
      { id: goalId },
    );
    expect(refreshFamilyReadModel).toHaveBeenCalledWith("savings");
    expect(notify).toHaveBeenCalledWith("notifications.goals.cancellationRequested");
  });

  it("sends approval and rejection decisions without calculating votes locally", async () => {
    const { context, execute } = createSavingsActionContext();
    const goalId = createDomainId.savingGoal("family-goal");

    await context.decideGoalCancellation(goalId, true);
    await context.decideGoalCancellation(goalId, false);

    expect(execute).toHaveBeenNthCalledWith(
      1,
      context.$familyWorld.familyContext.activeFamilyId,
      context.signedInMemberId,
      "savings.decide-goal-cancellation",
      { approved: true, id: goalId },
    );
    expect(execute).toHaveBeenNthCalledWith(
      2,
      context.$familyWorld.familyContext.activeFamilyId,
      context.signedInMemberId,
      "savings.decide-goal-cancellation",
      { approved: false, id: goalId },
    );
  });
});
