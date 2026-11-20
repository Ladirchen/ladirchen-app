import type { FamilyMemberId } from "@/domain/shared/identifiers";

export type ContributionPersonFilter = "all" | FamilyMemberId;
export type ContributionScopeFilter = "all" | "mine" | "open";

export function matchesContributionPersonFilter(
  assigneeId: FamilyMemberId | undefined,
  personFilter: ContributionPersonFilter,
): boolean {
  return personFilter === "all" || assigneeId === personFilter;
}

export function matchesChildContributionScope(
  assigneeId: FamilyMemberId | undefined,
  signedInMemberId: FamilyMemberId,
  scopeFilter: ContributionScopeFilter,
): boolean {
  if (assigneeId !== undefined && assigneeId !== signedInMemberId) {
    return false;
  }

  if (scopeFilter === "mine") {
    return assigneeId === signedInMemberId;
  }

  if (scopeFilter === "open") {
    return assigneeId === undefined;
  }

  return true;
}
