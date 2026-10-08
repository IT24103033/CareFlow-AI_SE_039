# Member 3 submission and reproduction

Dinuwara K.D.S - IT24103033 - Group SE_039 - 8 October 2026

This is the Member 3 contribution, not the complete group report. Merge with the other members' work before submitting.

## Verified result

Final suite: 82 cases, 81 passed, 0 failed, 1 skipped. There are 21 newly added cases (6 planning retry, 6 action, 6 safety, 1 HTTP workflow, 2 captured-response checks). Repeated baseline and later runs are not additive.

## Reproduce

From the repository root, using .NET SDK 8:

```bash
dotnet test backend-api.Tests/CareFlowAI.API.Tests.csproj \
  --filter 'FullyQualifiedName~Member3|FullyQualifiedName~PlanningAgentTests|FullyQualifiedName~DomainAnalysisAgentTests|FullyQualifiedName~TriageOrchestrationIntegrationTests|FullyQualifiedName~TriageReviewTests|FullyQualifiedName~AppointmentSchedulingTests' \
  --logger 'trx;LogFileName=member3-reproduction.trx' \
  --results-directory output/testing/member3/reproduction
```

Focused demonstration:

```bash
dotnet test backend-api.Tests/CareFlowAI.API.Tests.csproj --filter 'FullyQualifiedName~Member3WorkflowIntegrationTests'
dotnet test backend-api.Tests/CareFlowAI.API.Tests.csproj --filter 'FullyQualifiedName~Member3AgentTests'
```

The tests use isolated EF InMemory, controlled provider responses, test JWTs and local captured JSON. They do not require the live API or Gemini. Keep both live JSON files under output/testing/member3/live-ai. The test project copies them to its output directory.

The integration test creates its own synthetic fixture; it does not change the shared PostgreSQL database. It tests real HTTP routing/authentication and business services but not React/Flutter or PostgreSQL constraints. Domain/assessment responses are stubbed. The existing concurrency case is explicitly skipped, not passed.

## Package layout and source

The ZIP preserves repository-relative paths. source-changes.patch contains the tracked changes against base commit a0718be37435ec2f9ad96b7ec967134ce73b7b17. New test files are supplied separately at backend-api.Tests/. Existing repository tests are dependencies of the new integration fixture. Apply the patch only to a matching clean checkout after checking it with git apply --check; do not blindly overwrite a teammate's modified files. The changed source-file snapshots are also included for review.

Main report: output/pdf/SE3110_Member3_IT24103033_Testing_Report.pdf
Completed new-case document and full execution inventory: output/testing/member3/submission/TEST_CASES.md
Final evidence: output/testing/member3/final/member3-final.trx
Supplementary action/safety rerun after refining the wrong-slot fixture to a valid interval: agent-final/action-safety-final.trx

Failure evidence:
- agents-before-fix/agents-before-fix.trx: missing booking ID incorrectly reported Success.
- workflow-before-fix/workflow-before-fix.trx: patient GET omitted review history.
- integration/member3-integration.trx: initial test expected 400; application correctly used 409. This is a test correction, not a product defect.

Live evidence: two user-captured Swagger JSON responses. Their offline xUnit assertions are not new live calls. Both had no available slots; the normal/injection urgency difference remains an observation, not a proven causal defect. Do not submit token/password-bearing screenshots.

## Before group submission

Review the PDF and test code, run and explain a new test, commit relevant changes/evidence on test/ai-integration-member3, and supply the resulting commit link to the group. No commit, push or LMS submission was performed by the assistant. Include required performance/security sections from the other members.

AI assistance: Codex assisted with test implementation, debugging, execution and documentation; the student performed the initial baseline/retry and live Swagger runs. Adapt this factual disclosure to the official CLEAR template and verify all submitted claims.

## Latest student-run evidence and documents

- AI summary: ai-summary/member3-ai-summary.trx — 46 passed, 0 failed, 0 skipped.
- Workflow: workflow/member3-workflow.trx — 1 passed, 0 failed.
- Required table: output/pdf/SE3110_Member3_IT24103033_Test_Cases.pdf (47 selected cases).
- Editable table: submission/TEST_CASES.md.
- Defect table: submission/DEFECT_REPORT.md; also covered in the report PDF.
- To rebuild both PDFs and the editable table, run submission/build_case_tables.py from the repository root using Python with reportlab installed; it invokes build_report.py first.

These student runs overlap the broader regression suite; do not add counts.
