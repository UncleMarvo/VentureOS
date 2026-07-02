# VentureOS – Phase 4 Complete Handover

You are joining VentureOS at the completion of Phase 4.

Assume nothing. Challenge architectural decisions if appropriate, but do not suggest changes simply because there is another possible approach. Improvements should only be proposed when they materially improve clarity, maintainability or correctness.

Read `docs/constitution/ventureos-constitution.md`, `docs/architecture/reasoning-model.md`, and `docs/roadmap/minimun-foundation-platform_v1.1.md` before making any design decisions — they define the philosophy this codebase is built to prove, not just its shape. `docs/handovers/phase3-complete.md` still describes the Domain/Application/Infrastructure/API layers accurately for everything except Red Team, which this phase added — read it first, then treat this document as the delta.

## What this phase added: Red Team Review

Red Team Review is a new AI capability, `IRedTeamReviewService`, in the same shape as the four Research capabilities (`Application/RedTeam/IRedTeamReviewService.cs`, `Infrastructure/AI/Ollama/OllamaRedTeamReviewService.cs`, registered via `AddHttpClient` in `DependencyInjection.cs`). It reviews a Case's already-accepted Evidence, Assumptions, Hypotheses, and Opportunities and proposes `Challenge` objects against the weak, unsupported, or contradicted ones. **No Domain changes were made or needed** — `Challenge` and `Case.RaiseChallenge` already supported everything Red Team requires.

It goes through the exact same propose → quality-check → human-review → accept workflow Research established:

* `POST /cases/{id}/red-team/review` — runs `RedTeamCaseHandler`, does not touch the Domain, returns `RedTeamCaseResultDto` (a `RedTeamReviewResultDto` plus `RedTeamQualityIssueDto` list).
* `POST /cases/{id}/red-team/accept` — takes the `RedTeamReviewResultDto` back in the body (client round-trips exactly what `/review` returned, mirroring `/research/accept`), runs `AcceptRedTeamReviewHandler`, mutates the Domain.

**The one deliberate structural divergence from Research, and why it matters:** Research's proposed Challenges target other *proposed, not-yet-real* items by array index, because at Research-review time nothing exists in the Domain yet. Red Team runs *after* Research has already been accepted, so Evidence/Assumptions/Hypotheses/Opportunities are already real Domain objects with real Guids by the time Red Team reviews the case. So `RedTeamProposedChallengeDto` carries a real `Guid TargetId`, not an index — and `RedTeamQualityChecker.Check(RedTeamReviewResultDto, Case)` takes the real `Case` and validates `TargetId` existence against it, something `ResearchQualityChecker.Check(package)` deliberately cannot do (nothing real exists yet at its check time). If you're tempted to unify the two checkers' signatures, don't — the divergence is load-bearing, not accidental.

`AcceptRedTeamReviewHandler` is consequently simpler than `AcceptResearchPackageHandler`: no index-remapping dictionary, one entity type, one loop calling the pre-existing `RaiseChallengeHandler` directly.

### New vs. reused types — a deliberate choice, not an oversight

`RedTeamGenerationDto` is structurally identical to `ResearchGenerationDto`. `RedTeamQualityIssueDto`/`Code`/`Severity` overlap conceptually with `ResearchQualityIssueDto`/`Code`/`Severity`. Both were built as **new, Red-Team-scoped copies** rather than reused, for two reasons: (1) reusing them would make Red Team's Application code depend on Research's folder for an unrelated capability, and (2) `ResearchQualityCode` already carries Research-specific members (`InvalidObservationIndex`, `WeakAssumption`, etc.) that don't belong in a shared Red Team context. This matches the codebase's existing precedent of duplicating small structural types per capability (each Ollama service's own `GenerateAsync`/`OllamaGenerateResponse` pair). The correct long-term answer is a shared `Application/Common` extraction — already flagged in the Charter's backlog as "AI Contracts split from Application DTOs" — but that's a three-capability-or-more refactor, not a two-capability one. Do it when Board Review makes a third near-identical copy undeniable, not before.

### Persona and prompt

`RedTeamAnalystPersona` is grounded directly in the Constitution's Red Team Doctrine (constructive, evidence-led, specific, challengeable, open to being convinced, not cynical for its own sake) and explicitly states that raising zero challenges is correct when reasoning is sound — this is reinforced at both the persona level and the prompt level, not left to prompt instructions alone. `RedTeamReviewPrompt.Build(Case)` renders each real Evidence/Assumption/Hypothesis/Opportunity with its real `id` (Guid) so the model can target it directly; it explicitly forbids inventing ids and forbids targeting Decisions.

## Known gaps, deliberately not fixed this phase (do not silently redo or ignore)

* **Guid-targeting reliability is unproven.** Asking a small local model (e.g. `qwen3:8b`) to copy a 36-character Guid verbatim is a harder generation task than Research's small-integer indexes. `RedTeamQualityChecker`'s `InvalidChallengeTarget` check makes bad ids safe (rejected as quality issues, never silently corrupt data), but expect a higher rejection rate than Research ever saw. If this proves too lossy in practice, the fix is confined to `OllamaRedTeamReviewService` internals — present short reference tags (`E1`/`A1`/`H1`/`O1`) to the model and resolve tag→Guid internally — without touching the DTO/checker/accept-handler contract. Treat this the same way the Charter already treats the opportunity-count range: a tunable, not a bug.
* **`Challenge`'s shape doesn't fully match the Constitution's Red Team Doctrine.** The Doctrine (constitution lines 97–103) wants severity, evidence, confidence, proposed mitigation, and withdrawal conditions per objection. `Challenge` only has `Statement`, `Reasoning`, `Confidence`, `Target`, `TargetId`. The prompt asks the model to fold severity/mitigation/withdrawal-conditions into the free-text `Reasoning` field as a stopgap. Actually closing this gap means changing `Challenge`'s shape — an already-shipped entity — which is out of scope for "add a capability," same reasoning phase 3 used to defer the `Decision`/`Opportunity` cross-reference gap.
* **No transactional rollback in `AcceptRedTeamReviewHandler`'s loop**, inherited from `AcceptResearchPackageHandler`'s identical pattern — a failure partway through leaves earlier challenges created. Not new, but worth naming so nobody assumes it's fine "because Research does it" without recognizing it's the same known gap twice now.
* **No `CaseStatus` gate** prevents calling `/red-team/review` before Research has been accepted — it just yields an empty challenge list (the model sees nothing to challenge), which is safe but silent. No stage in this pipeline currently gates on the previous stage's completion; this is consistent, not a regression.
* All Phase 3 known gaps (`Decision` doesn't reference `Opportunity`; `ChallengeTarget.Decision` hard-coded `false`; stale `domain-backlog.md` `## Ready` section; no Infrastructure/API test coverage; `technology-stack.md` still specs PostgreSQL) remain untouched and still apply.

## Testing

Added `tests/VentureOS.Application.Tests/RedTeam/RedTeamQuality/RedTeamQualityCheckerTests.cs` (11 tests), following the exact override-builder pattern `ResearchQualityCheckerTests.cs` established, but its `ValidCase()` helper builds a real `Case` through the actual Domain API (same style as `CaseTests.cs`'s `CreateValidHypothesis`-style factories) so `TargetId`s are genuine Guids that exist in the case. Total suite is now 95 Domain tests + 28 Application tests, all green. No handler-level tests for `RedTeamCaseHandler`/`AcceptRedTeamReviewHandler`, consistent with zero handler tests existing for their Research counterparts.

## Immediate Goal

Per `docs/roadmap/minimun-foundation-platform_v1.1.md`, the three essential MFP organisational roles are Research (done), Red Team (done this phase), Board (not yet built). The next milestone, also recorded in `cases/case-000001-ventureos/case.md`, is:

**Build the Board Review capability.**

Carry into that design: the MFP doc's Board Review section describes a briefing containing executive summary, observations, evidence, assumptions, hypotheses, objections, risks, confidence, quality findings, and recommended investigations — this is a *read/synthesis* capability over already-real Domain data (much like Red Team turned out to be), not a new Domain object beyond what already exists. `Decision` already has the recording side (`RecordDecision`); Board Review's job is almost certainly to prepare the human-facing briefing that a Board reviews *before* calling `/decisions`, not to introduce a new aggregate. Verify that assumption rather than re-deriving it, per the same instinct that correctly kept Red Team Domain-change-free this phase.
