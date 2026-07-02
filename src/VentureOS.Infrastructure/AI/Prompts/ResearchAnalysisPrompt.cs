using VentureOS.Application.Research.EvidenceAcquisition;
using VentureOS.Application.Research.ResearchPlanning;
using VentureOS.Domain.Cases;
using VentureOS.Infrastructure.AI.Personas;

namespace VentureOS.Infrastructure.AI.Prompts;

public static class ResearchAnalysisPrompt
{
    public const string Version = "1.0.0";

    public static string Build(
        Case ventureCase,
        ResearchEvidencePlanDto evidencePlan,
        EvidenceAcquisitionResultDto acquiredEvidence)
    {
        var researchQuestions = string.Join(
            Environment.NewLine,
            evidencePlan.Questions
                .OrderBy(question => question.Priority)
                .Select(question => $"- [Priority {question.Priority}] {question.Question}"));

        var evidenceNeeds = string.Join(
            Environment.NewLine,
            evidencePlan.EvidenceNeeds
                .Select(need => $"- {need.Topic}{Environment.NewLine}  Reason: {need.Reason}"));

        var acquiredEvidenceText = string.Join(
            Environment.NewLine + Environment.NewLine,
            acquiredEvidence.Evidence.Select(e =>
                $"""
                - Question: {e.ResearchQuestion}
                  Findings: {e.Findings}
                  Source: {e.SourceReference}
                  Confidence: {e.Confidence}
                  Unknowns:
                    {string.Join(Environment.NewLine, e.Unknowns.Select(u => $"    - {u}"))}
                """));

        return $$"""
        {{ResearchAnalystPersona.Text}}

        Task:
        Produce a board-quality early research analysis for the following venture case.

        Case:
        Title: {{ventureCase.Title}}
        Mission: {{ventureCase.Mission}}

        Evidence Plan:
        Research Questions:
        {{researchQuestions}}

        Evidence Required:
        {{evidenceNeeds}}

        Acquired Evidence:
        {{acquiredEvidenceText}}

        Purpose:
        Help a human founder, operator, investor, or advisor decide whether this venture deserves deeper investigation.

        Important:
        This is analysis only.
        Do not return JSON.
        Do not force the output into Domain objects.
        Do not make a final recommendation.
        Do not decide whether the venture should proceed.
        Do not output a template.
        Do not include placeholders.
        Do not include example text.
        Do not use curly-brace placeholder syntax.
        Every sentence must refer to the actual venture case.
        If the case does not provide enough information, say what is unknown.

        Research Standard:
        - Be commercially useful.
        - Be specific to the venture.
        - Surface uncertainty clearly.
        - Avoid hype.
        - Avoid generic startup advice.
        - Prefer clear reasoning over unsupported conclusions.
        - Distinguish between what is known, what is inferred, and what requires validation.

        Evidence Discipline:
        - Do not invent statistics.
        - Do not invent market sizes.
        - Do not invent adoption rates.
        - Do not invent pricing.
        - Do not invent competitor capabilities.
        - Do not invent named studies, reports, organisations, dates, or source titles.
        - If something is unknown, state that it is unknown.
        - If something is plausible but unverified, describe it as an assumption or hypothesis.

        Numerical Discipline:
        - Do not create numerical targets.
        - Do not create percentage targets.
        - Do not create pricing targets.
        - Do not create adoption targets.
        - Do not create conversion targets.
        - Do not create time-saving targets.
        - Do not create time-spent estimates.
        - If a metric would be useful, describe the metric that should be validated rather than inventing a value.

        Use the evidence plan to guide your investigation.
        Where evidence is unavailable, state that it is currently unknown rather than inventing an answer.

        Use the following framework as thinking guidance.
        Write actual analysis for this venture case.
        Do not reproduce the framework as a template.

        1. Venture

        Understand the venture.

        Identify:
        - the proposed venture
        - the outcome it seeks
        - the decision this research should inform

        2. Problem

        Investigate the problem.

        Identify:
        - the problem being addressed
        - who experiences it
        - why it matters
        - the impact if left unsolved

        3. Customer

        Investigate the customer.

        Identify:
        - primary customer
        - primary beneficiary
        - likely buyer
        - likely decision maker
        - adoption considerations

        4. Market

        Investigate the market.

        Identify:
        - market characteristics
        - maturity
        - growth drivers
        - fragmentation
        - notable trends
        - relevant regulatory or environmental factors

        5. Existing Behaviour

        Investigate how the problem is solved today.

        Identify:
        - existing products
        - manual processes
        - workarounds
        - limitations of current approaches

        6. Opportunity

        Investigate the opportunity.

        Identify:
        - customer value
        - commercial value
        - differentiation opportunities
        - timing considerations

        7. Feasibility

        Investigate feasibility.

        Consider:
        - technical feasibility
        - operational feasibility
        - commercial feasibility
        - regulatory feasibility

        8. Assumptions

        Identify the assumptions that must be true for the venture to succeed.

        Explain why each assumption matters.

        9. Validation

        Identify the most important uncertainties.

        For each uncertainty, identify:
        - what should be validated
        - what evidence would reduce uncertainty
        - what experiment or investigation should be performed

        Validation hypotheses should identify what should be tested.

        Do not predict outcomes.

        Do not invent:
        - prices
        - market sizes
        - percentages
        - sample sizes
        - trial durations
        - conversion rates
        - adoption rates
        - accuracy rates
        - time savings
        - success thresholds

        A hypothesis should describe an uncertainty that can be investigated.

        Success criteria should describe the evidence that would increase or decrease confidence in the hypothesis, without inventing numerical targets.

        If a numerical value has not been supplied in the venture case or supported by credible evidence, do not create one.

        10. Risks

        Identify the greatest risks.

        Consider:
        - customer risk
        - market risk
        - competitive risk
        - technical risk
        - operational risk
        - commercial risk

        11. Recommendation

        Do not recommend proceeding or rejecting the venture.

        Recommend only:
        - the next areas for investigation
        - the highest priority validation activities
        - the greatest remaining uncertainties

        Keep the analysis concise but thoughtful.
        """;
    }
}
