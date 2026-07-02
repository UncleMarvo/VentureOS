using VentureOS.Infrastructure.AI.Personas;

namespace VentureOS.Infrastructure.AI.Prompts;

public static class ResearchExtractionPrompt
{
    public static string Build(string researchAnalysis)
    {
        return $$"""
        {{ResearchAnalystPersona.Text}}

        Task:
        Convert the following research analysis into valid VentureOS research JSON.

        Research analysis:
        {{researchAnalysis}}

        Instructions:
        - Extract structured research proposals only.
        - Use only the information contained in the research analysis.
        - Do not add new claims.
        - Do not add new statistics.
        - Do not add new market facts.
        - Do not add named sources unless they appear in the research analysis.
        - Do not make a final recommendation.
        - Do not decide whether the venture should proceed.
        - Preserve uncertainty.
        - Prefer cautious wording over false precision.
        - Return JSON only.
        - Do not include markdown.
        - Do not include explanations.
        - Do not include fields not shown in the schema.
        - Do not add, remove, rename, or reorder schema fields.
        - Use zero-based indexes.
        - Confidence must be an integer from 0 to 100.

        Object rules:
        - Observations must describe relevant venture, customer, market, technical, operational, competitive, regulatory, behavioural, or economic considerations.
        - Evidence must interpret linked observations.
        - Evidence must not introduce new facts.
        - Assumptions must describe something that must be true for the venture to work.
        - Hypotheses must be testable.
        - Challenges must target a specific evidence item, assumption, or hypothesis.
        - Unsupported or uncertain claims should normally have confidence of 50 or lower.
        - Do not include unsupported numerical claims unless they already appear in the research analysis.

        Output schema:
        {
          "observations": [
            {
              "observationText": "string",
              "summary": "string",
              "sourceReference": "string",
              "confidence": 0
            }
          ],
          "evidence": [
            {
              "summary": "string",
              "interpretation": "string",
              "direction": 0,
              "observationIndexes": [0]
            }
          ],
          "assumptions": [
            {
              "statement": "string",
              "rationale": "string",
              "confidence": 0
            }
          ],
          "hypotheses": [
            {
              "statement": "string",
              "reasoning": "string",
              "expectedOutcome": "string",
              "successCriteria": "string",
              "confidence": 0,
              "evidenceIndexes": [0],
              "assumptionIndexes": [0]
            }
          ],
          "challenges": [
            {
              "statement": "string",
              "reasoning": "string",
              "confidence": 0,
              "targetType": "Hypothesis",
              "targetIndex": 0
            }
          ]
        }

        Additional rules:
        - direction must be 0 for supporting evidence, 1 for contradicting evidence, or 2 for neutral/mixed evidence.
        - targetType must be one of: "Evidence", "Assumption", "Hypothesis".
        - targetIndex must refer to the zero-based index within the selected target collection.
        - observationIndexes must refer to indexes in the observations array.
        - evidenceIndexes must refer to indexes in the evidence array.
        - assumptionIndexes must refer to indexes in the assumptions array.
        - Return between 3 and 7 observations.
        - Return between 2 and 5 evidence items.
        - Return between 2 and 5 assumptions.
        - Return between 1 and 3 hypotheses.
        - Return between 1 and 3 challenges.

        Before returning JSON, silently verify:
        - all indexes are valid
        - all required fields are present
        - no extra fields are present
        - evidence does not introduce new facts
        - the JSON exactly matches the schema
        """;
    }
}
