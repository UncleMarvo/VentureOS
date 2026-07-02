using VentureOS.Application.Board;
using VentureOS.Infrastructure.AI.Personas;

namespace VentureOS.Infrastructure.AI.Prompts;

public static class BoardReviewPrompt
{
    public const string Version = "1.0.0";

    public static string Build(BoardDossierDto dossier)
    {
        var observationsText = RenderList(
            dossier.Observations.Select(observation =>
                $"- summary: {observation.Summary}{Environment.NewLine}" +
                $"  source: {observation.SourceReference}{Environment.NewLine}" +
                $"  confidence: {observation.Confidence}"));

        var supportingEvidenceText = RenderList(
            dossier.SupportingEvidence.Select(evidence =>
                $"- summary: {evidence.Summary}{Environment.NewLine}" +
                $"  interpretation: {evidence.Interpretation}"));

        var contradictingEvidenceText = RenderList(
            dossier.ContradictingEvidence.Select(evidence =>
                $"- summary: {evidence.Summary}{Environment.NewLine}" +
                $"  interpretation: {evidence.Interpretation}"));

        var neutralEvidenceText = RenderList(
            dossier.NeutralEvidence.Select(evidence =>
                $"- summary: {evidence.Summary}{Environment.NewLine}" +
                $"  interpretation: {evidence.Interpretation}"));

        var assumptionsText = RenderList(
            dossier.UnresolvedAssumptions.Select(assumption =>
                $"- statement: {assumption.Statement}{Environment.NewLine}" +
                $"  rationale: {assumption.Rationale}{Environment.NewLine}" +
                $"  confidence: {assumption.Confidence}"));

        var hypothesesText = RenderList(
            dossier.Hypotheses.Select(hypothesis =>
                $"- statement: {hypothesis.Statement}{Environment.NewLine}" +
                $"  reasoning: {hypothesis.Reasoning}{Environment.NewLine}" +
                $"  confidence: {hypothesis.Confidence}"));

        var opportunitiesText = RenderList(
            dossier.Opportunities.Select(opportunity =>
                $"- statement: {opportunity.Statement}{Environment.NewLine}" +
                $"  customerValue: {opportunity.CustomerValue}{Environment.NewLine}" +
                $"  commercialValue: {opportunity.CommercialValue}{Environment.NewLine}" +
                $"  confidence: {opportunity.Confidence}"));

        var challengesText = RenderList(
            dossier.Challenges.Select(challenge =>
                $"- targeting: {challenge.TargetText}{Environment.NewLine}" +
                $"  statement: {challenge.Statement}{Environment.NewLine}" +
                $"  reasoning: {challenge.Reasoning}{Environment.NewLine}" +
                $"  confidence: {challenge.Confidence}"));

        var researchQualityText = RenderList(
            dossier.ResearchQualityFindings.Select(finding =>
                $"- [{finding.Severity}] {finding.Code}: {finding.Message}"));

        var redTeamQualityText = RenderList(
            dossier.RedTeamQualityFindings.Select(finding =>
                $"- [{finding.Severity}] {finding.Code}: {finding.Message}"));

        return $$"""
        {{BoardPersona.Text}}

        Task:
        Prepare a Board briefing for the following venture case, based only on the material below.

        Case:
        Title: {{dossier.Title}}
        Mission: {{dossier.Mission}}

        Observations:
        {{observationsText}}

        Supporting Evidence:
        {{supportingEvidenceText}}

        Contradicting Evidence:
        {{contradictingEvidenceText}}

        Neutral Evidence:
        {{neutralEvidenceText}}

        Unresolved Assumptions:
        {{assumptionsText}}

        Hypotheses:
        {{hypothesesText}}

        Opportunities:
        {{opportunitiesText}}

        Challenges Raised:
        {{challengesText}}

        Research Quality Findings:
        {{researchQualityText}}

        Red Team Quality Findings:
        {{redTeamQualityText}}

        Instructions:
        - Use only the material provided above. Do not invent facts, figures, or sources.
        - You have not been given any identifiers. Do not invent or reference one.
        - Do not decide the outcome. Frame the decision; the human Board decides.
        - If every assumption is unresolved, or evidence is thin, say so plainly.
        - decisionOptions must contain exactly these four outcomes, in this exact order, every
          time: "Approved", "Rejected", "Deferred", "MoreResearchRequired". You supply the
          rationale for each; you do not choose which options exist.
        - Return JSON only.
        - Do not include markdown.
        - Do not include explanations outside the JSON.

        Output schema:
        {
          "executiveSummary": "string",
          "decisionFraming": "string",
          "risks": ["string"],
          "overallConfidenceNarrative": "string",
          "recommendedInvestigations": ["string"],
          "decisionOptions": [
            { "outcome": "Approved", "rationale": "string" },
            { "outcome": "Rejected", "rationale": "string" },
            { "outcome": "Deferred", "rationale": "string" },
            { "outcome": "MoreResearchRequired", "rationale": "string" }
          ]
        }

        Before returning JSON, silently verify:
        - decisionOptions has exactly 4 entries, in the exact order shown above
        - every claim traces back to material provided above
        - no identifier of any kind appears anywhere in the response
        - the JSON exactly matches the schema
        """;
    }

    private static string RenderList(IEnumerable<string> items)
    {
        var list = items.ToList();

        return list.Count == 0
            ? "(none)"
            : string.Join(Environment.NewLine, list);
    }
}
