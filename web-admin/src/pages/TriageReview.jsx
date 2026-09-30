import { useEffect, useState, useMemo } from 'react';
import { createTriageApi } from '../services/triageApi';
import { useAuth } from '../context/AuthContext';
import './TriageReview.css';
import { readPlan } from '../services/triagePlan';

const statuses = ['Pending', 'InReview', 'Approved', 'Rejected', 'RevisionRequested', 'AssessmentFailed', 'ReassessmentInProgress'];
const label = (value) => ({ InReview: 'Awaiting review', RevisionRequested: 'Revision requested', AssessmentFailed: 'Assessment unavailable' }[value] || value || 'Not available');
const date = (value) => value ? new Date(value).toLocaleString() : 'Not available';
function Badge({ value }) {
  return <span className={`triage-badge badge-${value || 'unknown'}`}>{label(value)}</span>;
}

export default function TriageReview({ api }) {
  const { getAccessToken } = useAuth();
  const activeApi = useMemo(() => api || createTriageApi(getAccessToken), [api, getAccessToken]);

  const [filters, setFilters] = useState({ search: '', status: 'InReview', severity: '', sort: 'urgency', page: 1, pageSize: 10 });
  const [search, setSearch] = useState('');
  const [queue, setQueue] = useState({ items: [], total: 0 });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [selected, setSelected] = useState(null);
  const [refresh, setRefresh] = useState(0);
  const [notice, setNotice] = useState('');
  useEffect(() => {
    const controller = new AbortController();
    activeApi.list(filters, controller.signal).then(data => {
      if (!controller.signal.aborted) { setQueue(data); setLoading(false); }
    }).catch(err => {
      if (!controller.signal.aborted) { setError(err.message); setLoading(false); }
    });
    return () => controller.abort();
  }, [activeApi, filters, refresh]);
  function updateFilters(changes) {
    setLoading(true); setError(''); setSelected(null); setNotice('');
    setFilters(current => ({ ...current, page: 1, ...changes }));
  }
  function reload() { setLoading(true); setError(''); setRefresh(value => value + 1); }
  const pages = Math.max(1, Math.ceil(queue.total / filters.pageSize));
  return <main className="triage-page">
    <header className="triage-heading">
      <div><p className="triage-eyebrow">CLINICAL WORKSPACE</p><h1>Triage review</h1><p>Review patient submissions and record your clinical decision.</p></div>
      <button onClick={reload} disabled={loading}>Refresh queue</button>
    </header>
    {notice && <p className="triage-notice" role="status">{notice}</p>}
    <form className="triage-filters" onSubmit={event => { event.preventDefault(); updateFilters({ search: search.trim() }); }}>
      <label className="triage-search">Search patient or symptoms<input value={search} maxLength={100} onChange={event => setSearch(event.target.value)} placeholder="Patient name or symptom" /></label>
      <button type="submit">Search</button>
      <label>Status<select value={filters.status} onChange={event => updateFilters({ status: event.target.value })}>
        <option value="">All statuses</option>{statuses.map(value => <option key={value} value={value}>{label(value)}</option>)}
      </select></label>
      <label>Urgency<select value={filters.severity} onChange={event => updateFilters({ severity: event.target.value })}>
        <option value="">All urgency levels</option>{['Critical', 'High', 'Medium', 'Low', 'Unassessed'].map(value => <option key={value}>{value}</option>)}
      </select></label>
      <label>Sort<select value={filters.sort} onChange={event => updateFilters({ sort: event.target.value })}>
        <option value="urgency">Urgency first</option><option value="newest">Newest first</option><option value="oldest">Oldest first</option>
      </select></label>
    </form>
    <div className="triage-layout">
      <section className="triage-panel" aria-label="Triage queue" aria-busy={loading}>
        <h2>Patient submissions</h2>
        {loading ? <p role="status">Loading submissions…</p> : error ? <div role="alert"><p>{error}</p><button onClick={reload}>Retry</button></div>
          : queue.items.length === 0 ? <div className="triage-empty"><h3>No matching submissions</h3><p>Try changing your filters or refresh after a new submission.</p></div>
          : <ul className="triage-cases">{queue.items.map(record => <li key={record.id}>
            <button className={`triage-case ${selected === record.id ? 'selected' : ''}`} aria-pressed={selected === record.id} onClick={() => setSelected(record.id)}>
              <strong>{record.patientName || 'Patient'}</strong><span className="triage-chips"><Badge value={record.severityLevel} /><Badge value={record.triageStatus} /></span>
              <span className="triage-preview">{record.symptoms}</span><small>Submitted {date(record.createdAt)}</small>
            </button>
          </li>)}</ul>}
        {!loading && !error && <footer className="triage-pagination">
          <span>{queue.total} case{queue.total === 1 ? '' : 's'} · Page {filters.page} of {pages}</span>
          <button disabled={filters.page <= 1} onClick={() => updateFilters({ page: filters.page - 1 })}>Previous</button>
          <button disabled={filters.page >= pages} onClick={() => updateFilters({ page: filters.page + 1 })}>Next</button>
        </footer>}
      </section>
      {selected ? <CaseDetail key={selected} id={selected} api={activeApi} onDecision={decision => {
        setNotice(`${label(decision)} recorded. This decision does not confirm an appointment.`); reload();
      }} /> : <section className="triage-panel triage-empty"><h2>Select a submission</h2><p>Patient details, the AI assessment and review actions will appear here.</p></section>}
    </div>
  </main>;
}

function CaseDetail({ id, api, onDecision }) {
  const [record, setRecord] = useState(null);
  const [error, setError] = useState('');
  const [conflict, setConflict] = useState(false);
  const [notes, setNotes] = useState('');
  const [decision, setDecision] = useState('Approved');
  const [saving, setSaving] = useState(false);
  const [refresh, setRefresh] = useState(0);
  useEffect(() => {
    const controller = new AbortController();
    api.detail(id, controller.signal).then(data => { if (!controller.signal.aborted) setRecord(data); })
      .catch(err => { if (!controller.signal.aborted) setError(err.message); });
    return () => controller.abort();
  }, [api, id, refresh]);
  const plan = readPlan(record?.aiPlan);
  const ready = record?.triageStatus === 'InReview' && record?.aiAgentStatus === 'Completed' && record?.approvalStatus === 'Pending' && plan;
  function reload() { setRecord(null); setError(''); setConflict(false); setRefresh(value => value + 1); }
  async function submit(event) {
    event.preventDefault();
    if (!ready || saving || conflict) return;
    if (decision !== 'Approved' && !notes.trim()) { setError('Provide a reason for rejection or revision.'); return; }
    setSaving(true); setError('');
    try {
      const updated = await api.review(id, { decision, doctorNotes: notes.trim() || null, expectedUpdatedAt: record.updatedAt });
      setRecord(updated); setNotes(''); onDecision(decision);
    } catch (err) { setError(err.message); setConflict(err.status === 409 || err.status === 401 || err.status === 403); }
    finally { setSaving(false); }
  }
  return <section className="triage-panel triage-detail" aria-label="Case details">
    <div className="triage-detail-heading"><h2>Case details</h2><button onClick={reload} disabled={saving}>Reload case</button></div>
    {error && <p className="triage-error" role="alert">{error}</p>}
    {!record ? <p role="status">{error ? 'Case could not be loaded.' : 'Loading case…'}</p> : <>
      <h3>{record.patientName || 'Patient'}</h3><p className="triage-id">Case {record.id}</p>
      <div className="triage-chips"><Badge value={record.severityLevel} /><Badge value={record.triageStatus} /></div>
      <h3>Symptoms</h3>
      <p className="triage-text">{record.symptoms}</p>
      {record.imageUrl && (
        <div style={{ marginTop: '12px', marginBottom: '20px' }}>
          <h4>Attached Image</h4>
          <img src={record.imageUrl} alt="Triage attachment" style={{ maxWidth: '100%', maxHeight: '400px', borderRadius: '8px', border: '1px solid var(--border)' }} />
        </div>
      )}
      <h3>AI assessment</h3>
      <p className="triage-muted">Decision support — review the assessment before recording a decision.</p>
      {plan ? <dl><dt>Suggested specialty</dt><dd>{plan.suggestedspecialist}</dd><dt>Recommended action</dt><dd>{plan.recommendedaction}</dd><dt>Assessment summary</dt><dd>{plan.rationale}</dd><dt>Analysis method</dt><dd>{record.analysisMethod || plan.analysismethod || 'Not recorded'}</dd></dl>
        : <p>No valid assessment is available. This case cannot be reviewed yet.</p>}
      {plan?.warnings?.length > 0 && <div className="triage-warning"><h3>Review considerations</h3><ul>{plan.warnings.map((warning, index) => <li key={index}>{warning}</li>)}</ul></div>}
      {plan?.steps?.length > 0 && <><h3>Proposed next steps</h3>
        <p className="triage-muted">These steps are a plan, not evidence that the other services have executed them.</p>
        <ol className="triage-plan-steps">{plan.steps.map(step => <li key={step.id}>
          <strong>{{ CheckEmergencyRules: 'Safety validation', GetAvailableSlots: 'Find an available appointment',
            CreateTentativeAppointment: 'Hold an appointment slot', ReviewTriage: 'Doctor review',
            ConfirmAppointment: 'Confirm the appointment', SendAppointmentNotification: 'Send confirmation' }[step.tool] || step.tool}</strong>
          <span> · {step.status}{step.requireshumanapproval ? ' · Approval required' : ''}</span>
        </li>)}</ol></>}
      {record.planningExecution && <details className="triage-execution"><summary>Planning execution summary</summary>
        <p>Status: {record.planningExecution.status} · Model attempts: {record.planningExecution.modelAttempts}</p>
        {record.planningExecution.failureCode && <p className="triage-error">Assessment unavailable. No urgency was assigned. Staff follow-up is required.</p>}
        <ul>{record.planningExecution.events.map((event, index) => <li key={index}>{event.operation}: {event.outcome} ({event.durationMs} ms)</li>)}</ul>
      </details>}
      <dl><dt>Agent status</dt><dd>{label(record.aiAgentStatus)}</dd><dt>Approval status</dt><dd>{label(record.approvalStatus)}</dd><dt>Last updated</dt><dd>{date(record.updatedAt)}</dd></dl>
      {record.reviewHistories?.length > 0 && <div className="triage-history">
        <h3>Review history</h3>
        <div className="triage-history-list">
          {record.reviewHistories.map(h => (
            <div key={h.id} className="triage-history-item" style={{ borderLeft: '3px solid #cbd5e1', paddingLeft: '12px', marginBottom: '16px' }}>
              <div className="triage-history-header" style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '4px' }}>
                <strong>{label(h.action)}</strong>
                <span className="triage-muted">{date(h.createdAt)}</span>
              </div>
              {h.reviewerId && <p style={{ margin: '4px 0', fontSize: '0.85rem', color: '#64748b' }}>Reviewer ID: {h.reviewerId}</p>}
              {h.linkedAttemptId && <p style={{ margin: '4px 0', fontSize: '0.85rem', color: '#64748b' }}>Linked Attempt: {h.linkedAttemptId}</p>}
              {h.notes && <p style={{ margin: '4px 0', fontSize: '0.95rem' }}><strong>Notes:</strong> {h.notes}</p>}
              <p className="triage-muted" style={{ margin: '4px 0', fontSize: '0.85rem' }}>Symptoms at review: {h.symptomsAtReview.length > 80 ? h.symptomsAtReview.substring(0, 80) + '...' : h.symptomsAtReview}</p>
              {h.previousPlan && (
                <details style={{ marginTop: '8px', fontSize: '0.85rem' }}>
                  <summary>Previous Plan</summary>
                  <pre style={{ whiteSpace: 'pre-wrap', background: '#f1f5f9', padding: '8px', borderRadius: '4px', marginTop: '4px' }}>
                    {h.previousPlan}
                  </pre>
                </details>
              )}
            </div>
          ))}
        </div>
      </div>}
      {record.doctorNotes && record.reviewHistories?.length === 0 && <><h3>Recorded review notes</h3><p className="triage-text">{record.doctorNotes}</p></>}
      {ready ? <form className="triage-review-form" onSubmit={submit}>
        <h3>Record a decision</h3><label>Decision<select value={decision} disabled={saving || conflict} onChange={event => setDecision(event.target.value)}>
          <option value="Approved">Approve assessment</option><option value="Rejected">Reject assessment</option><option value="RevisionRequested">Request revision</option>
        </select></label>
        <label>Review notes {decision === 'Approved' ? '(optional)' : '(required)'}<textarea value={notes} onChange={event => setNotes(event.target.value)} maxLength={2000} required={decision !== 'Approved'} disabled={saving || conflict} rows={4} /></label>
        <p className="triage-muted">Approval records acceptance of the assessment. Appointment confirmation is a separate workflow step.</p>
        {decision === 'RevisionRequested' && <p className="triage-muted">Revision pauses this case for follow-up; it does not automatically rerun the agent.</p>}
        <button className="triage-primary" disabled={saving || conflict}>{saving ? 'Saving decision…' : 'Save decision'}</button>
      </form> : <p className="triage-muted">This case is not awaiting a review decision.</p>}
    </>}
  </section>;
}
