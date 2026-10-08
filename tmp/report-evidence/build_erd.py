from pathlib import Path
import re, json
from reportlab.pdfgen import canvas
from reportlab.lib.colors import HexColor, white
from reportlab.lib.pagesizes import A2, landscape
from pypdf import PdfReader, PdfWriter
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'output/pdf'; OUT.mkdir(exist_ok=True,parents=True)
ctx=(ROOT/'backend-api/Data/ApplicationDbContext.cs').read_text()
pairs=re.findall(r'DbSet<(\w+)>\s+(\w+)',ctx)
scalar=r'(Guid\??|string\??|DateTime\??|DateOnly\??|TimeOnly\??|int\??|bool\??|decimal\??)'
fields={table:re.findall(r'public\s+'+scalar+r'\s+(\w+)\s*\{\s*get;', (ROOT/f'backend-api/Models/{model}.cs').read_text()) for model,table in pairs}
# Principal, dependent, foreign key, optional principal, delete behavior.
rels=[('PatientProfiles','Admissions','PatientProfileId',False,'Cascade'),('Wards','Admissions','WardId',False,'Cascade'),('PatientProfiles','TriageRecords','PatientId',False,'Restrict'),('TriageAttachments','TriageRecords','AttachmentId',True,'SetNull'),('TriageRecords','TriageAttachments','TriageRecordId',True,'SetNull'),('TriageRecords','AgentWorkflows','TriageRecordId',False,'Cascade'),('TriageRecords','TriageReviewHistories','TriageRecordId',False,'Cascade'),('AgentWorkflows','TriageReviewHistories','AgentWorkflowStateId',True,'SetNull'),('Doctors','DoctorAvailabilities','DoctorId',False,'Cascade'),('Doctors','Appointments','DoctorId',False,'Restrict'),('PatientProfiles','Appointments','PatientId',False,'Restrict'),('PatientProfiles','Prescriptions','PatientId',False,'Restrict'),('TriageRecords','Prescriptions','TriageRecordId',False,'Restrict'),('Prescriptions','PrescriptionItems','PrescriptionId',False,'Cascade'),('Medicines','PrescriptionItems','MedicineId',False,'Restrict')]
fks={(d,f):i+1 for i,(p,d,f,o,b) in enumerate(rels)}
refs={('Users','DoctorId'),('Users','PatientProfileId'),('TriageRecords','AssignedDoctorId'),('TriageRecords','TentativeAppointmentId'),('TriageReviewHistories','AssignedDoctorId'),('TriageAttachments','UploaderId'),('Appointments','ApprovedByDoctorId'),('Prescriptions','IssuedByDoctorId')}
pos={'Admissions':(70,110),'Wards':(70,330),'PatientProfiles':(480,110),'TriageRecords':(890,110),'AgentWorkflows':(1300,110),'Appointments':(70,490),'Doctors':(70,790),'DoctorAvailabilities':(70,990),'Prescriptions':(480,490),'PrescriptionItems':(480,930),'Medicines':(890,860),'TriageAttachments':(890,490),'TriageReviewHistories':(1300,490),'Users':(1300,860)}
W,H=landscape(A2); BW=300
navy=HexColor('#17324D'); blue=HexColor('#356D99'); gray=HexColor('#526476'); amber=HexColor('#9A5D09')
component={**{t:'#147D78' for t in ['Admissions','Wards','PatientProfiles']},**{t:'#355FA0' for t in ['TriageRecords','AgentWorkflows','TriageReviewHistories','TriageAttachments']},**{t:'#76569C' for t in ['Doctors','DoctorAvailabilities','Appointments']},**{t:'#A96A1E' for t in ['Prescriptions','PrescriptionItems','Medicines']},'Users':'#526476'}
def height(t): return 38+16*len(fields[t])
def make(path,report=False):
 c=canvas.Canvas(str(path),pagesize=(W,H));c.setTitle('CareFlow AI - complete entity relationship diagram');c.setAuthor('Group SE_039')
 def text(x,y,s,size=12,color=navy,bold=False):
  c.setFillColor(color);c.setFont('Helvetica-Bold' if bold else 'Helvetica',size);c.drawString(x,H-y,s)
 text(70,42,'5 Database Design | Complete Entity Relationship Diagram' if report else 'CareFlow AI | Complete Entity Relationship Diagram',25,bold=True)
 text(70,68,'14 mapped tables | 15 foreign-key relationships | Current EF Core model and migration snapshot | 5 October 2026',13)
 def edge(i,points):
  p,d,f,opt,delete=rels[i-1];c.setStrokeColor(blue);c.setLineWidth(1.5)
  q=c.beginPath();q.moveTo(points[0][0],H-points[0][1])
  for x,y in points[1:]:q.lineTo(x,H-y)
  c.drawPath(q)
  # Cardinality at each endpoint: number of principals per dependent, dependents per principal.
  for point,near,label in [(points[0],points[1],'0..1' if opt else '1'),(points[-1],points[-2],'0..*')]:
   x,y=point;dx=near[0]-x;dy=near[1]-y
   xx=x+(8 if dx>0 else -32 if dx<0 else 8); yy=y+(18 if dy>0 else -8 if dy<0 else -7)
   text(xx,yy,label,11,blue,True)
  # Key labels refer to the exact dependent field in the entity table.
  a,b=max(zip(points,points[1:]),key=lambda z:abs(z[0][0]-z[1][0])+abs(z[0][1]-z[1][1]))
  x=(a[0]+b[0])/2;y=(a[1]+b[1])/2
  c.setFillColor(white);c.rect(x-14,H-y-7,28,15,fill=1,stroke=0);text(x-11,y+3,f'F{i:02}',9,blue,True)
 edge(1,[(480,165),(370,165)])
 edge(2,[(220,330),(220,110+height('Admissions'))])
 edge(3,[(780,170),(890,170)])
 edge(4,[(1100,490),(1100,110+height('TriageRecords'))])
 edge(5,[(980,110+height('TriageRecords')),(980,490)])
 edge(6,[(1190,170),(1300,170)])
 edge(7,[(1190,270),(1250,270),(1250,570),(1300,570)])
 edge(8,[(1450,110+height('AgentWorkflows')),(1450,490)])
 edge(9,[(220,790+height('Doctors')),(220,990)])
 edge(10,[(220,790),(220,490+height('Appointments'))])
 edge(11,[(480,240),(425,240),(425,550),(370,550)])
 edge(12,[(630,110+height('PatientProfiles')),(630,490)])
 edge(13,[(890,270),(835,270),(835,550),(780,550)])
 edge(14,[(630,490+height('Prescriptions')),(630,930)])
 edge(15,[(890,975),(780,975)])
 types={'Guid':'uuid','string':'text','DateTime':'timestamp','DateOnly':'date','TimeOnly':'time','int':'integer','bool':'boolean','decimal':'numeric(10,2)'}
 for table,fs in fields.items():
  x,y=pos[table];h=height(table);color=HexColor(component[table]);c.setFillColor(white);c.setStrokeColor(color);c.setLineWidth(1)
  c.rect(x,H-y-h,BW,h,fill=1,stroke=1);c.setFillColor(color);c.rect(x,H-y-28,BW,28,fill=1,stroke=0)
  text(x+10,y+19,table,15,white,True)
  for k,(typ,name) in enumerate(fs):
   yy=y+44+16*k
   tag='PK' if name=='Id' else f'F{fks[(table,name)]:02}' if (table,name) in fks else 'REF' if (table,name) in refs else ''
   col=amber if tag=='REF' else navy
   text(x+8,yy,tag,9,col,True);text(x+41,yy,name,11,col)
   t=types[typ.rstrip('?')]+('?' if typ.endswith('?') else '')
   c.setFillColor(gray);c.setFont('Helvetica',9);c.drawRightString(x+BW-8,H-yy,t)
 text(890,713,'Reading the diagram',16,bold=True)
 lines=['PK = primary key. F01-F15 = foreign keys; match each labelled connector.',
 '1 = exactly one; 0..1 = optional; 0..* = zero or many. ? = nullable column.',
 'REF = application reference only; no foreign key is configured in this model.',
 'Users therefore has no enforced relationship lines. REF fields are not foreign keys.',
 'The two attachment links are independent many-to-one mappings, not one-to-one.',
 'All scalar entity properties are shown. Navigation properties are excluded.']
 for j,line in enumerate(lines):text(890,735+19*j,line,12,gray)
 text(70,1145,'Source: backend-api/Models/*.cs; Data/ApplicationDbContext.cs; Migrations/ApplicationDbContextModelSnapshot.cs.',12)
 text(70,1164,'Code-derived schema; live database migration state not verified. A = teal | B = blue | C = purple | D = amber | Authentication = grey.',11,gray)
 if report:text(W-90,1164,'8',11,gray)
 c.save()
make(OUT/'CareFlow_AI_Complete_ERD.pdf')
make(ROOT/'tmp/report-evidence/erd-report-page.pdf',True)
# Keep a portable editable ERD source alongside the PDF.
mer=['erDiagram']
for table,fs in fields.items():
 mer.append('    '+table+' {')
 for typ,name in fs:
  tag=' PK' if name=='Id' else ' FK' if (table,name) in fks else ''
  comment='nullable' if typ.endswith('?') else ''
  if (table,name) in refs:comment+=('; ' if comment else '')+'application reference, not FK'
  mer.append(f'        {typ.rstrip("?")} {name}{tag}'+(f' "{comment}"' if comment else ''))
 mer.append('    }')
for i,(p,d,f,opt,delete) in enumerate(rels,1):mer.append(f'    {p} '+('|o' if opt else '||')+f'--o{{ {d} : "F{i:02} {f}"')
(OUT/'CareFlow_AI_Complete_ERD.mmd').write_text('\n'.join(mer)+'\n')
report=OUT/'SE3090_SE039_Consolidated_Report_Draft.pdf'
reader=PdfReader(report);writer=PdfWriter();replacement=PdfReader(ROOT/'tmp/report-evidence/erd-report-page.pdf').pages[0]
assert len(reader.pages)>=34
for i,page in enumerate(reader.pages):writer.add_page(replacement if i==7 else page)
writer.add_metadata({'/Title':'CareFlow AI Consolidated Report - Review Copy','/Author':'Group SE_039'})
with report.open('wb') as f:writer.write(f)
print(f'Created complete ERD: {len(fields)} tables, {sum(map(len,fields.values()))} columns, {len(rels)} FK relationships. Replaced report page 8.')
