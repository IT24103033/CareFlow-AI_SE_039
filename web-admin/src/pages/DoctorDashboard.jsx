import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { apiFetch } from '../services/api';
import { summarizeDashboard, dashboardCsv, statusLabels } from '../services/doctorDashboard';
import './DoctorDashboard.css';

export default function DoctorDashboard() {
  const [summary, setSummary] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [refresh, setRefresh] = useState(0);
  const [downloadMessage, setDownloadMessage] = useState('');
  useEffect(() => {
    const controller = new AbortController();
    let timedOut = false;
    setLoading(true); setError(''); setSummary(null); setDownloadMessage('');
    const timer = setTimeout(() => { timedOut = true; controller.abort(); }, 60000);
    (async () => {
      try {
        const response = await apiFetch('/api/triage/dashboard', { signal: controller.signal });
        if (!response.ok) throw new Error(response.status === 403 ? 'Only doctors can access this dashboard.' : response.status === 401 ? 'Your session expired. Please sign in again.' : 'Unable to load dashboard statistics. Please try again.');
        const data = summarizeDashboard(await response.json());
        if (!controller.signal.aborted) setSummary(data);
      } catch (err) {
        if (!controller.signal.aborted || timedOut) setError(timedOut ? 'The server is taking too long. Please retry in a moment.' : err.message);
      } finally {
        clearTimeout(timer);
        if (!controller.signal.aborted || timedOut) setLoading(false);
      }
    })();
    return () => { clearTimeout(timer); controller.abort(); };
  }, [refresh]);

  function download() {
    if (!summary) return;
    const url = URL.createObjectURL(new Blob([dashboardCsv(summary)], { type: 'text/csv;charset=utf-8;' }));
    const anchor = document.createElement('a');
    anchor.href = url; anchor.download = `careflow-doctor-summary-${summary.generatedAt.slice(0, 10)}.csv`;
    document.body.appendChild(anchor); anchor.click(); anchor.remove();
    setTimeout(() => URL.revokeObjectURL(url), 1000);
    setDownloadMessage('Summary download started. It contains aggregate statistics only.');
  }
  const cards = summary ? [
    ['Total submissions', summary.total, 'All-time shared review queue'],
    ['Awaiting review', summary.statuses.InReview, 'Ready for a doctor’s decision'],
    ['Urgent reviews', summary.urgentReview, 'High or critical cases awaiting review'],
    ['Assessment unavailable', summary.statuses.AssessmentFailed, 'Cases needing follow-up'],
  ] : [];
  const peak = summary ? Math.max(1, ...summary.trend.map(day => day.count)) : 1;
  return <main className="doctor-overview">
    <header className="doctor-overview-header">
      <div><p className="doctor-eyebrow">CLINICAL WORKSPACE</p><h1>Doctor dashboard</h1><p>A quick overview of the shared triage queue.</p></div>
      <div className="doctor-actions"><button disabled={loading} onClick={() => setRefresh(n => n + 1)}>{loading ? 'Refreshing…' : 'Refresh'}</button><button className="doctor-primary" disabled={loading || !summary} onClick={download}>Download summary CSV</button></div>
    </header>
    <nav className="doctor-shortcuts" aria-label="Doctor quick actions"><Link to="/doctor/triage">Open triage review →</Link><Link to="/doctor/prescriptions">Manage prescriptions →</Link></nav>
    <p role="status" className="doctor-download-message">{downloadMessage}</p>
    {loading ? <section className="doctor-panel" role="status">Loading dashboard statistics…</section>
      : error ? <section className="doctor-panel doctor-error" role="alert"><h2>Dashboard unavailable</h2><p>{error}</p><button onClick={() => setRefresh(n => n + 1)}>Retry</button></section>
      : summary && <>
        <p className="doctor-updated">Updated {new Date(summary.generatedAt).toLocaleString()} · All-time totals · Daily chart uses UTC</p>
        {summary.total === 0 && <section className="doctor-panel"><h2>No submissions yet</h2><p>Statistics will appear after patients submit their symptoms.</p></section>}
        <section className="doctor-stat-grid" aria-label="Case statistics">{cards.map(([title, value, description]) => <article className="doctor-stat" key={title}><h2>{title}</h2><strong>{value}</strong><p>{description}</p></article>)}</section>
        <div className="doctor-detail-grid">
          <section className="doctor-panel"><h2>Submissions over seven days</h2><p>Number of new cases per UTC day, including today.</p><div className="doctor-trend">{summary.trend.map(day => <div className="doctor-trend-column" key={day.date}><strong>{day.count}</strong><div className="doctor-bar-track" aria-hidden="true"><div style={{ height: `${day.count / peak * 100}%` }} /></div><time dateTime={day.date}>{day.date.slice(5)}</time></div>)}</div></section>
          <section className="doctor-panel"><h2>Case status</h2><dl className="doctor-breakdown">{Object.entries(summary.statuses).map(([key, count]) => <div key={key}><dt>{statusLabels[key] || key}</dt><dd>{count}</dd></div>)}</dl></section>
          <section className="doctor-panel"><h2>Urgency distribution</h2><p>All submissions, including completed cases.</p><dl className="doctor-breakdown">{Object.entries(summary.severities).map(([key, count]) => <div key={key}><dt>{key}</dt><dd>{count}</dd></div>)}</dl></section>
          <section className="doctor-panel doctor-follow-up"><h2>Next actions</h2><p>Review urgent cases first, follow up on unavailable assessments, and check patients’ revised submissions.</p><Link to="/doctor/triage">Go to the review queue →</Link><h3>About the download</h3><p>The CSV includes the counts shown here and seven days of submission totals. Patient identities, symptoms and clinical notes are excluded.</p></section>
        </div>
      </>}
  </main>;
}
