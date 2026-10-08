const fs = require('node:fs');
const path = require('node:path');
const os = require('node:os');
const {spawn} = require('node:child_process');
const assert = require('node:assert/strict');
const root = path.resolve(__dirname, '../..');
const cfg = JSON.parse(fs.readFileSync(path.join(__dirname,'cross-platform.local.json')));
const runId = 'M3-' + new Date().toISOString().replace(/[:.]/g,'-');
const out = path.join(root,'output/testing/member3/cross-platform',runId);
fs.mkdirSync(out,{recursive:true});
const report = {runId, startedAt:new Date().toISOString(), status:'Running', stages:[]};
const secretValues = [cfg.PATIENT_PASSWORD,cfg.DOCTOR_PASSWORD];
function scrub(s) { for(const x of secretValues) if(x) s=s.split(x).join('[REDACTED]');return s; }
function save() {fs.writeFileSync(path.join(out,'result.json'),JSON.stringify(report,null,2));}
async function api(route,token,body) {
 const r=await fetch(cfg.HOST_API_BASE_URL+route,{method:body?'POST':'GET',headers:{'Content-Type':'application/json',...(token?{Authorization:'Bearer '+token}:{})},...(body?{body:JSON.stringify(body)}:{}),signal:AbortSignal.timeout(30000)});
 if(!r.ok) throw Error(`${route}: HTTP ${r.status}`);
 return r.json();
}
function command(bin,args,cwd,log) {
 return new Promise((resolve,reject)=>{
 const p=spawn(bin,args,{cwd,env:process.env});let output='';
 const receive=b=>{const s=scrub(b.toString());output+=s;fs.appendFileSync(path.join(out,log),s);};
 p.stdout.on('data',receive);p.stderr.on('data',receive);p.on('error',reject);
 const deadline=setTimeout(()=>p.kill('SIGTERM'),12*60*1000);
 p.on('close',code=>{clearTimeout(deadline);code===0?resolve(output):reject(Error(`${log} failed with exit ${code}; see saved log`));});
 });
}
async function flutter(phase,extra={}) {
 const tmp=fs.mkdtempSync(path.join(os.tmpdir(),'careflow-m3-'));
 const file=path.join(tmp,'cross-platform.local.json');
 fs.writeFileSync(file,JSON.stringify({API_BASE_URL:cfg.API_BASE_URL,PATIENT_USERNAME:cfg.PATIENT_USERNAME,PATIENT_PASSWORD:cfg.PATIENT_PASSWORD,RUN_ID:runId,E2E_PHASE:phase,...extra}),{mode:0o600});
 try{return await command('flutter',['test','integration_test/member3_cross_platform_test.dart','-d',cfg.DEVICE_ID,'--dart-define-from-file='+file,'--reporter','expanded'],path.join(root,'mobile_app'),'flutter-'+phase+'.log');}
 finally {fs.unlinkSync(file);fs.rmdirSync(tmp);}
}
(async()=>{
 let browser;
 try {
 for(const key of ['PATIENT_USERNAME','PATIENT_PASSWORD','DOCTOR_USERNAME','DOCTOR_PASSWORD'])assert.ok(cfg[key],`Missing ${key}`);
 console.log('Checking doctor and creating/reusing tomorrow availability');
 const auth=await api('/api/auth/login',null,{username:cfg.DOCTOR_USERNAME,password:cfg.DOCTOR_PASSWORD});
 assert.equal(auth.user.role,'Doctor');assert.ok(auth.user.doctorId);
 const tomorrow=new Date(Date.now()+86400000).toISOString().slice(0,10);
 const slots=await api('/api/DoctorAvailability/'+auth.user.doctorId,auth.token);
 let available=slots.find(s=>s.date===tomorrow);
 if(!available)available=await api('/api/DoctorAvailability',auth.token,{doctorId:auth.user.doctorId,date:tomorrow,startTime:'09:00:00',endTime:'10:00:00'});
 assert.equal(available.specialization,'General Practitioner','Use a GP doctor matching the normal scenario');
 report.availability={id:available.id,doctorId:auth.user.doctorId,date:tomorrow};save();
 console.log('Running Flutter submission and slot selection');
 const output=await flutter('submit');
 const match=output.match(/M3_HANDOFF:(\{[^\r\n]+\})/);assert.ok(match,'Missing Flutter handoff');
 const handoff=JSON.parse(match[1]);report.handoff=handoff;report.stages.push({name:'Flutter submission and booking',status:'Passed'});save();
 assert.equal(handoff.doctorId,auth.user.doctorId,'Booked doctor must match reviewing test doctor');
 console.log('Running React doctor review through Playwright');
 const {chromium}=require(process.env.PLAYWRIGHT_MODULE || 'playwright');
 browser=await chromium.launch({channel:'chrome',headless:true});
 const page=await browser.newPage({viewport:{width:1440,height:1000}});page.setDefaultTimeout(30000);
 await page.goto(cfg.WEB_URL+'/login');
 await page.getByPlaceholder('Username or Email').fill(cfg.DOCTOR_USERNAME);
 await page.getByPlaceholder('Password',{exact:true}).fill(cfg.DOCTOR_PASSWORD);
 const loginResponse=page.waitForResponse(r=>r.url()===cfg.HOST_API_BASE_URL+'/api/auth/login'&&r.request().method()==='POST');
 await page.getByRole('button',{name:'Login',exact:true}).click();assert.equal((await loginResponse).status(),200);
 await page.waitForURL('**/doctor/dashboard');
 await page.goto(cfg.WEB_URL+'/doctor/triage');
 await page.getByPlaceholder('Patient name or symptom').fill(handoff.runId);
 await page.getByRole('button',{name:'Search',exact:true}).click();
 const row=page.locator('button.triage-case').filter({hasText:handoff.runId});await row.waitFor();assert.equal(await row.count(),1);await row.click();
 await page.getByText('Case '+handoff.caseId,{exact:true}).waitFor();
 await page.screenshot({path:path.join(out,'react-before-review.png'),fullPage:true});
 await page.getByLabel('Decision',{exact:true}).selectOption('Approved');
 await page.getByLabel('Review notes',{exact:false}).fill('Synthetic cross-platform test '+handoff.runId);
 const reviewResponse=page.waitForResponse(r=>r.url()===cfg.HOST_API_BASE_URL+`/api/triage/${handoff.caseId}/review`&&r.request().method()==='PATCH');
 await page.getByRole('button',{name:'Save decision',exact:true}).click();
 const reviewed=await reviewResponse;assert.equal(reviewed.status(),200);
 const data=await reviewed.json();assert.equal(data.id,handoff.caseId);assert.equal(data.triageStatus,'Approved');assert.equal(data.tentativeAppointmentId,handoff.appointmentId);
 await page.getByRole('status').filter({hasText:'Approved.'}).waitFor();
 await page.screenshot({path:path.join(out,'react-after-review.png'),fullPage:true});
 fs.writeFileSync(path.join(out,'review-result.json'),JSON.stringify({id:data.id,triageStatus:data.triageStatus,approvalStatus:data.approvalStatus,tentativeAppointmentId:data.tentativeAppointmentId},null,2));
 report.stages.push({name:'React review of same case',status:'Passed'});save();await browser.close();browser=null;
 console.log('Running Flutter confirmation verification');
 const verified=await flutter('verify',{CASE_ID:handoff.caseId,APPOINTMENT_ID:handoff.appointmentId});assert.ok(verified.includes('M3_VERIFIED:'));
 report.stages.push({name:'Flutter confirmed appointment display and API identity checks',status:'Passed'});report.status='Passed';
 }catch(e){report.status='Failed';report.error=scrub(e.message);console.error(report.error);process.exitCode=1;}
 finally{if(browser)await browser.close();report.finishedAt=new Date().toISOString();save();console.log('Evidence: '+out);}
})();
