import { describe, expect, it, vi } from "vite-plus/test";

import type { FamilyReadModelType } from "@/application/contracts/family-read-model-contract";
import { createContributions } from "@/infrastructure/fixtures/family-world-fixtures";
import { createFamilyWorldTestDependencies, createFamilyWorldTestStore } from "@/stores/family-world-test-support";

describe("contribution reward actions", () => {
  it("closes the reward dialog before persisting the acknowledgement", async () => {
    let completeCommand: ((readModelTypes: ReadonlyArray<FamilyReadModelType>) => void) | undefined;

    function executeCommand() {
      return new Promise<ReadonlyArray<FamilyReadModelType>>((resolve) => {
        completeCommand = resolve;
      });
    }

    const execute = vi.fn(executeCommand);
    const context = createFamilyWorldTestStore(createFamilyWorldTestDependencies({ familyCommands: { execute } }));
    const contribution = createContributions()[0]!;
    context.contributions = [contribution];
    context.rewardAnimation.contributionId = contribution.id;
    context.rewardAnimation.visible = true;

    const dismissal = context.dismissRewardAnimation();

    expect(context.rewardAnimation.visible).toBe(false);
    expect(context.rewardAnimation.contributionId).toBeUndefined();

    completeCommand?.([]);
    await dismissal;
  });
});
