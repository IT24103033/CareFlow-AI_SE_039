from pathlib import Path
p=Path('tmp/report-evidence/build_report.py')
s=p.read_text()
block=''' if component=='B':
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
  txt('Log supplied by the student using project phases; exact dates and Gemini model version were not supplied. Codex assistance with documentation, verification and deployment configuration is disclosed in the consolidated AI usage declaration.','SmallX')
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
  continue
'''
s=s.replace(" if component=='A':",block+" if component=='A':",1)
s=s.replace("'24 to 42'","'24 to 43'").replace("'43 to 45'","'44 to 46'")
s=s.replace('Component B personal evidence is still pending.','Dinuwara supplied his contribution statement, seven commit references, phase-based Gemini usage log, original reflection and declaration dated 6 October 2026. His signature, exact log dates and Gemini model version remain to be completed.')
s=s.replace('A/C/D material supplied; B personal evidence pending','A/B/C/D material supplied; signatures/log details incomplete')
s=s.replace('A/C include signatures. Log details remain incomplete.','A/C include signatures. B supplied his individual section directly on 6 October 2026. Log details remain incomplete.')
p.write_text(s)
