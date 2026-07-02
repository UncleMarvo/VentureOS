using VentureOS.Domain.Cases;
using VentureOS.Infrastructure.AI.Personas;

namespace VentureOS.Infrastructure.AI.Prompts;

public static class ResearchEvidencePlanningPrompt
{
    public static string Build(Case ventureCase)
    {
        return $$"""
        {{ResearchAnalystPersona.Text}}

        Task:
        Create an evidence plan for investigating the following venture case.

        Case:
        Title: {{ventureCase.Title}}
        Mission: {{ventureCase.Mission}}

        Purpose:
        Identify the most important questions and evidence needs before producing research analysis.

        Instructions:
        - Do not answer the questions.
        - Do not perform the research.
        - Do not invent facts.
        - Do not include sources.
        - Identify what should be investigated.
        - Prioritise commercially important uncertainty.
        - Return JSON only.
        - Do not include markdown.
        - Do not include explanations.
        - Do not include fields not shown in the schema.
        - Priority must be an integer from 1 to 5.
        - 1 means highest priority.
        - 5 means lowest priority.

        Output schema:
        {
          "questions": [
            {
              "question": "string",
              "priority": 1
            }
          ],
          "evidenceNeeds": [
            {
              "topic": "string",
              "reason": "string"
            }
          ]
        }

        Additional rules:
        - Return between 5 and 10 questions.
        - Return between 5 and 10 evidence needs.
        - Questions should focus on what must be learned.
        - Evidence needs should explain what kind of evidence would reduce uncertainty.
        - Avoid generic questions.
        - Avoid questions that could apply to any venture.
        """;
    }
}
