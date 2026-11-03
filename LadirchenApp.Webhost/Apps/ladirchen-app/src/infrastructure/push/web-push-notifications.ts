export type WebPushState = "disabled" | "enabled" | "unavailable";
type PushConfiguration = Readonly<{ configured: boolean; publicKey?: string }>;

function decodeApplicationServerKey(value: string): Uint8Array<ArrayBuffer> {
  const padding = "=".repeat((4 - (value.length % 4)) % 4);
  const decoded = window.atob((value + padding).replaceAll("-", "+").replaceAll("_", "/"));
  const bytes = new Uint8Array(new ArrayBuffer(decoded.length));

  for (let index = 0; index < decoded.length; index += 1) {
    bytes[index] = decoded.charCodeAt(index);
  }

  return bytes;
}

function isSupported(): boolean {
  return "serviceWorker" in navigator && "PushManager" in window && "Notification" in window;
}

async function configuration(): Promise<PushConfiguration> {
  const response = await fetch("/api/push/configuration", {
    credentials: "same-origin",
    headers: { Accept: "application/json" },
  });

  if (!response.ok) {
    throw new Error("The Web Push configuration could not be loaded.");
  }

  return (await response.json()) as PushConfiguration;
}

async function saveSubscription(subscription: PushSubscription, locale: string): Promise<void> {
  const serialized = subscription.toJSON();
  const response = await fetch("/api/push/subscriptions", {
    body: JSON.stringify({
      auth: serialized.keys?.auth,
      endpoint: subscription.endpoint,
      locale,
      p256dh: serialized.keys?.p256dh,
    }),
    credentials: "same-origin",
    headers: { "Content-Type": "application/json" },
    method: "POST",
  });

  if (!response.ok) {
    throw new Error("The Web Push subscription could not be saved.");
  }
}

export async function readWebPushState(): Promise<WebPushState> {
  if (!isSupported()) {
    return "unavailable";
  }

  const settings = await configuration();

  if (!settings.configured) {
    return "unavailable";
  }

  const registration = await navigator.serviceWorker.ready;
  const subscription = await registration.pushManager.getSubscription();

  return subscription ? "enabled" : "disabled";
}

export async function enableWebPush(locale: string): Promise<WebPushState> {
  if (!isSupported()) {
    return "unavailable";
  }

  const permission = await Notification.requestPermission();

  if (permission !== "granted") {
    return "disabled";
  }

  const settings = await configuration();

  if (!settings.configured || !settings.publicKey) {
    return "unavailable";
  }

  const registration = await navigator.serviceWorker.ready;
  const existing = await registration.pushManager.getSubscription();
  const subscription =
    existing ??
    (await registration.pushManager.subscribe({
      applicationServerKey: decodeApplicationServerKey(settings.publicKey),
      userVisibleOnly: true,
    }));
  await saveSubscription(subscription, locale);

  return "enabled";
}

export async function disableWebPush(): Promise<WebPushState> {
  if (!isSupported()) {
    return "unavailable";
  }

  const registration = await navigator.serviceWorker.ready;
  const subscription = await registration.pushManager.getSubscription();

  if (subscription) {
    const query = new URLSearchParams({ endpoint: subscription.endpoint });
    const response = await fetch(`/api/push/subscriptions?${query.toString()}`, {
      credentials: "same-origin",
      method: "DELETE",
    });

    if (!response.ok) {
      throw new Error("The Web Push subscription could not be removed.");
    }

    await subscription.unsubscribe();
  }

  return "disabled";
}
