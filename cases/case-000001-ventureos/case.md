# Case 000001 — VentureOS

## Status

Build

---

## Title

VentureOS

---

## Mission

Design and build an open-source, local-first Business Operating System capable of discovering, validating, building, marketing and improving software ventures through evidence-based reasoning, structured decision making and organisational learning.

---

## Case Type

Platform Development

---

## Owner

Founding Board

---

## Created

2026-06-26

---

## Current Phase

Minimum Foundation Platform

---

## Vision

Create a venture operating system that behaves like a well-governed organisation rather than a collection of AI agents.

The system should preserve knowledge, challenge assumptions, justify every decision with evidence, learn continuously and improve itself through governed evolution.

---

## Success Criteria

Success is achieved when VentureOS can successfully guide the complete lifecycle of software ventures while applying its own principles to its own development.

---

## Current Objectives

* Ratify the Constitution
* Define the ontology (object model)
* Design the knowledge model
* Design governance
* Define departments
* Define agents
* Design architecture
* Build the Minimum Foundation Platform

---

## Current Confidence

35%

The vision is clear.

The implementation remains largely unexplored.

---

## Risks

* Scope creep
* Excessive architectural complexity
* Agent-centric rather than knowledge-centric design
* Premature implementation
* Over-engineering
* Endless debate without decision

---

## Assumptions

* Open-source LLMs will continue improving.
* Local-first architecture is viable.
* Organisational reasoning produces better outcomes than isolated agents.
* Evidence-led decision making improves venture success.
* Long-term knowledge accumulation creates competitive advantage.

---

## Related Documents

* Constitution
* README
* ADRs
* Architecture
* Research
* Decisions

---

## Current Status Summary

The Domain, Application, Infrastructure and API layers are implemented in .NET 9 on DuckDB, covering the full reasoning chain: Case, Observation, Evidence, Assumption, Opportunity, Hypothesis, Challenge, Decision and Lesson.

Venture research is generated through a four-stage AI pipeline — Research Planning, Evidence Acquisition, Research Analysis and Research Extraction — each an independently swappable capability, followed by automated quality checks and human review before any AI proposal is accepted into the Domain.

Test coverage exists across two projects, covering Domain behaviour and Application-layer research quality checks.

Red Team Review is now built: an AI capability (`IRedTeamReviewService`) reviews a case's already-accepted Evidence, Assumptions, Hypotheses and Opportunities, and proposes Challenges against the weak, unsupported, or contradicted ones, subject to the same quality-check-then-human-acceptance workflow Research already established.

Board Review is now built: an AI capability (`IBoardReviewService`) prepares a Board briefing over a case's current Observations, Evidence (split into supporting/contradicting/neutral), Assumptions, Hypotheses, Opportunities, Challenges, and any Research/Red Team quality findings the caller still has on hand. Unlike Research and Red Team, Board Review never proposes new Domain objects and has no accept step — the deterministic facts are assembled directly from the Domain with zero AI involvement, and the AI is given no identifiers at all, only rendered text, to produce a narrative (executive summary, decision framing, risks, confidence, recommended investigations, and rationale for each of the four possible decision outcomes). The human Board reads the briefing, then records the actual decision through the pre-existing `POST /cases/{id}/decisions` endpoint — Board Review does not touch the Domain.

Per `docs/roadmap/minimun-foundation-platform_v1.1.md`'s own Success Definition ("a single user can investigate a venture from initial idea through to a fully auditable Board decision") and Definition of Done, the Minimum Foundation Platform is now functionally complete: Case, Research, Evidence, Hypotheses, Challenge, Board Review, and Decision recording all exist and are wired end to end. `Action` and `Outcome` remain unimplemented as Domain concepts — they appear only in the roadmap's pipeline diagram, not in its Core Capabilities or Definition of Done, so this is a deliberate, out-of-v1.1-scope gap, not an oversight.

---

## Next Milestone

Exercise the full MFP loop end to end against a real case (Case → Research → Red Team → Board → Decision) to validate the demonstration the roadmap's Definition of Done describes, and decide what's next: closing the Action/Outcome loop, addressing the known Domain gaps (Assumption/Hypothesis/Opportunity status transitions, `Decision`↔`Opportunity` cross-referencing, `ChallengeTarget.Decision`), or moving toward the first commercially valuable slice per the Project Charter.