# CareFlow AI Triage Workflow

This document details the end-to-end lifecycle of a patient triage request in the CareFlow AI system. It covers the mobile submission, AI planning, human-in-the-loop review, and the patient revision process.

## 1. Patient Submission (Flutter Mobile App)

**Actor:** Patient  
**Component:** Flutter App (`triage_service.dart`)

1. **Authentication:** The patient logs in. The Flutter app stores a JWT containing their `patient_id` claim.
2. **Image Upload (Optional):** If the patient attaches an image of their symptoms, the app calls `POST /api/triage/upload-image` (multipart/form-data). The backend streams this securely to a **Neon Object Storage (S3-compatible)** bucket and returns a public URL.
3. **Data Submission:** The app calls `POST /api/triage` with the `patientId`, `symptoms`, and optional `imageUrl`.

## 2. AI Planning & Assessment (ASP.NET Core Backend & AI Orchestrator)

**Actor:** AI Agents (Component A, B, D)  
**Component:** `TriageController.cs`, `PlanningAgentService.cs`

1. **Initialization:** The backend creates a `TriageRecord` in the database with status `Pending`.
2. **Component A (Context Assembly):** The `PatientContextTool` securely queries the database for the patient's `DateOfBirth`, `BloodGroup`, and `MedicalHistorySummary`. This ensures the LLM has deep clinical context without exposing PII.
3. **Component B (LLM Planning):** The `PlanningAgentService` invokes the `GeminiAssessmentClient`. It passes the symptoms and patient context. Gemini generates a structured `ClinicalPlan` containing:
   - Suggested Specialist
   - Urgency Level (Low, Medium, High, Critical)
   - Recommended Action
   - Clinical Rationale
4. **Validation:** `PlanningPlanValidator` parses the LLM JSON and generates strict downstream **Execution Contracts** (an array of `PlanStep` elements defining exactly what tools should execute next).
5. **Component D (Safety Agent):** The backend immediately executes the `CheckEmergencyRules` step synchronously. It checks hardcoded red-flag keywords (e.g., "chest pain", "stroke"). If found, the AI's urgency is forcefully escalated to `Critical`.
6. **State Transition:** The record status transitions to `InReview` (or `AssessmentFailed` if the LLM hallucinated/failed).

*(Note: If the agent crashes or hangs in `Running` state for >5 minutes, a background `WorkflowManager` automatically restarts the assessment without user intervention).*

## 3. Human-in-the-Loop Review (React Admin Dashboard)

**Actor:** Doctor / Triage Nurse  
**Component:** React Web App (`TriageReview.jsx`)

1. **Queue Management:** Authorized medical staff log into the React dashboard and fetch the active queue (`GET /api/triage/queue`).
2. **Review:** The clinician selects an `InReview` case. They view the patient's symptoms, the attached image, and the AI's `ClinicalPlan` (including urgency and rationale).
3. **Decision Making:** The clinician submits a decision (`POST /api/triage/{id}/review`). Valid decisions are:
   - `Approved`: The AI's plan is accepted.
   - `Rejected`: The case is dismissed or handled out-of-band.
   - `RevisionRequested`: The clinician needs more information (e.g., "Please clarify how long you've had a fever").

## 4. Patient Revision & Reassessment (Flutter & Backend)

**Actor:** Patient & AI Orchestrator  
**Component:** Flutter App (`triage_dashboard_screen.dart`), `TriageController.cs`

1. **Status Check:** The patient checks their dashboard (`GET /api/triage`). They see their case is marked `RevisionRequested` and read the doctor's notes.
2. **Revision:** The patient submits additional details via `POST /api/triage/{id}/revise`. To prevent lost updates, they must include the `ExpectedUpdatedAt` timestamp (Optimistic Concurrency).
3. **Audit Trail:** The backend archives the previous symptoms and the doctor's notes into the immutable `TriageReviewHistory` table.
4. **Reassessment:** The `TriageRecord` status shifts to `ReassessmentInProgress`. The AI `PlanningAgent` runs *again* to factor in the new symptoms.
5. **Cycle Repeats:** The case goes back to `InReview` for the doctor to approve.

---

## Security & Concurrency Constraints

- **Authorization:** Only the authenticated Patient who owns a record can view/revise it. Staff can view all records but their identities are logged upon review.
- **Idempotency & Concurrency:** Updating records or submitting revisions requires the `ExpectedUpdatedAt` token. This guarantees the doctor and patient are not overwriting each other's state simultaneously.
- **Data Provenance:** No AI plan is executed directly on the patient without Human Approval. The `TriageReviewHistory` preserves exactly what the doctor saw when they made a decision.
