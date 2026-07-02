using VentureOS.Application.Research.ResearchPlanning;
using VentureOS.Domain.Cases;
using VentureOS.Infrastructure.AI.Personas;

namespace VentureOS.Infrastructure.AI.Prompts;

public static class EvidenceAcquisitionPrompt
{
    public static string Build(
        Case ventureCase,
        ResearchQuestionDto researchQuestion)
    {
        return $$"""
        {{ResearchAnalystPersona.Text}}

        Task:
        Investigate one research question for the following venture case.

        Case:
        Title: {{ventureCase.Title}}
        Mission: {{ventureCase.Mission}}

        Research Question:
        {{researchQuestion.Question}}

        Purpose:
        Produce a concise evidence acquisition result that can support later venture analysis.

        Instructions:
        - Answer only the research question provided.
        - Do not answer other research questions.
        - Do not make a final recommendation.
        - Do not decide whether the venture should proceed.
        - Do not create observations, evidence, assumptions, hypotheses, or challenges.
        - Do not invent named studies, reports, organisations, dates, statistics, prices, or market sizes.
        - If you do not have reliable evidence, say what is unknown.
        - If your finding is inferred rather than evidenced, say so.
        - Use "AI-generated evidence acquisition" as sourceReference unless a real source is explicitly available.
        - Return JSON only.
        - Do not include markdown.
        - Do not include explanations outside the JSON.
        - Confidence must be an integer from 0 to 100.

        Output schema:
        {
          "researchQuestion": "string",
          "findings": "string",
          "sourceReference": "string",
          "confidence": 0,
          "unknowns": [
            "string"
          ]
        }

        Quality rules:
        - Findings should be concise but useful.
        - Findings should distinguish between evidence, inference, and unknowns.
        - Unknowns should identify missing information that would improve confidence.
        - Do not use unsupported numerical claims.
        - Prefer cautious language over false precision.
        """;
    }
}
