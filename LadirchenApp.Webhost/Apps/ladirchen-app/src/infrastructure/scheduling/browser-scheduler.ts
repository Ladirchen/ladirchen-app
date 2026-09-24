import type { Scheduler } from "@/application/ports/scheduler";

export const browserScheduler: Scheduler = {
  schedule: (callback, delayMilliseconds) => window.setTimeout(callback, delayMilliseconds),
  cancel: handle => window.clearTimeout(handle),
};
