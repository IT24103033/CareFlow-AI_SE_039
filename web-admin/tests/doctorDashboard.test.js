import test from 'node:test';
import assert from 'node:assert/strict';
import { summarizeDashboard, dashboardCsv } from '../src/services/doctorDashboard.js';
const data = { generatedAt: '2026-10-16T10:00:00Z', groups: [
  { status: 'InReview', severity: 'High', count: 2 },
  { status: 'Approved', severity: 'Critical', count: 3 },
  { status: 'AssessmentFailed', severity: 'Unassessed', count: 1 },
], trend: [{ date: '2026-10-16', count: 2 }] };
test('counts urgent cases only while awaiting review, retains failed assessments', () => {
  const result = summarizeDashboard(data);
  assert.equal(result.total, 6);
  assert.equal(result.urgentReview, 2);
  assert.equal(result.statuses.AssessmentFailed, 1);
  assert.equal(result.severities.Critical, 3);
});
test('empty dashboard exports zero counts and rejects invalid responses', () => {
  const summary = summarizeDashboard({ ...data, groups: [] });
  assert.equal(summary.total, 0);
  assert.match(dashboardCsv(summary), /"Total submissions","0"/);
  assert.throws(() => summarizeDashboard({ ...data, groups: [{ count: -1 }] }));
  assert.throws(() => summarizeDashboard({ ...data, generatedAt: 'invalid' }));
});
test('CSV contains aggregate totals, escapes labels, and excludes unrelated patient fields', () => {
  const summary = summarizeDashboard({ ...data, patientName: 'Private Patient', groups: [...data.groups, { status: '=FORMULA("x")', severity: 'Low', count: 1 }] });
  const csv = dashboardCsv(summary);
  assert.ok(csv.startsWith('\uFEFF'));
  assert.match(csv, /"Total submissions","7"/);
  assert.ok(csv.includes(`"'=FORMULA(""x"")"`));
  assert.ok(!csv.includes('Private Patient'));
});
