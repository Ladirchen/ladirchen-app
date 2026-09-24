import { afterEach, describe, expect, it, vi } from "vitest";
import { reactive } from "vue";

import type { SaveVersionedAggregateCommand, VersionedAggregateSnapshot } from "@/application/contracts/versioned-aggregate-contract";
import { AggregateConflictError } from "@/application/ports/versioned-aggregate-repository";
import type { VersionedAggregateRepository } from "@/application/ports/versioned-aggregate-repository";
import { createDomainId } from "@/domain/shared/identifiers";

import { VersionedAggregateService } from "./versioned-aggregate-service";

interface TestState {
  label?: string;
  value: number;
}

const familyId = createDomainId.family("c776a9b6-c37d-4a51-91ba-269a67274478");
const memberId = createDomainId.familyMember("member-1");

describe("VersionedAggregateService", () => {
  afterEach(() => vi.useRealTimers());

  it("coalesces pending drafts, clones their state, and preserves the loaded revision", async () => {
    vi.useFakeTimers();
    const savedCommands: Array<SaveVersionedAggregateCommand<"savings", TestState>> = [];
    const loaded: VersionedAggregateSnapshot<"savings", 1, TestState> = {
      aggregateType: "savings",
      familyId,
      revision: 4,
      schemaVersion: 1,
      state: { value: 1 },
      updatedAt: "2026-09-10T08:00:00.000Z",
      updatedBy: memberId,
    };
    const repository: VersionedAggregateRepository<"savings", 1, TestState> = {
      load: vi.fn().mockResolvedValue(loaded),
      save: vi.fn(async (command) => {
        savedCommands.push(command);
        return {
          ...command,
          revision: command.expectedRevision + 1,
          schemaVersion: 1,
          updatedAt: "2026-09-10T08:01:00.000Z",
        };
      }),
    };
    const service = new VersionedAggregateService("savings", repository, 10);
    await service.load(familyId);

    const firstState = { value: 2 };
    service.scheduleSave({ familyId, state: firstState, updatedBy: memberId }, vi.fn());
    firstState.value = 99;
    service.scheduleSave({ familyId, state: { value: 3 }, updatedBy: memberId }, vi.fn());

    await vi.advanceTimersByTimeAsync(10);

    expect(savedCommands).toHaveLength(1);
    expect(savedCommands[0]).toMatchObject({ expectedRevision: 4, state: { value: 3 } });
  });

  it("materializes reactive state before it reaches a repository", async () => {
    vi.useFakeTimers();
    const save = vi.fn(async (command: SaveVersionedAggregateCommand<"savings", TestState>): Promise<VersionedAggregateSnapshot<"savings", 1, TestState>> => ({
      ...command,
      revision: command.expectedRevision + 1,
      schemaVersion: 1,
      updatedAt: "2026-09-10T08:01:00.000Z",
    }));
    const repository: VersionedAggregateRepository<"savings", 1, TestState> = {
      load: vi.fn().mockResolvedValue(null),
      save,
    };
    const service = new VersionedAggregateService("savings", repository, 10);

    const state = reactive<TestState>({ value: 2 });
    service.scheduleSave({ familyId, state, updatedBy: memberId }, vi.fn());
    state.value = 99;

    await vi.advanceTimersByTimeAsync(10);

    expect(save).toHaveBeenCalledOnce();
    expect(save.mock.calls[0]?.[0].state).toEqual({ value: 2 });
  });

  it("reloads, merges, and retries automatically after a conflict", async () => {
    vi.useFakeTimers();
    const base: VersionedAggregateSnapshot<"savings", 1, TestState> = {
      aggregateType: "savings", familyId, revision: 4, schemaVersion: 1,
      state: { label: "base", value: 1 }, updatedAt: "2026-09-10T08:00:00.000Z", updatedBy: memberId,
    };
    const remote: VersionedAggregateSnapshot<"savings", 1, TestState> = {
      ...base, revision: 5, state: { label: "remote", value: 1 }, updatedAt: "2026-09-10T08:01:00.000Z",
    };
    const savedCommands: Array<SaveVersionedAggregateCommand<"savings", TestState>> = [];
    const repository: VersionedAggregateRepository<"savings", 1, TestState> = {
      load: vi.fn().mockResolvedValueOnce(base).mockResolvedValueOnce(remote),
      save: vi.fn(async (command) => {
        savedCommands.push(command);
        if (savedCommands.length === 1) {throw new AggregateConflictError("savings");}
        return { ...command, revision: command.expectedRevision + 1, schemaVersion: 1, updatedAt: "2026-09-10T08:02:00.000Z" };
      }),
    };
    const onError = vi.fn();
    const service = new VersionedAggregateService("savings", repository, 10);
    await service.load(familyId);
    service.scheduleSave({ familyId, state: { label: "base", value: 2 }, updatedBy: memberId }, onError);

    await vi.advanceTimersByTimeAsync(10);

    expect(savedCommands).toHaveLength(2);
    expect(savedCommands[1]).toMatchObject({ expectedRevision: 5, state: { label: "remote", value: 2 } });
    expect(onError).not.toHaveBeenCalled();
  });
});
