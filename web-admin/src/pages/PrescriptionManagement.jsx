import React, { useState, useEffect, useCallback } from 'react';

const API_BASE = 'http://localhost:5241/api';

// ─── Helpers ──────────────────────────────────────────────────────────────────
const statusColor = { Draft: '#fa8c16', Issued: '#1890ff', Dispensed: '#52c41a', Cancelled: '#ff4d4f' };
const aiColor     = { Safe: '#52c41a', Warning: '#fa8c16', Blocked: '#ff4d4f' };
const aiIcon      = { Safe: '✅', Warning: '⚠️', Blocked: '🚫' };

const Badge = ({ label, colorMap }) => {
  const c = colorMap[label] || '#999';
  return (
    <span style={{
      display: 'inline-block', padding: '2px 10px', borderRadius: 12,
      fontSize: 11, fontWeight: 700, background: c + '22', color: c, border: `1px solid ${c}55`
    }}>{label}</span>
  );
};

// ─── Main Component ───────────────────────────────────────────────────────────
const PrescriptionManagement = () => {
  const [prescriptions, setPrescriptions] = useState([]);
  const [medicines, setMedicines]         = useState([]);
  const [patients, setPatients]           = useState([]);
  const [total, setTotal]                 = useState(0);
  const [loading, setLoading]             = useState(true);
  const [error, setError]                 = useState(null);
  const [selected, setSelected]           = useState(null); // expanded row detail

  // Query state
  const [search, setSearch]   = useState('');
  const [status, setStatus]   = useState('');
  const [aiStatus, setAiStatus] = useState('');
  const [page, setPage]       = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const pageSize = 8;

  // Create prescription modal
  const [showCreate, setShowCreate] = useState(false);
  const [creating, setCreating]     = useState(false);
  const [createResult, setCreateResult] = useState(null);
  const [newPx, setNewPx] = useState({
    patientId: '', triageRecordId: '', notes: '', items: [
      { medicineId: '', quantity: 1, dosage: '', durationDays: 7 }
    ]
  });

  // Approve modal
  const [approveTarget, setApproveTarget] = useState(null);
  const [approveDecision, setApproveDecision] = useState('Approved');
  const [approveNotes, setApproveNotes]     = useState('');
  const [approveDoctorId, setApproveDoctorId] = useState('');

  // AI safety panel detail
  const [aiDetail, setAiDetail] = useState(null);

  // ── Fetch ───────────────────────────────────────────────────────────────────
  const fetchPrescriptions = useCallback(async () => {
    setLoading(true); setError(null);
    try {
      const params = new URLSearchParams({ search, status, aiStatus, page, pageSize, sortDir: 'desc' });
      if (!status)   params.delete('status');
      if (!aiStatus) params.delete('aiStatus');
      const res  = await fetch(`${API_BASE}/prescriptions?${params}`);
      if (!res.ok) throw new Error('Failed to fetch prescriptions');
      const data = await res.json();
      setPrescriptions(data.items);
      setTotal(data.totalCount);
      setTotalPages(data.totalPages);
    } catch (e) { setError(e.message); }
    finally { setLoading(false); }
  }, [search, status, aiStatus, page]);

  useEffect(() => { fetchPrescriptions(); }, [fetchPrescriptions]);

  useEffect(() => {
    // Fetch medicines for the create form dropdown
    fetch(`${API_BASE}/medicines?pageSize=50`)
      .then(r => r.json())
      .then(d => setMedicines(d.items || []))
      .catch(() => {});
    // Fetch patients for dropdown
    fetch(`${API_BASE}/patientprofiles`)
      .then(r => r.json())
      .then(d => setPatients(Array.isArray(d) ? d : []))
      .catch(() => {});
  }, []);

  // ── Create Prescription ──────────────────────────────────────────────────────
  const handleCreate = async () => {
    setCreating(true); setCreateResult(null);
    try {
      const res = await fetch(`${API_BASE}/prescriptions`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(newPx)
      });
      const data = await res.json();
      setCreateResult({ ok: res.ok, data });
      if (res.ok) { fetchPrescriptions(); }
    } catch (e) { setCreateResult({ ok: false, data: { message: e.message } }); }
    finally { setCreating(false); }
  };

  const addItemRow = () => setNewPx(p => ({
    ...p, items: [...p.items, { medicineId: '', quantity: 1, dosage: '', durationDays: 7 }]
  }));

  const updateItem = (i, field, val) => setNewPx(p => {
    const items = [...p.items];
    items[i] = { ...items[i], [field]: val };
    return { ...p, items };
  });

  const removeItem = (i) => setNewPx(p => ({
    ...p, items: p.items.filter((_, idx) => idx !== i)
  }));

  // ── Approve / Reject ─────────────────────────────────────────────────────────
  const handleApprove = async () => {
    if (!approveTarget) return;
    try {
      const res = await fetch(`${API_BASE}/prescriptions/${approveTarget.id}/approve`, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          doctorId: approveDoctorId || '00000000-0000-0000-0000-000000000001',
          decision: approveDecision,
          doctorNotes: approveNotes
        })
      });
      if (!res.ok) { const d = await res.json(); alert(d.message); return; }
      setApproveTarget(null);
      setApproveNotes('');
      setApproveDoctorId('');
      fetchPrescriptions();
    } catch (e) { alert('Error: ' + e.message); }
  };

  // ── Dispense ─────────────────────────────────────────────────────────────────
  const handleDispense = async (id) => {
    if (!window.confirm('Dispense this prescription? Stock will be deducted.')) return;
    const res = await fetch(`${API_BASE}/prescriptions/${id}/dispense`, { method: 'PATCH' });
    const d   = await res.json();
    alert(d.message);
    fetchPrescriptions();
  };

  // ── Styles ───────────────────────────────────────────────────────────────────
  const s = {
    page:    { padding: '28px 32px', fontFamily: "'Segoe UI', sans-serif", background: '#f8f9ff', minHeight: '100vh' },
    header:  { display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 24 },
    title:   { margin: 0, fontSize: 26, fontWeight: 700, color: '#1a1a2e' },
    sub:     { margin: '4px 0 0', fontSize: 13, color: '#666' },
    addBtn:  { background: 'linear-gradient(135deg,#6c63ff,#8b5cf6)', color: '#fff', border: 'none',
               padding: '10px 22px', borderRadius: 10, fontSize: 14, fontWeight: 600, cursor: 'pointer' },
    toolbar: { display: 'flex', gap: 12, marginBottom: 20, flexWrap: 'wrap', alignItems: 'center' },
    input:   { padding: '9px 14px', borderRadius: 8, border: '1px solid #ddd', fontSize: 13, outline: 'none', minWidth: 200 },
    select:  { padding: '9px 14px', borderRadius: 8, border: '1px solid #ddd', fontSize: 13, background: '#fff', cursor: 'pointer' },
    table:   { width: '100%', borderCollapse: 'collapse', background: '#fff', borderRadius: 12, overflow: 'hidden',
               boxShadow: '0 2px 12px rgba(0,0,0,.07)' },
    th:      { padding: '12px 16px', textAlign: 'left', background: '#f0efff', color: '#3d3a8f',
               fontSize: 12, fontWeight: 700, textTransform: 'uppercase', letterSpacing: '0.5px' },
    td:      { padding: '12px 16px', borderBottom: '1px solid #f0f0f0', fontSize: 13, color: '#333' },
    btn:     (bg) => ({ padding: '5px 12px', borderRadius: 6, border: 'none', cursor: 'pointer',
               fontSize: 12, fontWeight: 600, background: bg, color: '#fff', marginRight: 6 }),
    modal:   { position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.45)', zIndex: 999,
               display: 'flex', alignItems: 'center', justifyContent: 'center' },
    modalBox:{ background: '#fff', borderRadius: 16, padding: 32, width: 560, maxHeight: '90vh',
               overflowY: 'auto', boxShadow: '0 20px 60px rgba(0,0,0,.25)' },
    label:   { display: 'block', fontSize: 12, fontWeight: 600, color: '#555', marginBottom: 4 },
    field:   { width: '100%', padding: '9px 12px', borderRadius: 8, border: '1px solid #ddd',
               fontSize: 13, marginBottom: 14, boxSizing: 'border-box' },
    saveBtn: { background: 'linear-gradient(135deg,#6c63ff,#8b5cf6)', color: '#fff', border: 'none',
               padding: '11px 0', width: '100%', borderRadius: 10, fontSize: 14, fontWeight: 700, cursor: 'pointer' },
    cancelBtn:{ background: '#f5f5f5', color: '#333', border: '1px solid #ddd', padding: '10px 0',
                width: '100%', borderRadius: 10, fontSize: 14, cursor: 'pointer', marginTop: 8 },
    aiBox:   (status) => ({
      background: (aiColor[status] || '#999') + '11',
      border: `1px solid ${(aiColor[status] || '#999')}44`,
      borderRadius: 10, padding: '14px 18px', marginBottom: 16
    }),
    detailPanel: {
      background: '#fafafa', border: '1px solid #eee', borderRadius: 10, padding: 16, margin: '8px 0 0'
    }
  };

  // ─── Render ──────────────────────────────────────────────────────────────────
  return (
    <div style={s.page}>
      {/* Header */}
      <div style={s.header}>
        <div>
          <h1 style={s.title}>📋 Prescription Management</h1>
          <p style={s.sub}>E-Prescription lifecycle — AI safety validated, doctor approved</p>
        </div>
        <button id="btn-new-prescription" style={s.addBtn} onClick={() => { setShowCreate(true); setCreateResult(null); }}>
          + New Prescription
        </button>
      </div>

      {/* Toolbar */}
      <div style={s.toolbar}>
        <input id="input-px-search" style={s.input} placeholder="🔍  Search patient name..."
          value={search} onChange={e => { setSearch(e.target.value); setPage(1); }} />
        <select id="select-px-status" style={s.select} value={status}
          onChange={e => { setStatus(e.target.value); setPage(1); }}>
          <option value="">All Statuses</option>
          <option value="Draft">Draft</option>
          <option value="Issued">Issued</option>
          <option value="Dispensed">Dispensed</option>
          <option value="Cancelled">Cancelled</option>
        </select>
        <select id="select-px-ai" style={s.select} value={aiStatus}
          onChange={e => { setAiStatus(e.target.value); setPage(1); }}>
          <option value="">All AI Statuses</option>
          <option value="Safe">✅ Safe</option>
          <option value="Warning">⚠️ Warning</option>
          <option value="Blocked">🚫 Blocked</option>
        </select>
      </div>

      {/* Error / Loading */}
      {error   && <p style={{ color: '#ff4d4f', padding: '12px 0' }}>⚠ {error} — Is the API running?</p>}
      {loading && <p style={{ color: '#6c63ff', padding: '12px 0' }}>Loading prescriptions...</p>}

      {/* Table */}
      {!loading && !error && (
        <table style={s.table}>
          <thead>
            <tr>
              <th style={s.th}>Patient</th>
              <th style={s.th}>Triage Severity</th>
              <th style={s.th}>Medicines</th>
              <th style={s.th}>AI Safety</th>
              <th style={s.th}>Status</th>
              <th style={s.th}>Created</th>
              <th style={s.th}>Actions</th>
            </tr>
          </thead>
          <tbody>
            {prescriptions.length === 0 ? (
              <tr><td colSpan={7} style={{ ...s.td, textAlign: 'center', color: '#999', padding: 32 }}>
                No prescriptions found.
              </td></tr>
            ) : prescriptions.map(px => (
              <React.Fragment key={px.id}>
                <tr style={{ background: selected?.id === px.id ? '#f5f3ff' : 'transparent' }}>
                  <td style={s.td}><strong>{px.patientName}</strong></td>
                  <td style={s.td}>{px.triageSeverity && <Badge label={px.triageSeverity} colorMap={{ Low: '#52c41a', Medium: '#1890ff', High: '#fa8c16', Critical: '#ff4d4f' }} />}</td>
                  <td style={s.td}>{px.items?.length} item(s)</td>
                  <td style={s.td}>
                    {px.aiSafetyStatus ? (
                      <span>
                        <Badge label={px.aiSafetyStatus} colorMap={aiColor} />
                        <button style={{ marginLeft: 6, border: 'none', background: 'none', cursor: 'pointer', color: '#6c63ff', fontSize: 12 }}
                          onClick={() => setAiDetail(px)}>details</button>
                      </span>
                    ) : '—'}
                  </td>
                  <td style={s.td}><Badge label={px.status} colorMap={statusColor} /></td>
                  <td style={s.td} title={px.createdAt}>{new Date(px.createdAt).toLocaleDateString()}</td>
                  <td style={s.td}>
                    <button style={s.btn('#6c63ff')}
                      onClick={() => setSelected(selected?.id === px.id ? null : px)}>
                      {selected?.id === px.id ? 'Hide' : 'Details'}
                    </button>
                    {px.status === 'Draft' && (
                      <button style={s.btn('#1890ff')} onClick={() => { setApproveTarget(px); setApproveDecision('Approved'); }}>
                        Review
                      </button>
                    )}
                    {px.status === 'Issued' && (
                      <button style={s.btn('#52c41a')} onClick={() => handleDispense(px.id)}>
                        Dispense
                      </button>
                    )}
                  </td>
                </tr>

                {/* Expanded detail row */}
                {selected?.id === px.id && (
                  <tr>
                    <td colSpan={7} style={{ padding: '0 16px 16px' }}>
                      <div style={s.detailPanel}>
                        <strong>💊 Medicines:</strong>
                        <table style={{ width: '100%', marginTop: 8, fontSize: 13 }}>
                          <thead>
                            <tr style={{ color: '#999' }}>
                              <th style={{ textAlign: 'left', paddingBottom: 4 }}>Medicine</th>
                              <th style={{ textAlign: 'left' }}>Dosage</th>
                              <th style={{ textAlign: 'left' }}>Qty</th>
                              <th style={{ textAlign: 'left' }}>Days</th>
                              <th style={{ textAlign: 'left' }}>Stock</th>
                            </tr>
                          </thead>
                          <tbody>
                            {px.items?.map(item => (
                              <tr key={item.id}>
                                <td style={{ paddingBottom: 4 }}>{item.medicineName}</td>
                                <td>{item.dosage}</td>
                                <td>{item.quantity}</td>
                                <td>{item.durationDays}d</td>
                                <td style={{ color: item.stockAvailable < item.quantity ? '#ff4d4f' : '#52c41a' }}>
                                  {item.stockAvailable} available
                                </td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                        {px.notes && <p style={{ margin: '12px 0 0', color: '#555' }}><strong>Notes:</strong> {px.notes}</p>}
                        {px.notificationSent && (
                          <p style={{ margin: '8px 0 0', color: '#52c41a', fontSize: 12 }}>
                            ✅ Patient notified via {px.notificationChannel} on {new Date(px.notifiedAt).toLocaleString()}
                          </p>
                        )}
                      </div>
                    </td>
                  </tr>
                )}
              </React.Fragment>
            ))}
          </tbody>
        </table>
      )}

      {/* Pagination */}
      {totalPages > 1 && (
        <div style={{ display: 'flex', justifyContent: 'center', gap: 12, marginTop: 20 }}>
          <button disabled={page <= 1} onClick={() => setPage(p => p - 1)}
            style={{ padding: '7px 14px', borderRadius: 8, border: 'none', cursor: 'pointer', background: '#f0f0f0' }}>‹ Prev</button>
          <span style={{ lineHeight: '34px', fontSize: 13 }}>Page {page} of {totalPages}</span>
          <button disabled={page >= totalPages} onClick={() => setPage(p => p + 1)}
            style={{ padding: '7px 14px', borderRadius: 8, border: 'none', cursor: 'pointer', background: '#f0f0f0' }}>Next ›</button>
        </div>
      )}

      {/* ── AI Safety Detail Modal ─────────────────────────────────────────── */}
      {aiDetail && (
        <div style={s.modal} onClick={() => setAiDetail(null)}>
          <div style={{ ...s.modalBox, maxWidth: 600 }} onClick={e => e.stopPropagation()}>
            <h2 style={{ margin: '0 0 16px', color: '#1a1a2e' }}>
              {aiIcon[aiDetail.aiSafetyStatus]} AI Safety Report
            </h2>
            <div style={s.aiBox(aiDetail.aiSafetyStatus)}>
              <strong style={{ color: aiColor[aiDetail.aiSafetyStatus], fontSize: 18 }}>
                Verdict: {aiDetail.aiSafetyStatus}
              </strong>
            </div>
            {aiDetail.aiSafetyCheckResult && (() => {
              try {
                const json = JSON.parse(aiDetail.aiSafetyCheckResult);
                return (
                  <div>
                    <p style={{ color: '#555', marginBottom: 10 }}><strong>Summary:</strong> {json.Summary}</p>
                    {json.Errors?.length > 0 && (
                      <div>
                        <strong style={{ color: '#ff4d4f' }}>Critical Errors:</strong>
                        {json.Errors.map((e, i) => <p key={i} style={{ color: '#ff4d4f', margin: '4px 0', fontSize: 13 }}>{e}</p>)}
                      </div>
                    )}
                    {json.Warnings?.length > 0 && (
                      <div style={{ marginTop: 8 }}>
                        <strong style={{ color: '#fa8c16' }}>Warnings:</strong>
                        {json.Warnings.map((w, i) => <p key={i} style={{ color: '#fa8c16', margin: '4px 0', fontSize: 13 }}>{w}</p>)}
                      </div>
                    )}
                    <p style={{ color: '#999', fontSize: 11, marginTop: 12 }}>
                      Agent ran at {new Date(json.RunAt).toLocaleString()} — Severity: {json.TriageSeverity}
                    </p>
                  </div>
                );
              } catch { return <pre style={{ fontSize: 11, overflowX: 'auto' }}>{aiDetail.aiSafetyCheckResult}</pre>; }
            })()}
            <button style={{ ...s.cancelBtn, marginTop: 16 }} onClick={() => setAiDetail(null)}>Close</button>
          </div>
        </div>
      )}

      {/* ── Doctor Approve / Reject Modal ─────────────────────────────────── */}
      {approveTarget && (
        <div style={s.modal}>
          <div style={s.modalBox}>
            <h2 style={{ margin: '0 0 16px', color: '#1a1a2e' }}>🩺 Doctor Review</h2>
            <p style={{ color: '#555', marginBottom: 16 }}>
              Patient: <strong>{approveTarget.patientName}</strong> |
              AI Safety: <Badge label={approveTarget.aiSafetyStatus} colorMap={aiColor} />
            </p>

            {/* Show AI summary inline */}
            {approveTarget.aiSafetyCheckResult && (() => {
              try {
                const json = JSON.parse(approveTarget.aiSafetyCheckResult);
                return (
                  <div style={s.aiBox(approveTarget.aiSafetyStatus)}>
                    <strong>{aiIcon[approveTarget.aiSafetyStatus]} {json.Summary}</strong>
                    {json.Errors?.map((e, i) => <p key={i} style={{ color: '#ff4d4f', fontSize: 12, margin: '4px 0' }}>{e}</p>)}
                    {json.Warnings?.map((w, i) => <p key={i} style={{ color: '#fa8c16', fontSize: 12, margin: '4px 0' }}>{w}</p>)}
                  </div>
                );
              } catch { return null; }
            })()}

            <label style={s.label}>Decision</label>
            <select id="select-approve-decision" style={{ ...s.field, cursor: 'pointer' }}
              value={approveDecision} onChange={e => setApproveDecision(e.target.value)}>
              <option value="Approved">✅ Approve (Issue Prescription)</option>
              <option value="Rejected">❌ Reject (Cancel Prescription)</option>
            </select>

            <label style={s.label}>Doctor ID (GUID)</label>
            <input id="input-doctor-id" style={s.field} placeholder="Enter your doctor GUID..."
              value={approveDoctorId} onChange={e => setApproveDoctorId(e.target.value)} />

            <label style={s.label}>Notes (optional)</label>
            <textarea style={{ ...s.field, minHeight: 70, resize: 'vertical' }}
              value={approveNotes} onChange={e => setApproveNotes(e.target.value)}
              placeholder="Add clinical notes or rejection reason..." />

            <button id="btn-confirm-decision" style={{
              ...s.saveBtn,
              background: approveDecision === 'Approved'
                ? 'linear-gradient(135deg,#52c41a,#389e0d)'
                : 'linear-gradient(135deg,#ff4d4f,#cf1322)'
            }} onClick={handleApprove}>
              {approveDecision === 'Approved' ? '✅ Confirm Approval' : '❌ Confirm Rejection'}
            </button>
            <button style={s.cancelBtn} onClick={() => setApproveTarget(null)}>Cancel</button>
          </div>
        </div>
      )}

      {/* ── Create Prescription Modal ──────────────────────────────────────── */}
      {showCreate && (
        <div style={s.modal}>
          <div style={s.modalBox}>
            <h2 style={{ margin: '0 0 20px', color: '#1a1a2e' }}>➕ New Prescription</h2>

            {/* Patient */}
            <label style={s.label}>Patient</label>
            <select id="select-new-px-patient" style={{ ...s.field, cursor: 'pointer' }}
              value={newPx.patientId}
              onChange={e => setNewPx(p => ({ ...p, patientId: e.target.value }))}>
              <option value="">Select patient...</option>
              {patients.map(pt => <option key={pt.id} value={pt.id}>{pt.fullName}</option>)}
            </select>

            {/* Triage Record ID */}
            <label style={s.label}>Triage Record ID (GUID)</label>
            <input id="input-new-px-triage" style={s.field} placeholder="Paste Triage Record GUID..."
              value={newPx.triageRecordId}
              onChange={e => setNewPx(p => ({ ...p, triageRecordId: e.target.value }))} />

            {/* Notes */}
            <label style={s.label}>Notes (optional)</label>
            <textarea style={{ ...s.field, minHeight: 60, resize: 'vertical' }}
              value={newPx.notes}
              onChange={e => setNewPx(p => ({ ...p, notes: e.target.value }))} />

            {/* Medicine Items */}
            <label style={s.label}>💊 Medicine Items</label>
            {newPx.items.map((item, i) => (
              <div key={i} style={{ background: '#f8f9ff', borderRadius: 8, padding: 12, marginBottom: 10 }}>
                <div style={{ display: 'flex', gap: 8 }}>
                  <select style={{ flex: 2, padding: '7px 10px', borderRadius: 6, border: '1px solid #ddd', fontSize: 13 }}
                    value={item.medicineId}
                    onChange={e => updateItem(i, 'medicineId', e.target.value)}>
                    <option value="">Select medicine...</option>
                    {medicines.map(m => <option key={m.id} value={m.id}>{m.name} (stock: {m.stockQuantity})</option>)}
                  </select>
                  <input type="number" min="1" style={{ flex: 0.5, padding: '7px 10px', borderRadius: 6, border: '1px solid #ddd', fontSize: 13 }}
                    placeholder="Qty" value={item.quantity}
                    onChange={e => updateItem(i, 'quantity', parseInt(e.target.value))} />
                  <button style={{ padding: '7px 12px', borderRadius: 6, border: 'none', background: '#ff7875', color: '#fff', cursor: 'pointer' }}
                    onClick={() => removeItem(i)}>✕</button>
                </div>
                <input style={{ width: '100%', marginTop: 6, padding: '7px 10px', borderRadius: 6, border: '1px solid #ddd', fontSize: 13, boxSizing: 'border-box' }}
                  placeholder="Dosage e.g. 1 tablet twice daily"
                  value={item.dosage} onChange={e => updateItem(i, 'dosage', e.target.value)} />
                <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginTop: 6 }}>
                  <label style={{ fontSize: 12, color: '#777' }}>Duration (days):</label>
                  <input type="number" min="1" style={{ width: 70, padding: '5px 8px', borderRadius: 6, border: '1px solid #ddd', fontSize: 13 }}
                    value={item.durationDays} onChange={e => updateItem(i, 'durationDays', parseInt(e.target.value))} />
                </div>
              </div>
            ))}
            <button style={{ border: '1px dashed #6c63ff', background: 'none', color: '#6c63ff', padding: '8px 0',
              width: '100%', borderRadius: 8, cursor: 'pointer', marginBottom: 14, fontSize: 13 }}
              onClick={addItemRow}>+ Add Medicine Item</button>

            {/* AI result feedback */}
            {createResult && (
              <div style={{
                padding: '12px 16px', borderRadius: 8, marginBottom: 14,
                background: createResult.ok ? '#f6ffed' : '#fff2f0',
                border: `1px solid ${createResult.ok ? '#b7eb8f' : '#ffa39e'}`
              }}>
                <strong style={{ color: createResult.ok ? '#389e0d' : '#cf1322' }}>
                  {createResult.ok ? '✅' : '❌'} {createResult.data?.message}
                </strong>
                {createResult.ok && createResult.data?.aiSafetyStatus && (
                  <p style={{ margin: '6px 0 0', fontSize: 13 }}>
                    AI Safety Verdict: <Badge label={createResult.data.aiSafetyStatus} colorMap={aiColor} />
                  </p>
                )}
              </div>
            )}

            <button id="btn-submit-prescription" style={s.saveBtn}
              onClick={handleCreate} disabled={creating}>
              {creating ? '⏳ Running AI Safety Check...' : '🤖 Create & Run AI Safety Check'}
            </button>
            <button style={s.cancelBtn} onClick={() => setShowCreate(false)}>Close</button>
          </div>
        </div>
      )}
    </div>
  );
};

export default PrescriptionManagement;
