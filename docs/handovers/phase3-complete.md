# VentureOS – Phase 3 Complete Handover

You are joining VentureOS at the completion of Phase 3.

Assume nothing. Challenge architectural decisions if appropriate, but do not suggest changes simply because there is another possible approach. Improvements should only be proposed when they materially improve clarity, maintainability or correctness.

Read `docs/constitution/ventureos-constitution.md`, `docs/architecture/reasoning-model.md`, and `docs/roadmap/minimun-foundation-platform_v1.1.md` before making any design decisions — they define the philosophy this codebase is built to prove, not just its shape.

## Project

VentureOS is a Domain-Driven Design (DDD) application built in .NET 9 using Minimal APIs and DuckDB. It is an evidence-led venture reasoning system, not an AI agent platform: AI proposes structured artefacts, the Domain is the sole source of truth, and every AI contribution passes through Extraction → Quality → Human Acceptance before it becomes a Domain object.

The architecture separates API / Application / Domain / Infrastructure. The Domain is completely technology independent.

Case #000001 (`cases/case-000001-ventureos/case.md`) is VentureOS's own development, managed under its own principles — keep that file's status current as work lands.

---

## Current Architecture

### Domain

* Aggregate Root: `Case`
* Child entities (in `Case.cs`'s internal collection order): Observation, Evidence, Assumption, **Opportunity**, Hypothesis, Challenge, Decision, Lesson

Conventions, applied identically to every child entity:

* Static `Create(...)` / static `Restore(...)`
* Aggregate (`Case`) owns all mutations — one `CreateX(XDraft draft)` method per child type
* Validation order inside `CreateX`: null-check → archived-case check → required-field blank checks → required-collection-not-empty checks → `HashSet<Guid>`-based "belongs to this case" checks for every cross-referenced ID collection → construct with `Guid.NewGuid()` and `.Distinct().ToArray()` on ID collections → add to internal list → bump `UpdatedAtUtc` → `AddDomainEvent(new XCreatedEvent(...))` → return `Result<X>.Success(...)`
* Domain events live under `Cases/Events/`, not under the entity's own folder
* Status-lifecycle entities (Hypothesis, Opportunity) get `MarkSupported`/`MarkChallenged`/`Accept`/`Reject`/`Supersede`, each a `Result` method guarding forbidden current-state transitions
* `ChallengeTarget` enum: `Evidence | Assumption | Hypothesis | Decision | Opportunity`. `Case.TargetExists` is the single switch validating a challenge target belongs to the case — note `ChallengeTarget.Decision` is hard-coded `false` there (pre-existing, decisions can never actually be a valid challenge target today; not yet fixed, flag before touching)
* No persistence concerns anywhere in Domain

**Opportunity, specifically** (added this phase — the design reasoning matters, don't re-derive it):

Opportunity and Hypothesis are **peers**, not parent/child. Both are independent consumers of Evidence. A Hypothesis answers "what do we believe might be true?"; an Opportunity answers "given what we know, what commercially meaningful possibility appears to exist?" Opportunity does **not** reference Hypotheses at all — only Evidence (required, ≥1) and Assumptions (optional, may be empty). This was an explicit product-owner decision: if Opportunity only referenced Hypotheses, every opportunity would inherit evidence indirectly, you couldn't ask "what evidence supports this opportunity?" directly, and it would become hostage to the current hypothesis structure. Fields: `Statement`, `CustomerValue`, `CommercialValue`, `Differentiation`, `Timing` (map 1:1 onto `ResearchAnalysisPrompt`'s existing "6. Opportunity" section), `Confidence`, `EvidenceIds`, `AssumptionIds`. Status lifecycle mirrors `HypothesisStatus` exactly for vocabulary consistency.

---

### Application

One handler per use case, one `Command`/`Result` pair per handler, all under `Cases/CreateX/`. Repository abstraction: `ICaseRepository` (only `AddAsync`/`GetByIdAsync`/`UpdateAsync` — child entities persist transitively as part of the whole aggregate, never their own repository methods). All handlers return `Result<T>`. Handlers orchestrate only: load aggregate → build a Domain `Draft` from the command → call `ventureCase.CreateX(draft)` → persist via `UpdateAsync` → map to a `Result` DTO.

Read models that enumerate all child types (must be extended whenever a new child entity is added, easy to miss):

* `GetCase/GetCaseResult.cs` + `GetCaseHandler.cs` — per-type counts
* `GetCaseBrief/GetCaseBriefDto.cs` (`CaseBriefCountsDto`) + `GetCaseBriefHandler.cs` — counts + latest-activity timestamp
* `Common/CaseTimelineItemType.cs` + `GetCaseTimeline/GetCaseTimelineHandler.cs` — full timeline

#### AI research pipeline (fully capability-extracted this phase)

Four independently swappable capabilities, each with its own interface in Application (`Research/<Capability>/I<Capability>Service.cs`) and its own Ollama-backed implementation in Infrastructure (`AI/Ollama/Ollama<Capability>Service.cs`), registered via `AddHttpClient<TInterface, TImpl>` in `DependencyInjection.cs`:

1. **`IResearchPlanningService`** — `PlanResearchAsync(Case) → ResearchEvidencePlanDto`
2. **`IEvidenceAcquisitionService`** — `AcquireEvidenceAsync(caseId, questions) → EvidenceAcquisitionResultDto` (re-fetches the Case itself since it only receives a `caseId`)
3. **`IResearchAnalysisService`** — `AnalyzeAsync(Case, evidencePlan, acquiredEvidence) → ResearchAnalysisResultDto` (wraps free-text analysis; the prompt explicitly forbids JSON output at this stage)
4. **`IResearchExtractionService`** — `ExtractAsync(analysisText) → ExtractedResearchDto` (six `Proposed*Dto` lists only — deliberately not `ResearchPackageDto` itself, since `CaseId`/`Mission`/`Generation` aren't extraction's concern)

`OllamaResearchService` (`IResearchService`) is now a thin orchestrator: Plan → Acquire Evidence → Analyze → Extract → assemble `ResearchGenerationDto` + final `ResearchPackageDto` → run `ResearchQualityChecker.Check(...)` → log issues → return. It has no `HttpClient` of its own and makes no Ollama calls directly (registered `AddScoped`, not `AddHttpClient`).

Each Ollama-backed service duplicates its own private `GenerateAsync`/`OllamaGenerateResponse` pair rather than sharing one — deliberate, matches the pattern the first extraction (Evidence Acquisition) set. None of them do debug console logging — that was dropped when each stage was extracted, matching Evidence Acquisition's precedent.

Every live prompt class (`ResearchEvidencePlanningPrompt`, `EvidenceAcquisitionPrompt`, `ResearchAnalysisPrompt`, `ResearchExtractionPrompt`) carries `public const string Version`. `ResearchGenerationDto.PromptVersion` is a composite of all four — it now changes whenever any one stage's prompt is revised, not a dead hardcoded literal.

`AcceptResearchPackageHandler` processes proposals sequentially, one `for` loop per type, each building a `Dictionary<int, Guid>` mapping proposed-array index → real created Guid, consumed by later loops to resolve cross-reference indexes. Order: **Observations → Evidence → Assumptions → Opportunities → Hypotheses → Challenges** (Opportunities sit right after Assumptions, before Hypotheses, because they only depend on Evidence + Assumptions — reflecting the peer relationship above). Challenges can target any of Evidence/Assumption/Hypothesis/Opportunity by string discriminator.

`ResearchQualityChecker` (static, pure functions, in `Research/ResearchQuality/`) validates each proposed object type's required fields, numeric-claim provenance, and cross-reference index bounds against the proposed package — **not** against real Case state (Domain does that, at acceptance time, which is the actual authoritative check). Only Observations can suppress an unsupported-numeric-claim warning via a real `SourceReference`; every other type (Evidence/Assumptions/Opportunities/Hypotheses/Challenges) always warns on a bare number, since those DTOs carry no source-reference field at all. Now has full test coverage (see Testing below).

---

### Infrastructure

DuckDB persistence via `DuckDbCaseRepository` (implements `ICaseRepository`): persists the aggregate root's scalar fields directly, delegates every child collection to its own `Store` class (`ObservationStore`, `EvidenceStore`, `AssumptionStore`, `OpportunityStore`, `HypothesisStore`, `ChallengeStore`, `DecisionStore`, `LessonStore`), all sharing one open `IDbConnection` per repository call. Each Store: `LoadAsync` / `InsertAsync` / `ReplaceAsync` (= delete + reinsert, no diffing) / private `DeleteAsync`. Stores own all persistence conversions — `Confidence` ↔ int-in-a-VARCHAR-column, enums ↔ string, `Guid` collections ↔ JSON-in-a-VARCHAR-column. The Domain never knows how persistence works. `Case.Restore(...)`'s parameter order must exactly match the order `DuckDbCaseRepository.GetByIdAsync` loads and passes each collection — this is the one place a new child entity forces a signature-breaking change with a single call site to fix.

`DuckDbSchemaInitializer` holds one `CREATE TABLE IF NOT EXISTS` block per entity in a single multi-statement command.

---

### API

Minimal APIs. `Program.cs` is only the composition root — one `app.MapXEndpoints()` call per feature. Endpoints organised by feature under `Endpoints/`, request DTOs declared at the bottom of the same file as their endpoint. `ResearchEndpoints` exposes two routes: `POST /cases/{id}/research/deep-dive` (runs the pipeline, does not touch the Domain) and `POST /cases/{id}/research/accept` (runs `AcceptResearchPackageHandler`, mutates the Domain).

---

## Testing

Two test projects now, both registered in `VentureOS.sln`:

* **`tests/VentureOS.Domain.Tests`** — 95 tests, one file (`Cases/CaseTests.cs`), exhaustive per-entity coverage (valid-creates-successfully, each-required-field-blank-fails, archived-case-fails, unknown-cross-ref-fails, duplicate-cross-ref-dedupes, domain-event-raised, status-transition guards). Helper factory methods at the bottom (`CreateValidHypothesis`, `CreateValidOpportunity`, etc.) build minimal valid prerequisite chains — follow this exact pattern for any new entity.
* **`tests/VentureOS.Application.Tests`** — 17 tests, `Research/ResearchQuality/ResearchQualityCheckerTests.cs`, covering every check method with a `ValidPackage(...)`-with-overrides helper builder.

**Still zero coverage** on Infrastructure (DuckDB stores/repository) and API (endpoints) layers — a known, previously-flagged gap, not yet closed. Don't assume it's fine just because Domain/Application are now tested.

`dotnet build VentureOS.sln` and `dotnet test VentureOS.sln` should both be run — and both green — before and after any change of consequence.

---

## Git Workflow

Small, focused, independently-committable commits — build and test between each, not just at the end. Recent history (most recent first) demonstrates the expected granularity:

```
13b01c6 docs: list Opportunity as completed in the domain backlog
a1091c9 docs: update case-000001 status to reflect actual implementation state
bf856a3 docs: remove stale duplicate PROJECT_CHARTER.md
3976a26 test(research): add VentureOS.Application.Tests and cover ResearchQualityChecker
7b6866b feat(research): extract research extraction into its own capability
73b22ed feat(research): extract research analysis into its own capability
acd5203 feat(research): extract research planning into its own capability
330219f fix(research): tie prompt version metadata to actual prompt versions
dc56153 chore(research): move evidence acquisition files into matching folder
2a90b44 chore(research): remove dead pre-pipeline prompt and unused quality wrapper
cf76205 feat(research): introduce evidence acquisition stage
3a59d64 feat(opportunities): add first-class opportunity support
```

Only commit when explicitly asked. Never `git config`, never force-push, never skip hooks.

---

## Known gaps, deliberately not fixed this phase (do not silently redo or ignore)

* **`Decision` doesn't yet reference Opportunity.** Once Red Team/Board need to act on Opportunities, `Decision`/`DecisionDraft`/`RecordDecision` will likely need an `OpportunityIds` cross-reference, same as it already has for Evidence/Assumptions/Hypotheses/Challenges. Not built — it's a change to an already-shipped entity's shape, out of scope for "add a new child entity."
* **`ChallengeTarget.Decision` is hard-coded `false`** in `Case.TargetExists` — decisions can never actually be validly challenged today. Pre-existing, unrelated to this phase's work.
* **`backlog/domain-backlog.md`'s `## Ready` section is still stale** — it lists Hypothesis/Assumption/Challenge/Decision/Lesson as not-yet-done (only Opportunity was corrected this phase). A broader pass is owed.
* **AI-extraction opportunity count range (`1-2`)** in `ResearchExtractionPrompt` is a starting guess, will need empirical tuning against whatever local model is running.
* No Infrastructure/API test coverage (see Testing above).
* `docs/architecture/technology-stack.md` still specs PostgreSQL as the system database; the actual implementation uses DuckDB for everything. Never reconciled — flag if it becomes load-bearing.

---

## Immediate Goal

Per `docs/roadmap/minimun-foundation-platform_v1.1.md`, three organisational roles are essential to the MFP: Research (done), Red Team, Board. All essential MFP objects now exist. The next milestone (also recorded in `cases/case-000001-ventureos/case.md`) is:

**Build the Red Team Review capability.**

Two things to carry into that design, both settled this phase and worth re-reading rather than re-deriving:

1. **Prefer "capability," not "role" or "agent."** This is a stated Engineering Principle (`docs/PROJECT_CHARTER_2026_07_02.md`) and the codebase already follows it structurally — `IResearchService`, `IEvidenceAcquisitionService`, `IResearchPlanningService`, `IResearchAnalysisService`, `IResearchExtractionService` are all capability interfaces, not agent classes. Red Team should land as `IRedTeamReviewService` in the same shape, and — per the pipeline extraction work just completed — its Infrastructure implementation should probably be its own `Ollama*Service` from day one rather than something to extract out later.
2. **The pipeline is the product, not the individual objects.** The full lifecycle is `Case → Research → Opportunity → Red Team → Board → Decision → Action → Outcome → Lesson`. Decision recording and Lesson recording already exist and work; the Action/Outcome/Lesson closing loop needs little to no new Domain work (the MFP doc explicitly allows Outcome to be manual, and neither Action nor Outcome appear in the essential-objects list) — verify that assumption rather than re-building anything there. Red Team, then Board Review, are what's actually missing.
