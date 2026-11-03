self.addEventListener("push", (event) => {
  if (!event.data) {
    return;
  }

  const payload = event.data.json();
  event.waitUntil(
    self.registration.showNotification(payload.title, {
      badge: "/ladirli-icon-192x192.png",
      body: payload.body,
      data: { url: payload.url },
      icon: "/ladirli-icon-192x192.png",
      tag: payload.tag,
    }),
  );
});

self.addEventListener("notificationclick", (event) => {
  event.notification.close();
  const destination = new URL(event.notification.data?.url ?? "/", self.location.origin).href;
  event.waitUntil(
    (async () => {
      const windows = await self.clients.matchAll({ includeUncontrolled: true, type: "window" });
      const existing = windows.find((client) => client.url.startsWith(self.location.origin));

      if (existing) {
        await existing.navigate(destination);

        return existing.focus();
      }

      return self.clients.openWindow(destination);
    })(),
  );
});
