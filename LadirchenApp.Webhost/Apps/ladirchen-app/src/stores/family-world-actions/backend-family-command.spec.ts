import { describe, expect, it, vi } from "vite-plus/test";

import { createDomainId } from "@/domain/shared/identifiers";
import { FamilyCommandHttpError } from "@/infrastructure/http/http-family-command-gateway";
import { createFamilyWorldTestDependencies, createFamilyWorldTestStore } from "@/stores/family-world-test-support";
import { executeBackendFamilyCommand } from "./backend-family-command";

describe("executeBackendFamilyCommand", () => {
  it("restores a server session changed in another browser tab without replaying the command", async () => {
    const originalMemberId = createDomainId.familyMember("original-member");
    const currentMemberId = createDomainId.familyMember("current-member");
    const execute = vi.fn().mockRejectedValue(new FamilyCommandHttpError(403, "Forbidden"));
    const context = createFamilyWorldTestStore(createFamilyWorldTestDependencies({ familyCommands: { execute } }));
    context.signedInMemberId = originalMemberId;
    vi.spyOn(context, "restoreAuthenticatedSession").mockImplementation(async () => {
      context.signedInMemberId = currentMemberId;

      return true;
    });
    const notify = vi.spyOn(context, "notify").mockImplementation(() => undefined);

    await executeBackendFamilyCommand(
      context,
      "reward-shop.republish",
      { id: createDomainId.shopReward("reward") },
      "notifications.save.shop",
    );

    expect(execute).toHaveBeenCalledOnce();
    expect(context.restoreAuthenticatedSession).toHaveBeenCalledOnce();
    expect(notify).toHaveBeenCalledWith("notifications.auth.sessionChanged");
  });

  it("returns to sign-in when the server session has expired", async () => {
    const execute = vi.fn().mockRejectedValue(new FamilyCommandHttpError(401, "Unauthorized"));
    const context = createFamilyWorldTestStore(createFamilyWorldTestDependencies({ familyCommands: { execute } }));
    vi.spyOn(context, "restoreAuthenticatedSession").mockResolvedValue(false);
    const notify = vi.spyOn(context, "notify").mockImplementation(() => undefined);

    await executeBackendFamilyCommand(
      context,
      "reward-shop.republish",
      { id: createDomainId.shopReward("reward") },
      "notifications.save.shop",
    );

    expect(context.restoreAuthenticatedSession).toHaveBeenCalledOnce();
    expect(notify).toHaveBeenCalledWith("notifications.auth.sessionExpired");
  });

  it("keeps the command-specific notification for a valid session", async () => {
    const execute = vi.fn().mockRejectedValue(new FamilyCommandHttpError(403, "Forbidden"));
    const context = createFamilyWorldTestStore(createFamilyWorldTestDependencies({ familyCommands: { execute } }));
    vi.spyOn(context, "restoreAuthenticatedSession").mockResolvedValue(true);
    const notify = vi.spyOn(context, "notify").mockImplementation(() => undefined);

    await executeBackendFamilyCommand(
      context,
      "reward-shop.republish",
      { id: createDomainId.shopReward("reward") },
      "notifications.save.shop",
    );

    expect(notify).toHaveBeenCalledWith("notifications.save.shop");
  });
});
