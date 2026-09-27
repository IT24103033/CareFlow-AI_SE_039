import { test } from 'node:test';
import assert from 'node:assert/strict';
import { createTriageApi } from '../src/services/triageApi.js';
import { readPlan } from '../src/services/triagePlan.js';

const plan = { SuggestedSpecialist: 'General Practitioner', UrgencyLevel: 'Medium', RecommendedAction: 'Review case', Rationale: 'Test assessment' };
test('invalid assessment payloads cannot be displayed as valid recommendations', () => {
  for (const payload of [null, '{}', 'null', '[]', 'invalid', JSON.stringify({ ...plan, UrgencyLevel: 'Unknown' }), JSON.stringify({ ...plan, Rationale: '' })]) {
    assert.equal(readPlan(payload), null);
  }
  assert.equal(readPlan(JSON.stringify(plan)).suggestedspecialist, 'General Practitioner');
  assert.equal(readPlan(JSON.stringify({ ...plan, SuggestedSpecialist: 123 })), null);
});

test('queue encodes search and filters, pagination, token and abort signal', async (t) => {
  const signal = new AbortController().signal;
  t.mock.method(globalThis, 'fetch', async (url, options) => {
    const parsed = new URL(url);
    assert.equal(parsed.pathname, '/api/triage/review-queue');
    assert.equal(parsed.searchParams.get('search'), 'A & B');
    assert.equal(parsed.searchParams.get('status'), 'InReview');
    assert.equal(parsed.searchParams.get('page'), '2');
    assert.equal(parsed.searchParams.has('severity'), false);
    assert.equal(options.headers.Authorization, 'Bearer test-token');
    assert.equal(options.signal, signal);
    return new Response(JSON.stringify({ items: [], total: 0 }), { status: 200 });
  });
  const result = await createTriageApi(() => 'test-token').list({ search: 'A & B', status: 'InReview', severity: '', page: 2 }, signal);
  assert.deepEqual(result, { items: [], total: 0 });
});

test('review sends case version and decision without a browser-supplied reviewer', async (t) => {
  const decision = { decision: 'RevisionRequested', doctorNotes: 'Need more context', expectedUpdatedAt: '2026-09-27T07:00:00.123456Z' };
  t.mock.method(globalThis, 'fetch', async (url, options) => {
    assert.equal(new URL(url).pathname, '/api/triage/case-1/review');
    assert.equal(options.method, 'PATCH');
    assert.deepEqual(JSON.parse(options.body), decision);
    assert.equal(options.headers.Authorization, undefined);
    return new Response(JSON.stringify({ triageStatus: 'RevisionRequested' }));
  });
  assert.equal((await createTriageApi().review('case-1', decision)).triageStatus, 'RevisionRequested');
});

test('staff detail uses the protected staff route', async (t) => {
  t.mock.method(globalThis, 'fetch', async (url) => {
    assert.equal(new URL(url).pathname, '/api/triage/review-queue/case-1');
    return new Response(JSON.stringify({ id: 'case-1' }));
  });
  assert.equal((await createTriageApi().detail('case-1')).id, 'case-1');
});

for (const [status, text, expected] of [
  [401, 'Unauthorized', /Sign in with a doctor account/],
  [403, 'Forbidden', /does not have permission/],
  [409, 'Another reviewer changed this case.', /Another reviewer/],
  [400, JSON.stringify({ errors: { DoctorNotes: ['Provide a reason.'] } }), /Provide a reason/],
]) {
  test(`HTTP ${status} retains actionable error and status`, async (t) => {
    t.mock.method(globalThis, 'fetch', async () => new Response(text, { status }));
    await assert.rejects(createTriageApi().detail('case-1'), error => error.status === status && expected.test(error.message));
  });
}

test('network failure is propagated instead of presenting an empty queue', async (t) => {
  t.mock.method(globalThis, 'fetch', async () => { throw new TypeError('Network unavailable'); });
  await assert.rejects(createTriageApi().list({}), /Network unavailable/);
});

test('versioned plan exposes pending steps but rejects incomplete or failed planning', () => {
  const structured = { ...plan, SchemaVersion: 2, Warnings: [],
    Execution: { Status: 'Completed', FailureCode: null },
    Steps: [{ Id: 'safety', Agent: 'SafetyAgent', Tool: 'CheckEmergencyRules', DependsOn: [], RequiresHumanApproval: false, Status: 'Pending' }] };
  assert.equal(readPlan(JSON.stringify(structured)).steps[0].tool, 'CheckEmergencyRules');
  for (const invalid of [
    { ...structured, SchemaVersion: 99 }, { ...structured, Steps: null },
    { ...structured, Steps: [null] }, { ...structured, Warnings: [42] },
    { ...structured, Execution: { Status: 'Failed' } },
    { ...structured, Steps: [{ ...structured.Steps[0], Status: 'Completed' }] },
  ]) assert.equal(readPlan(JSON.stringify(invalid)), null);
});
