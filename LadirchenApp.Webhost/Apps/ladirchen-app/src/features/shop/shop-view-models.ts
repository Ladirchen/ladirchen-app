export type ShopRewardPublicationTone = "expired" | "hidden" | "scheduled" | "visible";

export interface ShopRewardPublicationStatus {
  readonly icon: string;
  readonly label: string;
  readonly tone: ShopRewardPublicationTone;
}
