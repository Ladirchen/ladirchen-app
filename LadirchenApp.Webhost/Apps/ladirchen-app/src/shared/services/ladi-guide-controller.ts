export type LadiGuideActionId =
  | "house-energy:next"
  | "piggy:next"
  | "savings-interest:next"
  | "savings-interest:start";

export interface LadiGuideMessage {
  readonly actionId?: LadiGuideActionId;
  readonly actionLabel?: string;
  readonly celebration?: "gift";
  readonly heading?: string;
  readonly message: string;
  readonly pageIntro?: boolean;
  readonly progress?: string;
  readonly smart?: boolean;
}

type GuideMessageListener = (message: LadiGuideMessage) => void;
type GuideAction = () => void;

class LadiGuideController {
  private readonly actionHandlers = new Map<LadiGuideActionId, Set<GuideAction>>();
  private readonly messageListeners = new Set<GuideMessageListener>();

  public registerAction(actionId: LadiGuideActionId, action: GuideAction): () => void {
    const handlers = this.actionHandlers.get(actionId) ?? new Set<GuideAction>();
    handlers.add(action);
    this.actionHandlers.set(actionId, handlers);
    return () => {
      handlers.delete(action);
      if (handlers.size === 0) {this.actionHandlers.delete(actionId);}
    };
  }

  public say(message: LadiGuideMessage): void {
    for (const listener of this.messageListeners) {listener(message);}
  }

  public subscribe(listener: GuideMessageListener): () => void {
    this.messageListeners.add(listener);
    return () => this.messageListeners.delete(listener);
  }

  public trigger(actionId: LadiGuideActionId): void {
    for (const action of this.actionHandlers.get(actionId) ?? []) {action();}
  }
}

export const ladiGuideController = new LadiGuideController();
