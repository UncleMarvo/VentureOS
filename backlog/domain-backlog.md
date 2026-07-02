# VentureOS Domain Backlog

The Domain Backlog records the implementation order of the VentureOS domain model.

It is not a product roadmap or feature request list.

Items are ordered according to the dependencies within the domain model, ensuring that foundational concepts are implemented before higher-level reasoning capabilities.

Each completed task should result in:

* A measurable increase in VentureOS capability.
* Passing tests.
* A single, meaningful Git commit.

---

## Current Status

### Completed

* [x] Domain primitives

  * Entity
  * AggregateRoot
  * DomainEvent
  * Result

* [x] Case aggregate

* [x] Observation

* [x] ObservationDraft

* [x] Evidence

* [x] EvidenceDraft

* [x] Confidence

* [x] ObservationSource

* [x] Opportunity

* [x] OpportunityDraft

---

## Ready

### TASK-0006 — Hypothesis

Purpose:

Allow VentureOS to formulate testable business hypotheses supported by evidence.

---

### TASK-0007 — Assumption

Purpose:

Record assumptions independently from evidence so they can be challenged and validated.

---

### TASK-0008 — Risk

Purpose:

Capture risks identified during venture evaluation and link them to supporting evidence.

---

### TASK-0009 — Challenge

Purpose:

Allow VentureOS to preserve constructive objections against hypotheses, assumptions, evidence, risks, or decisions.

---

### TASK-0010 — Decision

Purpose:

Record board decisions together with rationale, supporting evidence, objections, and confidence.

---

### TASK-0011 — Lesson

Purpose:

Capture validated learning generated throughout the venture lifecycle.

---

## Future

* Board Review
* Opportunity Score
* Market
* Company
* Competitor
* Customer Segment
* Workflow
* Agent
* Research Session
* Prompt
* Model
* Experiment
* KPI
* Metric

---

## Backlog Rules

1. Domain capabilities are implemented before infrastructure.
2. Every task should produce a working build and a passing test suite.
3. Every commit should represent one meaningful capability.
4. Objects should be introduced only when required by the domain.
5. The backlog may change as the domain becomes better understood.