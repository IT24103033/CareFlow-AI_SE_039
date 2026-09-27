# Component B: triage review slice

## Implemented

- React `/triage`: searchable, paginated staff queue; urgency/status filters; case details; assessment summary; approval, rejection and revision request; loading, empty, error and conflict states.
- `GET /api/triage/review-queue`: `search` (max 100), `status`, `severity`, `sort` (`urgency`, `newest`, `oldest`), `page` and `pageSize` (1–100). Returns `{items, total, page, pageSize}`. Search uses database substring matching and is case-sensitive with the current PostgreSQL collation.
- `GET /api/triage/review-queue/{id}`: staff case detail.
- `PATCH /api/triage/{id}/review`: accepts `decision`, optional `doctorNotes`, and required `expectedUpdatedAt` copied exactly from the last response. Decisions are `Approved`, `Rejected` or `RevisionRequested`. Rejection/revision require notes. Notes are limited to 2,000 characters.
- A decision requires an `InReview` case and its latest Planning Agent execution to be `Completed`, valid and awaiting approval. Duplicate, stale or non-reviewable cases return 409. EF compares the original `UpdatedAt` on updates to detect simultaneous changes; record and agent updates share one SaveChanges transaction.
- Reviewer ID is taken from the authenticated `doctor_id` claim and checked against an active Doctor row. Client-supplied reviewer IDs are no longer accepted. `AssignedDoctorId` stores that reviewer; `UpdatedAt` records the decision time for this slice.
- Symptoms require 20–4,000 characters, with whitespace checked on the server. Invalid AI payloads do not enter `InReview`. Required plan strings and urgency values are validated. The API now returns `analysisMethod` and staff responses include `patientName`.

## Component A authentication handoff — required before live use

The shared API still has no authentication middleware. Staff endpoints intentionally return 401 rather than accept an anonymous decision. This implementation does not invent a development identity or token bypass.

Component A must install and configure trusted JWT validation and run authentication before authorization/controller execution. Its authenticated principal must have:

- role `Doctor`;
- `doctor_id` containing the matching active `Doctors.Id` GUID for mutations.

Web integration: create a stable `createTriageApi(getAccessToken)` instance from `src/services/triageApi.js` using Component A's token getter, and pass it to `<TriageReview api={api} />`. The default route sends no token until that integration exists. Do not generate identities or trust role/doctor IDs supplied by the browser.

Set `VITE_API_BASE_URL` to the ASP.NET API origin (default `http://localhost:5241`). No credentials belong in Vite environment variables.

## Deliberate scope boundaries

- Approval accepts the assessment only; it does **not** confirm an appointment. Scheduling/finalization belongs to the shared orchestrator and Component C.
- `RevisionRequested` records notes and pauses the case. An orchestrator operation to revise, revalidate and return it to `InReview` remains to be implemented. It must preserve previous decisions in an append-only review history before repeat reviews are supported.
- Existing mobile `GET /api/triage` and `GET /api/triage/{id}` response shapes are preserved. Those legacy endpoints and patient submission still need Component A's authentication and ownership enforcement. They remain unsuitable for live patient data.
- Structured planning and durable pre-execution state are now implemented; see [the planning slice](component-b-planning.md). Execution by the other agents, medical-history features, image upload, deployment and the complete four-agent acceptance scenario remain outstanding.
- Assessment validation is structural, not a claim of clinical correctness. New version-2 plans enforce a bounded specialty vocabulary. Reconciliation with the scheduling catalog and independent safety/business checks remain outstanding.
- EF concurrency metadata changes do not add a database column. No live database or migration was executed during this slice.

## Verification

Run:

```sh
dotnet test backend-api.Tests/CareFlowAI.API.Tests.csproj
cd web-admin
npm ci
npm test
npm run build
npx eslint src/App.jsx src/pages/TriageReview.jsx src/services/triageApi.js
```

Backend tests use EF InMemory and controlled ClaimsPrincipal identities. They cover authorization guards, valid decisions, failed/malformed assessments, latest-run selection, stale/repeated decisions, reason requirements, filtering, input rejection and EF concurrency detection. They do not substitute for PostgreSQL transaction tests or actual JWT middleware integration tests.

Frontend tests use Node’s built-in test runner and controlled fetch responses to verify request filters, the staff detail route, decision payloads, conflict/authentication errors and invalid assessment handling. They do not render React components or prove live backend/database connectivity.

Verified in this implementation session: 26 backend tests passed; 9 frontend API/assessment tests passed; backend build passed; the changed JSX parsed successfully using the locally available Babel transformer. React build, lint and browser interaction checks remain unverified: npm registry requests returned HTTP 403, preventing dependency restoration. `npm ci` cleared the previous web node_modules before failing; run `npm ci` again once registry access is restored. No optional test libraries were added.


Later verification: the next planning slice passed the React production build and lint checks for changed files after dependencies became available. See `component-b-planning.md` for the current test totals and limitations.
