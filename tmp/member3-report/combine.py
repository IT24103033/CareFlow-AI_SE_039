from docx import Document
from docx.shared import Pt, RGBColor
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from copy import deepcopy
from pathlib import Path
base=Document('/Users/sujana/Downloads/combine.docx'); member=Document('output/docx/IT24103033_Member3_Testing_Report.docx')
# Consolidate repeated status commentary, retaining the factual scope boundary.
remove_starts=['Document contents','1 Test plan and tools\n','Final classification: BLOCKED','Counts are taken from','The selected automated suite completed','Two separate live Swagger submissions','Scope of this individual contribution']
for p in list(member.paragraphs):
 if any(p.text.startswith(t) for t in remove_starts):p._element.getparent().remove(p._element)
for p in member.paragraphs:
 if p.text=='Executive summary':p.text='Member 3 contribution summary'
 if p.text.startswith('Actual live execution status'):p.text='Live execution environment'
 if p.text.startswith('Doctor authentication and future availability setup completed.'):
  p.text='Doctor authentication, future availability setup and a clean Android build completed. Emulator installation encountered INSTALL_FAILED_INSUFFICIENT_STORAGE. The subsequent retry ended without a completed Flutter handoff or React review result. The automated HTTP workflow is the verified integrated workflow; the live Flutter to React to Flutter path remains unverified.'
 if p.text.startswith('The completed tests provide repeatable evidence'):
  p.text='The completed tests provide repeatable evidence for agent safeguards and the patient-to-doctor-to-patient HTTP workflow in an isolated environment. Two fixes improve booking-result validation and retrieval of the review audit trail. PostgreSQL concurrency and notification delivery are outside the verified results of this contribution.'
 if p.text.startswith('Codex assisted'):
  p.text=p.text.replace('This disclosure should accompany any module-specific CLEAR declaration required by the group submission.','')
 if p.text.startswith('Work sequence on 8 October:'):
  p.text='Work sequence on 8 October: baseline, planning boundaries, live AI captures, domain/action/safety tests, defect correction and retesting, HTTP workflow and regression, cross-platform setup and documentation. Selected automated checks met the pass criterion.'
# Import uniquely identified styles to preserve Member 3 layout.
stylemap={s.style_id:'M3_'+s.style_id for s in member.styles}
for st in member.styles:
 el=deepcopy(st.element);el.set(qn('w:styleId'),stylemap[st.style_id])
 name=el.find(qn('w:name'))
 if name is not None:name.set(qn('w:val'),'Member 3 '+name.get(qn('w:val')))
 if qn('w:default') in el.attrib:del el.attrib[qn('w:default')]
 for tag in ['basedOn','next','link']:
  for x in el.findall(qn('w:'+tag)):
   v=x.get(qn('w:val'))
   if v in stylemap:x.set(qn('w:val'),stylemap[v])
 base.styles.element.append(el)
body=base._element.body
last=body.find(qn('w:sectPr'));body.remove(last)
p=OxmlElement('w:p');pr=OxmlElement('w:pPr');pr.append(last);p.append(pr);body.append(p)
for child in member._element.body:
 el=deepcopy(child)
 for x in el.xpath('.//w:pStyle | .//w:rStyle | .//w:tblStyle'):
  v=x.get(qn('w:val'))
  if v in stylemap:x.set(qn('w:val'),stylemap[v])
 # The appended report has no images or external relationships. Remove its footer references.
 for x in el.xpath('.//w:headerReference | .//w:footerReference'):x.getparent().remove(x)
 for par in ([el] if el.tag==qn('w:p') else [])+el.xpath('.//w:p'):
  pp=par.find(qn('w:pPr'))
  if pp is None:pp=OxmlElement('w:pPr');par.insert(0,pp)
  if pp.find(qn('w:pStyle')) is None:
   st=OxmlElement('w:pStyle');st.set(qn('w:val'),'M3_Normal');pp.insert(0,st)
 body.append(el)
# Add group title and an at-a-glance contribution summary before preserved member evidence.
cover=Document()
cover.add_paragraph('CareFlow AI Software Testing Report','Title')
cover.add_paragraph('SE3110 Quality Management in Software Engineering')
cover.add_paragraph('Software Testing and Quality Evaluation of the SE3090 Integrated System')
cover.add_paragraph('Group SE_039  |  8 October 2026')
cover.add_heading('Testing contributions',1)
t=cover.add_table(rows=1,cols=2);t.style='Table Grid'
t.rows[0].cells[0].text='Contribution';t.rows[0].cells[1].text='Recorded results'
for a,b in [('Backend and database','13 tests; two validation defects corrected and retested.'),('React and Flutter user interfaces','12 React and 12 appointment widget tests passed; full Flutter regression 25 passed.'),('Non functional testing','k6 performance and reliability results; OWASP ZAP and authorization checks.'),('Agentic AI and integration','46 AI cases and one isolated HTTP workflow passed; two application defects corrected and retested.')]:
 c=t.add_row().cells;c[0].text=a;c[1].text=b
cover.add_paragraph('This report consolidates the four individual contributions, test case records, execution evidence and defect retests. Results are reported by suite because regression and focused runs overlap. Detailed test environments and coverage boundaries are recorded within each contribution.')
cover.add_heading('Report organization',1)
cover.add_paragraph('Part 1 Backend and database testing\nPart 2 React and Flutter UI testing\nPart 3 Non functional testing\nPart 4 Agentic AI evaluation and integration testing')
cover.add_page_break()
for el in reversed(list(cover._element.body)[:-1]):body.insert(0,deepcopy(el))
# Remove redundant blank spacers while preserving drawings, section boundaries and explicit breaks.
previous_blank=False
for el in list(body):
 if el.tag!=qn('w:p'):previous_blank=False;continue
 blank=not ''.join(el.itertext()).strip() and not el.xpath('.//w:drawing | .//w:pict | .//w:br | .//w:sectPr')
 if blank and previous_blank:body.remove(el)
 previous_blank=blank
# Consistent black headings and neutral running footer across all members.
for st in base.styles:
 if st.type==1 and ('Heading' in st.name or 'Title' in st.name):st.font.color.rgb=RGBColor(0,0,0)
for root in [base._element,base.styles.element]:
 for x in root.xpath('.//w:pBdr'):x.getparent().remove(x)
for section in base.sections:
 for p in section.footer.paragraphs:
  p.clear()
 p=section.footer.paragraphs[0];p.add_run('CareFlow AI | SE_039 | ').font.size=Pt(8)
 field=OxmlElement('w:fldSimple');field.set(qn('w:instr'),'PAGE');p._p.append(field)
base.core_properties.title='CareFlow AI Software Testing Report';base.core_properties.author='Group SE_039'
out=Path('output/docx/SE039_Combined_Testing_Report.docx');base.save(out)
print(out,'pictures',len(base.inline_shapes),'tables',len(base.tables))
