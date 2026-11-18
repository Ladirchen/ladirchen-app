import { describe, expect, it } from "vite-plus/test";
import { createDomainId } from "@/domain/shared/identifiers";
import { matchesContributionPersonFilter } from "./contribution-person-filter";

describe("matchesContributionPersonFilter", () => {
  const firstMember = createDomainId.familyMember("member-1");
  const secondMember = createDomainId.familyMember("member-2");

  it("includes assigned and unassigned contributions when all people are selected", () => {
    expect(matchesContributionPersonFilter(firstMember, "all")).toBe(true);
    expect(matchesContributionPersonFilter(undefined, "all")).toBe(true);
  });

  it("includes only contributions assigned to the selected person", () => {
    expect(matchesContributionPersonFilter(firstMember, firstMember)).toBe(true);
    expect(matchesContributionPersonFilter(secondMember, firstMember)).toBe(false);
    expect(matchesContributionPersonFilter(undefined, firstMember)).toBe(false);
  });
});
