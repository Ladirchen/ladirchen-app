import type { Promotion } from './types';

export const promotionDeadline = (deadline: string, now = new Date()): Date => {
  const [hours = 0, minutes = 0] = deadline.split(':').map(Number);
  const end = new Date(now);
  end.setHours(hours, minutes, 0, 0);
  return end;
};

export const isPromotionAvailable = (promotion: Promotion, now = new Date()): boolean =>
  promotion.active && promotionDeadline(promotion.deadline, now).getTime() > now.getTime();
