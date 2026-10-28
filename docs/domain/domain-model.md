# Core Economy Domain

## Scope

The first C# implementation covers family membership, contributions, promotions,
wallets, saving goals, gifts and shop redemption. It references only the BCL and
the existing [DDD primitives](../../LadirchenApp.Prototypes/README.md). It does not
add an API, persistence adapter, authentication provider or scheduler.

The frontend is the behavioral reference. Its TypeScript domain modules contain
calculations, but Pinia action groups own many lifecycle and authorization rules.
[The vision](../Ladirchen.md) and [event storming](event-storming.md) explain intent;
they do not mean every proposed feature already exists.

## Frontend Analysis

All frontend paths below are relative to
`LadirchenApp.Webhost/Apps/ladirchen-app/src`.

| Area               | Decisive source                                                                                                 | Observed model and behavior                                                                                                        |
| ------------------ | --------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------- |
| Family             | `domain/family/permissions.ts`, `stores/family-world-actions/family-actions.ts`, `stores/family-world-state.ts` | Member identities, child/guardian roles, admin/supporter access, invitation state, timezone and settings                           |
| Contributions      | `stores/family-world-actions/contributions-actions.ts`                                                          | Available/pending/approved lifecycle, assignment, claiming, reciprocal review, return, deletion and extra-task partner invitations |
| Promotions         | `domain/contributions/promotions.ts`, `domain/contributions/rating.ts`                                          | First eligible promotion, recurring local deadline, multiplier/teamwork amount and perfect-rating bonus                            |
| Participation      | `domain/contributions/energy.ts`, `domain/contributions/weekly-progress.ts`                                     | Weighted approved basic-task energy and distinct approved local dates in an ISO week                                               |
| Savings            | `stores/family-world-actions/savings-actions.ts`, `domain/savings/types.ts`                                     | Member/shared goals, visibility, bonus, transfers, cancellation, cheer and immediate gifts                                         |
| Interest           | `domain/savings/interest.ts`, `stores/family-world.ts`                                                          | Participation-weighted rate and bounded credits; explicit demo credit commands                                                     |
| Shop               | `stores/family-world-actions/shop-actions.ts`, `domain/shop/rules.ts`                                           | Publication and inclusive date windows, reservation, guardian decision, stock reduction and local 18:00 cutoff                     |
| Home               | `stores/family-world-actions/home-actions.ts`, `domain/house/`                                                  | Accessories, sets, themes, room designs, ownership, placement, room unlocks and pro-subscription arrangement                       |
| Mascot/progression | `domain/ladi/stages.ts`, `application/contracts/family-progression-contract.ts`                                 | Score-to-stage definitions and stored house level, completed weekly streak and target                                              |
| Storage            | `application/contracts/`, `infrastructure/persistence/`                                                         | Independently versioned family-scoped snapshots and browser persistence                                                            |

The app initializes from fixtures and persists family-profile, contributions,
savings, reward-shop, family-progression and home-customization state separately.
Those partitions are storage boundaries, not guarantees of atomic money updates.
UI selection, animation queues, simulated values, notifications and localization
are not financial domain state.

## Aggregate Roots

| Root            | Identity   | Owned state and responsibility                                                             |
| --------------- | ---------- | ------------------------------------------------------------------------------------------ |
| `Family`        | `FamilyId` | Members, invitations, guardian access, timezone, currency and exchange rate                |
| `FamilyEconomy` | `FamilyId` | Wallets, contributions, promotions, goals, offers, requests, ledger and applied operations |

`FamilyEconomy` is a family-sized consistency boundary. Contribution approval
changes task and wallet together. Shop approval changes balance, reservation,
request and stock together. Saving deposits and gifts update both sides of the
transfer together. Separate roots for each frontend snapshot would require
transaction coordination or reconciliation infrastructure absent from this slice.

The economy consults the `Family` supplied to its constructor for current
membership and permissions; it does not mutate that root. The application must
load a consistent pair and serialize access. These aggregates are not thread-safe
or distributed transaction managers.

## Entities and Value Types

| Entity              | Identity and lifecycle                                                           |
| ------------------- | -------------------------------------------------------------------------------- |
| `FamilyMember`      | Same member through invitation acceptance, renaming and access changes           |
| `MemberWallet`      | Member-owned balance and reserved funds                                          |
| `Contribution`      | Persistent identity through assignment, submission, return and approval          |
| `Promotion`         | Independently identified rule linked to a contribution, with activation/deletion |
| `SavingGoal`        | Same goal through editing, deposits, withdrawal, support and cancellation        |
| `ShopReward`        | Catalog offer through publication, stock changes and requests                    |
| `RedemptionRequest` | Requester, captured price, request time and terminal decision retained for audit |

The frontend stores one active requester directly on each offer. The C# model
retains request entities while preserving one pending request per offer, even
when stock exceeds one. Cancelled goals and deleted offers remain internal audit
state; their identities cannot be reused. Deleted contribution identities are
retained separately, and deletion never reverses or removes earned ledger credits.

Value types include:

- `FamilyId`: nonempty UUID. Other named IDs retain validated string identifiers,
  including the app's non-UUID member/task identifiers, with 1-128 allowed characters.
- `LadirchenAmount`: nonnegative decimal virtual currency; positive validation is
  additionally required for transfers. Zero-price offers remain valid.
- `StarRating`, `Percentage`, `RewardMultiplier`: validated 1-5, 0-100 and positive ranges.
- `ContributionRewardBreakdown`: immutable promoted base, rating bonus, multiplier and total.
- `GoalOwner`: explicit member-or-family ownership without a magic member ID.
- `IanaTimeZone`, `PromotionDeadline`, `AvailabilityWindow`, `IsoWeekPeriod`: local-calendar rules.
- `ExchangeRate`, currency, role, visibility, category and status: settings and choices.
- Contribution, goal and shop definition records: immutable creation inputs, validated before insertion.
- `InterestEvaluationContext`, `SavingsInterestPolicy`: participation inputs and policy weights.
- `LedgerEntry` and business events: immutable audit facts, not mutable aggregate roots.

Every class, record and enum has its own named file. Related types live under
`Identifiers`, `Values`, `Calendar`, `Rules`, `Members`, `Contributions`,
`Promotions`, `Savings`, `Shop`, `Wallets`, `Policies` and `Events` folders.
Existing namespaces are retained. Each partial aggregate file contains only its
own partial type. C# members use full block bodies.

## Permissions

Multiple administrators are allowed. The C# family starts with an administrator
and rejects demoting its last active administrator. Pending invitees cannot act.
Member deletion is not implemented here because economic history must retain its
referenced identities.

| Workflow                                        | Child           | Admin guardian  | Supporter guardian   |
| ----------------------------------------------- | --------------- | --------------- | -------------------- |
| Manage family/settings                          | No              | Yes             | No                   |
| Manage child contributions, promotions and shop | No              | Yes             | No                   |
| Create guardian's own contribution              | No              | Yes             | Own only             |
| Submit contribution                             | Assigned member | Assigned member | Assigned member      |
| Review child contribution                       | No              | Yes             | No                   |
| Review guardian contribution                    | Yes             | No              | No                   |
| Issue special gift                              | No              | Yes             | No                   |
| Support child/family goal                       | No              | Visible goals   | Family-visible goals |
| View guardians-visible goal                     | Own only        | Yes             | Own only             |
| View private goal                               | Own only        | Own only        | Own only             |
| Request shop reward                             | Yes             | No              | No                   |
| Gift existing wallet funds                      | Yes             | No              | No                   |

Members may create their own personal goals; administrators may create children's
goals. Children and administrators may create family goals. Withdrawal and
cancellation are owner-only. Commands resolve actual active members rather than
accepting permission booleans or trusting the UI's selected child.

The application must authenticate the actor and pass their actual member ID.
Goal queries enforce visibility. Raw aggregate wallets, requests, ledger and
events are privileged application state and must not be serialized directly to
user-facing responses.

## Contribution Rewards

The lifecycle is `Available -> Pending -> Approved`; review may return pending
to available. Claiming changes the assignee without submitting. Only the assignee
submits. Children reviewing guardian tasks is an implemented reciprocal workflow.
Extra contributions have zero participation energy; invitations do not split
rewards because no such payout rule exists in the prototype.

```text
promotedBase = reward * multiplier + teamworkBonus
ratingBonus = fiveStars && bonusPercent > 0
  ? max(1, round(promotedBase * bonusPercent / 100))
  : 0
totalCredit = promotedBase + ratingBonus
```

The default perfect-rating bonus is 5%. Fractional base rewards remain decimal;
whole-unit rounding uses `MidpointRounding.AwayFromZero` for the nonnegative
amounts. Approval stores the rating, immutable reward breakdown and UTC instant
and credits the assignee once, including guardians.

The first active matching promotion applies strictly before its local time
deadline. Deadlines recur daily while active, rather than expiring on a single
calendar date. Promotions do not stack.

## Wallets and Shop

```text
balance >= reserved >= 0
available = balance - reserved
```

Requests reserve the captured price without debiting balance. Cancellation or
rejection releases it. Approval captures it, decrements stock once and completes
the request. The offer becomes available if stock remains, otherwise redeemed.
Deleting a pending offer cancels its request and releases funds first.

Requests require publication, stock and sufficient available funds. Publication
dates are inclusive in the family timezone. Request and approval both close at
local 18:00; cancellation/rejection remain possible afterward. Hiding an offer
does not invalidate a pending request. Pending requests lock price and stock edits.

Successful monetary commands consume caller-supplied transaction IDs. Reuse is
rejected. Lifecycle guards also reject repeated approval with a new ID.
Preconditions and overflow-sensitive arithmetic are checked before money moves.
The append-only ledger records credits, debits, reserves, releases, captures,
goal movements and bonus forfeiture, preserving member, actor, source, amount,
reason and UTC time. Linked transfer entries share an operation ID.

## Saving Goals and Gifts

Personal goals start with a bonus of `min(5, target)`; family goals start at zero
and always have family visibility. Savings cannot exceed the target. Deposits
and support cap at remaining target capacity; withdrawals cap at withdrawable
funds. Transfers exceeding available balance are rejected instead of silently
shrinking the transaction.

An incomplete goal locks its starter bonus. Completion unlocks all savings.
Cancellation returns withdrawable funds and records any bonus forfeiture. Target
updates below saved funds are rejected. The bonus lock follows current completion,
matching the prototype: increasing the target or withdrawing below it makes the
goal incomplete again.

Guardian support mints new goal funds; it does not debit the guardian wallet.
Special gifts mint child wallet funds, require a reason and must be 1-10000
Ladirchen. Child gifts debit the sender and credit the recipient immediately.
Reserved funds cannot be gifted. The prototype's pending gift queue schedules
animation only. Cheer remains a boolean, not an invented supporter-count ledger.

## Interest

```text
rate = min(8,
  1 + streakDays * 0.5
    + completionPercent / 100 * 3.5
    + averageBasicRating / 5 * 2.5)
credit = saved <= 0 || saved >= target
  ? 0
  : min(target - saved, max(1, round(saved * rate / 100)))
```

Rates round to two decimal places. Completion is rounded approved basic energy
divided by total basic energy, defaulting to 60% without basic energy. Only basic
ratings affect interest. Approval-day counts use distinct local dates in a
Monday-Sunday ISO week, including ISO-year and daylight-saving boundaries.
The minimum credit of one at a zero rate deliberately preserves prototype behavior.

The frontend's streak calculation depends on the selected child: shared completed
weeks/current-week days for that child, stored `weeklyStreak` for other members.
The C# helper evaluates an explicit member/period and receives completed streak
and weekly target inputs. This removes viewer-dependent calculation without
inventing progression settlement.

Weekly settlement requires a closed ISO period and owner evaluations. Every goal
update is precomputed before any is applied. Repeating a settled period is a no-op
without new ledger entries or events. Family-owned, empty, complete and newly
created-after-period goals are excluded. The caller owns scheduling and historical
evaluation snapshots; current domain state cannot reconstruct historical balances
or deleted/changed tasks. The frontend's credit actions are demos, not a production
weekly settlement mechanism.

## Deferred Domains

- Pets retain entity identity; avatar appearance is a value selection. Rendering
  assets, skin palettes and localization stay in presentation.
- A future family-home root could own furniture, themes, designs, bundles and
  room unlocks. Accessories/placements have identity; coordinates, scale, zone
  and catalog definitions are values. Home purchases will need explicit wallet
  coordination.
- Detailed indoor/outdoor placement, fixed/movable constraints, collision geometry
  and pro-subscription arrangement are deferred, not copied into the core economy.
- Mascot stages are score-to-tier definitions, not mutable entities. Stored house
  levels and streak fields do not establish automatic advancement/reset rules.
- The vision's separate savings-account product and optional goal dates are not
  added where the inspected app implements goals instead.
- UI selection, simulations, celebrations, browser revisions and snapshot migrations
  are application/infrastructure or presentation concerns.

## Executable Specifications

English features are in `LadirchenApp.Domain.Tests/Features`, with scenario-scoped
Reqnroll bindings and TUnit assertions. Existing TUnit tests remain. Reqnroll.TUnit
3.3.4 is installed in the existing project; generated feature code goes under
`obj`. The .NET 10 native test runner is configured in `global.json`.

```powershell
dotnet test --project LadirchenApp.Domain.Tests/LadirchenApp.Domain.Tests.csproj
dotnet test --project LadirchenApp.Domain.Tests/LadirchenApp.Domain.Tests.csproj --treenode-filter '/*/LadirchenApp.Domain.Tests.Features/*/*'
```

Specifications assert balances, reservations, stock, savings, reward breakdowns,
ledger and event payloads, plus unchanged state on rejection. IDs and time inputs
are deterministic per scenario. Events use explicit business instants; generated
fact IDs do not affect eligibility. The application handles pending events before
clearing them; there is no event-sourcing replay or message transport here.
