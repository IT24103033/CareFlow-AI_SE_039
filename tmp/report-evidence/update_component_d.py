from pathlib import Path
from docx import Document
p=Path('tmp/report-evidence/build_report.py');s=p.read_text()
doc=Document('/Users/sujana/Downloads/ComponentD.docx');paragraphs=[p.text for p in doc.paragraphs if p.text.strip()]
a=paragraphs.index('AI Reflection');b=paragraphs.index('Declaration');reflection=paragraphs[a+1:b]
block=''' if component=='D':
  page('17 Individual Report Component D')
  txt('Dulshan P A | IT24102599 | Pharmacy inventory, e-prescriptions and safety verification')
  sub('Individual contribution')
  txt('My responsibility was Component D: medicine inventory, e-prescriptions, safety validation, dispensing and notifications. I implemented inventory CRUD, stock and expiry tracking, low-stock handling, prescription creation and approval, and the pharmacy dispensing workflow. I also worked on related React and Flutter interfaces, migrations, automated tests and integration fixes.')
  sub('Backend, agent and patient features')
  txt('The component uses Medicines, Prescriptions and PrescriptionItems, with MedicinesController, PrescriptionsController, PharmacyAiService and NotificationService. My safety work covered unsafe approval, approving-doctor identity and active status, patient/triage consistency, duplicate medicines, drug interactions, emergency rules and duplicate stock deductions.')
  txt('I added SMS/email integration, patient contact fields, notification status, timestamps, failure details and retry tracking. Patient features included QR prescription pickup and medication reminders. I also contributed Docker and GitHub Actions configuration. Provider delivery, device reminders and deployed operation require separate runtime evidence.')
  sub('Traceable contribution evidence')
  table(['Commit / PR','Contribution'],[['6eadd16 / #6','Inventory, prescriptions, interfaces and migrations.'],['89ecf78 / #12','SMS/email notification integration.'],['88b01eb / #16','Safety, authorization, ownership and stock concurrency.'],['db70e2d, 68ffbe2 / #20','QR pickup, reminders, Docker and CI fixes.'],['69c0982, 725e22c / #26','Notification tracking/retry, contact fields and migration integration.']],[155,328])
  txt('Branches: feature/inventory, InventoryNew, fix/dulshan and bugfix/dulshan. Local Git history confirms the listed PR merges, including #26 (27182a5); this updates the pending merge note in the supplied report. Shared integration fixes, including eea64f4, are not claimed as sole authorship.', 'SmallX')
  sub('Testing evidence')
  txt('Dulshan reports 24 passed, 0 failed and 0 skipped in PharmacySafetyTests. The current file contains 24 Fact tests covering approval safety, identity, ownership, duplicate dispensing/medicines, emergency rules, notification failures, contact data, retry, access control and missing configuration. This edit did not rerun that test command.', 'SmallX')
  txt('Command: dotnet test backend-api.Tests/CareFlowAI.API.Tests.csproj --filter "FullyQualifiedName~PharmacySafetyTests"', 'SmallX')
  page('17 Personal Evidence and Reflection')
  txt('Dulshan P A | IT24102599')
  sub('Individual AI usage log')
  txt('Tool recorded in all five entries: Antigravity / Gemini. Exact model version was not supplied. Dates and details require confirmation against the session history, as noted by Dulshan.', 'SmallX')
  table(['Date','Task assisted by AI','Student review and verification'],[['2026-09-21','Inventory and e-prescription implementation','Reviewed generated code, tested and changed it before committing.'],['2026-09-25','SMS/email notification integration','Reviewed, configured the service and tested the workflow.'],['2026-09-28','Safety, authorization, concurrency and tests','Reviewed changes and ran tests before committing.'],['2026-10-02','QR pickup, reminders, Docker and CI','Debugged, changed the implementation and verified changes.'],['2026-10-02','Notification retry and migration conflicts','Reviewed migrations, fixed conflicts and tested the system.']],[72,180,231])
  sub('Individual reflection')
REFLECTION_LINES
  sub('Individual declaration')
  txt('I declare that the information in this document accurately represents my individual contribution to Component D of the CareFlow AI project. I confirm that the Git branches, commits, pull requests and test results mentioned in this document are related to my contribution.', 'SmallX')
  txt('Signature: __________________________     Date: __________________', 'SmallX')
  continue
'''
block=block.replace('REFLECTION_LINES','\n'.join('  txt('+repr(t)+", 'SmallX')" for t in reflection))
s=s.replace("def txt(s): story.append(p(s)); current['content'].append(s)","def txt(s,style='BodyX'): story.append(p(s,style)); current['content'].append(s)")
needle='for num,component,name,sid,area,scope,db,api,ui,agent,tests,git in members:\n'
assert needle in s
s=s.replace(needle,needle+block)
s=s.replace("('E7','Historical", "('E8','ComponentD.docx supplied by Dulshan (IT24102599) on 5 October 2026: contribution details, AI usage log, original reflection and declaration. Log dates/model details and signature remain for member confirmation.'),\n('E7','Historical")
s=s.replace("['Individual sections','Technical summaries included','Confirm ownership and add own logs/reflections/signatures.']", "['Individual sections','D contribution, log and reflection supplied; A-C pending','Confirm logs/models and ownership; obtain signatures.']")
s=s.replace("These statements do not replace each member\\'s actual dated log.","Dulshan supplied a five-entry Antigravity/Gemini log and reflection in ComponentD.docx; the exact model version and log dates remain for his confirmation. A-C logs are still pending.")
p.write_text(s)
print('Inserted Component D source material; preserved',len(reflection),'reflection paragraphs verbatim.')
