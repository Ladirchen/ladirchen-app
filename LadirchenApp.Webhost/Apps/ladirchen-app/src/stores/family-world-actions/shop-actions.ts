import { createDomainId } from "@/domain/shared/identifiers";
import type { FamilyMemberId, ShopRewardId } from "@/domain/shared/identifiers";
import type { NewShopReward } from "@/domain/shop/types";
import type { FamilyWorldActionGroup, FamilyWorldStoreContext } from "@/stores/family-world-store-context";
import { executeBackendFamilyCommand } from "./backend-family-command";
import { createUuid } from "./family-world-store-utils";

export const shopActions = {
  async addShopReward(this: FamilyWorldStoreContext, input: NewShopReward) {
    const id = createDomainId.shopReward(createUuid());
    await executeBackendFamilyCommand(this, "reward-shop.add", { id, ...input }, "notifications.save.shop", () => {
      if (input.isVisible === false) {
        this.notify("notifications.shop.savedHidden", { title: input.title });

        return;
      }

      if (input.availableFrom) {
        this.notify("notifications.shop.savedScheduled", { date: input.availableFrom, title: input.title });

        return;
      }

      this.notify("notifications.shop.savedVisible", { title: input.title });
    });
  },
  async setShopRewardVisibility(this: FamilyWorldStoreContext, id: ShopRewardId, isVisible: boolean) {
    const title = this.shopRewards.find((item) => item.id === id)?.title ?? "";
    await executeBackendFamilyCommand(
      this,
      "reward-shop.set-visibility",
      { id, isVisible },
      "notifications.save.shop",
      () => this.notify(isVisible ? "notifications.shop.visible" : "notifications.shop.hidden", { title }),
    );
  },
  async setShopTaskGateOverride(this: FamilyWorldStoreContext, memberId: FamilyMemberId, enabled: boolean) {
    await executeBackendFamilyCommand(
      this,
      "reward-shop.set-task-gate-override",
      { enabled, memberId },
      "notifications.save.shop",
      () => this.notify(enabled ? "notifications.shop.taskGateOpened" : "notifications.shop.taskGateRestored"),
    );
  },
  async updateShopReward(this: FamilyWorldStoreContext, id: ShopRewardId, input: NewShopReward) {
    await executeBackendFamilyCommand(this, "reward-shop.update", { id, ...input }, "notifications.save.shop", () =>
      this.notify("notifications.shop.updated", { title: input.title }),
    );
  },
  async republishShopReward(this: FamilyWorldStoreContext, id: ShopRewardId) {
    const title = this.shopRewards.find((item) => item.id === id)?.title ?? "";
    await executeBackendFamilyCommand(this, "reward-shop.republish", { id }, "notifications.save.shop", () =>
      this.notify("notifications.shop.republished", { title }),
    );
  },
  async deleteShopReward(this: FamilyWorldStoreContext, id: ShopRewardId) {
    const title = this.shopRewards.find((item) => item.id === id)?.title ?? "";
    await executeBackendFamilyCommand(this, "reward-shop.delete", { id }, "notifications.save.shop", () =>
      this.notify("notifications.shop.deleted", { title }),
    );
  },
  async requestShopReward(this: FamilyWorldStoreContext, id: ShopRewardId) {
    const reward = this.shopRewards.find((item) => item.id === id);
    await executeBackendFamilyCommand(this, "reward-shop.request", { id }, "notifications.save.shop", () =>
      this.notify("notifications.shop.reserved", { price: reward?.price ?? 0, title: reward?.title ?? "" }),
    );
  },
  async cancelShopRewardRequest(this: FamilyWorldStoreContext, id: ShopRewardId) {
    await executeBackendFamilyCommand(this, "reward-shop.cancel", { id }, "notifications.save.shop", () =>
      this.notify("notifications.shop.reservationCancelled"),
    );
  },
  async decideShopReward(this: FamilyWorldStoreContext, id: ShopRewardId, approved: boolean) {
    const title = this.shopRewards.find((item) => item.id === id)?.title ?? "";
    await executeBackendFamilyCommand(this, "reward-shop.decide", { approved, id }, "notifications.save.shop", () =>
      this.notify(approved ? "notifications.shop.approved" : "notifications.shop.rejected", { title }),
    );
  },
  async pauseShopTimer(this: FamilyWorldStoreContext, requestId: string) {
    await executeBackendFamilyCommand(this, "reward-shop.pause-timer", { requestId }, "notifications.save.shop");
  },
  async resumeShopTimer(this: FamilyWorldStoreContext, requestId: string) {
    await executeBackendFamilyCommand(this, "reward-shop.resume-timer", { requestId }, "notifications.save.shop");
  },
  async resetShopTimer(this: FamilyWorldStoreContext, requestId: string) {
    await executeBackendFamilyCommand(this, "reward-shop.reset-timer", { requestId }, "notifications.save.shop");
  },
  async cancelShopTimer(this: FamilyWorldStoreContext, requestId: string) {
    await executeBackendFamilyCommand(this, "reward-shop.cancel-timer", { requestId }, "notifications.save.shop");
  },
} satisfies FamilyWorldActionGroup;
