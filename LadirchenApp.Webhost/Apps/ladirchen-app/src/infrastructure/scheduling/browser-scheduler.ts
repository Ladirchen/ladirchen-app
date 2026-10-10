import type { Scheduler } from "@/application/ports/scheduler";
export const browserScheduler: Scheduler = {
  schedule(this: void, callback, delayMilliseconds) {

    return window.setTimeout(callback, delayMilliseconds);
  },
  cancel(this: void, handle) {

    return window.clearTimeout(handle);
  },
};
