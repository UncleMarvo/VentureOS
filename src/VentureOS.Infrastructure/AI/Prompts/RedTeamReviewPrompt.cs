using VentureOS.Domain.Cases;
using VentureOS.Infrastructure.AI.Personas;

namespace VentureOS.Infrastructure.AI.Prompts;

public static class RedTeamReviewPrompt
{
    public const string Version = "1.0.0";

    public static string Build(Case ventureCase)
    {
        var evidenceText = string.Join(
            Environment.NewLine,
            ventureCase.Evidence.Select(evidence =>
                $"- id: {evidence.Id}{Environment.NewLine}" +
                $"  direction: {evidence.Direction}{Environment.NewLine}" +
                $"  summary: {evidence.Summary}{Environment.NewLine}" +
                $"  interpretation: {evidence.Interpretation}"));

        var assumptionsText = string.Join(
            Environment.NewLine,
            ventureCase.Assumptions.Select(assumption =>
                $"- id: {assumption.Id}{Environment.NewLine}" +
                $"  statement: {assumption.Statement}{Environment.NewLine}" +
                $"  rationale: {assumption.Rationale}{Environment.NewLine}" +
                $"  confidence: {assumption.Confidence.Value}"));

        var hypothesesText = string.Join(
            Environment.NewLine,
            ventureCase.Hypotheses.Select(hypothesis =>
                $"- id: {hypothesis.Id}{Environment.NewLine}" +
                $"  statement: {hypothesis.Statement}{Environment.NewLine}" +
                $"  reasoning: {hypothesis.Reasoning}{Environment.NewLine}" +
                $"  expectedOutcome: {hypothesis.ExpectedOutcome}{Environment.NewLine}" +
                $"  successCriteria: {hypothesis.SuccessCriteria}{Environment.NewLine}" +
                $"  confidence: {hypothesis.Confidence.Value}"));

        var opportunitiesText = string.Join(
            Environment.NewLine,
            ventureCase.Opportunities.Select(opportunity =>
                $"- id: {opportunity.Id}{Environment.NewLine}" +
                $"  statement: {opportunity.Statement}{Environment.NewLine}" +
                $"  customerValue: {opportunity.CustomerValue}{Environment.NewLine}" +
                $"  commercialValue: {opportunity.CommercialValue}{Environment.NewLine}" +
                $"  differentiation: {opportunity.Differentiation}{Environment.NewLine}" +
                $"  timing: {opportunity.Timing}{Environment.NewLine}" +
                $"  confidence: {opportunity.Confidence.Value}"));

        return $$"""
        {{RedTeamAnalystPersona.Text}}

        Task:
        Review the following venture case's accepted evidence, assumptions, hypotheses, and
        opportunities. Raise challenges only where a specific weakness, contradiction, unsupported
        claim, or missing validation exists.

        Case:
        Title: {{ventureCase.Title}}
        Mission: {{ventureCase.Mission}}

        Evidence:
        {{evidenceText}}

        Assumptions:
        {{assumptionsText}}

        Hypotheses:
        {{hypothesesText}}

        Opportunities:
        {{opportunitiesText}}

        Instructions:
        - Each challenge must target exactly one id shown above, using its exact value.
        - Never invent an id. Never target an id that is not listed above.
        - Never target a Decision. Decisions are not reviewable by Red Team.
        - Do not propose new evidence, assumptions, hypotheses, or opportunities.
        - Do not make a final recommendation.
        - Do not decide whether the venture should proceed.
        - Return zero challenges if the reasoning is genuinely sound. There is no minimum count.
        - Prefer a small number of specific, well-reasoned challenges over many shallow ones.
        - Return JSON only.
        - Do not include markdown.
        - Do not include explanations outside the JSON.
        - Confidence must be an integer from 0 to 100, expressing how confident you are in the
          challenge itself.

        Output schema:
        {
          "challenges": [
            {
              "statement": "string",
              "reasoning": "string",
              "confidence": 0,
              "targetType": "Evidence",
              "targetId": "guid-string"
            }
          ]
        }

        Additional rules:
        - targetType must be one of: "Evidence", "Assumption", "Hypothesis", "Opportunity".
        - targetId must be copied verbatim from the id field of the item being challenged.
        - reasoning should explain the specific weakness, and may describe its severity, a possible
          mitigation, and the conditions under which the challenge would be withdrawn.

        Before returning JSON, silently verify:
        - every targetId matches an id shown above
        - no targetType is "Decision"
        - all required fields are present
        - no extra fields are present
        - the JSON exactly matches the schema
        """;
    }
}
