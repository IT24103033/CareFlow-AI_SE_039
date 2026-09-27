# Component B: structured Planning Agent

## Implemented boundary

Component B now prepares a versioned plan from symptoms, optional duration and patient context. It records a proposal for the other components; it does not execute their agents, reserve appointments, send notifications or claim the complete four-agent acceptance workflow is finished.

Runtime implementation is `backend-api/Services/PlanningAgentService.cs` and `backend-api/Services/Planning/`. The older `ai-orchestrator/Agents/PlanningAgent.cs` is not included in the API project and is not the running agent.

## Input and controlled tools

`POST /api/triage` still accepts `patientId` and `symptoms`. It additionally accepts optional `duration` (at most 200 characters). Existing Flutter payloads remain compatible. Symptoms require 20–4,000 characters.

The API saves the triage record and its `Running` agent row in one SaveChanges before executing context/model calls. Initial input contains the workflow ID, patient ID, fixed domain objective, symptoms and duration. On completion, the input also contains the context snapshot used for assessment.

The Planning Agent can call only `IPatientContextTool.GetPatientProfileAsync` through a typed, read-only interface. Its current EF adapter projects the requested patient's ID and medical-history summary. It does not provide arbitrary SQL, writes or other patients. Missing/mismatched/oversized context is rejected; a blank history is explicitly flagged as missing rather than invented.

Component A can replace this adapter with its Context Agent output behind the same interface. The current adapter is not itself evidence of a fourth distinct agent. Patient ownership and authentication for the legacy submission/history endpoints still depend on Component A.

The Gemini adapter receives symptoms, duration and the medical-history summary only. Patient IDs, workflow IDs, names and contact details are not included in its prompt. No model tool-execution capability is provided. Patient text is serialized as untrusted data. This is defense in depth, not proof that prompt injection can never affect a recommendation.

## Output contract

`ClinicalPlan` preserves the existing specialist, urgency, action, rationale and analysis-method fields. Version 2 adds:

- `SchemaVersion: 2` and a fixed `Objective`;
- `Steps`: ID, responsible agent, allow-listed tool, dependencies, approval requirement and `Pending` status;
- `Warnings`: missing context or limitations of keyword screening;
- `Execution`: planning status, model attempts, sanitized failure code and operation outcomes/timings.

The model supplies only four assessment strings. Unknown fields (including attempts to supply tools, steps or approvals), duplicate fields, invalid urgency, unsupported specialties, missing fields and oversized responses are rejected. Model recommendations are bounded to the existing specialty vocabulary, which the team must reconcile with Component C's doctor catalog.

Component B constructs the delegation steps under deterministic policy:

| Step | Assigned responsibility | Prerequisite |
|---|---|---|
| Safety validation | D / SafetyAgent / CheckEmergencyRules | Assessment available |
| Find slot | C / ActionAgent / GetAvailableSlots | Safety validation |
| Tentative hold | C / ActionAgent / CreateTentativeAppointment | Slot selection |
| Doctor review | Authorized human / ReviewTriage | Tentative hold |
| Confirmation | C / ActionAgent / ConfirmAppointment | Approved human review |
| Notification | D / NotificationService / SendAppointmentNotification | Confirmation and approval |

Critical assessments generate only safety validation and urgent human review, never routine scheduling steps. Existing keyword rules are retained as a preliminary screen and explicitly labeled as not being a completed independent Safety Agent check. These rules and model outputs still require domain/safety evaluation.

Every downstream step remains `Pending`. Structured-plan validation rejects altered tool/agent assignments, reordered dependencies, missing approval gates, cycles or invented completion claims. The shared orchestrator must enforce the same permissions and approval requirements when it implements execution. Plan declarations alone are not runtime authorization.

## Failure and persistence behavior

There is no fallback that assigns Low urgency when Gemini fails. Invalid outputs, provider failures, missing configuration, timeouts and cancellation create a recorded failed assessment:

- `AgentStatus = Failed`;
- `TriageStatus = AssessmentFailed`;
- `SeverityLevel = Unassessed`;
- `AiPlan = null` in the client response;
- `PlanningExecution` carries sanitized failure details.

The completed/failed state is saved even after an HTTP cancellation, using an independent final database save. A server crash or failed final database save may still leave a durable `Running` row. Detecting/recovering abandoned runs, submission idempotency and automatic restart are outstanding orchestration work.

Provider exception messages/raw invalid responses are not stored because they may contain credentials or sensitive text. Each model attempt has a bounded timeout and the workflow has an overall deadline. Missing configuration is not retried. Other failed/invalid attempts are bounded; successful recovery retains earlier attempt outcomes.

Persisted schema-version-1 assessments remain readable/reviewable for compatibility. New submissions always use version 2. The existing JSON payload columns store the new contract; no schema migration or live database write was needed for this change.

## Configuration

Use environment or secret configuration; do not commit keys:

- `Gemini__ApiKey`: existing provider secret.
- `Gemini__Model`: explicitly configured model name; no silent model default.
- `Planning__MaxAttempts`: default 2, clamped to 1–3.
- `Planning__AttemptTimeoutSeconds`: default 10, clamped to 1–30.
- `Planning__WorkflowTimeoutSeconds`: default 30, clamped to 1–90.

When increasing deadlines, align mobile/API client timeouts. The current submission endpoint still executes synchronously; a background worker/status-polling submission protocol is future work.

## Client behavior

React shows the proposed next steps, missing-history warnings and a planning execution summary. Failed assessments can be found with the `AssessmentFailed` status filter and cannot be approved. Flutter reports that a submission was saved but assessment is unavailable; it no longer claims successful analysis in that case.

## Verification performed

- 45 backend tests passed: previous review tests plus golden planning, context isolation, critical routing, missing history, malformed/extra/duplicate model fields, retry recovery, error sanitization, configuration failure, timeout, cancellation, pre-execution persistence and approval/tool tampering.
- 10 frontend contract tests passed, including versioned-plan parsing and invalid execution states.
- React production build passed.
- ESLint passed for the changed React/API/plan files.
- After disk space was restored, all 12 Flutter widget tests passed. These exercise the real submission/history widgets with controlled repository responses: successful assessment, saved assessment failure, input validation, transport errors/retry, empty history, late responses after navigation, stale refreshes, narrow-screen rendering, and the login gate.
- Flutter analysis passed for the changed screens, repository adapter and widget tests. The new Flutter CI workflow runs these checks with Flutter 3.44.8 / Dart 3.12.2; a remote GitHub Actions run has not yet been observed.

Backend tests use controlled model/context doubles and EF InMemory. No live Gemini request, live PostgreSQL transaction, actual JWT middleware integration or complete A→B→C→D workflow was verified. The tests establish structural/permission boundaries, not clinical accuracy.


## Mobile verification follow-up

Widget verification exposed and fixed the following issues:

- `AssessmentFailed` and `RevisionRequested` now have distinct history labels/icons instead of appearing as Pending. Unassessed urgency uses a neutral color rather than low-urgency green.
- History badges and legend wrap on narrow screens. Expandable record cards provide a Material surface for their tap/ink effects.
- Submission, camera and history responses check mounted state before updating widgets. History requests also use a generation counter so an older refresh cannot overwrite a newer review status.
- The symptom form enforces the backend's 4,000-character limit.
- Screens depend on a small `TriageRepository` interface. The production adapter keeps the existing HTTP/session behavior; widget tests do not call live patient services or Gemini. This does not replace the outstanding JWT/patient-ownership work.

Run `flutter test --no-pub` from `mobile_app` to repeat the widget suite. These tests execute widgets in Flutter's test runtime; they do not verify an installed phone application, camera hardware, live provider calls or the complete shared workflow. Image upload and secure patient identity remain separate outstanding features.
