Technical Debt: Simplify ResearchAnalysisPrompt

Priority: Medium

Background

The current ResearchAnalysisPrompt has grown significantly as we've iterated against smaller local models (currently qwen3:8b).

While the prompt is now functionally correct, it has become long and contains repeated guidance around evidence discipline, numerical discipline, and investigation behaviour.

Long prompts increase token usage, generation time, and may reduce consistency on smaller models.

Objective

Refactor the prompt into a shorter, clearer version while preserving research quality.

The refactoring should:

remove duplicated instructions
consolidate repeated guidance into higher-level rules
reduce overall prompt size
maintain board-quality output
maintain compatibility with the Research Extraction stage
validate against both local and frontier models

Success Criteria

Prompt length reduced significantly (target ~30–40%)
No reduction in extraction quality
No increase in unsupported numerical claims
Equal or better research quality during evaluation

Notes

Do not optimise specifically for qwen3:8b.

Optimise for the Research Procedure.

Evaluate prompt behaviour across multiple providers before considering the work complete.