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

Red Team Review is now built: an AI capability (`IRedTeamReviewService`) reviews a case's already-accepted Evidence, Assumptions, Hypotheses and Opportunities, and proposes Challenges against the weak, unsupported, or contradicted ones, subject to the same quality-check-then-human-acceptance workflow Research already established. Board Review is not yet built. Until it exists, the reasoning chain stops at Decision without a Board governance capability actively reviewing it.

---

## Next Milestone

Build the Board Review capability.