import { afterEach, describe, expect, it, vi } from "vite-plus/test";

const { mount } = vi.hoisted(() => ({ mount: vi.fn() }));

vi.mock("vue", () => ({ createApp: () => ({ mount }) }));
vi.mock("./App.vue", () => ({ default: {} }));
vi.mock("@/plugins", () => ({ registerPlugins: vi.fn() }));
vi.mock("@/plugins/i18n", () => ({ initializeI18n: () => Promise.resolve() }));

describe("application bootstrap", () => {
  afterEach(() => {
    vi.unstubAllGlobals();
    vi.resetModules();
    vi.clearAllMocks();
  });

  it.each(["/#/", "/#/login", "/#/app", "/?source=test#/app/contributions?new=1"])(
    "preserves the hash route when starting at %s",
    async (path) => {
      const location = new URL(path, "http://localhost:3000");
      const replaceState = vi.fn();
      vi.stubGlobal("window", {
        addEventListener: vi.fn(),
        history: { replaceState },
        location,
      });

      await import("./main");
      await vi.waitFor(() => expect(mount).toHaveBeenCalledWith("#app"));

      expect(replaceState).not.toHaveBeenCalled();
      expect(location.href).toBe(`http://localhost:3000${path}`);
    },
  );
});
