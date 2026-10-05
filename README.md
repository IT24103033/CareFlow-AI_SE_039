# CareFlow AI — SE3090 Assignment 1

CareFlow AI is Group SE_039's integrated hospital workflow application. It connects patient records and admissions, symptom triage, appointment scheduling, and pharmacy operations through a shared ASP.NET Core API and PostgreSQL database. Patients use Flutter; hospital users use React. AI supports assessment and planning, while authenticated users retain responsibility for clinical approval.

This README covers the documentation requested in assignment Section 14.1. Deployment is being finalized; entries marked **Pending** are not verified live services. This is an academic prototype, not a validated clinical system.

## Submission and access

| Deliverable | Location / status |
|---|---|
| Repository | https://github.com/IT24103033/CareFlow-AI_SE_039 |
| React website (Vercel) | **Pending:** add final public URL |
| Backend API (Render) | **Pending:** add final public base URL |
| API health URL | **Pending:** implement and publish a health endpoint; none is currently mapped |
| Production Swagger | **Pending:** current Swagger middleware is Development-only |
| Android APK | **Pending:** add download link, version and installation instructions |
| Demo video | [Google Drive demo folder](https://drive.google.com/drive/folders/1mDBlpug9Q9nrtR5LMSYQq7J4UoKvjaYr?usp=sharing). Upload reported complete; anonymous playback and duration remain to be checked. |
| Consolidated report | Generated locally under `output/pdf/SE3090_SE039_Consolidated_Report_Draft.pdf`; review copy, not necessarily tracked in Git |
| ER diagram | Generated locally under `output/pdf/CareFlow_AI_Complete_ERD.pdf`; editable Mermaid source alongside it |
| Evaluator accounts | **Pending:** supply dedicated synthetic accounts for Patient, Doctor, Staff and Admin through the submission access instructions |

Open all submitted links in an incognito browser before submission. The assignment requests a 10-minute video accessible without permission requests and availability of the repository, video and deployed services until at least **21 October 2026**. Never publish real patient credentials or production secrets in this repository.

## Problem, users and features

Separate patient, scheduling and pharmacy records make it difficult to follow a request from symptoms through assessment, approval and treatment-related operations. CareFlow provides a shared case history across these stages.

| Role | Main activities |
|---|---|
| Patient | Register/sign in, submit symptoms and an optional image, view triage progress, revise symptoms, choose appointment slots, view appointments and prescriptions |
| Doctor | Review assessments, approve/reject or request revision, review appointment actions and prescriptions |
| Staff | Manage permitted patient, admission, ward, scheduling and pharmacy operations |
| Admin | Manage accounts and administrative records and inspect system activity within authorized routes |

Features include patient history, ward allocation/risk analysis, structured AI plans, persisted agent attempts, doctor review history, conflict-aware tentative appointments, medicine inventory, prescription safety checks, dispensing and notification retry tracking. Camera/image upload, QR pickup presentation and medication reminders provide device-related functionality. External notification delivery requires configured providers and separate verification.

## Technology choices and architecture

| Technology | Purpose and rationale |
|---|---|
| ASP.NET Core / .NET 8 | Typed REST controllers, dependency injection, authentication and shared business services |
| PostgreSQL / EF Core / Npgsql | Relational constraints, migrations, persistent business and workflow records |
| React / Vite | Browser-based administrative and clinical workflows; reusable components and fast production builds |
| Flutter / Dart | Patient mobile application, reusable widgets and device integrations |
| Gemini with C# orchestration | Model-assisted analysis/planning combined with explicit service contracts, tool boundaries and validation |
| Cloudinary | Image upload storage; credentials remain on the backend |
| Render / Vercel | Selected backend container and React hosting platforms; deployment not yet verified |

React and Flutter call the same API. Controllers authenticate requests and delegate to services. EF Core accesses PostgreSQL; agent services use patient context, Gemini, scheduling tools and safety checks. The API persists results for review and returns updated state to both clients. The `ai-orchestrator` project is referenced by the API and runs in the same backend process; no separate agent server is required.

React uses authentication context and screen-level hooks. Flutter uses widgets/services, secure token storage and local screen state, with Provider available in the project. Keep the final ADRs in the consolidated report aligned with the implementation: React/Flutter state management, agent orchestration, workflow-state storage and hosting decisions.

### Four agent responsibilities

| Component | Agent responsibility | Integration |
|---|---|---|
| A | Patient context/domain analysis | Patient history, risk factors and admission/ward recommendations |
| B | Planning and triage orchestration | Builds and validates structured plans, tracks attempts and routes cases for review |
| C | Appointment action/tools | Finds slots, checks conflicts and creates tentative bookings through provider abstractions |
| D | Safety verification | Emergency and prescription safety checks, with deterministic validation and pharmacy integration |

A representative workflow is: patient submits symptoms → API persists the case → context and planning services produce a validated assessment → safety and scheduling steps supply results → doctor reviews/approves the relevant action → patient sees the updated case and appointment. Revision and retry paths preserve review/attempt history. Agent participation and downstream success must be checked in the recorded workflow; a saved submission alone does not prove a successful assessment.

The scheduling tool allow-list includes finding available slots, checking conflicts and tentative booking. Model output does not itself authorize confirmation or dispensing. Failed assessments remain distinguishable from successful reviews, and external failures must not be presented as completed clinical actions.

## Database design

The inspected EF model has 14 mapped application tables:

| Area | Tables |
|---|---|
| Identity and A | `Users`, `PatientProfiles`, `Wards`, `Admissions` |
| B | `TriageRecords`, `AgentWorkflows`, `TriageReviewHistories`, `TriageAttachments` |
| C | `Doctors`, `DoctorAvailabilities`, `Appointments` |
| D | `Medicines`, `Prescriptions`, `PrescriptionItems` |

Patients have admissions, triage cases, appointments and prescriptions. Wards have admissions; doctors have availability and appointments. Triage cases have workflow attempts and reviews; prescriptions have medicine-linked items. UUIDs identify records. Selected entities use `UpdatedAt` for optimistic concurrency.

The model contains 15 foreign-key relationships. Some fields, including user/profile references and assigned-doctor IDs, are application references without configured foreign keys. The two triage/attachment references are separate many-to-one mappings, not a one-to-one constraint. See `backend-api/Data/ApplicationDbContext.cs`, `backend-api/Models/` and the migration snapshot for authoritative definitions. A deployed database may also contain EF migration metadata.

## Repository structure

```text
backend-api/        REST API, controllers, DTOs, services, EF model/migrations
ai-orchestrator/    Agent contracts, agents and tool/provider abstractions
backend-api.Tests/  xUnit tests for API, planning, scheduling and pharmacy
web-admin/         React/Vite application and web contract tests
mobile_app/        Flutter patient application and tests
.github/workflows/ Backend, web, Flutter and container-publishing workflows
docs/              Component B planning and review documentation
Dockerfile         Multi-stage .NET API container build
output/pdf/        Locally generated submission artifacts (may be untracked)
```

## Prerequisites

- Git, .NET SDK 8 and a compatible EF Core 8 CLI tool.
- Node.js/npm compatible with the locked Vite dependencies. The successful local build used Node 25.8.1; verify the version selected by your CI/hosting provider against `package-lock.json` engine requirements.
- Flutter with Dart satisfying `mobile_app/pubspec.yaml` (`^3.12.2`), Android SDK and an emulator or physical device. The Flutter CI configuration currently selects Flutter 3.47.5.
- A PostgreSQL database accessible from your machine/backend host.
- Working Gemini and Cloudinary credentials; email/SMS gateway configuration if demonstrating those deliveries.
- Docker only if building/running the backend container locally.

## Configuration

ASP.NET Core accepts hierarchical environment variables using double underscores. Set secrets in your shell or hosting provider's secret settings. The API also attempts to load `../.env.local` relative to its working directory; do not rely on that relative path in production. Do not commit a real environment file.

| Variable | Meaning |
|---|---|
| `ConnectionStrings__DefaultConnection` | Npgsql connection string: host, database, username, password and appropriate TLS settings |
| `Jwt__Secret` | Strong signing secret, at least 32 random bytes for HS256 |
| `Jwt__Issuer` | Configured issuer; current setting is `CareFlowAI` |
| `Jwt__Audience` | Configured audience; current setting is `CareFlowAI.Clients` |
| `Jwt__ExpiryMinutes` | Token lifetime; current setting is 120 |
| `Gemini__ApiKey` | Backend-only Gemini credential |
| `Gemini__Model` | Model available to your account; repository configuration currently names `gemini-3.1-flash-lite`, which must be verified with your actual provider |
| `CLOUDINARY_URL` | Cloudinary connection URL for image uploads |
| `Planning__MaxAttempts` | Planning attempt count; Development configuration currently sets 2 |
| `Planning__AttemptTimeoutSeconds` | Attempt timeout input; Development configuration currently sets 300 |
| `Planning__WorkflowTimeoutSeconds` | Workflow timeout input; Development configuration currently sets 600 |
| `Notifications__EmailEndpoint` | Email webhook gateway endpoint |
| `Notifications__SmsEndpoint` | SMS webhook gateway endpoint |
| `Notifications__ApiKey` | Gateway authentication key |
| `ASPNETCORE_ENVIRONMENT` | `Development` locally; `Production` on hosting |
| `ASPNETCORE_URLS` | Listening address; container uses `http://+:8080` |
| `VITE_API_BASE_URL` | Public backend origin for React, without a trailing `/api`; this value is public |

`PlanningAgentService` clamps attempts to 1–3, the attempt timeout to 1–30 seconds and the workflow timeout to 1–90 seconds. Consequently, Development settings of 300/600 do not produce five-/ten-minute budgets; their effective caps are 30/90 seconds. Client timeouts and cancellation also apply. Never put Gemini, database or JWT secrets in a `VITE_` variable or the APK. Inspect and remove/rotate any real credentials accidentally committed in configuration before public release.

## Local installation and startup

All commands below assume the repository root unless a `cd` is shown. Configure the backend environment first, then initialize the database, start the API, and start the clients.

### 1. Clone and restore

```bash
git clone https://github.com/IT24103033/CareFlow-AI_SE_039.git
cd CareFlow-AI_SE_039
dotnet restore backend-api/CareFlowAI.API.csproj
```

### 2. Initialize the database

Use a disposable development database first. Set `ConnectionStrings__DefaultConnection` in the shell running EF commands. Review migrations and seed data before applying them to an existing/shared database; do not reset a populated database to resolve a migration conflict.

```bash
# Install once if dotnet-ef is not already available.
dotnet tool install --global dotnet-ef --version '8.*'
dotnet ef migrations list --project backend-api --startup-project backend-api
dotnet ef database update --project backend-api --startup-project backend-api
```

Database initialization is a separate step; the current API startup does not automatically run migrations. CI schema creation with `EnsureCreated` is not evidence that every migration replays successfully. Validate the migration chain on a fresh database and use restricted runtime credentials in deployment.

### 3. Start the API and agents

```bash
dotnet run --project backend-api --launch-profile http
```

Local API: `http://localhost:5241`. Swagger: `http://localhost:5241/swagger/index.html`. Agents start with the API. If already inside `backend-api`, use `dotnet run --launch-profile http` instead.

### 4. Start React in another terminal

```bash
cd web-admin
npm ci
```

Create an uncommitted `web-admin/.env.local` containing:

```dotenv
VITE_API_BASE_URL=http://localhost:5241
```

Then run `npm run dev` and open the URL printed by Vite. Restart the dev server after changing environment variables.

### 5. Start Flutter in another terminal

```bash
cd mobile_app
flutter doctor
flutter pub get
flutter devices
flutter run
```

Current services use `10.0.2.2:5241` for the Android emulator and localhost for other local targets. A physical phone cannot reach your computer through its own localhost. Before physical-device or release testing, update the service URL configuration to the appropriate reachable API. There is currently no single verified `--dart-define` switch that updates all services.

Review `api_service.dart`, `auth_service.dart`, `triage_service.dart` and `prescription_service.dart` under `mobile_app/lib/services/`; also search other call sites. Keep `/api` prefixes consistent with each service's existing route construction.

## API documentation and evaluator access

Use Swagger for the full request/response schemas. Log in through `POST /api/auth/login`, then provide the JWT through Swagger's Authorize control as requested by its bearer scheme. `GET /api/auth/me` verifies the authenticated identity. Never include real tokens in evidence screenshots.

| Area | Representative routes |
|---|---|
| Authentication | `POST /api/auth/login`, `POST /api/auth/register-patient`, `POST /api/auth/register-staff`, `GET /api/auth/me` |
| Patient/admissions | `/api/PatientProfiles`, `GET /api/Wards`, `POST /api/Admissions/allocate-ward`, `POST /api/Admissions/analyze-risk` |
| Triage | `POST /api/triage`, `GET /api/triage/review-queue`, `PATCH /api/triage/{id}/review` |
| Revision/recovery | `POST /api/triage/{id}/revise`, `POST /api/triage/{id}/retry`, `POST /api/triage/{id}/book-slot` |
| Scheduling | `GET /api/DoctorAvailability/slots`, `POST /api/Appointments/tentative`, `POST /api/Appointments/{id}/approve`, `POST /api/Appointments/{id}/confirm` |
| Pharmacy | `/api/Medicines`, `/api/Prescriptions`, `PATCH /api/Medicines/{id}/restock`, `PATCH /api/Prescriptions/{id}/approve`, `PATCH /api/Prescriptions/{id}/notify-retry` |

Routes have their own role, ownership and state requirements; the table does not grant access to every role. Supply four tested evaluator identities in the final access instructions. Patient accounts can be registered through the patient flow; staff/doctor/admin setup must use authorized administration or reviewed initialization. Do not invent default passwords or assume a seeded identity can authenticate.

## Testing and evidence

```bash
# Repository root
dotnet test backend-api.Tests/CareFlowAI.API.Tests.csproj

# From web-admin/
npm ci
npm test
npm run lint
npm run build

# From mobile_app/
flutter analyze
flutter test
```

Recorded verification on 6 October 2026: the user-provided backend run reports **101 passed, 0 failed, 1 skipped**; local React verification reports **10 tests passed** and a successful production build. These are point-in-time results, not proof of a later deployment. No new Flutter verification is claimed here. `ConcurrentBookings_ShouldNotBothSucceed` is explicitly skipped because EF InMemory does not support the needed concurrent constraints; real PostgreSQL race testing remains necessary.

Final E2E checklist:

1. Sign in with each role and verify protected operations and patient ownership.
2. Submit symptoms and an image from Flutter; find the same case in React.
3. Inspect context, planning, scheduling and safety outcomes plus persisted workflow history.
4. Review/approve, choose/book a slot in the supported order, and verify the appointment in both clients.
5. Request a revision and resubmit; check stale/duplicate actions and failure handling.
6. Create and safety-check a prescription; dispense once and verify stock/patient output.
7. Test notification failure/retry and actual delivery separately.
8. Repeat the main flow on the deployed API/web app and installed release APK.

Retain actual logs, screenshots and tested commit IDs. GitHub runner-allocation failures are infrastructure failures, not passed checks. The web workflow currently suppresses lint failures with `|| true`; a green build does not establish clean lint.

## Deployment

### Render backend

- Create a Docker Web Service connected to this repository and the tested `main` commit.
- Leave the root directory at repository root; Dockerfile path is `./Dockerfile`. Both C# projects are needed in the build context.
- Set backend variables above in Render, including `ASPNETCORE_ENVIRONMENT=Production`, `ASPNETCORE_URLS=http://+:8080` and `PORT=8080`.
- Apply reviewed EF migrations separately using the production connection in an authorized environment; verify database access and schema compatibility.
- Deploy and save the public HTTPS origin. Verify authentication, database operations and a real agent request.
- Add a health endpoint and deliberately configure evaluator Swagger access: neither a production health URL nor production Swagger is currently ready. Do not label a nonexistent `/health` route as working.

The Dockerfile builds and publishes the API with .NET 8. `.github/workflows/deploy.yml` currently builds/tests and pushes an image to GHCR; despite its introductory comments, it contains no VM/Render deployment job. Render's Git-linked deployment is a separate hosting configuration.

### Vercel React

- Import the repository; choose Vite and root directory `web-admin`.
- Install with `npm ci`, build with `npm run build`, and publish `dist`.
- Set `VITE_API_BASE_URL=https://YOUR-API.onrender.com` before building, without `/api`.
- Configure an SPA fallback to `/index.html` for client-side routes and verify direct navigation/refresh.
- Replace hardcoded localhost requests in `PatientEditModal.jsx` and `AdminPatients.jsx` with the shared authenticated API client before release.
- Restrict backend CORS to the intended deployed frontend origins; the current policy allows any origin.

### Android APK

After updating all mobile API origins to the deployed HTTPS backend:

```bash
cd mobile_app
flutter pub get
flutter build apk --release
```

The normal output is `mobile_app/build/app/outputs/flutter-apk/app-release.apk`. Review signing configuration, install the APK on a physical Android device, permit installation from the trusted download source when prompted, and test authentication, uploads, appointments and prescriptions. Verify network permissions and notification permissions. Upload the tested APK, record its version/commit and add its download URL above. Do not distribute an emulator-localhost build as the final deployed app.

## Troubleshooting and known limits

- **AssessmentFailed:** inspect the persisted workflow error and backend logs. Check model/key access, provider quota, validation and timeout paths. Saving symptoms is not successful AI assessment.
- **Client timeout:** inspect whether the server saved/processed the request before retrying to avoid duplicate cases.
- **npm ENOTEMPTY:** stop concurrent installs/dev tooling, move the incomplete `node_modules` outside the app as a backup, then run `npm ci`. Preserve `package-lock.json`.
- **GitHub job queued/runner not acquired:** preserve the infrastructure error, rerun when capacity recovers and retain local verification; do not report the queued job as passed.
- **Authentication/403:** confirm role claims, linked active profile and record ownership; never bypass authorization to fix a UI failure.
- **Deployment works locally only:** check all API origins, CORS, Vercel route fallback and production environment variables.
- **Historical integration issues:** B/C structured-result mismatches, Dart filename case sensitivity, migration conflicts and AI response validation required shared fixes. Use commits and tests to distinguish original component work from integration contributions.

## Security and responsible AI

JWT authentication, role/ownership checks, BCrypt password hashing, DTO validation and explicit review states support access control. Verify their enforcement on the final deployed routes. Model output requires deterministic validation and authorized human approval; it must not bypass scheduling or dispensing rules. Do not log credentials, raw tokens or unnecessary patient data. Use synthetic evaluator data and least-privilege database credentials. Review permissive CORS, environment files, seeded accounts and configuration history before publication.

## Team contributions

| Student | ID | Primary ownership |
|---|---|---|
| Bandara MMSD | IT24103089 | A: patient context, profiles, admissions and wards |
| Dinuwara K.D.S | IT24103033 | B: triage planning, review/revision, workflow history and integration |
| Pallawala S R | IT24102852 | C: doctor availability, appointments and action/tool agent |
| Dulshan P A | IT24102599 | D: inventory, prescriptions, safety and notifications |

Ownership spans backend, database, React, Flutter and agent work; shared fixes must be credited through actual Git/PR history. Individual contribution statements, evidence, challenges, AI logs, reflections and signed declarations belong in the consolidated report. Supporting Component B notes are in [planning](docs/component-b-planning.md) and [review](docs/component-b-review.md).

## AI usage declaration

AI assistance was used during development, debugging, testing and documentation. Antigravity/Gemini use is reported in member evidence; Codex assisted with repository inspection, verification, report preparation, diagrams and this README. Exact tools/models, dates, outputs, changes/rejections and verification must be recorded in each member's own log. This summary does not replace those logs or personal reflections.

Members remain responsible for understanding and verifying submitted work. Do not fabricate test results, signatures, contribution history or AI logs. During the final evaluation, external AI assistants must not answer questions, generate explanations or modify work; the application's own agent subsystem must be demonstrated.
