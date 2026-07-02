using VentureOS.Domain.Cases;
using VentureOS.Infrastructure.AI.Personas;

namespace VentureOS.Infrastructure.AI.Prompts;

public static class ResearchAnalysisPrompt
{
    public static string Build(Case ventureCase)
    {
        return $$"""
        {{ResearchAnalystPersona.Text}}

        Task:
        Produce a board-quality early research analysis for the following venture case.

        Case:
        Title: {{ventureCase.Title}}
        Mission: {{ventureCase.Mission}}

        Purpose:
        Help a human founder, operator, or investor understand whether this case deserves deeper investigation.

        Important:
        This is analysis only.
        Do not return JSON.
        Do not force the output into Domain objects.
        Do not make a final recommendation.
        Do not decide whether the venture should proceed.

        Research standard:
        - Be commercially useful.
        - Be specific to the case.
        - Surface uncertainty clearly.
        - Avoid hype.
        - Avoid generic startup advice.
        - Prefer sharp insight over volume.
        - Distinguish between what is known, what is inferred, and what must be validated.

        Evidence discipline:
        - Do not invent statistics.
        - Do not invent market sizes.
        - Do not invent adoption rates.
        - Do not invent pricing.
        - Do not invent competitor claims.
        - Do not invent named studies, reports, organisations, dates, or source titles.
        - If something is not known, say it is unknown.
        - If something is plausible but unverified, describe it as a hypothesis or assumption.

        Numerical discipline:
        - Do not create numerical targets.
        - Do not create percentage targets.
        - Do not create pricing targets.
        - Do not create adoption targets.
        - Do not create conversion targets.
        - Do not create time-saving targets.
        - Do not create time-spent estimates.
        - If a metric would be useful, describe the metric to validate, not the expected number.

        Analyse the case using these sections:

        1. Situation Understanding
        Explain what the venture is trying to determine.

        2. Commercial Relevance
        Identify why this problem may or may not matter commercially.

        3. Customer and Buyer Considerations
        Identify likely customer pain, buyer motivation, adoption friction, and willingness-to-pay uncertainty.

        4. Market and Competitive Considerations
        Identify likely market dynamics, substitutes, differentiation risks, and competitive pressure.

        5. Operational, Technical, and Trust Considerations
        Identify workflow, integration, accuracy, privacy, reliability, and implementation concerns.

        6. Key Assumptions
        Identify what must be true for the venture to work.

        7. Testable Hypotheses
        Identify what should be validated next.

        8. Challenges and Risks
        Identify the strongest reasons the opportunity may fail or be less attractive than expected.

        9. Validation Priorities
        Identify the most important things a human should investigate next.

        Hypotheses should define what to test, not predict the result.

        Bad:
        "SMEs will pay $5-10 per user per month."

        Good:
        "SMEs will show willingness to pay for an AI meeting assistant when the perceived administrative time saving is clear."

        Keep the analysis concise but thoughtful.
        """;
    }
}
