import { shopRedemptionIsOpen, shopRewardIsPublished } from "@/domain/shop";
import { createDomainId } from "@/domain/shared/identifiers";
import type { ShopRewardId } from "@/domain/shared/identifiers";
import type { NewShopReward } from "@/domain/shop/types";
import { createUuid } from "./family-world-store-utils";
import type { FamilyWorldActionGroup, FamilyWorldStoreContext } from "@/stores/family-world-store-context";
import { executeBackendFamilyCommand } from "./backend-family-command";

export const shopActions = {
  async addShopReward(this: FamilyWorldStoreContext, input: NewShopReward) {
    const id = createDomainId.shopReward(createUuid());

    if (
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
      })
    ) {
      return;
    }

    if (!this.permissions.canManageContent) {
      return;
    }

    this.shopRewards.unshift({
      id,
      ...input,
      isVisible: input.isVisible ?? true,
      status: "available",
    });
    this.persistRewardShop();

    if (input.isVisible === false) {
      this.notify("notifications.shop.savedHidden", { title: input.title });

      return;
    }

    if (input.availableFrom) {
      this.notify("notifications.shop.savedScheduled", { title: input.title, date: input.availableFrom });

      return;
    }

    this.notify("notifications.shop.savedVisible", { title: input.title });
  },
  async setShopRewardVisibility(this: FamilyWorldStoreContext, id: ShopRewardId, isVisible: boolean) {
    const title = this.shopRewards.find((item) => item.id === id)?.title ?? "";

    if (
      await executeBackendFamilyCommand(
        this,
        "reward-shop.set-visibility",
        { id, isVisible },
        "notifications.save.shop",
        () => this.notify(isVisible ? "notifications.shop.visible" : "notifications.shop.hidden", { title }),
      )
    ) {
      return;
    }

    if (!this.permissions.canManageContent) {
      return;
    }

    const reward = this.shopRewards.find((item) => item.id === id);

    if (!reward) {
      return;
    }

    reward.isVisible = isVisible;
    this.persistRewardShop();
    this.notify(isVisible ? "notifications.shop.visible" : "notifications.shop.hidden", { title: reward.title });
  },
  async deleteShopReward(this: FamilyWorldStoreContext, id: ShopRewardId) {
    const title = this.shopRewards.find((item) => item.id === id)?.title ?? "";

    if (
      await executeBackendFamilyCommand(this, "reward-shop.delete", { id }, "notifications.save.shop", () =>
        this.notify("notifications.shop.deleted", { title }),
      )
    ) {
      return;
    }

    if (!this.permissions.canManageContent) {
      return;
    }

    const rewardIndex = this.shopRewards.findIndex((item) => item.id === id);

    if (rewardIndex < 0) {
      return;
    }

    const [reward] = this.shopRewards.splice(rewardIndex, 1);
    this.persistRewardShop();
    this.notify("notifications.shop.deleted", { title: reward?.title ?? "" });
  },
  async requestShopReward(this: FamilyWorldStoreContext, id: ShopRewardId) {
    const rewardBeforeRequest = this.shopRewards.find((item) => item.id === id);

    if (
      await executeBackendFamilyCommand(this, "reward-shop.request", { id }, "notifications.save.shop", () =>
        this.notify("notifications.shop.reserved", {
          price: rewardBeforeRequest?.price ?? 0,
          title: rewardBeforeRequest?.title ?? "",
        }),
      )
    ) {
      return;
    }

    const reward = this.shopRewards.find((item) => item.id === id);

    if (
      this.viewerRole !== "child" ||
      !reward ||
      reward.status !== "available" ||
      reward.quantity < 1 ||
      !shopRewardIsPublished(reward, this.familyTimeZone) ||
      reward.price > this.availableBalance
    ) {
      return;
    }

    if (!shopRedemptionIsOpen(this.familyTimeZone)) {
      this.notify("notifications.shop.closed");

      return;
    }

    reward.status = "requested";
    reward.requesterId = this.activeChildId;
    this.persistRewardShop();
    this.notify("notifications.shop.reserved", { price: reward.price, title: reward.title });
  },
  async cancelShopRewardRequest(this: FamilyWorldStoreContext, id: ShopRewardId) {
    if (
      await executeBackendFamilyCommand(this, "reward-shop.cancel", { id }, "notifications.save.shop", () =>
        this.notify("notifications.shop.reservationCancelled"),
      )
    ) {
      return;
    }

    const reward = this.shopRewards.find((item) => item.id === id && item.requesterId === this.activeChildId);

    if (!reward || reward.status !== "requested") {
      return;
    }

    reward.status = "available";
    reward.requesterId = undefined;
    this.persistRewardShop();
    this.notify("notifications.shop.reservationCancelled");
  },
  async decideShopReward(this: FamilyWorldStoreContext, id: ShopRewardId, approved: boolean) {
    const title = this.shopRewards.find((item) => item.id === id)?.title ?? "";

    if (
      await executeBackendFamilyCommand(this, "reward-shop.decide", { approved, id }, "notifications.save.shop", () =>
        this.notify(approved ? "notifications.shop.approved" : "notifications.shop.rejected", { title }),
      )
    ) {
      return;
    }

    const reward = this.shopRewards.find((item) => item.id === id);

    if (!this.permissions.canManageContent || !reward || reward.status !== "requested") {
      return;
    }

    if (approved) {
      if (!shopRedemptionIsOpen(this.familyTimeZone)) {
        this.notify("notifications.shop.approvalClosed");

        return;
      }

      const requesterId = reward.requesterId;

      if (!requesterId) {
        return;
      }

      this.balances[requesterId] = this.balanceFor(requesterId) - reward.price;
      reward.quantity = Math.max(0, reward.quantity - 1);
      reward.status = reward.quantity > 0 ? "available" : "redeemed";
      reward.requesterId = undefined;
      this.persistSavings();
      this.notify("notifications.shop.approved", { title: reward.title });
    } else {
      reward.status = "available";
      reward.requesterId = undefined;
      this.notify("notifications.shop.rejected");
    }

    this.persistRewardShop();
  },
} satisfies FamilyWorldActionGroup;
