import { describe, expect, it } from "vite-plus/test";
import { createDomainId } from "@/domain/shared/identifiers";
import { matchesChildContributionScope, matchesContributionPersonFilter } from "./contribution-person-filter";

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

describe("matchesChildContributionScope", () => {
  const child = createDomainId.familyMember("child");
  const sibling = createDomainId.familyMember("sibling");

  it("defaults a child view to their own assigned tasks", () => {
    expect(matchesChildContributionScope(child, child, "mine")).toBe(true);
    expect(matchesChildContributionScope(undefined, child, "mine")).toBe(false);
    expect(matchesChildContributionScope(sibling, child, "mine")).toBe(false);
  });

  it("shows only unassigned tasks in the open scope", () => {
    expect(matchesChildContributionScope(undefined, child, "open")).toBe(true);
    expect(matchesChildContributionScope(child, child, "open")).toBe(false);
    expect(matchesChildContributionScope(sibling, child, "open")).toBe(false);
  });

  it("allows own and unassigned tasks in the combined scope", () => {
    expect(matchesChildContributionScope(child, child, "all")).toBe(true);
    expect(matchesChildContributionScope(undefined, child, "all")).toBe(true);
    expect(matchesChildContributionScope(sibling, child, "all")).toBe(false);
  });
});
