import type { ShopReward } from '@/domain/types';

export const SHOP_REDEMPTION_CUTOFF_HOUR = 18;

export const shopRedemptionIsOpen = (date = new Date()): boolean =>
  date.getHours() < SHOP_REDEMPTION_CUTOFF_HOUR;

const startOfCalendarDay = (date: string): number => new Date(`${date}T00:00:00`).getTime();
const endOfCalendarDay = (date: string): number => new Date(`${date}T23:59:59.999`).getTime();

export const shopRewardIsPublished = (reward: ShopReward, date = new Date()): boolean =>
  reward.isVisible !== false &&
  (!reward.availableFrom || startOfCalendarDay(reward.availableFrom) <= date.getTime()) &&
  (!reward.availableUntil || endOfCalendarDay(reward.availableUntil) >= date.getTime());
