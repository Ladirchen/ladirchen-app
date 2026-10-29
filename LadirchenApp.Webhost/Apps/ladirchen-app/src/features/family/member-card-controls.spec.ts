import { describe, expect, it } from "vite-plus/test";

import { familyMemberCardControls } from "./member-card-controls";
import type { FamilyMember } from "@/domain/family/types";
import { createDomainId } from "@/domain/shared/identifiers";
import { familyMemberColorPalette } from "@/theme/color-palette";

const administratorId = createDomainId.familyMember("administrator");
const guardianId = createDomainId.familyMember("guardian");
const guardian: FamilyMember = {
  avatar: "🧑",
  color: familyMemberColorPalette.hannes,
  guardianAccess: "supporter",
  id: guardianId,
  name: "Guardian",
  role: "guardian",
  weeklyStreak: 0,
};

describe("family member card controls", () => {
  it("shows guardian controls for an active member managed by an administrator", () => {
    expect(familyMemberCardControls(guardian, administratorId, true)).toEqual({
      canEditAccess: true,
      showParticipation: true,
    });
  });

  it("hides all mutable controls while the member invitation is pending", () => {
    expect(familyMemberCardControls({ ...guardian, invitationPending: true }, administratorId, true)).toEqual({
      canEditAccess: false,
      showParticipation: false,
    });
  });
});
