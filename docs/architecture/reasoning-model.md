# VentureOS Reasoning Model

## Purpose

The Reasoning Model defines how VentureOS transforms raw information into decisions and learning.

It explains the role of each major domain object in the reasoning pipeline.

## Core Principle

Every external input enters VentureOS as an Observation.

Every conclusion leaves VentureOS as a Decision.

Everything else is transformation.

## Reasoning Pipeline

```text
Observation
    ↓
Evidence
    ↓
Hypothesis
    ↓
Challenge
    ↓
Decision
    ↓
Lesson
```

Assumptions sit alongside the pipeline as explicit beliefs being relied upon.

```text
Evidence + Assumptions
        ↓
    Hypothesis
```

## Observation

An Observation preserves raw information.

Examples:

* document extract
* web page quote
* interview statement
* manual note
* previous case finding
* forum post
* review
* dataset entry

An Observation is not a conclusion.

## Evidence

Evidence is the interpretation of one or more Observations.

Evidence may support, contradict, or remain neutral toward a claim.

Evidence answers:

> What does this observation appear to mean?

## Assumption

An Assumption is a belief being relied upon but not yet proven.

Assumptions must remain explicit so they can be challenged, tested, validated, rejected, or revised.

Assumptions answer:

> What are we currently relying on without complete proof?

## Hypothesis

A Hypothesis is a structured, testable claim.

A Hypothesis should include:

* statement
* reasoning
* expected outcome
* success criteria
* supporting evidence
* supporting assumptions
* confidence

A Hypothesis answers:

> Given what we know and what we assume, what do we believe may be true?

## Challenge

A Challenge is a constructive objection.

A Challenge may target:

* Hypothesis
* Assumption
* Evidence
* Risk
* Decision
* Plan

A Challenge should not simply criticise. It should identify a specific weakness, uncertainty, contradiction, or missing piece of evidence.

A Challenge answers:

> What could make this wrong, weak, risky, or incomplete?

## Decision

A Decision records a chosen course of action.

A Decision should include:

* decision question
* chosen option
* alternatives considered
* supporting evidence
* assumptions
* challenges considered
* rationale
* confidence
* expected outcome

A Decision answers:

> Given the current evidence, assumptions, and challenges, what should we do?

## Lesson

A Lesson captures reusable knowledge created from outcomes.

Lessons preserve what VentureOS learned from decisions, experiments, failures, successes, and later evidence.

A Lesson answers:

> What should future cases know because of this?

## Why Challenge Comes Before Decision

VentureOS should not make significant decisions without preserving constructive disagreement.

Challenges ensure that decisions are not made only from supporting evidence.

They allow the Red Team function to strengthen ideas rather than simply reject them.

## Domain Responsibility

The domain model should preserve the reasoning chain.

Agents, users, and services may create or analyse objects, but the reasoning objects themselves remain the source of truth.

## Guiding Test

At any point, VentureOS should be able to answer:

> What did we observe, what did we infer, what did we assume, what did we believe, what challenged that belief, what did we decide, and what did we learn?