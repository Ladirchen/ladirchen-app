import { createPinia } from "pinia";
import { createSSRApp } from "vue";
import { renderToString } from "vue/server-renderer";
import { createI18n } from "vue-i18n";
import { createMemoryHistory, createRouter } from "vue-router";
import { describe, expect, it, vi } from "vite-plus/test";
import { NO_FAMILY_PERMISSIONS } from "@/domain/family/permissions";
import type { FamilyMember } from "@/domain/family/types";
import { createDomainId } from "@/domain/shared/identifiers";
import { createEmptyFamilyWorldInitialData } from "@/infrastructure/empty-family-world-initial-data";
import de from "@/locales/de.json";
import en from "@/locales/en.json";
import { createFamilyWorldStoreDefinition, FAMILY_WORLD_STORE_DEFINITION } from "@/stores/family-world";
import { familyMemberColorPalette } from "@/theme/color-palette";
import FamilyWorldHeader from "./FamilyWorldHeader.vue";
import { APPLICATION_INFO_GATEWAY } from "@/shared/composables/use-application-info";

vi.mock("/ladirchen-logo.png", () => ({ default: "/ladirchen-logo.png" }));

async function createHeader(locale = "en") {
  const pinia = createPinia();
  const useStore = createFamilyWorldStoreDefinition({ create: createEmptyFamilyWorldInitialData });
  const store = useStore(pinia);
  const router = createRouter({
    history: createMemoryHistory(),
    routes: [{ path: "/:pathMatch(.*)*", component: { render: () => null } }],
  });
  const app = createSSRApp(FamilyWorldHeader);
  app.use(pinia);
  app.use(createI18n({ legacy: false, locale, messages: { de, en } }));
  app.use(router);
  app.provide(FAMILY_WORLD_STORE_DEFINITION, useStore);
  app.provide(APPLICATION_INFO_GATEWAY, { getInfo: vi.fn() });
  await router.push("/app");
  await router.isReady();

  return { app, store };
}

function createMember(id: string, name: string, role: FamilyMember["role"]): FamilyMember {
  return {
    avatar: "",
    color: familyMemberColorPalette.mama,
    id: createDomainId.familyMember(id),
    name,
    permissions: NO_FAMILY_PERMISSIONS,
    role,
    weeklyStreak: 0,
  };
}

describe("current user in the family world header", () => {
  it.each(["guardian", "child"] as const)("shows the signed-in %s rather than the selected child", async (role) => {
    const { app, store } = await createHeader();
    const member = createMember("signed-in", "Alex", role);
    const child = createMember("selected-child", "Sam", "child");
    store.members = [member, child];
    store.signedInMemberId = member.id;
    store.activeChildId = child.id;
    store.viewerRole = role;

    const html = await renderToString(app);

    expect(html).toContain('aria-label="Signed in as Alex. Open profile"');
    expect(html).toContain('href="/app/profile"');
    expect(html).toContain('class="text-truncate">Alex</span>');
    expect(html).toContain(role === "guardian" ? "header-guardian-week" : "header-streak");
    expect(html).not.toContain("Signed in as Sam");
  });

  it("updates the name after a member switch and uses the existing nickname preference", async () => {
    const { app, store } = await createHeader();
    const guardian = createMember("guardian", "Alex", "guardian");
    const child = { ...createMember("child", "Sam", "child"), nickname: "  Sunny  " };
    store.members = [guardian, child];
    store.signedInMemberId = guardian.id;
    expect(await renderToString(app)).toContain("Signed in as Alex");

    store.signedInMemberId = child.id;
    store.viewerRole = "child";
    expect(await renderToString(app)).toContain("Signed in as Sunny");

    store.members[1]!.nickname = " ";
    expect(await renderToString(app)).toContain("Signed in as Sam");
  });

  it("shows the identity for external guardians even without header statistics", async () => {
    const { app, store } = await createHeader("de");
    const member = { ...createMember("external", "Alex", "guardian"), guardianAccess: "external" as const };
    store.members = [member];
    store.signedInMemberId = member.id;
    store.viewerRole = "guardian";

    const html = await renderToString(app);

    expect(html).toContain('aria-label="Angemeldet als Alex. Profil öffnen"');
    expect(html).not.toContain("header-guardian-week");
  });

  it("does not invent a current user before member hydration", async () => {
    const { app } = await createHeader();

    expect(await renderToString(app)).not.toContain("header-current-user");
  });

  it("retains the complete long name accessibly and escapes user-provided text", async () => {
    const { app, store } = await createHeader();
    const name = 'Alex <script>alert("name")</script> with a very long family name';
    const member = createMember("long-name", name, "guardian");
    store.members = [member];
    store.signedInMemberId = member.id;

    const html = await renderToString(app);

    expect(html).toContain('title="Signed in as Alex &lt;script&gt;');
    expect(html).toContain("with a very long family name. Open profile");
    expect(html).not.toContain("<script>");
  });
});
