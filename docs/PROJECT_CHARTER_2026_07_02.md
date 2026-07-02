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

**Phase 1 Complete – First End-to-End AI Vertical Slice**

---

# Current Capabilities

- Create venture cases
- Record observations
- Record evidence
- Record assumptions
- Record hypotheses
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

Improve the quality and commercial usefulness of the Research Analyst rather than expanding infrastructure or adding additional AI agents.

Focus on producing board-quality venture research suitable for founder decision-making.

---

# Next Tasks

1. Improve Research Analyst prompt quality.
2. Reduce hallucinated facts and unsupported statistics.
3. Introduce richer citation/provenance model.
4. Produce a board-quality research report from accepted research.
5. Evaluate the smallest commercially valuable deliverable for first customers.

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

---

# Recent Architectural Decisions

- AI produces proposals only.
- Domain remains authoritative.
- Research acceptance reuses existing application handlers.
- AI artifacts include generation metadata.
- Personas are first-class concepts.
- Prompt engineering is considered a core competitive advantage.
- AI Contract / DTO separation postponed until naturally required.

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