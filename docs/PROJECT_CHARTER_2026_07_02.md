# VentureOS Project Charter

> **Mission**
>
> Build the smallest commercially valuable slice of an AI Venture Operating System.
>
> Every technical decision should strengthen the long-term vision while increasing the probability of reaching the first paying customer before runway runs out.

---

# 1. Project Identity

## Project

**VentureOS**

## Vision

VentureOS is an AI Venture Operating System.

Its purpose is to help founders and businesses discover opportunities, perform structured research, build defensible business cases, and make better strategic decisions through AI agents operating under human oversight.

The AI proposes.

Humans decide.

The Domain enforces business rules.

---

# 2. Core Principles

## Business

- We are building an AI-native company operating system.
- Research is only the first capability.
- AI assists.
- Humans remain accountable.

## Architecture

- AI never owns business logic.
- The Domain remains the source of truth.
- AI produces structured proposals only.
- Every AI contribution must be reviewable.
- Every AI contribution must be auditable.
- Every AI provider must be replaceable.
- Avoid premature abstraction.

## Engineering

- Clean Architecture.
- Domain-first.
- Testable.
- Local-first AI.
- Incremental vertical slices.
- Prefer evolution over prediction.

---

# 3. Commercial Principles

There are always two roadmaps.

## Roadmap A

The Vision.

The AI Venture Operating System.

## Roadmap B

Runway.

Everything we build must increase the probability of obtaining the first paying customer.

When Roadmap A and Roadmap B conflict...

## **Runway wins.**

Every feature should answer:

> Does this increase the probability of getting the first paying customer?

If not...

Backlog it.

---

# 4. AI Philosophy

We are not writing prompts.

We are defining digital employees.

Each AI agent consists of:

- Persona
- Responsibilities
- Operating Principles
- Guardrails
- Prompt
- Output Schema
- Version

Prompt quality is expected to become one of VentureOS' primary competitive advantages.

---

# 5. AI Governance

Every AI-generated artifact should ultimately record:

- Provider
- Model
- Persona
- Persona Version
- Prompt Version
- Generated Time
- Duration
- Limitations

Future additions may include:

- Token usage
- Cost
- Citations
- Research Session
- Traceability

---

# 6. Product Positioning

We are **not** building:

- another chatbot
- another search engine
- another note-taking application

We are building:

> **An AI Venture Operating System**

Alternative positioning:

> **Decision Intelligence Platform**

---

# 7. Long-Term Vision

Specialist AI agents will eventually include:

- Opportunity Scout
- Research Analyst
- Competitive Analyst
- Customer Analyst
- Marketing Strategist
- CTO
- CFO
- CEO
- Board

Research is the first specialist.

The long-term goal is coordinated AI executives operating under human oversight.

---

# 8. Technical Principles

- Domain owns business logic.
- Application orchestrates.
- Infrastructure integrates.
- AI belongs in Infrastructure.
- AI contracts remain separate from Domain concepts.
- Only split AI Contracts from DTOs when the two naturally diverge.

---

# =====================================================================
# CURRENT PROJECT STATUS
# Update this section after every development session.
# =====================================================================



# Current Phase

Completed
Research pipeline extracted into four independently swappable capabilities (Planning, Evidence Acquisition, Analysis, Extraction).
Opportunity introduced as a first-class Domain object, peer to Hypothesis.
Red Team Review capability introduced: `IRedTeamReviewService` reviews a case's already-accepted Evidence, Assumptions, Hypotheses and Opportunities and proposes Challenges against real Domain ids (not proposal indexes, since the target state already exists).
Red Team proposals go through the same quality-check-then-human-acceptance workflow as Research (`RedTeamQualityChecker` validates required fields, target type, and target existence against the real case).
AI proposals remain auditable and are no longer silently trusted.

---

# Current Capabilities

- Create venture cases
- Record observations
- Record evidence
- Record assumptions
- Record hypotheses
- Record opportunities
- Raise challenges
- Record decisions
- Record lessons
- Timeline read model
- Executive brief read model
- Decision context read model

### AI

- Local Ollama integration
- Persona-driven prompts
- Structured AI research generation
- AI generation metadata
- Human review workflow
- AI research acceptance
- AI research persisted into the Domain
- AI red team review (challenges proposed against real case ids)
- AI red team review acceptance

---

# Current AI Configuration

Provider:

- Ollama

Model:

- qwen3:8b

Persona:

- Research Analyst

Persona Version:

- 1.0.0

Prompt Version:

- 1.0.0

---

# Current Workflow

```text
Create Case

↓

Generate Deep Research

↓

Research Package

↓

Human Review

↓

Accept Research

↓

Red Team Review

↓

Human Review

↓

Accept Red Team Review

↓

Application Handlers

↓

Domain

↓

DuckDB

↓

Timeline / Brief
```

---

# Current Sprint Goal

Build the Board Review capability, the last of the three MFP organisational roles (Research, Red Team, Board).

---

# Next Tasks

1. Build the Board Review capability (`IBoardReviewService`, briefing DTO covering evidence/objections/confidence, no Domain changes expected).
2. Empirically tune Red Team's Guid-targeting reliability against the running local model; fall back to short reference tags if the rejection rate proves too high.
3. Improve Research Analyst prompt quality.
4. Reduce hallucinated facts and unsupported statistics.
5. Introduce richer citation/provenance model.
6. Evaluate the smallest commercially valuable deliverable for first customers.

---

# Backlog

- Opportunity Scout
- Competitive Analyst
- Customer Analyst
- Marketing Strategist
- CTO
- CFO
- CEO
- AI Board
- Research Session aggregate
- AI Contracts split from Application DTOs
- Multi-provider AI support
- Prompt/specification version management

---

# Known Issues

- Research quality depends heavily on prompt engineering.
- AI citations are currently placeholders and not independently verified.
- Research output quality is more important than response speed.
- Red Team asks the model to copy real Guids verbatim as challenge targets, a harder generation task than Research's small-integer indexes — expect a higher rejection rate at the quality-check stage until tuned.
- `Challenge` does not yet carry severity, proposed mitigation, or withdrawal-condition fields, though the Constitution's Red Team Doctrine calls for them — folded into the free-text Reasoning field for now.

---

# Recent Architectural Decisions

- AI produces proposals only.
- Domain remains authoritative.
- Research acceptance reuses existing application handlers.
- AI artifacts include generation metadata.
- Personas are first-class concepts.
- Prompt engineering is considered a core competitive advantage.
- AI Contract / DTO separation postponed until naturally required.
- Red Team's proposed challenges target real Domain Guids directly, not proposal-array indexes, because Red Team runs after Research has already been accepted into the Domain (unlike Research, which references only its own not-yet-created proposals).
- Red Team's quality checker validates target existence against the real Case, a deliberate divergence from Research's checker (which can't, since nothing real exists at Research-review time).

---

# Documents

Useful reference documents:

- Vision
- Roadmap
- Domain model
- API surface
- AI architecture
- Personas
- Prompt specifications

---

# Conversation Notes

This section intentionally remains short.

Record only decisions that affect future development.

Do not use it as a development diary.