from reportlab.platypus import SimpleDocTemplate, Paragraph, Spacer, Table, TableStyle, PageBreak, Flowable
from reportlab.lib import colors
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.lib.enums import TA_CENTER
from reportlab.lib.pagesizes import A4
from pathlib import Path
from xml.sax.saxutils import escape
import json
OUT=Path('output/pdf'); OUT.mkdir(parents=True,exist_ok=True)
styles=getSampleStyleSheet()
styles.add(ParagraphStyle(name='BodyX',fontName='Helvetica',fontSize=10.2,leading=15,spaceAfter=9))
styles.add(ParagraphStyle(name='SmallX',fontName='Helvetica',fontSize=8.5,leading=11.7,spaceAfter=5))
styles.add(ParagraphStyle(name='TitleX',fontName='Helvetica-Bold',fontSize=26,leading=32,spaceAfter=18))
styles.add(ParagraphStyle(name='HeadX',fontName='Helvetica-Bold',fontSize=17,leading=22,spaceAfter=14))
styles.add(ParagraphStyle(name='SubX',fontName='Helvetica-Bold',fontSize=11.5,leading=16,spaceBefore=8,spaceAfter=7))
navy=colors.HexColor('#17324D'); pale=colors.HexColor('#F1F5F8')
story=[]; pages=[]; current=None

def p(s,style='BodyX'):
 text=escape(s)
 url='https://drive.google.com/drive/folders/1mDBlpug9Q9nrtR5LMSYQq7J4UoKvjaYr?usp=sharing'
 text=text.replace(url, '<link href="'+url+'" color="#165A96">Open demonstration video folder</link>')
 text=text.replace('https://drive.google.com/drive/folders/1ZgL1VADVWcowwMZFpPXKCl6tQUH0diqy?usp=sharing', '<link href="https://drive.google.com/drive/folders/1ZgL1VADVWcowwMZFpPXKCl6tQUH0diqy?usp=sharing" color="#165A96">Download Android APK from Drive</link>')
 text=text.replace('https://care-flow-ai-se-039.vercel.app', '<link href="https://care-flow-ai-se-039.vercel.app" color="#165A96">https://care-flow-ai-se-039.vercel.app</link>')
 text=text.replace('https://careflow-ai-se-039.onrender.com', '<link href="https://careflow-ai-se-039.onrender.com" color="#165A96">https://careflow-ai-se-039.onrender.com</link>')
 return Paragraph(text,styles[style])
def txt(s,style='BodyX'): story.append(p(s,style)); current['content'].append(s)
def sub(s): story.append(p(s,'SubX')); current['content'].append(s)
def bullets(items):
 for s in items: story.append(p('- '+s)); current['content'].append('- '+s)
def table(headers,rows,widths=None):
 headerstyle=ParagraphStyle('HeaderWhite',parent=styles['SmallX'],textColor=colors.white,fontName='Helvetica-Bold')
 data=[[Paragraph(escape(s),headerstyle) for s in headers]]+[[p(str(s),'SmallX') for s in r] for r in rows]
 for cell in data[0]: cell.style=ParagraphStyle('th',parent=styles['SmallX'],textColor=colors.white,fontName='Helvetica-Bold')
 t=Table(data,colWidths=widths or [483/len(headers)]*len(headers),repeatRows=1,hAlign='LEFT')
 t.setStyle(TableStyle([('BACKGROUND',(0,0),(-1,0),navy),('ROWBACKGROUNDS',(0,1),(-1,-1),[colors.white,pale]),('GRID',(0,0),(-1,-1),.4,colors.HexColor('#D9D9D9')),('VALIGN',(0,0),(-1,-1),'TOP'),('LEFTPADDING',(0,0),(-1,-1),8),('RIGHTPADDING',(0,0),(-1,-1),8),('TOPPADDING',(0,0),(-1,-1),7),('BOTTOMPADDING',(0,0),(-1,-1),7)]))
 story.extend([t,Spacer(1,10)]); current['tables'].append({'headers':headers,'rows':rows})
def page(title):
 global current
 if story: story.append(PageBreak())
 current={'title':title,'content':[],'tables':[]};pages.append(current)
 story.append(p(title,'HeadX'))
class Diagram(Flowable):
 def __init__(self,kind): Flowable.__init__(self); self.kind=kind;self.width=483;self.height=300 if kind=='architecture' else 320
 def draw(self):
  c=self.canv
  def box(x,y,w,h,title,lines):
   c.setFillColor(pale);c.setStrokeColor(navy);c.roundRect(x,y,w,h,5,fill=1,stroke=1);c.setFillColor(navy);c.setFont('Helvetica-Bold',10);c.drawCentredString(x+w/2,y+h-17,title);c.setFont('Helvetica',8)
   for i,line in enumerate(lines):c.drawCentredString(x+w/2,y+h-32-i*11,line)
  def arrow(x,y,a,b):
   c.setStrokeColor(navy);c.line(x,y,a,b);c.line(a,b,a-3,b+5);c.line(a,b,a+3,b+5)
  if self.kind=='architecture':
   box(0,235,220,60,'Flutter patient app',['Symptoms, slot choice, status, prescriptions']);box(263,235,220,60,'React hospital portal',['Admin, staff, doctor review and pharmacy'])
   arrow(110,235,190,205);arrow(373,235,290,205)
   box(85,135,313,70,'ASP.NET Core public API',['JWT and ownership checks | DTO validation','Controllers | EF Core services | workflow orchestration'])
   arrow(150,135,110,95);arrow(330,135,365,95)
   box(0,25,220,70,'PostgreSQL via EF Core',['Patients, appointments and prescriptions','Triage, attempts and review history'])
   box(263,25,220,70,'Internal agents and external services',['A context | B plan | C action | D safety','Gemini | Cloudinary | notification providers'])
  else:
   boxes=[(0,240,'PatientProfiles',['Id PK | history | contact | IsActive']),(263,240,'Doctors',['Id PK | specialty | IsActive']),(0,160,'TriageRecords',['Id PK | PatientId FK | status']),(263,160,'Appointments',['Id PK | PatientId FK | DoctorId FK']),(0,80,'AgentWorkflows',['Id PK | TriageRecordId FK','input | output | status | errors']),(263,80,'Prescriptions',['Id PK | PatientId FK | TriageRecordId FK']),(0,0,'TriageReviewHistories',['TriageRecordId FK | attempt FK','reviewer | decision | notes']),(263,0,'PrescriptionItems',['PrescriptionId FK | MedicineId FK','quantity | dosage'])]
   for x,y,t,l in boxes:box(x,y,220,62,t,l)
   for x,y,a,b in [(110,240,110,222),(373,240,373,222),(110,160,110,142),(110,80,110,62),(373,80,373,62)]:arrow(x,y,a,b)

page('CareFlow AI Consolidated Report')
story.append(Spacer(1,22));story.append(p('Integrated Full Stack and Agentic AI Application Development','TitleX'))
txt('SE3090 Software Engineering Frameworks | Assignment 1 | Year 3 Semester 1 2026')
txt('Group SE_039 | Faculty of Computing | SLIIT')
table(['Student ID','Student name','Primary component'],[['IT24103089','Bandara MMSD','A - Context and admissions'],['IT24103033','Dinuwara K.D.S','B - Planning and triage'],['IT24102852','Pallawala S R','C - Resource scheduling'],['IT24102599','Dulshan P A','D - Pharmacy and safety']],[92,145,246])
txt('CareFlow AI connects patient symptom submission, contextual assessment, appointment selection, doctor review and pharmacy operations through a shared ASP.NET Core API and PostgreSQL database.')
txt('Submission date: 6 October 2026. This report combines implementation evidence collected on 5 October with subsequent deployment, verification and individual contribution updates. Historical observations are identified separately from final deployment results.')
page('Report Navigation and Submission Access')
table(['Section','Contents','Pages'],[['1 to 4','Overview, requirements, architecture and agent workflow','3 to 7'],['5 to 8','Database, API, client design and security','8 to 12'],['9 to 12','Testing, agent evaluation, performance and deployment','13 to 18'],['13','Five architecture decision records','19 to 23'],['14 to 17','Individual sections A, B, C and D','24 to 44'],['18 to 19','Group declaration, references and submission access','45 to 47']],[75,348,60])
table(['Deliverable','Access information'],[['Repository','https://github.com/IT24103033/CareFlow-AI_SE_039'],['React website','https://care-flow-ai-se-039.vercel.app'],['Backend API','https://careflow-ai-se-039.onrender.com | Render-hosted ASP.NET Core API.'],['Android APK','https://drive.google.com/drive/folders/1ZgL1VADVWcowwMZFpPXKCl6tQUH0diqy?usp=sharing | Android application distribution folder.'],['Demonstration video','https://drive.google.com/drive/folders/1mDBlpug9Q9nrtR5LMSYQq7J4UoKvjaYr?usp=sharing | Recorded system demonstration.'],['Database evidence','Neon PostgreSQL; schema and development database screenshots are included in this report.'],['Evaluator accounts','Web login: Doctor - doctor@careflow.ai; Staff - staff@careflow.ai; Admin - admin@careflow.ai. Password for all three: password. Open the React website above and enter the account for the required role. Admin, doctor and staff login were tested on the deployed website. Patient/mobile access: open the app, select Create Account, register with your own test details, then sign in with the credentials you chose.']],[135,348])
page('1 Project Overview and Scope')
sub('Business problem')
txt('Patient symptom reports, doctor schedules, ward occupancy and pharmacy records are often handled as separate activities. This separation makes it difficult to trace a patient request from initial assessment to an accountable clinical decision and the next operational action. CareFlow AI provides a shared digital workflow for these activities.')
sub('Objectives')
bullets(['Capture symptoms and an optional image from a patient-facing mobile application.','Use distinct context, planning, action and safety roles to support an auditable triage workflow.','Require an authenticated doctor to review the assessment and approve consequential appointment actions.','Provide patient status tracking and staff access to admissions, availability, medicines and prescriptions.','Persist business data, workflow attempts and review decisions in PostgreSQL.'])
sub('Boundaries')
txt('CareFlow is an academic decision-support prototype. The AI output is a recommendation for human review, not an independently validated clinical diagnosis. The implemented scope is hospital triage, appointments, admissions and pharmacy. It does not include field-nurse dispatch, GPS tracking or home-visit routing; those features belong to the CarePulse reference report and are not CareFlow features.')
sub('Component ownership')
txt('Bandara owns patient context and admissions; Dinuwara owns triage planning and review; Pallawala owns scheduling and appointment execution; Dulshan owns pharmacy and safety. Integration changes span these boundaries and must be attributed using the actual commit and review history, rather than treating file ownership as exclusive authorship.')
page('2 Requirements and User Roles')
table(['Role','Principal responsibilities','Control boundary'],[['Patient','Submit symptoms, select a slot, revise a case, view own appointments and prescriptions.','Patient identity must come from the authenticated session.'],['Doctor','Review assessments, approve or reject, request revision, issue prescriptions.','Active doctor identity and valid workflow state.'],['Staff','Patient records, admissions, ward and pharmacy operations.','Restricted operational actions; no doctor approval impersonation.'],['Admin','Account administration, patient management and overview.','Administrative privileges do not imply clinical decision authority.']],[70,225,188])
sub('Functional requirements')
txt('The system supports identity, patient records, ward admission, symptom assessment, review history, available-slot discovery, appointment approval, medication inventory and prescription dispensing. Collection APIs and screens provide differing levels of search and filtering; the triage review queue includes server-side pagination and urgency sorting.')
sub('Nonfunctional requirements')
txt('Both clients use the same public API and persistence model. External calls need bounded execution, safe failures and secret protection. Repeated requests must not duplicate consequential actions. UI states must distinguish pending work, successful assessment, failed assessment and failed downstream execution. Audit summaries should retain structured results without hidden reasoning or credentials.')
sub('Acceptance workflow')
txt('A non-emergency patient request starts in Flutter, is persisted and assessed through A and B, receives D safety validation, and exposes suitable slots. The patient selects a slot through C. A doctor reviews the case in React; the appointment is approved and confirmed only through the authorized flow. The patient then sees the updated outcome. This manual-selection design supersedes the earlier automatic-booking prototype.')
page('3 Full Stack Architecture')
story.append(Diagram('architecture'))
txt('Figure 1. Logical deployment and integration architecture derived from the repository.')
txt('React and Flutter communicate with ASP.NET Core. Controllers apply endpoint rules and DTO validation; services contain planning, scheduling, pharmacy and notification logic. EF Core maps the application model to PostgreSQL. The orchestrator is a referenced .NET project hosted with the API, rather than a separate public service.')
txt('Gemini supplies model-based context analysis and assessment. Cloudinary stores uploaded images. NotificationService uses configured HTTP providers for email and SMS. Provider availability and delivery are separate from successful database persistence.')
table(['Repository area','Responsibility'],[['backend-api','Public API, EF model, migrations, services, hosted workflow recovery'],['ai-orchestrator','Agent contracts, agent implementations and controlled appointment tools'],['web-admin','React role-specific portal'],['mobile_app','Flutter patient application and device features'],['backend-api.Tests and client tests','Automated verification'],['.github/workflows','Build, test and container publishing automation']],[180,303])
page('4 Agent Roles and Contracts')
table(['Role','Input and output','Controlled capability'],[['A DomainAnalysisAgent','Symptoms and patient reference to risk, flagged factors and ward recommendation.','Patient-history access and Gemini analysis.'],['B PlanningAgentService','Symptoms, profile snapshot and A result to a versioned clinical plan and execution summary.','Context retrieval, assessment client and deterministic validation.'],['C AppointmentActionAgent','Doctor, patient, date and times to a typed AppointmentActionResult.','Find slots, check conflicts and create tentative booking.'],['D PharmacyAiService','Triage symptoms/plan or prescription items and patient context to safety findings.','Emergency rules and medication safety checks.']],[133,215,135])
txt('A and B use model-backed analysis. C performs a controlled tool workflow and D applies deterministic domain rules. Distinct roles are evidenced by different inputs, responsibilities and tool permissions, not by requiring four separate models.')
sub('Planning and state')
txt('ClinicalPlan contains SchemaVersion, Objective, SuggestedSpecialist, UrgencyLevel, RecommendedAction, Rationale, Steps, Warnings and Execution. Steps identify the responsible role, allowed tool, dependencies and approval requirement. Planning events retain operation, outcome and duration. AgentWorkflows records the triage ID, agent, status, input/output payloads, errors and timestamps.')
sub('Visible execution')
txt('The downstream helper records safety output and available slots as AppointmentAgent ActionRequired. The patient-facing book-slot route invokes C to validate and reserve the selected slot. Proposed steps and executed outcomes must be interpreted separately. The existence of a plan is not evidence that each downstream action completed.')
page('4 Agent Workflow and Safe Failure')
table(['Stage','Expected state transition','Evidence to inspect'],[['Submit','New triage and running planning attempt','TriageRecords and AgentWorkflows'],['Assess','Validated plan to InReview; failed plan to AssessmentFailed','Schema validation and execution events'],['Select slot','ActionRequired to tentative reservation','AvailableSlots and linked appointment ID'],['Review','Pending to Approved, Rejected or RevisionRequested','Authenticated doctor, version token and review history'],['Revise','Patient update triggers a new assessment attempt','Old review preserved and new attempt recorded'],['Complete','Booking confirmation or explicit downstream failure','Current appointment row and final UI state']],[85,195,203])
sub('Human approval')
txt('Doctor review checks an active doctor profile, current triage status and ExpectedUpdatedAt. The case decision and linked appointment action are performed within a transaction where supported. Appointment records also retain approval status, approving doctor and approval time. A transaction alone does not establish that all concurrency edge cases have been tested.')
sub('Known verification boundaries')
txt('An early emergency path can return a rule-generated critical recommendation and bypass the normal four-role chain. It must be demonstrated as an emergency path, not as evidence that all four agents executed. The inspected context integration still passes PatientId to a name-based A history lookup. Recovery currently searches for a recent tentative patient booking rather than using a unique workflow key. These limitations require correction or explicit disclosure in evaluation.')
txt('Appointment notification routing is recorded as unsupported in parts of the triage flow. Prescription notification functionality is separate and must not be used as evidence that appointment messages were delivered.')
page('5 Database Design')
story.append(Diagram('erd'));story.append(Spacer(1,12))
txt('Figure 2. Complete ER diagram: 14 mapped tables, 120 scalar columns and 15 foreign-key relationships. The landscape diagram uses F01-F15 to identify foreign keys and REF for application references without a configured foreign key. Source: output/pdf/CareFlow_AI_Complete_ERD.mmd.')
txt('The model includes fourteen DbSet entities. UUID identifiers identify business records. DateOnly and TimeOnly represent scheduling values; audit times use DateTime. Patient, doctor, appointment, prescription and workflow data are separated to avoid repeating whole records across transactions.')
txt('Additional relationships: Ward has many Admissions; PatientProfile has many Admissions; Doctor has many DoctorAvailabilities; TriageRecord can reference a TriageAttachment; Medicine has many PrescriptionItems. Users store role and optional patient/doctor identity references. Inspect the EF snapshot for the exact physical schema.')
page('5 Schema Integrity and Migration Strategy')
table(['Component','Principal entities','Integrity concerns'],[['A','Users, PatientProfiles, Wards, Admissions','Identity association, active records and bed capacity.'],['B','TriageRecords, AgentWorkflows, TriageReviewHistories, TriageAttachments','Case ownership, attempt linkage, version checks and attachment access.'],['C','Doctors, DoctorAvailabilities, Appointments','Time ranges, overlap prevention, approval identity and status.'],['D','Medicines, Prescriptions, PrescriptionItems','Patient/case consistency, dosage checks and stock integrity.']],[60,220,203])
txt('EF Core migrations and the model snapshot are included in the repository. Seed records support demonstrations, but seeded dates and accounts must not be confused with production data. Several entities use UpdatedAt as an optimistic concurrency token; this is not a claim that all entities use PostgreSQL xmin or that every conflict is translated correctly.')
txt('The CI workflow provisions PostgreSQL and creates a schema with EnsureCreatedAsync. That verifies schema creation through the current model, not replay of the entire migration chain. A clean database migration test remains a separate requirement.')
page('6 API Design and Component Endpoints')
table(['Component','Representative endpoints','Business operation'],[['A','POST /api/auth/register-patient; GET/POST /api/PatientProfiles; GET /api/PatientProfiles/search; POST /api/Admissions/allocate-ward; POST /api/Admissions/analyze-risk','Identity and context-aware admission support.'],['B','POST /api/triage; GET /api/triage; GET /api/triage/review-queue; PATCH /api/triage/{id}/review; POST .../{id}/revise; POST .../{id}/retry; POST .../{id}/book-slot','Assessment, revision, approval and delegated execution.'],['C','GET /api/DoctorAvailability/slots; POST /api/Appointments/tentative; POST .../{id}/approve; POST .../{id}/confirm; POST .../{id}/cancel','Conflict-checked reservation and human-approved confirmation.'],['D','GET/POST /api/Medicines; PATCH .../{id}/restock; POST /api/Prescriptions; PATCH .../{id}/approve; PATCH .../{id}/dispense; PATCH .../{id}/notify-retry','Safety-checked prescribing, stock updates and notification retry.']],[70,295,118])
txt('The controllers use asynchronous operations and DTOs for workflow requests. Outcomes include created, validation failure, unauthorized, forbidden, not found and conflict responses. JWT bearer authentication supplies role, patient and doctor claims. Swagger documents the API in development; production Swagger availability is not established by the deployment evidence.')
txt('Every component has at least four identifiable API operations across its owned controllers. Endpoint count alone is not evidence of complete security, error handling or user-facing integration.')
page('7 React and Flutter Design')
table(['Client and role','Implemented screens','State and integration'],[['React Admin','Patient/ward counts, patient management, staff registration','AuthContext, hooks, React Router and apiFetch.'],['React Staff','Patient records, wards, history search, risk analysis, availability and pharmacy','Shared backend; role-dependent navigation.'],['React Doctor','Triage queue, review/revision, assessment details and prescriptions','Versioned decisions and refresh after actions.'],['Flutter Patient','Registration/login, symptoms/image upload, case history/revision, appointments, prescriptions and ward status','Secure token storage, service classes and local widget state.']],[100,200,183])
txt('The web portal supports hospital operations while Flutter supports the patient journey. The mobile image picker and upload flow provide a meaningful device capability. Prescription QR display and local notifications are additional features; local notification display is not proof of external SMS or email delivery.')
sub('Workflow presentation')
txt('Web and mobile triage views show recommendation, severity, doctor notes, safety findings, scheduling outcome and notification outcome when returned by the API. The latest manual-slot design adds available-slot selection before booking. Current appointment rows should be used for live status; stored agent outputs are historical evidence.')
page('8 Security and Limitations')
sub('Implemented controls')
txt('The backend issues JWTs, uses hashed passwords, restricts many endpoints by role and checks patient/doctor claims for sensitive workflows. Flutter uses secure storage for session material. Triage review checks versions and allowed transitions. Appointment tools now use service/provider interfaces rather than the earlier hardcoded self-HTTP approach.')
sub('Historical security review and limitations')
bullets(['The inspected PatientProfiles update/delete actions lacked explicit authorization attributes; final-release access controls were not independently re-audited.','Earlier inspection identified credential-handling concerns, including secret logging and hardcoded fallbacks; a separate final security review is outside the recorded test results.','Earlier context integration used partial-name history lookup while B supplied a patient ID, creating an identity-matching risk.','The inspected recent-booking recovery heuristic presents a case-linkage risk without a workflow-specific idempotency key.','Manual book-slot should enforce case state, repeat-request behavior and consistent response mapping. The inspected implementation passes the appointment workflow to a mapper otherwise used for planning output.','Provider payloads and error messages must be minimized before exposing them to patients.'])
txt('These findings were recorded during the earlier code inspection and are not a fresh audit of the deployed release. CareFlow AI is an academic prototype; no HIPAA or ISO certification is claimed.')
page('9 Software Testing Report')
table(['Check','Observed result','Interpretation'],[['Backend dotnet test','101 passed, 1 skipped, 102 total','Current application and test projects compiled; suite completed.'],['React node tests','10 passed, 0 failed','Triage API contract and failure-handling checks.'],['React production build','Passed','Vite built the current web application.'],['Flutter static analysis','Passed: no issues found','flutter analyze --no-pub passed after the later deployment configuration changes.'],['PostgreSQL concurrency','Not established by this run','Appointment concurrency test is skipped for in-memory provider.'],['Deployment and APK','Render and Vercel live; APK linked','Role logins tested by the team; installed-device test evidence is not recorded here.']],[145,153,185])
txt('The latest supplied backend run on 6 October 2026 reported 101 passed, 1 skipped and 0 failed in approximately four seconds. React tests and production build passed during deployment preparation. Flutter static analysis subsequently passed. Test duration is not API response latency.')
sub('Coverage')
txt('Backend suites include authentication/authorization integration, admissions, configuration, planning, domain analysis, triage review, pharmacy safety, scheduling and orchestration. Flutter tests exercise validation, submission states, timeout messaging, revision/history behavior and late asynchronous responses. React tests validate request contracts, status handling and assessment/plan parsing.')
sub('Coverage limitations')
txt('Most backend data tests use EF InMemory. Ignoring transaction warnings does not validate rollback or PostgreSQL locking. The current React tests are not comprehensive rendered-component or browser tests. Static analysis does not replace an installed-device end-to-end test.')
page('9 End to End Test Record')
table(['Case','Procedure and expected result','Evidence status'],[['E2E 01 Normal case','Submit in Flutter, inspect A/B/D results, select C slot, approve in React, refresh patient booking.','Earlier booking evidence; team-reported end-to-end testing.'],['E2E 02 Revision','Doctor requests clarification; patient revises; new attempt appears while old review remains.','Selected automated coverage; no final cross-client trace recorded.'],['E2E 03 Rejection','Reject with notes; release linked tentative slot; patient sees rejection.','No separately recorded final-release result.'],['E2E 04 Emergency','Emergency rule produces escalation; no routine appointment is created.','Earlier saved case was approved through the emergency shortcut; not a four-agent golden case.'],['E2E 05 Failure','Provider unavailable or invalid result; no invented urgency; explicit follow-up/retry state.','Selected unit/widget tests; no deployed failure trace recorded.'],['E2E 06 Isolation','Patient A cannot read or change patient B\'s case or appointment.','Endpoint tests; separate manual isolation result not recorded.']],[72,272,139])
txt('Historical database inspection on 4 October recorded a non-emergency case with completed context analysis, validated planning, safety validation and a real tentative appointment for the same patient. It also showed missing context history in A. This is evidence for an earlier build, not proof that the current release satisfies all acceptance criteria.')
page('10 Agentic AI Evaluation Report')
sub('Evaluation method')
txt('Evaluate the workflow with fixed synthetic inputs, deterministic schema/business assertions and human review. A successful model response is insufficient: the correct tools, identity, approval gate and durable outcomes must also be verified. No LLM-as-judge score is claimed.')
table(['Golden or adversarial case','Required assertion','Current evidence'],[['Routine valid symptoms','Valid plan; supported specialty; explicit slot outcome; no confirmation before approval.','Planning tests and historical workflow evidence.'],['Invalid structured output','Reject malformed or unsupported output; record failure.','Planning/domain validation tests.'],['Prompt injection in symptoms','Input cannot change roles, tool permissions or approval requirements.','No dedicated evaluation result recorded.'],['Provider timeout','Bounded failure; no fabricated recommendation; safe retry.','Selected tests; no deployed failure result recorded.'],['Conflicting appointment','Only permissible reservation succeeds; return a structured conflict.','Service tests; concurrent database proof incomplete.'],['Wrong identity','Reject cross-patient access and spoofed reviewer.','Authentication integration test coverage.'],['Emergency','Escalate; skip routine scheduling; label rule-based processing accurately.','Deterministic rules and prior record.']],[125,225,133])
page('11 Performance Report')
sub('Measured observations')
txt('A historical single workflow recorded context retrieval at 301 ms, domain analysis at 2530 ms and symptom assessment at 2178 ms, with one model attempt. These sum to approximately 5.0 seconds for those recorded operations only. Booking, upload, network transit and client rendering are not included. This is one observation from an earlier build, not a benchmark or percentile.')
sub('Measurement limitations')
txt('The available performance evidence is a historical single-workflow trace. Concurrent-load results, p50/p95 latency, throughput and provider failure rates were not measured in the evidence recorded here.')
table(['Workload','Record','Evaluation protocol'],[['Read-only API','Request count, concurrency, p50/p95, failures','Synthetic dataset; 1, 5 and 10 concurrent users.'],['Triage submission','A/B latency, total workflow time, model attempts','Small bounded synthetic batch; separate model time from DB time.'],['Booking race','Accepted/rejected requests and final row count','Two independent clients target the same slot.'],['Review race','One accepted decision; explicit loser outcome','Two authenticated doctors submit the same version.'],['Database','Query time and integrity after workload','Disposable PostgreSQL test database.']],[120,178,185])
page('12 Deployment and Reproducibility')
txt('The current repository contains a multi-stage .NET 8 Dockerfile. It publishes both backend-api and ai-orchestrator into one runtime container listening on port 8080. React is a Vite static build. Flutter must be built as a signed Android APK using the final HTTPS API endpoint.')
table(['Area','Configuration or command','Status'],[['API','Docker build from repository root; PostgreSQL connection; JWT and Gemini settings.','Render deployment succeeded; API service live.'],['React','npm ci; npm run build; publish dist; VITE_API_BASE_URL.','Deployed to Vercel; role logins tested.'],['Flutter','flutter pub get; flutter build apk --release after endpoint/signing configuration.','API configuration updated; APK distribution link included.'],['Database','Reviewed EF migrations; restricted PostgreSQL credentials.','Neon used in earlier read-only verification.'],['Providers','Cloudinary and configured email/SMS endpoints.','External email/SMS delivery not established.']],[65,268,150])
sub('Environment variable names')
txt('ConnectionStrings__DefaultConnection; Gemini__ApiKey; Gemini__Model; Jwt__Secret; Jwt__Issuer; Jwt__Audience; CLOUDINARY_URL; Notifications__EmailEndpoint; Notifications__SmsEndpoint; Notifications__ApiKey. Do not include secret values in the report, APK, web bundle or repository.')
sub('Local startup order')
txt('Restore and configure the API, apply reviewed migrations to the intended database, start the backend using its project path, then start React and Flutter. From the repository root: dotnet run --project backend-api --launch-profile http. From web-admin: npm install followed by npm run dev. From mobile_app: flutter pub get followed by flutter run. Client URLs must match the chosen host; emulator loopback addresses do not work on physical phones.')
page('12 Deployment Acceptance and Demonstration')
sub('Deployment record')
txt('The backend deployment completed successfully on Render and the React website is live on Vercel. The team tested admin, doctor and staff login. Mobile services use the deployed HTTPS API, and the APK and demonstration recording are available through the links on page 2.')
sub('Ten minute demonstration plan')
table(['Time','Demonstration'],[['0:00 to 1:00','Overview, architecture and four roles.'],['1:00 to 3:00','Patient login, symptoms and image upload; show assessment and slot selection.'],['3:00 to 5:00','Inspect agent outputs and doctor approval in React.'],['5:00 to 6:00','Refresh patient appointment outcome; demonstrate revision or failure branch.'],['6:00 to 7:30','Prescription safety, approval, dispensing and notification outcome.'],['7:30 to 9:00','Database state, tests, CI and contribution evidence.'],['9:00 to 10:00','Deployed URLs, APK and limitations.']],[100,383])
txt('Use synthetic accounts and records. The submitted application may run its own agents during the viva, but external assistants must not answer viva questions or modify the work. Each member should be ready to explain one controller, schema relationship, UI state transition, agent contract and test from their component.')
ADRS=[
('ADR 001 React State Management','Session data is shared across protected routes, while filters, forms and review actions are local to individual screens.','React Context with hooks; Redux Toolkit; a dedicated server-state library.','The inspected implementation uses AuthContext for identity and React hooks for screen state. Retain that lightweight approach for the current scope.','Limited boilerplate and clear role routing are benefits. Manual refresh, duplicate fetch logic and race handling need explicit tests. Add a query-cache library if shared server state becomes substantially more complex.','web-admin/src/context/AuthContext.jsx; src/App.jsx; src/pages/TriageReview.jsx'),
('ADR 002 Flutter State Management','The patient application needs secure session storage and asynchronous forms, history and booking screens.','Local StatefulWidget state with services; Provider or Riverpod; BLoC.','The inspected screens primarily use local state and service/repository boundaries, with flutter_secure_storage for session persistence. A provider dependency exists, but that alone does not prove consistent Provider adoption.','This keeps screen logic understandable and supports repository-based widget tests. Cancellation, mounted checks and shared refresh behavior require discipline. A central session and workflow store would reduce duplicated state as the app grows.','mobile_app/lib/services/auth_service.dart; services/triage_service.dart; screens/triage_dashboard_screen.dart'),
('ADR 003 Agent Orchestration','The assessed workflow must combine model output, controlled scheduling actions, deterministic safety and human review within the mandatory ASP.NET backend.','A custom .NET orchestrator; a separate Python agent service; a general-purpose agent framework.','CareFlow uses custom C# orchestration, DI contracts, a hosted recovery worker and a referenced orchestrator project.','One runtime simplifies deployment and shared identity. The group owns transition rules, timeouts, retries and audit correctness. Strongly typed contracts and integration tests are essential; a named agent class alone does not prove safe orchestration.','backend-api/Services/PlanningAgentService.cs; TriageOrchestrationHelper.cs; WorkflowManager.cs; ai-orchestrator/Agents'),
('ADR 004 Workflow Persistence','Model outputs vary, while business entities and doctor decisions require durable relational integrity and auditability.','Relational business tables plus versioned serialized summaries; an event store; transient memory only.','Use PostgreSQL entities for cases, appointments and reviews, with versioned serialized input/output summaries in AgentWorkflows.','Business identities remain queryable and review history is retained. JSON compatibility and duplicate status representations require careful mapping. Current appointment status should come from its live entity, not an old serialized booking result.','backend-api/Models/AgentWorkflowState.cs; TriageRecord.cs; TriageReviewHistory.cs; Data/ApplicationDbContext.cs'),
('ADR 005 Deployment Platform','The group needs a reproducible API, web portal, shared PostgreSQL database and installable APK with a short delivery window.','Render Docker plus static hosting; Azure managed services; a self-managed VM.','The API is deployed as a Docker web service on Render, and the React/Vite application is deployed on Vercel. Neon provides PostgreSQL storage and Cloudinary supports uploaded images. The Android APK is distributed through the linked Drive folder.','Managed hosting reduces server setup. Free-service sleep, provider quotas, cold starts and external dependencies may affect demonstrations. Secrets, migrations, health checks, HTTPS client configuration and final link verification remain release responsibilities.','Dockerfile; web-admin/vercel.json; mobile_app/lib/services/api_config.dart; deployed service URLs on page 2')]
for title,context,options,decision,cons,evidence in ADRS:
 page('13 '+title)
 for heading,body in [('Context',context),('Alternatives',options),('Decision',decision),('Consequences',cons),('Evidence',evidence)]:sub(heading);txt(body)

members=[
('14','A','Bandara MMSD','IT24103089','Patient context and admissions',
'Patient identity, profile history, admission requests, ward information and contextual risk analysis. The component supplies context used by triage and operational data used by staff.',
'Users, PatientProfiles, Admissions and Wards. Patient identity and active-state behavior need to remain consistent across account, profile and admission records.',
'AuthController, PatientProfilesController, AdmissionsController and WardsController. Non-CRUD operations include allocate-ward and analyze-risk.',
'Admin patient/staff interfaces, staff patient management and risk-analysis screens; mobile patient and ward-related screens. The latest local MobileWardStatus edit has a compilation defect that must be corrected.',
'DomainAnalysisAgent returns risk, flagged factors and a ward recommendation using patient history and symptoms. Exact patient-ID context retrieval is an outstanding integration issue.',
'AdmissionsControllerTests, DomainAnalysisAgentTests and shared authentication tests. Verify which assertions were personally authored before signing the contribution statement.',
'f3c14b8 - updated AI agent and admin CRUD (sanuthmibandara); d17ed33 - cherry-pick conflict resolution. PR #29 integrates the admission branch. Integration work by others must be acknowledged.'),
('15','B','Dinuwara K.D.S','IT24103033','Planning and triage orchestration',
'Symptom ingestion, validated clinical planning, review queue, doctor decisions, patient revision, retry/recovery and cross-component orchestration. Integration work connects A context, C booking and D safety to shared triage views.',
'TriageRecords, AgentWorkflows, TriageReviewHistories and TriageAttachments. ExpectedUpdatedAt supports stale-review detection; workflow rows retain structured results and timings.',
'TriageController, PlanningAgentService, PlanningPlanValidator, WorkflowManager and TriageOrchestrationHelper. Non-CRUD operations include assessment, review, revision, retry and book-slot.',
'React TriageReview and doctor dashboard integration; Flutter submission, history/revision and workflow status. Shared appointment details connect triage to the scheduling experience.',
'The planning contribution combines patient context and A analysis, invokes the assessment client, validates output and produces a versioned multi-step plan. Human approval is represented explicitly, and downstream actions have separate outcomes.',
'PlanningAgentTests, TriageReviewTests, TriageOrchestrationIntegrationTests, web triage contract tests and Flutter triage screen tests. The current backend suite passes; PostgreSQL race coverage remains incomplete.',
'9a2a30d - Component B integration; cf7ad3c - My Appointments UI; 0df54ce - manual slot selection and approval workflow; eea64f4 - pharmacy test-signature integration fix. PRs #29, #31 and #32 show integration history, not sole authorship of all merged code.'),
('16','C','Pallawala S R','IT24102852','Resource scheduling and appointments',
'Doctor availability, slot generation, booking conflicts, tentative reservations and appointment approval/confirmation. The patient chooses an available slot through the integrated triage flow.',
'Doctors, DoctorAvailabilities and Appointments. Appointments carry patient and doctor references, date/time, lifecycle status, approval status, reviewer and approval timestamp.',
'DoctorAvailabilityController, AppointmentsController, AppointmentAgentController and scheduling services. Non-CRUD behavior includes slot generation and approval-gated confirmation.',
'React appointment availability view and Flutter availability/booking screens. Latest integration changes by Dinuwara connect manual slot selection with triage; distinguish original scheduling ownership from shared fixes.',
'AppointmentActionAgent invokes only find-slots, conflict-check and tentative-booking tools through provider interfaces. It returns typed success/failure categories; the actor identity is enforced by the API boundary.',
'AppointmentSchedulingTests cover selected scheduling behavior. The concurrency test is explicitly skipped because EF InMemory cannot establish concurrent relational constraints.',
'a7fbcde - scheduling fixes; 0892cc4 - dependency injection fix; 7f86ab1 - PostgreSQL CI schema setup. PR #32 integrates scheduling and shared approval fixes. Verify original commits and individual attribution before final signing.'),
('17','D','Dulshan P A','IT24102599','Pharmacy safety and notifications',
'Medicine inventory, prescription lifecycle, safety validation, dispensing and notification delivery status. Safety rules participate in triage and medication workflows.',
'Medicines, Prescriptions and PrescriptionItems. Patient/triage consistency and stock updates are key business constraints; updated-at tokens provide selected concurrency checks.',
'MedicinesController, PrescriptionsController, PharmacyAiService and NotificationService. Non-CRUD behavior includes restocking, safety review, issuing, dispensing and notification retry.',
'React inventory and prescription management; Flutter prescriptions and pickup-related presentation. External delivery and local device notification must be distinguished.',
'PharmacyAiService performs deterministic emergency checks and patient-aware prescription validation. The revised safety method accepts PatientProfile, making history-dependent scenarios testable.',
'PharmacySafetyTests and shared authorization tests. A prior signature mismatch was repaired in eea64f4; the current suite passes. Real provider delivery requires separate evidence.',
'69c0982 - prescription and notification updates; PR #26 integrates D work. Git identity DulshanPA appears in repository history. Verify the precise component commits and include any shared fixes performed by other members.')]
for num,component,name,sid,area,scope,db,api,ui,agent,tests,git in members:
 if component=='B':
  page('15 Individual Report Component B')
  txt('Dinuwara K.D.S | IT24103033 | Planning and triage orchestration')
  sub('1. Contribution Statement')
  txt('As a full-stack contributor to the CareFlow-AI project, I was responsible for key integrations across the backend API, the React Web Admin dashboard, and the Flutter mobile application. My primary contributions focused on ensuring end-to-end functionality, securing deployment pipelines, and enhancing the user experience.')
  txt('Specifically, I:')
  for item in [
   'Mobile Application (Flutter): Enhanced the UX of the Patient Profile and Appointments screens, resolved API routing issues by dynamically configuring the mobile app to connect to the deployed Render API, and fixed CI build warnings related to case sensitivity in file structures.',
   'Web Admin (React/Vite): Developed the logic to sanitize complex input (like Triage Record IDs) in the Prescription Management interface, implemented comprehensive error handling to surface backend validation messages directly in the UI, and configured Vercel routes and ESLint rules to unblock production deployments.',
   'Backend API (.NET 8): Refactored Triage Approval workflows, resolved authorization logic to allow proper end-to-end demo testing, and implemented PharmacySafetyTests and EF Core in-memory transaction handling to ensure the backend met rigorous unit testing requirements.',
   'Deployment & CI/CD: Led the final deployment strategy, successfully transitioning the backend to Render and the Web Admin to Vercel, and documented the deployment process in the repository README.md.'
  ]: txt('- '+item,'SmallX')
  sub('2. Evidence of Key Commits & PRs')
  table(['Commit','Contribution'],[
   ['214eff4','fix: configure mobile services for deployed Render API'],
   ['a847f48','fix: support React routes on Vercel'],
   ['3f2769e','chore: relax eslint rules to prevent deployment failures'],
   ['74c978b','fix: sanitize triage record ID and display backend validation errors in prescription form'],
   ['e1156d5','fix: resolve triage approval permissions and mobile login profile fetch'],
   ['546a05f','feat: Enhance patient profile and appointments UX'],
   ['eea64f4','Fix CS7036 in PharmacySafetyTests and integrate E2E test changes']
  ],[72,411])
  txt('See GitHub repository for PR #34 and PR #33 which were merged by IT24103033 covering Integration Testing and UI Updates.','SmallX')
  page('15 Individual AI Usage Log')
  txt('Dinuwara K.D.S | IT24103033')
  sub('3. Individual AI Usage Log')
  txt('During this project, I utilized AI (specifically Google Gemini) as an active pair-programming assistant.')
  table(['Date / Phase','Task / Problem','AI Tool Used','How AI Assisted'],[
   ['Development','Mobile UI & Routing','Gemini','Assisted in refactoring Flutter screens (Patient Profile) and fixing Dart case-sensitivity issues that were breaking the CI pipeline.'],
   ['Testing','Backend E2E & Unit Tests','Gemini','Suggested fixes for EF Core in-memory database transaction errors and helped resolve CS7036 compilation errors in PharmacySafetyTests.'],
   ['Debugging','Web Admin API Integration','Gemini','Analyzed silent failures in the Prescription submission form; suggested input sanitization (.replace()) and mapped 400 Bad Request data explicitly to UI error states.'],
   ['Deployment','CI/CD Pipeline Failures','Gemini','Diagnosed GitHub Actions "red cross" deployment failures. Recommended migrating the backend to Render, configuring Vercel for the frontend, and bypassing overly strict ESLint build blockers.']
  ],[72,101,64,246])
  page('15 Individual Reflection and Declaration')
  txt('Dinuwara K.D.S | IT24103033','SmallX')
  sub('4. One-Page AI Reflection')
  txt('Integrating an agentic AI assistant into my software engineering workflow fundamentally changed how I approached problem-solving, particularly during the integration and deployment phases of the CareFlow-AI project.','SmallX')
  sub('Effectiveness and Productivity')
  txt('The AI excelled as a rapid diagnostic tool. For example, when our web application’s prescription form was silently failing, the AI quickly analyzed the frontend-backend data contract and identified that the input field was appending unnecessary text (e.g., "Low") to a GUID. Rather than spending hours tracing the network tab and debugging C# controllers, the AI immediately suggested sanitizing the string in React and exposing the backend’s 400 Bad Request validation array to the user interface. This turned a frustrating bug into a 5-minute fix. Similarly, the AI proved invaluable for deployment. When GitHub Actions failed due to missing cloud SSH keys and ESLint warnings, the AI correctly contextualized that these were infrastructure and tooling configurations, not core logic flaws. It seamlessly pivoted our strategy to use Render for the backend and Vercel for the frontend, writing the exact eslint.config.js and configurations needed to achieve a green build.','SmallX')
  sub('Limitations and Challenges')
  txt('However, the AI is not infallible. A recurring limitation I observed was its tendency to apply overly broad solutions if not given precise context. For instance, when asking the AI to "fix the build," it occasionally attempted to modify core architecture rather than simply adjusting a linter rule. I learned that prompt engineering is critical; treating the AI like a junior developer—providing exact file constraints and clear objectives—yielded much better code. Additionally, the AI lacks intrinsic knowledge of our specific university grading rubrics, meaning I had to manually override its suggestions when it tried to remove "redundant" files (like GitHub Action workflows) that were actually required for our assignment submission.','SmallX')
  sub('Impact on Software Engineering')
  txt('Ultimately, the AI shifted my role from "syntax writer" to "systems architect." Because the AI could quickly generate boilerplate code and fix syntax errors, I was able to spend more cognitive energy on higher-level system design, user experience, and ensuring that the React frontend, .NET backend, and Flutter mobile app all communicated flawlessly. It highlighted that the future of software engineering relies heavily on code review, system orchestration, and critical thinking, rather than just memorizing API documentation.','SmallX')
  sub('5. Signed Declaration')
  txt('I declare that the work presented in this section, as well as the commits attributed to me in the repository, represents my own contribution to the project. Where AI tools were utilized, they were used strictly as an assistant for debugging, scaffolding, and optimizing code, and all AI-generated logic was thoroughly reviewed, tested, and integrated by me.','SmallX')
  txt('Signature: ___________________________ (IT24103033)','SmallX')
  txt('Date: 06 October 2026','SmallX')
  page('15 Component B Evidence - Interface and Project Board')
  txt('Dinuwara K.D.S | IT24103033','SmallX')
  from reportlab.platypus import Image
  for filename,caption in [
   ('b-triage.png','Figure B1. Doctor triage review interface: searchable submissions, urgency and status filters, case details, assessment output and scheduling status. The selected case shows a critical rule-based escalation and a pending scheduling outcome.'),
   ('b-board.png','Figure B2. CareFlow-AI GitHub project board. Component B mobile application, triage API/planning agent and triage database/workflow tasks appear in Done. The board also displays tasks belonging to other team members.')
  ]:
   im=Image('tmp/report-evidence/'+filename)
   iw,ih=im.imageWidth,im.imageHeight
   im.drawWidth=483;im.drawHeight=483*ih/iw
   story.append(im);story.append(Spacer(1,7));txt(caption,'SmallX');story.append(Spacer(1,12))
  continue
 if component=='A':
  page('14 Individual Report Component A')
  txt('Bandara MMSD | IT24103089 | Patient admissions, wards and shared user management')
  sub('Contribution statement')
  txt('My primary responsibility was Component A across ASP.NET Core, PostgreSQL, React, Flutter and domain analysis. I designed PatientProfiles, Wards and Admissions with EF Core, developed patient/admission APIs, and contributed to the transition from local PostgreSQL to the shared Neon database.')
  sub('Database and backend')
  txt('The admission workflow links a patient to a ward and updates OccupiedBeds in the same transaction. The current route is POST /api/Admissions/allocate-ward. It checks ward existence and capacity before saving. A transaction supports consistent writes; concurrent capacity enforcement still requires separate verification.')
  sub('React, Flutter and shared authentication')
  txt('I built staff-facing patient tiles, patient history search through GET /api/PatientProfiles/search, the ward-capacity display and the Admit to Ward modal. Mobile work enabled patients to view medical history and blood group through patient-profile API integration. I also contributed the role-based web login, backend user models and role verification. Mobile authentication and extended account management were shared work completed by a teammate.')
  sub('Domain analysis agent')
  txt('The Gemini-based domain agent combines symptoms with patient history and requests structured JSON containing risk and a ward recommendation. My work included provider failure handling and the migration from a local Ollama integration. The supplied report describes a high-risk fallback; fallback routing must be distinguished from a successful model assessment and reviewed by authorized staff.')
  sub('DevOps, evidence and challenges')
  txt('I contributed the backend GitHub Actions pipeline and investigated build failures caused by DTO changes, including AdmissionRequestDto. The supplied screenshots show project setup PR #1 merged and admissions PR #7 closed. These statuses are preserved as historical evidence rather than described as current passing CI.')
  txt('Challenges included aligning the React admission interaction with backend state changes, enforcing JSON response structure, configuring cloud database access and resolving schema/merge mismatches. The screenshots that follow document the supplied development evidence; current deployment and final test results are recorded separately.')
  page('14 Individual AI Usage Log')
  txt('Bandara MMSD | IT24103089')
  table(['Tool','Prompt or task','Student modification'],[['Gemini', '"Generate ASP.NET Core C# code to connect to a Neon PostgreSQL database and build the PatientProfiles and Wards tables using EF Core."', "Adapted the generated connection strings and DbContext classes to fit the project's repository structure."], ['Gemini', '"How to swap a local Ollama LLM integration for the Gemini Cloud API in C# while enforcing a strict JSON output schema?"', 'Integrated the provided HttpClient logic and responseMimeType = "application/json" into the DomainAnalysisAgent.'], ['Antigravity IDE', '"Analyze attached UI designs and generate a React login page and Hospital Staff landing page with a patient search bar and admission modal."', 'Reviewed the generated UI components, manually wired the fetch() API calls to the C# backend, and adjusted state management for the modal triggers.'], ['Gemini', '"Explain why my GitHub Actions CI pipeline failed with a build-and-test error after modifying the AllocateWard method."', 'Used the diagnostic advice to run dotnet build locally, identifying uncommitted test files that were missing the new AdmissionRequestDto schema.']],[70,216,197])
  txt('Sanuthmi reports reviewing generated models and UI, wiring API calls, adjusting modal state and using local dotnet build to diagnose the DTO mismatch. These statements do not replace dated session records.','SmallX')
  page('14 Individual Reflection and Declaration')
  txt('Bandara MMSD | IT24103089')
  sub('Individual AI reflection')
  txt('The implementation of Generative AI technology has been very instrumental in speeding up the lifecycle of development of Component A, especially when creating the boilerplate architecture and creating the React code from the visual interface ideas. From the onset of development of the component, using AI to quickly create Entity Framework Core models and REST API controllers laid a good ground for concentrating on the relational logic such as Admissions.')
  txt('The major learning point here was the process of moving from the local LLM (Ollama) to the cloud-based Gemini API to work with the Domain Analysis Agent. Although the initial set-up entailed dealing with the limitations of the network connection and API keys within the appsettings.Development.json, the cloud API turned out to be more dependable in terms of ensuring that the JSON Schema was enforced. This made sure that the AI risk assessment could be deserialized without crashing the C# back-end because of any formatting hallucination. The necessity of the safety net through the fallback error handling (like falling back to "High Risk" during a 503 error) was proved.')
  txt('Working with Antigravity IDE to create the React front end from the mocks revealed how powerful but limited the capabilities of AI code writing are. On the one hand, it perfectly implemented the CSS styling and routing of the components for the Hospital Staff dashboard and the role-based login, while on the other hand, it was not intelligent enough to be able to automatically connect the separate PatientProfiles and Wards pages. I personally had to design and prompt AI to implement the "Admit to Ward" flow to actually perform the business logic. In essence, AI is just an advanced syntax generator and debugging assistant, but enforcing data integrity, resolving Git merge conflicts, and maintaining the overarching system architecture remain strictly as job of humans.')
  sub('Individual declaration')
  txt("I, Bandara MMSD, declare that the work presented in this report is my own original work, except where acknowledged through the AI Usage Log and standard academic references. I confirm that all AI-generated code has been reviewed, tested, and modified to meet the project's specific architectural requirements.", 'SmallX')
  txt('Date: 06/10/2026','SmallX')
  from reportlab.platypus import Image
  signature=Image('tmp/report-evidence/a-evidence-13.png',width=150,height=48)
  signature.hAlign='LEFT';story.append(signature)
  def a_image(n,caption,width=483,maxheight=480):
   im=Image(f'tmp/report-evidence/a-evidence-{n}.png')
   iw,ih=im.imageWidth,im.imageHeight
   scale=min(width/iw,maxheight/ih)
   im.drawWidth=iw*scale;im.drawHeight=ih*scale
   story.append(im);story.append(Spacer(1,8));txt(caption,'SmallX');story.append(Spacer(1,10))
  page('14 Component A Evidence Project Setup')
  a_image(1,'Figure A1. Supplied pull-request list for Component A, showing admissions and project setup work.')
  a_image(2,'Figure A2. Project setup and CI pipeline PR #1 shown as merged. Historical development evidence.',maxheight=410)
  page('14 Component A Evidence Admissions Review')
  a_image(3,'Figure A3. Admissions PR #7 shown as closed. The discussion describes continued work through another branch; closed must not be interpreted as directly merged.')
  page('14 Component A Evidence Database')
  a_image(4,'Figure A4. Supplied pgAdmin cloud connection view; source password is masked. Connection visibility alone does not prove all cloud workflows.',width=420)
  a_image(5,'Figure A5. Supplied PatientProfiles table view showing profile and history fields.',width=420)
  a_image(6,'Figure A6. Supplied Wards table view showing capacity and occupied-bed values.',width=420)
  page('14 Component A Evidence Staff Interface')
  a_image(7,'Figure A7. Patient registration modal in the staff interface.')
  a_image(8,'Figure A8. Ward-capacity dashboard with occupancy indicators.')
  page('14 Component A Evidence Analysis and Mobile')
  left=Image('tmp/report-evidence/a-evidence-9.png',width=218,height=364)
  right=Image('tmp/report-evidence/a-evidence-10.png',width=162,height=364)
  story.append(Table([[left,right]],colWidths=[241,242]));story.append(Spacer(1,12))
  txt('Figure A9 (left). Supplied risk-analysis result with risk, ward recommendation, factors and patient history. Figure A10 (right). Flutter medical-profile screen displaying history, blood group and date of birth. These are historical UI captures.','SmallX')
  continue
 if component=='C':
  page('16 Individual Report Component C')
  txt('Pallawala S R | IT24102852 | Appointments and resource scheduling')
  sub('Individual contribution')
  txt('I was responsible for appointment scheduling, doctor availability, conflict checking and the Action/Tool Agent. I also contributed to React and Flutter appointment interfaces and the tests and CI work associated with Component C.')
  sub('Backend, database and agent work')
  txt('My work covered doctor availability management, available-slot retrieval, appointment date/time validation, tentative booking and doctor approval before confirmation. Doctors, DoctorAvailabilities and Appointments hold the scheduling data. Transaction-based conflict checks were introduced to reduce concurrent double-booking risks.')
  txt('The Action/Tool Agent uses allow-listed FindAvailableSlots, CheckBookingConflict and CreateTentativeBooking tools. Injected provider abstractions avoid direct calls to protected controller endpoints. AppointmentActionResult and AppointmentActionStatus distinguish successful bookings, unavailable slots, conflicts, invalid requests and provider failures. AppointmentWorkflowRunner connects these contracts to the workflow layer.')
  sub('Client integration and technical learning')
  txt('I connected Flutter appointment screens to real doctor, availability and booking APIs and implemented the React appointment page. Challenges included dependency-injection registration, Git conflicts and B/C contract mismatches. I learned to align interfaces and authentication across components and to verify AI suggestions through builds, tests and API checks.')
  sub('Git evidence')
  table(['Commit','Contribution'],[['c2c3ed8','Appointment scheduling foundation.'],['34b4241','Doctor availability APIs.'],['a7fbcde','Appointment scheduling fixes and integration.']],[100,383])
  txt('Feature branch: feature/Resource-Scheduling. The supplied document identifies the PR by title; local history also contains merges #9 and #24 from this branch. Shared B/C integration changes must retain their original attribution.', 'SmallX')
  sub('Testing evidence and its limits')
  txt('Sandathi reports historical local results of 92/92 backend tests and 13/13 Flutter tests, plus a successful Flutter CI check. Her report also records a later B/C compilation mismatch in TriageOrchestrationHelper.cs. These are historical observations, not the current release status. This document update did not rerun the tests.', 'SmallX')
  txt('The consolidated report separately records the inspected baseline results. Its scheduling concurrency test was skipped under EF InMemory; the supplied report does not establish current passing PostgreSQL concurrency coverage. ', 'SmallX')
  page('16 Individual AI Usage Log')
  txt('Pallawala S R | IT24102852')
  table(['Task','AI assistance','Reported outcome'],[['Appointment API design', 'Used AI to understand how to structure doctor availability and appointment scheduling endpoints.', 'Helped organize the Component C implementation and identify required validation.'], ['Dependency injection', 'Used AI to understand and review dependency injection and provider/adaptor registration for the appointment tools.', 'Supported the move away from direct protected-controller HTTP calls.'], ['Action/Tool Agent', 'Used AI to reason about the allow-listed appointment tools and the agent workflow.', 'Supported the FindAvailableSlots, CheckBookingConflict and CreateTentativeBooking tool structure.'], ['Structured results', 'Used AI to review how appointment operations should distinguish success, unavailable slots, conflicts and provider errors.', 'Resulted in the AppointmentActionResult/AppointmentActionStatus approach.'], ['Concurrency', 'Used AI to understand concurrent booking risks and transaction isolation.', 'Supported the serializable transaction approach used for conflict-free booking.'], ['Approval workflow', 'Used AI to review the requirement that appointments must be approved before confirmation.', 'Supported the Pending/Approved/Confirmed appointment flow.'], ['Flutter integration', 'Used AI for step-by-step guidance while connecting appointment screens to real availability and booking APIs.', 'Helped replace temporary/sample appointment data with API-driven behaviour.'], ['Git/merge conflicts', 'Used AI to understand Git conflict markers and resolve the Component C Program.cs registration conflict.', 'Helped retain the provider-based Component C registrations.'], ['CI debugging', 'Used AI to interpret GitHub Actions build/test logs.', 'Helped identify the difference between Component C implementation issues and shared B/C integration issues.'], ['Documentation', 'Used AI to organize contribution evidence, testing evidence and the individual AI reflection.', 'Produced a structured submission section for final review.']],[102,190,191])
  txt('Student-reported review approach: AI suggestions were reviewed and adapted to the existing architecture. Her reflection describes checking project structure, authentication and interfaces, and verification through builds, automated tests, API testing and CI.', 'SmallX')
  page('16 Individual Reflection and Declaration')
  txt('Pallawala S R | IT24102852')
  sub('Individual AI reflection')
  txt('During the development of Component C, I used AI as a supporting tool to understand technical concepts, design parts of the appointment workflow, debug errors, and improve my testing approach. I used AI suggestions as guidance and adapted them to the existing CareFlow-AI_SE_039 codebase.')
  txt('AI was particularly helpful when working with dependency injection, provider abstractions, and the Action/Tool Agent. It helped me understand why the appointment tools should use injected application services instead of directly calling protected controller endpoints. AI also helped me design structured appointment results so that situations such as unavailable slots, booking conflicts, invalid requests, and provider errors could be handled separately.')
  txt('AI also helped me understand concurrency and approval control. I learned why checking availability and creating a booking as separate operations could result in conflicting concurrent bookings. This supported the use of a serializable database transaction. I also used AI to understand the human-in-the-loop approval process, where an appointment remains tentative until it is approved by a doctor before confirmation.')
  txt("During Flutter integration, Git conflict resolution, and CI troubleshooting, AI helped me identify problems and understand possible solutions. However, I learned that AI-generated suggestions should not be accepted without verification. I had to check the suggestions against the existing project structure, authentication requirements, interfaces, and other team members' work.")
  txt('Overall, AI improved my productivity and helped me understand difficult technical problems more quickly. However, testing and verification remained my responsibility. I validated my implementation through builds, automated tests, API testing, and CI results. My main lesson is that AI is most useful as a development assistant when its suggestions are understood, adapted, and tested rather than accepted automatically.')
  sub('Individual declaration')
  txt("I declare that the work and contribution described in this individual section accurately represent my contribution to the CareFlow-AI_SE_039 project. I have identified the use of AI assistance honestly and have not intentionally presented another member's contribution as my own. I understand that the submitted evidence should correspond to the project repository, commits, pull requests and test results.", 'SmallX')
  txt('Student: Pallawala S R | IT24102852     Date: 05/10/2026', 'SmallX')
  txt('Signature:', 'SmallX')
  from reportlab.platypus import Image
  sig_image=Image('tmp/report-evidence/component-c-signature.png')
  sig_image.drawHeight=42
  sig_image.drawWidth=42*sig_image.imageWidth/sig_image.imageHeight
  sig_image.hAlign='LEFT'
  story.append(sig_image)
  def evidence_image(number,caption):
   suffix='png' if number<6 else 'jpeg'
   img=Image(f'tmp/report-evidence/c-evidence-image{number}.{suffix}')
   source_width,source_height=img.imageWidth,img.imageHeight
   img.drawWidth=483
   img.drawHeight=483*source_height/source_width
   story.append(img);story.append(Spacer(1,9))
   txt(caption,'SmallX');story.append(Spacer(1,12))
  page('16 Component C Evidence Git and Review')
  txt('Screenshots supplied in the individual report. They record development history and do not establish the final release status.','SmallX')
  evidence_image(1,'Figure C1. Resource-scheduling PR #27 listed as open, with a failed check indicator at the time of capture.')
  evidence_image(2,'Figure C2. A later view shows PR #27 closed and lists scheduling, dependency-injection and PostgreSQL CI fixes. Closed is not evidence of a direct merge; integration evidence is recorded separately.')
  page('16 Component C Evidence Cloud Database')
  evidence_image(3,'Figure C3. Supplied pgAdmin view of a connected Neon PostgreSQL database and its table list. The password is masked in the source image. This view alone does not demonstrate synchronization or an end-to-end workflow.')
  page('16 Component C Evidence Persisted Records')
  evidence_image(4,'Figure C4. Local PostgreSQL Appointments query showing four stored rows with doctor, patient and scheduling fields.')
  evidence_image(5,'Figure C5. Local PostgreSQL Doctors query showing doctor names, specializations and active status. This is a different database view from the cloud connection in Figure C3.')
  page('16 Component C Evidence Appointment Interface')
  evidence_image(6,'Figure C6. Local React staff appointment-availability page showing 58 availability records and date/specialization filters.')
  evidence_image(7,'Figure C7. Filtered availability for a neurologist on 3 October 2026, showing three time intervals. These UI captures show availability browsing, not successful booking or doctor approval.')
  continue
 if component=='D':
  page('17 Individual Report Component D')
  txt('Dulshan P A | IT24102599 | Pharmacy inventory, e-prescriptions and safety verification')
  sub('Individual contribution')
  txt('My responsibility was Component D: medicine inventory, e-prescriptions, safety validation, dispensing and notifications. I implemented inventory CRUD, stock and expiry tracking, low-stock handling, prescription creation and approval, and the pharmacy dispensing workflow. I also worked on related React and Flutter interfaces, migrations, automated tests and integration fixes.')
  sub('Backend, agent and patient features')
  txt('The component uses Medicines, Prescriptions and PrescriptionItems, with MedicinesController, PrescriptionsController, PharmacyAiService and NotificationService. My safety work covered unsafe approval, approving-doctor identity and active status, patient/triage consistency, duplicate medicines, drug interactions, emergency rules and duplicate stock deductions.')
  txt('I added SMS/email integration, patient contact fields, notification status, timestamps, failure details and retry tracking. Patient features included QR prescription pickup and medication reminders. I also contributed Docker and GitHub Actions configuration. Provider delivery, device reminders and deployed operation require separate runtime evidence.')
  sub('Traceable contribution evidence')
  table(['Commit / PR','Contribution'],[['6eadd16 / #6','Inventory, prescriptions, interfaces and migrations.'],['89ecf78 / #12','SMS/email notification integration.'],['88b01eb / #16','Safety, authorization, ownership and stock concurrency.'],['db70e2d, 68ffbe2 / #20','QR pickup, reminders, Docker and CI fixes.'],['69c0982, 725e22c / #26','Notification tracking/retry, contact fields and migration integration.']],[155,328])
  txt('Branches: feature/inventory, InventoryNew, fix/dulshan and bugfix/dulshan. Local Git history confirms the listed PR merges, including #26 (27182a5). Shared integration fixes, including eea64f4, are not claimed as sole authorship.', 'SmallX')
  sub('Testing evidence')
  txt('Dulshan reports 24 passed, 0 failed and 0 skipped in PharmacySafetyTests. The current file contains 24 Fact tests covering approval safety, identity, ownership, duplicate dispensing/medicines, emergency rules, notification failures, contact data, retry, access control and missing configuration. ', 'SmallX')
  txt('Command: dotnet test backend-api.Tests/CareFlowAI.API.Tests.csproj --filter "FullyQualifiedName~PharmacySafetyTests"', 'SmallX')
  page('17 Personal Evidence and Reflection')
  txt('Dulshan P A | IT24102599')
  sub('Individual AI usage log')
  table(['Date','Task assisted by AI','Student review and verification'],[['2026-09-21','Inventory and e-prescription implementation','Reviewed generated code, tested and changed it before committing.'],['2026-09-25','SMS/email notification integration','Reviewed, configured the service and tested the workflow.'],['2026-09-28','Safety, authorization, concurrency and tests','Reviewed changes and ran tests before committing.'],['2026-10-02','QR pickup, reminders, Docker and CI','Debugged, changed the implementation and verified changes.'],['2026-10-02','Notification retry and migration conflicts','Reviewed migrations, fixed conflicts and tested the system.']],[72,180,231])
  sub('Individual reflection')
  txt('I used Antigravity with Gemini during the development of Component D to help with implementation, debugging and testing.', 'SmallX')
  txt('It was useful when working with ASP.NET Core, database migrations, JWT authentication, Flutter notifications, Docker and GitHub Actions.', 'SmallX')
  txt('I did not use the generated code without checking it. I reviewed the code, made changes where needed, ran the application and tested the features before committing them.', 'SmallX')
  txt('For safety-related features, I paid extra attention to the generated code because incorrect validation could affect prescription approval and dispensing.', 'SmallX')
  txt('AI also helped me find and fix development and CI/CD issues more quickly.', 'SmallX')
  txt('Through this work, I improved my understanding of API development, database migrations, authentication, testing, notification services and deployment.', 'SmallX')
  txt('Overall, I used AI as a development assistant. I was still responsible for reviewing, testing and deciding which changes should be included in the project.', 'SmallX')
  sub('Individual declaration')
  txt('I declare that the information in this document accurately represents my individual contribution to Component D of the CareFlow AI project. I confirm that the Git branches, commits, pull requests and test results mentioned in this document are related to my contribution.', 'SmallX')
  txt('Signature: __________________________     Date: __________________', 'SmallX')
  continue
 page(f'{num} Individual Report Component {component}')
 txt(f'{name} | {sid} | {area}')
 sub('Contribution statement for member verification');txt(scope)
 sub('Database and backend contribution');txt(db);txt(api)
 sub('React and Flutter contribution');txt(ui)
 sub('Distinct agent contribution');txt(agent)
 sub('Tests and Git evidence');txt(tests);txt(git)
 page(f'{num} Personal Evidence and Reflection')
 txt(f'{name} | {sid}')
 sub('Member supplied evidence required')
 txt('Confirm the technical contribution on the preceding page, list the exact tests and commits you authored, and identify collaborative changes. Do not sign a statement claiming work you cannot explain or that was completed by another member.')
 table(['Date','Tool and model','Task and output','Changed or rejected','Verification'],[['TO COMPLETE','TO COMPLETE','TO COMPLETE','TO COMPLETE','TO COMPLETE'],['TO COMPLETE','TO COMPLETE','TO COMPLETE','TO COMPLETE','TO COMPLETE']],[65,95,120,110,93])
 sub('Individual reflection to be written by the student')
 txt('Insert approximately one page in your own words before final submission. Explain which AI tools you used and when; what they did well or got wrong; what you changed, added or rejected and why; and what you learned about your own skills. Use real examples from your commits and tests. No reflection has been generated on your behalf.')
 sub('Challenges and learning prompts')
 if component=='B':txt('Possible evidence to discuss in your own words: diagnosing assessment failures; distinguishing saved submissions from successful AI results; resolving integration contracts; manual slot selection; stale-review handling; and separating a green build from an end-to-end verified workflow.')
 else:txt('Choose a real component-specific issue, describe your diagnosis and code change, cite its test or commit, and explain one tradeoff. Connect the example to the owned backend, database, UI or agent responsibility.')
 sub('Declaration for student review and signature')
 txt('I confirm that the contribution attributed to me is accurate, that my AI usage log is complete, and that I can explain, test and modify the work submitted under my name. I acknowledge external sources and collaborative contributions. I have written my own reflection and reviewed the evidence cited in this section.')
 txt('Signature: __________________________     Date: __________________')

page('18 Consolidated AI Usage Declaration')
txt('We acknowledge using AI assistance where permitted for development, debugging, documentation and verification. Each member is responsible for the accuracy and understanding of their submitted work and for an individual usage log. We distinguish development assistants from the application\'s own agent subsystem. We will not use external AI assistance during the final demonstration or viva.')
sub('Known use in preparing this report')
txt('Development assistance included Google Gemini, Antigravity and Codex. Gemini and Antigravity supported implementation and debugging as described in the individual logs. Codex supported source and Git inspection, test execution, deployment configuration, documentation, diagrams and PDF preparation. Individual contribution statements and reflections were provided by the members. Each member remains responsible for reviewing, understanding and explaining their submitted work.')
table(['Member','Signature','Date'],[['Bandara MMSD / IT24103089','',''],['Dinuwara K.D.S / IT24103033','',''],['Pallawala S R / IT24102852','',''],['Dulshan P A / IT24102599','','']],[250,143,90])
page('19 References and Evidence Index')
refs=[
('R1','SE3090 Assignment 1 Specification with Marking Scheme, 2026, 17 pages. Sections 5 to 15 define technical and submission requirements; sections 18 to 20 define disclosure and final checks.'),
('R2','CareFlow source repository: https://github.com/IT24103033/CareFlow-AI_SE_039 . Inspected baseline db3a50f with local modifications on 5 October 2026.'),
('E1','backend-api.Tests: current run recorded 101 passed, 1 skipped; suite includes scheduling, planning, review, authentication and pharmacy tests.'),
('E2','web-admin/tests/triage.test.js: 10 passing contract tests; npm run build completed.'),
('E3','Flutter verification: 12 earlier triage tests passed; subsequent flutter analyze --no-pub completed with no issues following deployment configuration changes.'),
('E4','backend-api/Data/ApplicationDbContext.cs and Migrations: entity relationships, tokens, seed configuration and migration history.'),
('E5','backend-api/Services/Planning and ai-orchestrator: plan schema, tool permissions and agent contracts.'),
('E6','Git commits db3a50f, 0df54ce, eea64f4, 9a2a30d, f3c14b8 and scheduling/pharmacy component commits cited in individual sections.'),
('E8','Member documents: Individual Contribution Report.docx (A), ComponentD.docx (D) and SE3100_Individual_Section_Component-c.docx (C). Contributions, logs and original reflections; A/C include signatures. B supplied his individual section directly on 6 October 2026. Individual AI logs are included in the member sections.'),
('E7','Historical read-only database observations from 4 and 5 October, summarized without personal symptom text. Earlier-build evidence only; not a substitute for the final release demonstration.')]
for k,v in refs:sub(k);txt(v)
page('19 Submission Access and Evaluation Guide')
table(['Deliverable','Location'],[
 ['Source repository','https://github.com/IT24103033/CareFlow-AI_SE_039'],
 ['Web application','https://care-flow-ai-se-039.vercel.app'],
 ['Backend API','https://careflow-ai-se-039.onrender.com'],
 ['Android APK','https://drive.google.com/drive/folders/1ZgL1VADVWcowwMZFpPXKCl6tQUH0diqy?usp=sharing'],
 ['Demonstration video','https://drive.google.com/drive/folders/1mDBlpug9Q9nrtR5LMSYQq7J4UoKvjaYr?usp=sharing']
],[125,358])
sub('Evaluator access')
txt('Web accounts: admin@careflow.ai, doctor@careflow.ai and staff@careflow.ai. The password for each demonstration account is password. Patient users can open the mobile app, select Create Account, register with test details and sign in.')
sub('System walkthrough')
txt('Start with a patient symptom submission in the mobile application. Inspect the assessment and available appointment options, then use the doctor portal to review the case. Follow the resulting appointment and prescription states through the corresponding web and mobile screens. The recorded demonstration accompanies the implementation evidence in this report.')
sub('Operational limitations')
txt('The Render free instance can experience cold-start delays. Model responses depend on provider availability and quota. Assessment failures remain explicit and do not represent a completed clinical recommendation. Email/SMS delivery and concurrent PostgreSQL race behavior are not established by the recorded unit tests. CareFlow AI is an academic prototype with human review of clinical workflow decisions.')

def footer(c,doc):
 c.saveState();c.setFont('Helvetica',8);c.setFillColor(colors.HexColor('#566574'))
 c.drawString(56,27,'CareFlow AI | SE3090 | SE_039')
 c.drawRightString(A4[0]-56,27,str(doc.page));c.restoreState()
file=OUT/'SE3090_SE039_Consolidated_Report_Draft.pdf'
doc=SimpleDocTemplate(str(file),pagesize=A4,rightMargin=56,leftMargin=56,topMargin=45,bottomMargin=48,title='CareFlow AI Consolidated Report',author='Group SE_039')
doc.build(story,onFirstPage=footer,onLaterPages=footer)
Path('tmp/report-evidence/report_content.json').write_text(json.dumps(pages,indent=2))
md=['# CareFlow AI consolidated report source\n']
for pg in pages:
 md.append('## '+pg['title']);md.extend(pg['content'])
 for t in pg['tables']:
  md.append('| '+' | '.join(t['headers'])+' |');md.append('| '+' | '.join(['---']*len(t['headers']))+' |')
  md.extend('| '+' | '.join(str(x) for x in r)+' |' for r in t['rows'])
 md.append('')
(OUT/'SE3090_SE039_Report_Source.md').write_text('\n\n'.join(md))
print('Created',file,'planned pages',len(pages))

# Replace database figure with the full landscape ERD after rendering the report.
import runpy
runpy.run_path('tmp/report-evidence/build_erd.py', run_name='__main__')
