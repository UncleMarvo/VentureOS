# AI Contracts

This folder is reserved for provider/model-facing AI contracts.

At present, `ResearchPackageDto` is used directly as both:
- the Application/API DTO
- the LLM response contract

That is acceptable while the shapes are identical.

Introduce dedicated contract types when the LLM response needs fields that should not belong to the Application DTO, such as:
- token usage
- raw model metadata
- provider-specific fields
- citations/source traces
- internal confidence reasoning
- prompt diagnostics

Likely future type:

`ResearchPackageContract`

Flow:

LLM JSON → ResearchPackageContract → Application DTO
