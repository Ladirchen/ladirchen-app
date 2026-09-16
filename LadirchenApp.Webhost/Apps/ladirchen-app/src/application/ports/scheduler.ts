export interface Scheduler {
  schedule: (callback: () => void, delayMilliseconds: number) => ReturnType<typeof setTimeout>;
  cancel: (handle: ReturnType<typeof setTimeout>) => void;
}
