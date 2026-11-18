import type { FamilyMemberId } from "@/domain/shared/identifiers";

export type ContributionPersonFilter = "all" | FamilyMemberId;

export function matchesContributionPersonFilter(
  assigneeId: FamilyMemberId | undefined,
  personFilter: ContributionPersonFilter,
): boolean {
  return personFilter === "all" || assigneeId === personFilter;
}
