# End-to-End MFP Walkthrough (Postman)

This walks a single venture case through the full Minimum Foundation Platform loop —
`Case → Research → accept → Red Team → accept → Board Review → Decision` — using Postman
against a locally running API and Ollama instance.

Every request/response shape below was captured from a real run of this exact sequence
(2026-07-02, `qwen3:8b`, CPU inference), not hand-typed from the source. Field names, casing,
and structure are verified, not assumed.

## Prerequisites

1. **Ollama running locally** with the model configured in `src/VentureOS.Api/appsettings.json`
   (`Ollama:Model`, default `qwen3:8b`) already pulled:
   ```
   ollama list
   ```
   If it's missing: `ollama pull qwen3:8b`. Ollama must be reachable at `Ollama:BaseUrl`
   (default `http://localhost:11434`).

2. **The API running locally**:
   ```
   dotnet run --project src/VentureOS.Api/VentureOS.Api.csproj
   ```
   Note the URL it prints — the `http` launch profile defaults to `http://localhost:5197`
   (`src/VentureOS.Api/Properties/launchSettings.json`).

3. **Postman** with a new Environment containing one variable:
   | Variable  | Initial value           |
   |-----------|--------------------------|
   | `baseUrl` | `http://localhost:5197` |

   Every request below uses `{{baseUrl}}`. Select this environment before starting.

## Set expectations on timing

This pipeline calls a local LLM multiple times per step. On a CPU-only local model, expect:

| Step                    | Ollama calls | Observed duration |
|--------------------------|:---:|---|
| Research deep-dive       | 5 (plan, 3× evidence acquisition, analysis, extraction) | ~10 minutes |
| Red Team review          | 1 | ~3 minutes |
| Board review             | 1 | ~1.5 minutes |

In Postman, open **Settings → General → Request timeout** and set it to `0` (no timeout) or a
large value (e.g. `900000` ms) before running the research and red-team steps, or Postman will
abort the request before Ollama responds.

---

## Step 1 — Create a Case

**POST** `{{baseUrl}}/cases`

Body (raw JSON):
```json
{
  "Title": "AI-Powered Meeting Notes SaaS",
  "Mission": "Validate whether small consulting firms would pay for an AI tool that turns client call recordings into structured action items and follow-up emails."
}
```

Expect **201 Created**:
```json
{
  "caseId": "e597259e-08b9-4a76-96ad-6a37fcb3d8ac",
  "title": "AI-Powered Meeting Notes SaaS",
  "mission": "Validate whether small consulting firms would pay for an AI tool that turns client call recordings into structured action items and follow-up emails."
}
```

**In Postman:** add a Post-response Script (Tests tab) so every later step can reference
`{{caseId}}` automatically:
```javascript
const body = pm.response.json();
pm.environment.set("caseId", body.caseId);
```

---

## Step 2 — Research Deep-Dive (AI proposes, Domain untouched)

**POST** `{{baseUrl}}/cases/{{caseId}}/research/deep-dive`

No body needed. This runs all four Research capabilities (Planning → Evidence Acquisition →
Analysis → Extraction) and returns a proposal package — **nothing is persisted yet**.

Expect **200 OK**, shaped like:
```json
{
  "researchPackage": {
    "caseId": "e597259e-...",
    "mission": "...",
    "generation": {
      "provider": "Ollama",
      "model": "qwen3:8b",
      "persona": "Research Analyst",
      "personaVersion": "1.0.0",
      "promptVersion": "1.0.0+1.0.0+1.0.0+1.0.0",
      "generatedAtUtc": "2026-07-02T19:57:21Z",
      "duration": "00:10:22.89",
      "limitations": "AI-generated research requiring human review."
    },
    "observations": [ { "observationText": "...", "summary": "...", "sourceReference": "...", "confidence": 90 } ],
    "evidence": [ { "summary": "...", "interpretation": "...", "direction": 0, "observationIndexes": [0] } ],
    "assumptions": [ { "statement": "...", "rationale": "...", "confidence": 70 } ],
    "opportunities": [ { "statement": "...", "customerValue": "...", "commercialValue": "...", "differentiation": "...", "timing": "...", "confidence": 75, "evidenceIndexes": [0], "assumptionIndexes": [0] } ],
    "hypotheses": [ { "statement": "...", "reasoning": "...", "expectedOutcome": "...", "successCriteria": "...", "confidence": 60, "evidenceIndexes": [0], "assumptionIndexes": [0] } ],
    "challenges": [ { "statement": "...", "reasoning": "...", "confidence": 50, "targetType": "Hypothesis", "targetIndex": 0 } ]
  },
  "qualityIssues": []
}
```

`evidence[].direction` is an integer: `0 = Supports`, `1 = Contradicts`, `2 = Neutral`.
`challenges[].targetIndex` refers to an index into whichever array `targetType` names — these
are proposal-local indexes, not real ids, since nothing is persisted yet.

If `qualityIssues` is non-empty, read each entry (`severity`/`code`/`path`/`message`) before
proceeding — these flag unsupported numeric claims, bad indexes, or missing fields in the AI
proposal. You can still accept a package with `Warning`-level issues; treat `Error`-level ones
as things to fix before accepting (there's no automatic block — quality checking informs human
judgement, it doesn't replace it).

**In Postman:** add a Post-response Script to hold onto the package for the next step:
```javascript
const body = pm.response.json();
pm.environment.set("researchPackage", JSON.stringify(body.researchPackage));
```

---

## Step 3 — Accept Research (mutates the Domain)

**POST** `{{baseUrl}}/cases/{{caseId}}/research/accept`

Body (raw JSON) — paste `{{researchPackage}}` directly as the body if you used the script
above, or paste the entire `researchPackage` object from Step 2's response by hand:
```
{{researchPackage}}
```

Expect **200 OK**:
```json
{
  "caseId": "e597259e-...",
  "observationsCreated": 3,
  "evidenceCreated": 3,
  "assumptionsCreated": 2,
  "opportunitiesCreated": 1,
  "hypothesesCreated": 2,
  "challengesCreated": 2
}
```

**Verify:** `GET {{baseUrl}}/cases/{{caseId}}/brief` — `counts` should now match the numbers
above, with `latestDecision: null`.

---

## Step 4 — Red Team Review (AI proposes challenges against real Domain data)

**POST** `{{baseUrl}}/cases/{{caseId}}/red-team/review`

No body needed. Unlike Step 2, this reviews the **real, already-accepted** Evidence/
Assumptions/Hypotheses/Opportunities from Step 3 — so any challenges it raises target real
Guids, not proposal indexes.

Expect **200 OK**:
```json
{
  "review": {
    "caseId": "e597259e-...",
    "mission": "...",
    "generation": {
      "provider": "Ollama", "model": "qwen3:8b", "persona": "Red Team Analyst",
      "personaVersion": "1.0.0", "promptVersion": "1.0.0",
      "generatedAtUtc": "2026-07-02T20:01:15Z", "duration": "00:02:51.38",
      "limitations": "AI-generated red team review requiring human review."
    },
    "challenges": [
      {
        "statement": "No tool addresses structured action item extraction or automated follow-ups.",
        "reasoning": "The claim that no tool exists is an overstated assertion without specific examples or data...",
        "confidence": 70,
        "targetType": "Evidence",
        "targetId": "d0265950-8993-46d4-a7e8-9df05297b918"
      }
    ]
  },
  "qualityIssues": []
}
```

It is normal and correct to see **zero challenges** here if the model judges the accepted
reasoning sound — Red Team is instructed not to manufacture objections for their own sake.

A non-empty `qualityIssues` list most often means `targetId` didn't match any real id in the
case (the model mistyped a Guid) — this is a known reliability risk with small local models
copying long identifiers verbatim; see `docs/handovers/phase4-complete.md`.

**In Postman:**
```javascript
const body = pm.response.json();
pm.environment.set("redTeamReview", JSON.stringify(body.review));
```

---

## Step 5 — Accept Red Team Review

**POST** `{{baseUrl}}/cases/{{caseId}}/red-team/accept`

Body: `{{redTeamReview}}` (or paste the `review` object from Step 4 by hand).

Expect **200 OK**:
```json
{ "caseId": "e597259e-...", "challengesCreated": 1 }
```

**Verify:** `GET {{baseUrl}}/cases/{{caseId}}/brief` — `challenges` count should now be the
Step 3 count plus this step's `challengesCreated`.

---

## Step 6 — Board Review (briefing, not a decision)

**POST** `{{baseUrl}}/cases/{{caseId}}/board/review`

Body (raw JSON) — the whole body is optional; pass empty arrays if you have no quality findings
on hand, or paste the `qualityIssues` arrays captured from Steps 2/4 if you want them echoed
into the briefing:
```json
{
  "ResearchQualityFindings": [],
  "RedTeamQualityFindings": []
}
```

Expect **200 OK** with two top-level sections:

- **`dossier`** — deterministic facts assembled directly from the real Case, zero AI involved:
  `observations`, `supportingEvidence`/`contradictingEvidence`/`neutralEvidence`,
  `unresolvedAssumptions` (currently *all* assumptions — the Domain has no status-transition
  mechanism yet, so this is honestly named, not a filtered subset), `hypotheses`,
  `opportunities`, and `challenges` (each with `targetText` resolved to the actual challenged
  item's wording, not just a raw `targetId`).
- **`narrative`** — AI-synthesized, and deliberately contains **no identifiers at all**:
  `executiveSummary`, `decisionFraming`, `risks` (list), `overallConfidenceNarrative`,
  `recommendedInvestigations` (list), and `decisionOptions` — always exactly 4 entries, in this
  fixed order (`Approved`, `Rejected`, `Deferred`, `MoreResearchRequired`), each with a
  case-specific `rationale`.

Example `narrative` from the verified run:
```json
{
  "executiveSummary": "The venture proposes an AI tool to automate structured action item extraction and follow-up emails for small consulting firms. Evidence confirms pain points in manual processes and gaps in existing tools, but uncertainties remain about tool accuracy, pricing alignment, and the existence of niche solutions.",
  "decisionFraming": "The Board must evaluate whether the tool's value proposition addresses critical pain points...",
  "risks": [
    "AI-generated notes may lack the accuracy required for professional use",
    "Subscription pricing may exceed small firms' budget constraints",
    "The claim of no existing tools may be overstated, undermining the perceived market gap"
  ],
  "overallConfidenceNarrative": "Confidence is high in the problem statement and existing tool limitations (85-90%), but lower in assumptions about tool effectiveness (70%)...",
  "recommendedInvestigations": [
    "Validate AI tool accuracy through pilot testing with sample call recordings",
    "Conduct pricing sensitivity analysis with small firm stakeholders",
    "Perform a comprehensive audit of existing tools for structured action item extraction capabilities"
  ],
  "decisionOptions": [
    { "outcome": "Approved", "rationale": "If the tool demonstrates reliable accuracy, pricing aligns with budget constraints, and the market gap is validated..." },
    { "outcome": "Rejected", "rationale": "If tool reliability remains unproven, pricing exceeds affordability, or competing solutions exist..." },
    { "outcome": "Deferred", "rationale": "If unresolved uncertainties about tool effectiveness, pricing, and market gap persist without clear paths to resolution..." },
    { "outcome": "MoreResearchRequired", "rationale": "Critical gaps remain in validating tool accuracy, pricing models, and the uniqueness of the solution..." }
  ]
}
```

This is the point where a human reads the briefing and decides — the API never chooses an
outcome for you.

**In Postman**, pull out real ids from `dossier` for the next step (adjust field names to
whichever items you actually want to cite — this example cites two Evidence, two Assumptions,
two Hypotheses, and all three Challenges):
```javascript
const body = pm.response.json();
const d = body.dossier;
pm.environment.set("evidenceIds", JSON.stringify(d.supportingEvidence.concat(d.contradictingEvidence).map(e => e.id)));
pm.environment.set("assumptionIds", JSON.stringify(d.unresolvedAssumptions.map(a => a.id)));
pm.environment.set("hypothesisIds", JSON.stringify(d.hypotheses.map(h => h.id)));
pm.environment.set("challengeIds", JSON.stringify(d.challenges.map(c => c.id)));
```

---

## Step 7 — Record the Decision (human act, pre-existing endpoint)

**POST** `{{baseUrl}}/cases/{{caseId}}/decisions`

Board Review has no "accept" step of its own — there's nothing to accept, since a briefing is
never persisted. This is the actual decision-recording endpoint, unchanged by Board Review.

Body (raw JSON) — `Outcome` must be one of the four strings from Step 6's `decisionOptions`
(`Approved`/`Rejected`/`Deferred`/`MoreResearchRequired`); `EvidenceIds`/`HypothesisIds` must be
non-empty, `AssumptionIds`/`ChallengeIds` may be empty:
```json
{
  "Question": "Should we proceed with the AI meeting notes tool for small consulting firms?",
  "Outcome": "MoreResearchRequired",
  "Rationale": "Evidence supports a real pain point and a plausible gap in existing tools, but tool accuracy, pricing sensitivity, and the uniqueness of the market gap remain unresolved per the Board briefing.",
  "ExpectedOutcome": "A follow-up pilot will validate AI note accuracy and pricing tolerance before committing further investment.",
  "Confidence": 55,
  "EvidenceIds": {{evidenceIds}},
  "AssumptionIds": {{assumptionIds}},
  "HypothesisIds": {{hypothesisIds}},
  "ChallengeIds": {{challengeIds}}
}
```

Expect **201 Created**:
```json
{
  "caseId": "e597259e-...",
  "decisionId": "71c2d84a-ece5-47c0-9f9e-07215223832c",
  "outcome": "MoreResearchRequired",
  "question": "Should we proceed with the AI meeting notes tool for small consulting firms?"
}
```

---

## Step 8 — Verify the full audit trail

**GET** `{{baseUrl}}/cases/{{caseId}}/timeline`

Expect one `items[]` entry per Observation/Evidence/Assumption/Opportunity/Hypothesis/
Challenge/Decision created above, in creation order, ending with the `Decision` item. This is
the permanent record the Constitution requires — every observation, assumption, hypothesis,
challenge, and decision remains linked and reviewable.

**GET** `{{baseUrl}}/cases/{{caseId}}/brief` should also show `latestDecision` populated with
the Step 7 decision.

---

## Troubleshooting

- **Request hangs, then Postman times out**: increase or disable the Postman request timeout
  (see "Set expectations on timing" above) — this is Ollama running the model, not a bug.
- **`500` with an Ollama connection error**: Ollama isn't running or isn't at the configured
  `BaseUrl`. Check `ollama list` responds and `appsettings.Development.json`'s `Ollama:BaseUrl`.
- **`400 BadHttpRequestException: Required parameter ... was not provided from body`** on an
  accept/review-accept/decision call: the request body is missing or empty — check the Body tab
  is set to `raw`/`JSON`, not `none`, and that a Postman variable substitution didn't silently
  resolve to nothing (e.g. the Post-response Script from the prior step didn't run). An "empty"
  `{{variable}}` resolving to nothing is the most common cause.
- **Red Team or Board returns a non-empty `qualityIssues`/degraded narrative quality**: expected
  occasionally with small local models — Red Team's Guid-copying-verbatim task and Board's
  narrative synthesis are both flagged as areas that may need prompt tuning per
  `docs/handovers/phase4-complete.md` and `phase5-complete.md`. Re-running the same step against
  the same case state will typically produce a different (and sometimes better) result, since
  nothing about these AI calls is deterministic.
- **Every Assumption/Hypothesis/Opportunity shows `"status": "Proposed"` no matter what**: this
  is a known, documented Domain gap, not a bug in Board Review — see `phase5-complete.md`.
