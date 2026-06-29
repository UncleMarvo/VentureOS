# VentureOS Technology Stack

## Status

Proposed

## Purpose

This document defines the initial technology stack for the VentureOS Minimum Foundation Platform.

The stack should support:

- local-first operation
- open-source foundations
- evidence-led object storage
- browser-based interaction
- API-first architecture
- AI-assisted workflows
- future extensibility

## Guiding Principle

The main backend should be built in the technology most likely to help the Founding Board deliver and maintain the system.

Developer fluency is a valid architectural consideration.

## Initial Stack

### Frontend

React with TypeScript.

Purpose:

- browser-based interface
- Case screen
- evidence views
- decision views
- timeline views
- future dashboard

### Main Backend

ASP.NET Core.

Purpose:

- primary API
- domain model
- Case management
- object management
- decision recording
- audit trail
- authentication later
- integration with storage systems

Rationale:

The Founding Board has stronger experience with C# and ASP.NET Core than Python/FastAPI.

VentureOS has a strong object and domain model, which suits C# well.

### Agent / Intelligence Worker

Python service.

Purpose:

- local LLM orchestration
- Red Team review
- Board briefing generation
- document extraction
- AI-assisted classification
- future LangGraph workflows

Rationale:

The AI tooling ecosystem is strongest in Python. Python should support intelligence workflows without owning the core product backend.

### LLM Runtime

Ollama.

Purpose:

- local model execution
- open-source model support
- local-first experimentation

Candidate models:

- Mistral
- Qwen
- Llama
- Gemma
- other suitable open-source models

### System Database

PostgreSQL.

Purpose:

- durable VentureOS objects
- Cases
- observations
- evidence
- hypotheses
- objections
- decisions
- lessons
- audit trail
- object relationships

Rationale:

PostgreSQL is better suited than SQLite for the long-term application database.

### Analytics Database

DuckDB.

Purpose:

- local analytical queries
- large CSV / Parquet datasets
- imported business records
- scraped datasets
- research analysis
- market datasets

Rationale:

DuckDB is attractive for high-performance local analytical workloads and should be evaluated as part of the research and analytics layer.

### Vector Search

Qdrant.

Purpose:

- embeddings
- semantic search
- document similarity
- observation retrieval
- evidence discovery

### Document Storage

Local filesystem initially.

Purpose:

- attached documents
- research artefacts
- imported files
- generated reports

This may later evolve into more structured object storage.

## High-Level Architecture

```text
Browser UI
    ↓
ASP.NET Core API
    ↓
PostgreSQL / DuckDB / Qdrant / Local Files
    ↓
Python Agent Worker
    ↓
Ollama / Local Models / AI Tools