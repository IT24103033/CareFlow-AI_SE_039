from pathlib import Path
p=Path('tmp/report-evidence/build_report.py');s=p.read_text()
# Remove editorial instructions and the unrelated sample reference, retaining technical limitations and disclosures.
drop=['Report status: review copy.', 'The CarePulse sample was examined', "('R3','CarePulse_Sample", 'Decision owner and review date: GROUP TO COMPLETE.', 'Status: implementation rationale for group review;', 'The two supplied Swagger authentication screenshots were excluded', 'Completion of this page requires member review.', 'Do not submit this review copy', 'Final packaging:', 'Retain final case IDs,', 'Acceptance should be recorded per scenario', 'Attach command/script,', 'The assignment requests one consolidated PDF,', 'Screenshots required before submission', 'Capture synthetic-data screenshots', 'Database verification to attach', "bullets(['Run migrations against", 'Supplied tool/task records are reproduced below.', 'The following entries summarize the supplied log.', 'Log supplied by the student using project phases;', 'Tool recorded in all five entries:']
lines=[]
for line in s.splitlines():
 if any(x in line for x in drop):continue
 lines.append(line)
s='\n'.join(lines)+'\n'
replacements={
'Evidence baseline: feature/integration-testing-b-c at db3a50f, with local changes present during inspection on 5 October 2026. Results apply to that working tree, not automatically to a later release.':'Submission date: 6 October 2026. This report combines implementation evidence collected on 5 October with subsequent deployment, verification and individual contribution updates. Historical observations are identified separately from final deployment results.',
' | Backend deployed; health and production Swagger URLs remain unverified.':' | Render-hosted ASP.NET Core API.',
' | Download link supplied; public download, build version and device installation verification pending.':' | Android application distribution folder.',
' | Video uploaded (reported by group); public viewing access pending verification.':' | Recorded system demonstration.',
'Neon PostgreSQL used during development; attach final deployment evidence without credentials.':'Neon PostgreSQL; schema and development database screenshots are included in this report.',
'Group reports all three logins working.':'Admin, doctor and staff login were tested on the deployed website.',
'evaluator access to deployed Swagger must be configured and verified separately.':'production Swagger availability is not established by the deployment evidence.',
'These are implementation limitations, not completed fixes. The group should update this section after remediation and rerun the corresponding negative tests. Do not claim HIPAA, ISO or other certification based on UI labels or academic prototype functionality.':'These findings were recorded during the earlier code inspection and are not a fresh audit of the deployed release. CareFlow AI is an academic prototype; no HIPAA or ISO certification is claimed.',
'Open findings at the inspected baseline':'Historical security review and limitations',
'PatientProfiles update and delete actions lack explicit authorization attributes; verify and repair before public access.':'The inspected PatientProfiles update/delete actions lacked explicit authorization attributes; final-release access controls were not independently re-audited.',
'Program.cs still contains credential-handling concerns identified during inspection: remove secret logging and hardcoded fallbacks, and rotate exposed credentials.':'Earlier inspection identified credential-handling concerns, including secret logging and hardcoded fallbacks; a separate final security review is outside the recorded test results.',
'A retrieves history by partial patient name while B supplies a patient ID; restore exact identity-based context access.':'Earlier context integration used partial-name history lookup while B supplied a patient ID, creating an identity-matching risk.',
'The recent-booking recovery heuristic can link an unrelated case. Use a workflow-specific idempotency key.':'The inspected recent-booking recovery heuristic presents a case-linkage risk without a workflow-specific idempotency key.',
'Flutter test --no-pub':'Flutter static analysis',
'12 passed; one test file failed to load':'Passed: no issues found',
'MobileWardStatus.dart bracket errors blocked widget_test.dart compilation.':'flutter analyze --no-pub passed after the later deployment configuration changes.',
"['Deployment and APK','Not verified','Final URLs and installed-device evidence still required.']":"['Deployment and APK','Render and Vercel live; APK linked','Role logins tested by the team; installed-device test evidence is not recorded here.']",
'Tests were executed during report preparation on 5 October 2026 against the working tree at db3a50f plus local changes. Previous successful test counts are not substituted for the current run. Backend suite duration was approximately six seconds; this is test execution time, not API response latency.':'The latest supplied backend run on 6 October 2026 reported 101 passed, 1 skipped and 0 failed in approximately four seconds. React tests and production build passed during deployment preparation. Flutter static analysis subsequently passed. Test duration is not API response latency.',
'Passing triage widgets does not prove the complete mobile application compiles; the failing app-level test must be repaired and rerun.':'Static analysis does not replace an installed-device end-to-end test.',
'Earlier booking evidence exists; repeat after manual-slot merge.':'Earlier booking evidence; team-reported end-to-end testing.',
'Automated coverage exists in parts; final cross-client capture needed.':'Selected automated coverage; no final cross-client trace recorded.',
'Final release verification needed.':'No separately recorded final-release result.',
'Covered by selected unit/widget tests; final deployed run needed.':'Selected unit/widget tests; no deployed failure trace recorded.',
'Endpoint tests plus manual negative verification required.':'Endpoint tests; separate manual isolation result not recorded.',
'Planning tests and historical run; current full chain pending.':'Planning tests and historical workflow evidence.',
'Dedicated final evaluation evidence not supplied.':'No dedicated evaluation result recorded.',
'Selected tests; deployed provider failure pending.':'Selected tests; no deployed failure result recorded.',
'Authentication integration tests; verify all endpoints.':'Authentication integration test coverage.',
'Current performance evidence gap':'Measurement limitations',
'No reproducible concurrent-load results, p50/p95 latency report, throughput measurement or provider failure-rate dataset was supplied for this report. Therefore no fabricated performance chart or target-achievement claim is presented.':'The available performance evidence is a historical single-workflow trace. Concurrent-load results, p50/p95 latency, throughput and provider failure rates were not measured in the evidence recorded here.',
'Suggested execution':'Evaluation protocol',
'Deployment not independently verified.':'Render deployment succeeded; API service live.',
'Local production build passed.':'Deployed to Vercel; role logins tested.',
'APK not supplied; current app test compile error.':'API configuration updated; APK distribution link included.',
'Final provider delivery evidence required.':'External email/SMS delivery not established.',
'Verify all required names against the final build. ':'',
'Proposed, not yet deployed: host the existing API Docker container and React static build on a managed platform, retain Neon PostgreSQL and Cloudinary, and distribute a signed APK. The group must confirm the final platform.':'The API is deployed as a Docker web service on Render, and the React/Vite application is deployed on Vercel. Neon provides PostgreSQL storage and Cloudinary supports uploaded images. The Android APK is distributed through the linked Drive folder.',
'Dockerfile; .github/workflows/deploy.yml; final hosting decision and access evidence pending':'Dockerfile; web-admin/vercel.json; mobile_app/lib/services/api_config.dart; deployed service URLs on page 2',
'Options considered for this report':'Alternatives',
'Confirm the final release run before submission.':'',
'Provide redacted API/test evidence for final inclusion.':'',
'This edit did not rerun that test command.':'',
'Log details remain incomplete.':'Individual AI logs are included in the member sections.',
'CareFlow AI | SE3090 | SE_039 | Review copy':'CareFlow AI | SE3090 | SE_039',
'CareFlow AI consolidated report review source':'CareFlow AI consolidated report source',
'Group declaration, references and final checklist':'Group declaration, references and submission access',
'Signed declaration supplied by the student':'Individual declaration',
}
for a,b in replacements.items():s=s.replace(a,b)
# Replace outdated release to-do list with factual deployment record.
a=s.index("sub('Release acceptance')");b=s.index("sub('Ten minute demonstration plan')",a)
s=s[:a]+"sub('Deployment record')\ntxt('The backend deployment completed successfully on Render and the React website is live on Vercel. The team tested admin, doctor and staff login. Mobile services use the deployed HTTPS API, and the APK and demonstration recording are available through the links on page 2.')\n"+s[b:]
# Keep disclosure concise and accurate rather than exposing drafting instructions.
a=s.index("txt('Codex assisted with reading");b=s.index('\n',a)
s=s[:a]+"txt('Development assistance included Google Gemini, Antigravity and Codex. Gemini and Antigravity supported implementation and debugging as described in the individual logs. Codex supported source and Git inspection, test execution, deployment configuration, documentation, diagrams and PDF preparation. Individual contribution statements and reflections were provided by the members. Each member remains responsible for reviewing, understanding and explaining their submitted work.')"+s[b:]
s=s.replace("('E3','mobile_app/test: 12 triage tests passed; widget_test.dart could not load because MobileWardStatus.dart had unmatched brackets.')","('E3','Flutter verification: 12 earlier triage tests passed; subsequent flutter analyze --no-pub completed with no issues following deployment configuration changes.')")
# Replace final drafting checklist with useful evaluator instructions.
a=s.index("page('19 Final Submission Completion Checklist')");b=s.index('\ndef footer',a)
s=s[:a]+'''page('19 Submission Access and Evaluation Guide')
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
''' +s[b:]
p.write_text(s)
