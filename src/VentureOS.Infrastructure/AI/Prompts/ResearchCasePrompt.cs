using VentureOS.Domain.Cases;
using VentureOS.Infrastructure.AI.Personas;

namespace VentureOS.Infrastructure.AI.Prompts;

public static class ResearchCasePrompt
{
    public static string Build(Case ventureCase)
    {
        return $$"""
        {{ResearchAnalystPersona.Text}}

        Task:
        Research the following venture case mission.

        Case:
        Title: {{ventureCase.Title}}
        Mission: {{ventureCase.Mission}}

        Instructions:
        - Identify early research material that could help a human decide whether this case deserves deeper investigation.
        - Do not make a final recommendation.
        - Do not include named studies, reports, organisations, dates, statistics, or source titles unless they were provided in the case context. If no real source is available, use "AI-generated research hypothesis" as sourceReference.
        - Prefer useful uncertainty over false precision.
        - Keep outputs concise but specific.
        - Return JSON only.
        - Do not include markdown.
        - Do not include explanations.
        - Do not include fields not shown in the schema.
        - Use zero-based indexes.
        - Confidence must be an integer from 0 to 100.

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
        """;
    }
}
