import { describe, expect, it } from "vite-plus/test";
import type { Contribution } from "./types";
import { totalEarnedContributionReward } from "./rating";

function contribution(overrides: Partial<Contribution> = {}): Contribution {
  return {
    id: "contribution-1",
    title: "Clean the room",
    description: "Put the clothes in the wardrobe",
    icon: "✨",
    area: "",
    kind: "basic",
    status: "approved",
    reward: 10,
    energy: 10,
    dueLabel: "Today",
    ...overrides,
  } as Contribution;
}

describe("totalEarnedContributionReward", () => {
  it("includes the persisted five-star bonus in the displayed reward", () => {
    expect(totalEarnedContributionReward(contribution({ earnedReward: 10, earnedRatingBonus: 2 }))).toBe(12);
  });

  it("falls back to the configured reward before an earned reward exists", () => {
    expect(totalEarnedContributionReward(contribution({ status: "available" }))).toBe(10);
  });
});
