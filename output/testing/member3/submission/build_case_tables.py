import runpy
from pathlib import Path
from xml.etree import ElementTree as ET
from xml.sax.saxutils import escape
from reportlab.platypus import SimpleDocTemplate, Paragraph, Table, TableStyle, Spacer
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.lib import colors
from reportlab.lib.pagesizes import A4, landscape
v=runpy.run_path('output/testing/member3/submission/build_report.py'); cases=list(v['cases'])
base=Path('output/testing/member3'); ns=v['ns']
r=ET.parse(base/'ai-summary/member3-ai-summary.trx')
# Reused cases remain attributed as existing tests. Theory arguments identify individual cases.
for i,x in enumerate(sorted(r.findall('.//t:UnitTestResult',ns),key=lambda x:x.attrib['testName'])):
 name=x.attrib['testName']
 if 'Member3' in name or 'Invalid_outputs_respect_configured' in name: continue
 method=name.split('.',4)[-1]
 title=method.split('(')[0].replace('_',' ')
 assert x.attrib['outcome']=='Passed'
 cases.append((f'M3-E{i+1:02}',title,'Run the existing xUnit case using its controlled fixture'+ (': '+method[method.index('('):] if '(' in method else '.')+' See source mapping in TEST_CASES.md.', title+'. All assertions in the named source test must hold.','All assertions passed in student-run AI suite.','ai-summary/member3-ai-summary.trx'))
assert len(cases)==47
styles=getSampleStyleSheet(); styles.add(ParagraphStyle(name='Cell',fontName='Helvetica',fontSize=7.2,leading=9.3,wordWrap='CJK'))
def p(t): return Paragraph(escape(t),styles['Cell'])
rows=[]; md=['# Completed Test Case Table','Dinuwara K.D.S | IT24103033 | 8 October 2026','', '47 selected cases: 46 AI and 1 HTTP integration. 21 new cases and 26 reused cases. Repeated runs are not added. AI cases passed in ai-summary/member3-ai-summary.trx; workflow passed in workflow/member3-workflow.trx.','', '| Test ID | Feature | Preconditions | Steps/input | Expected result | Actual result | Pass/Fail |','|---|---|---|---|---|---|---|']
for cid,feature,steps,expected,actual,evidence in cases:
 pre='.NET 8; xUnit project restored; synthetic test fixtures.'
 if cid.startswith('M3-C'): pre+=' Both captured JSON files available; offline replay.'
 elif cid=='M3-I01': pre+=' Isolated EF InMemory; synthetic identities and future slot; AI stubs.'
 else: pre+=' Controlled responses; no live AI call.'
 if cid.startswith('M3-R'): actual='Expected attempt count and failure/approval assertions passed.'
 if cid.startswith('M3-A'): actual='Expected status, tool sequence and ID assertions passed.'
 if cid.startswith('M3-S'): actual='Expected emergency verdict and Pending approval assertions passed.'
 if cid=='M3-I01': actual='Approved case, review history and Confirmed appointment retrieved; early confirmation rejected; one stored booking.'
 if cid.startswith('M3-C'): actual='Completed/InReview; Pending approval; valid plan; review gate retained; no appointment.'
 cells=[cid,feature,pre,steps,expected,actual,'Passed']
 rows.append([p(c) for c in cells]); md.append('| '+' | '.join(c.replace('|','/').replace('\n',' ') for c in cells)+' |')
md+=['','## Source and evidence mapping','New cases: M3-R = PlanningAgentTests retry theory; M3-A/M3-S = Member3AgentTests; M3-C = Member3CapturedResponseTests; M3-I01 = Member3WorkflowIntegrationTests.','']
for i,x in enumerate(sorted(r.findall('.//t:UnitTestResult',ns),key=lambda x:x.attrib['testName'])):
 name=x.attrib['testName']
 if 'Member3' not in name and 'Invalid_outputs_respect_configured' not in name: md.append(f'- M3-E{i+1:02}: `{name}`')
md+=['','## Limits','These are selected-scope results, not a clinical validation. The HTTP workflow uses EF InMemory and AI stubs; the captured JSON assertions do not make new Gemini calls. Live normal/injection urgency differed; no live slots were available.','']
(base/'submission/TEST_CASES.md').write_text('\n'.join(md))
headers=['Test ID','Feature','Preconditions','Steps / input','Expected result','Actual result','Pass/Fail']
t=Table([[p(h) for h in headers]]+rows,colWidths=[48,112,113,148,145,154,49],repeatRows=1)
t.setStyle(TableStyle([('VALIGN',(0,0),(-1,-1),'TOP'),('BACKGROUND',(0,0),(-1,0),colors.HexColor('#dcebf0')),('GRID',(0,0),(-1,-1),.3,colors.HexColor('#b7c8cf')),('TOPPADDING',(0,0),(-1,-1),6),('BOTTOMPADDING',(0,0),(-1,-1),6),('ROWBACKGROUNDS',(0,1),(-1,-1),[colors.white,colors.HexColor('#f5f8fa')])]))
story=[Paragraph('Member 3 - Completed Test Case Document',styles['Title']),Paragraph('Dinuwara K.D.S | IT24103033 | 8 October 2026',styles['Normal']),Spacer(1,10),Paragraph('46 AI cases and 1 HTTP workflow case: all passed in the student-run evidence. Includes 21 new and 26 reused cases. See TEST_CASES.md for exact source mapping. Tools: xUnit, WebApplicationFactory and application-level plan validation.',styles['Normal']),Spacer(1,12),t]
def footer(c,d):
 c.setFont('Helvetica',8); c.drawString(35,20,'IT24103033 | AI evidence: ai-summary/member3-ai-summary.trx | Workflow: workflow/member3-workflow.trx');c.drawRightString(805,20,str(d.page))
out=Path('output/pdf/SE3110_Member3_IT24103033_Test_Cases.pdf')
SimpleDocTemplate(str(out),pagesize=landscape(A4),leftMargin=35,rightMargin=35,topMargin=28,bottomMargin=38).build(story,onFirstPage=footer,onLaterPages=footer)
print(out)
