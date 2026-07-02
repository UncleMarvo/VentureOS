# VentureOS – Phase 5 Complete Handover

You are joining VentureOS at the completion of Phase 5.

Assume nothing. Challenge architectural decisions if appropriate, but do not suggest changes simply because there is another possible approach. Improvements should only be proposed when they materially improve clarity, maintainability or correctness.

Read `docs/constitution/ventureos-constitution.md`, `docs/architecture/reasoning-model.md`, and `docs/roadmap/minimun-foundation-platform_v1.1.md` before making any design decisions. `docs/handovers/phase4-complete.md` still describes everything up to and including Red Team Review accurately — read it first, then treat this document as the delta.

## What this phase added: Board Review

Board Review is a new capability, `IBoardReviewService`, that prepares a human-facing briefing from a Case's current state — Observations, Evidence, Assumptions, Hypotheses, Opportunities, Challenges, and any Research/Red Team quality findings the caller still has on hand. **No Domain changes were made.** Unlike Research (proposes new entities) and Red Team (proposes Challenges against real entities), Board Review proposes nothing at all: it produces "a Board Briefing, not a decision," per the explicit product requirement this phase started from. The human reads the briefing and then calls the pre-existing `POST /cases/{id}/decisions` endpoint separately — Board Review has exactly one route (`POST /cases/{id}/board/review`) and no accept step, because there is nothing to accept into the Domain.

### The deterministic/AI split, and why it's stricter than Research or Red Team

`BoardDossierAssembler.Assemble(Case, researchFindings, redTeamFindings)` (`Application/Board/BoardDossierAssembler.cs`) is a **pure, static, AI-free** function that enumerates every factual section of the briefing directly off the real `Case`: Observations, Evidence partitioned into Supporting/Contradicting/Neutral by `Direction`, all Assumptions, all Hypotheses, all Opportunities, and all Challenges (with `TargetId` resolved back to the actual challenged item's text via a small private switch mirroring `Case.TargetExists`'s shape). None of this touches the AI. Only `IBoardReviewService.ReviewAsync(BoardDossierDto)` — deliberately typed to take the **assembled dossier, not the raw `Case`** — produces the narrative sections (executive summary, decision framing, risks, confidence narrative, recommended investigations, and a rationale for each of the four `DecisionOutcome` values).

This is a real, intentional divergence from `IRedTeamReviewService.ReviewCaseAsync(Case)`'s signature: Red Team's AI needs the real `Case` because it must copy real Guids out of it to target Challenges. Board's AI must **never see an identifier of any kind** — passing only the pre-rendered, Guid-free `BoardDossierDto` makes that guarantee structural, not a matter of prompt discipline. If you're tempted to make `IBoardReviewService` take `Case` for signature consistency with Red Team, don't — the two capabilities have genuinely different safety requirements, same as Phase 4's Guid-vs-index divergence from Research was load-bearing, not arbitrary.

Ranked by AI risk surface across all three capabilities built so far: Research (proposes brand-new entities, highest risk) > Red Team (must copy real Guids verbatim, medium risk, flagged in Phase 4 as an open reliability concern) > Board (sees no identifiers at all, narrative only, lowest risk). This was a deliberate design goal for Board Review, not an accident.

### Decision options are fixed, not proposed

`BoardNarrativeDto.DecisionOptions` always contains exactly the four `DecisionOutcome` values (`Approved`, `Rejected`, `Deferred`, `MoreResearchRequired`), in that order — the model supplies only a `Rationale` per option, never the set of options itself. `OllamaBoardReviewService.ParseDecisionOptions` defensively validates the count and order and throws `InvalidOperationException` on a mismatch, the same "throw on malformed AI output" precedent `OllamaRedTeamReviewService` established. This keeps the briefing's decision options and the pre-existing `RecordDecision` endpoint's accepted values in lockstep by construction — nobody has to remember to update both places if `DecisionOutcome` ever changes.

### Quality findings are pass-through only, not persisted

Research and Red Team quality issues (`ResearchQualityIssueDto`, `RedTeamQualityIssueDto`) are computed on the fly during their respective `/review` or `/deep-dive` calls and returned in the HTTP response — there is no table or Domain concept that stores them. Board Review's request (`BoardReviewRequest` in `Api/Endpoints/BoardEndpoints.cs`) accepts them as **optional lists the caller supplies** (`ResearchQualityFindings`/`RedTeamQualityFindings`, mapped to a shared, source-agnostic `BoardQualityFindingDto(string Severity, string Code, string Path, string Message)` — plain strings, not either source's own enum, since Board only echoes findings back without interpreting them). If the caller has nothing to pass, the briefing's quality-findings sections are simply empty; this is "where available" taken literally, not a lesser version of a "real" persisted-history design that doesn't exist yet.

## Known gaps, deliberately not fixed this phase (do not silently redo or ignore)

* **`Assumption` has no status-transition mutator at all**, and although `Hypothesis`/`Opportunity` do have `MarkSupported`/`MarkChallenged`/`Accept`/`Reject`/`Supersede` domain methods, no endpoint anywhere calls them — confirmed by reading `HypothesisEndpoints.cs`/`OpportunityEndpoints.cs`. Every Assumption, Hypothesis, and Opportunity is permanently stuck at its initial `Proposed` status today. `BoardDossierDto.UnresolvedAssumptions` is named that way deliberately — it is *all* assumptions, not a filtered subset, because there is currently no Domain-level way to mark one resolved. `BoardDossierAssemblerTests.Assemble_AllAssumptionsAreUnresolved_RegardlessOfStatus` exists specifically to force whoever eventually implements status transitions to notice and reconsider this definition.
* **Quality findings have no persistence**, as above — if a future session wants Board Review to automatically retrieve historical quality findings instead of relying on the caller to pass them back in, that requires designing actual storage for them first, which nobody has asked for yet.
* **`ChallengeTarget.Decision` is still dead code.** `BoardDossierAssembler.ResolveTargetText`'s `Decision` branch returns a fixed placeholder string and should never actually execute, since `Case.TargetExists` still hard-codes `Decision` to `false` (Phase 3 gap, unchanged).
* All Phase 3/4 known gaps not mentioned above remain as stated in `phase4-complete.md` (Decision↔Opportunity cross-reference; `Challenge`'s shape not matching the Constitution's Red Team Doctrine fields; no Infrastructure/API test coverage; `technology-stack.md` still specs PostgreSQL; Red Team Guid-targeting reliability unproven).

## Testing

Added `tests/VentureOS.Application.Tests/Board/BoardDossierAssemblerTests.cs` (9 tests): evidence partitioning by direction, the "all assumptions are unresolved" gap-documenting test, per-`ChallengeTarget`-type target-text resolution (theory over Evidence/Assumption/Hypothesis/Opportunity), count/ordering parity with the underlying `Case` collections, and quality-findings pass-through (empty when omitted, unmodified when supplied). Total suite is now 95 Domain tests + 37 Application tests, all green. No `BoardCaseHandlerTests.cs`, consistent with zero handler-level tests existing for `ResearchCaseHandler`/`RedTeamCaseHandler` — the interesting logic lives in the pure, independently-tested `BoardDossierAssembler`.

## MFP v1.1 completeness

Checked directly against `docs/roadmap/minimun-foundation-platform_v1.1.md`'s own Success Definition ("a single user can investigate a venture from initial idea through to a fully auditable Board decision") and Definition of Done (ends at "a decision is recorded... reasoning remains permanently auditable"). Both are satisfied now: Case, Research, Evidence, Hypotheses, Challenge, Board Review, and Decision recording all exist and are wired end to end. `Action`/`Outcome` appear only in the roadmap's pipeline diagram, never in its Core Capabilities or Definition of Done — their absence is a deliberate, out-of-v1.1-scope gap, not an oversight, and should not be quietly built "to complete the diagram" without a deliberate decision to expand v1.1's scope first.

## End-to-end verification (done this phase, after the sections above were written)

The full loop was run live against a real Case with a live local Ollama instance (`qwen3:8b`, CPU inference): Create Case → Research deep-dive → accept → Red Team review → accept → Board Review → record Decision → verify via timeline. Every request/response shape in this handover and in `docs/testing/e2e-postman-guide.md` was captured from that real run, not hand-typed. Notable results, not just "it worked":

* Red Team correctly copied a real Evidence Guid verbatim and raised one specific, well-reasoned challenge with zero quality issues — the Guid-targeting reliability risk flagged earlier in this document did not materialize in this run (one data point, not a statistical claim — still worth empirical tuning over more runs).
* Board's narrative referenced zero identifiers, as designed, and returned exactly the 4 fixed `DecisionOptions` in the required order.
* The full timeline shows one entry per created Observation/Evidence/Assumption/Opportunity/Hypothesis/Challenge/Decision, in order, ending with the recorded Decision — the audit trail the Constitution requires actually holds up end to end.

**`docs/testing/e2e-postman-guide.md` now exists** as a repeatable Postman walkthrough of this exact sequence, including realistic timing expectations (~10 min for Research, ~3 min for Red Team, ~1.5 min for Board on CPU inference) and a troubleshooting section. Use it rather than re-deriving the request shapes from source when demonstrating or re-verifying the loop.

One side effect worth knowing about: this verification run's test case ("AI-Powered Meeting Notes SaaS") was created against the real local `src/VentureOS.Api/data/ventureos.duckdb`, which is tracked in git (a pre-existing project convention, not introduced this phase) — that test case is now committed to `origin/main`'s copy of the file. Harmless, but if a clean database is ever expected, this is why it isn't.

## Immediate Goal

MFP v1.1 is now confirmed complete both on paper and in practice — there is no more MFP-mandated capability left to build, and the full loop has been demonstrated end to end, not just unit-tested in isolation. The next session should decide, with the user, between:

1. Closing the Action/Outcome loop (a deliberate scope expansion, not implied by v1.1).
2. Addressing the known Domain gaps listed above (Assumption/Hypothesis/Opportunity status transitions never exercised by any endpoint; `Decision`↔`Opportunity` cross-referencing; `ChallengeTarget.Decision` still dead; Red Team's Guid-targeting reliability, still only lightly tested).
3. Moving toward the Project Charter's "smallest commercially valuable slice" per its Roadmap A vs. Roadmap B framing ("Runway wins").
