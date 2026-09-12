export const SHOP_REDEMPTION_CUTOFF_HOUR = 18;

export const shopRedemptionIsOpen = (date = new Date()): boolean =>
  date.getHours() < SHOP_REDEMPTION_CUTOFF_HOUR;
