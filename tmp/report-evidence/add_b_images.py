from pathlib import Path
import shutil
p=Path('tmp/report-evidence/build_report.py');s=p.read_text()
for source,name in [('codex-clipboard-f4a8a83b-109d-4c59-aa72-04de1e30b268.png','b-triage.png'),('codex-clipboard-3bbcc442-139d-45f8-8b8b-7f68a7c4fe06.png','b-board.png')]:
 shutil.copy2(Path('/var/folders/n3/qqjc_tk90qn8tzy1mvjvm7w80000gn/T')/source,Path('tmp/report-evidence')/name)
insert='''  page('15 Component B Evidence - Interface and Project Board')
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
'''
s=s.replace("  continue\n if component=='A':",insert+"  continue\n if component=='A':",1)
s=s.replace("'24 to 43'","'24 to 44'").replace("'44 to 46'","'45 to 47'")
p.write_text(s)
