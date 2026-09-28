import { shopRedemptionIsOpen, shopRewardIsPublished } from "@/domain/shop";
import { createDomainId } from "@/domain/shared/identifiers";
import type { ShopRewardId } from "@/domain/shared/identifiers";
import type { NewShopReward } from "@/domain/shop/types";
import { createUuid } from "./family-world-store-utils";
import type { FamilyWorldActionGroup, FamilyWorldStoreContext } from "@/stores/family-world-store-context";

export const shopActions = {
  addShopReward(this: FamilyWorldStoreContext, input: NewShopReward) {
    if (!this.permissions.canManageContent) {return;}
    this.shopRewards.unshift({ id: createDomainId.shopReward(createUuid()), ...input, isVisible: input.isVisible ?? true, status: "available" });
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
  setShopRewardVisibility(this: FamilyWorldStoreContext, id: ShopRewardId, isVisible: boolean) {
    if (!this.permissions.canManageContent) {return;}
    const reward = this.shopRewards.find(item => item.id === id);
    if (!reward) {return;}
    reward.isVisible = isVisible;
    this.persistRewardShop();
    this.notify(isVisible ? "notifications.shop.visible" : "notifications.shop.hidden", { title: reward.title });
  },
  deleteShopReward(this: FamilyWorldStoreContext, id: ShopRewardId) {
    if (!this.permissions.canManageContent) {return;}
    const rewardIndex = this.shopRewards.findIndex(item => item.id === id);
    if (rewardIndex < 0) {return;}
    const [reward] = this.shopRewards.splice(rewardIndex, 1);
    this.persistRewardShop();
    this.notify("notifications.shop.deleted", { title: reward?.title ?? "" });
  },
  requestShopReward(this: FamilyWorldStoreContext, id: ShopRewardId) {
    const reward = this.shopRewards.find((item) => item.id === id);
    if (this.viewerRole !== "child" || !reward || reward.status !== "available" || reward.quantity < 1 || !shopRewardIsPublished(reward, this.familyTimeZone) || reward.price > this.availableBalance) {return;}
    if (!shopRedemptionIsOpen(this.familyTimeZone)) {
      this.notify("notifications.shop.closed");
      return;
    }
    reward.status = "requested";
    reward.requesterId = this.activeChildId;
    this.persistRewardShop();
    this.notify("notifications.shop.reserved", { price: reward.price, title: reward.title });
  },
  cancelShopRewardRequest(this: FamilyWorldStoreContext, id: ShopRewardId) {
    const reward = this.shopRewards.find((item) => item.id === id && item.requesterId === this.activeChildId);
    if (!reward || reward.status !== "requested") {return;}
    reward.status = "available";
    reward.requesterId = undefined;
    this.persistRewardShop();
    this.notify("notifications.shop.reservationCancelled");
  },
  decideShopReward(this: FamilyWorldStoreContext, id: ShopRewardId, approved: boolean) {
    const reward = this.shopRewards.find((item) => item.id === id);
    if (!this.permissions.canManageContent || !reward || reward.status !== "requested") {return;}
    if (approved) {
      if (!shopRedemptionIsOpen(this.familyTimeZone)) {
        this.notify("notifications.shop.approvalClosed");
        return;
      }
      const requesterId = reward.requesterId;
      if (!requesterId) {return;}
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
