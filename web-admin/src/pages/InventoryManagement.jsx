import React, { useState, useEffect, useCallback } from 'react';

const API_BASE = 'http://localhost:5241/api';

// ─── Helpers ─────────────────────────────────────────────────────────────────
const badge = (label, color) => (
  <span style={{
    display: 'inline-block', padding: '2px 10px', borderRadius: '12px',
    fontSize: '11px', fontWeight: 700, letterSpacing: '0.5px',
    background: color + '22', color: color, border: `1px solid ${color}55`
  }}>{label}</span>
);

const stockColor = (m) => {
  if (m.stockQuantity === 0)            return '#ff4d4f';
  if (m.stockQuantity <= m.reorderLevel) return '#fa8c16';
  return '#52c41a';
};

// ─── Main Component ───────────────────────────────────────────────────────────
const InventoryManagement = () => {
  const [medicines, setMedicines]   = useState([]);
  const [total, setTotal]           = useState(0);
  const [loading, setLoading]       = useState(true);
  const [error, setError]           = useState(null);

  // Query params
  const [search, setSearch]     = useState('');
  const [filter, setFilter]     = useState('all');
  const [sortBy, setSortBy]     = useState('name');
  const [sortDir, setSortDir]   = useState('asc');
  const [page, setPage]         = useState(1);
  const pageSize                = 8;
  const [totalPages, setTotalPages] = useState(1);

  // Modal state
  const [showModal, setShowModal]       = useState(false);
  const [editMedicine, setEditMedicine] = useState(null);
  const [restockId, setRestockId]       = useState(null);
  const [restockQty, setRestockQty]     = useState(1);
  const [formData, setFormData]         = useState({
    name: '', category: '', description: '', manufacturer: '',
    stockQuantity: 0, reorderLevel: 10, unitPrice: 0,
    expiryDate: new Date().toISOString().split('T')[0]
  });

  // ── Fetch data ──────────────────────────────────────────────────────────────
  const fetchMedicines = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const params = new URLSearchParams({
        search, filter, sortBy, sortDir, page, pageSize
      });
      const res  = await fetch(`${API_BASE}/medicines?${params}`);
      if (!res.ok) throw new Error('Failed to fetch medicines');
      const data = await res.json();
      setMedicines(data.items);
      setTotal(data.totalCount);
      setTotalPages(data.totalPages);
    } catch (e) {
      setError(e.message);
    } finally {
      setLoading(false);
    }
  }, [search, filter, sortBy, sortDir, page]);

  useEffect(() => { fetchMedicines(); }, [fetchMedicines]);

  // ── CRUD handlers ────────────────────────────────────────────────────────────
  const openAddModal = () => {
    setEditMedicine(null);
    setFormData({ name: '', category: '', description: '', manufacturer: '',
      stockQuantity: 0, reorderLevel: 10, unitPrice: 0,
      expiryDate: new Date().toISOString().split('T')[0] });
    setShowModal(true);
  };

  const openEditModal = (med) => {
    setEditMedicine(med);
    setFormData({
      name: med.name, category: med.category, description: med.description,
      manufacturer: med.manufacturer, stockQuantity: med.stockQuantity,
      reorderLevel: med.reorderLevel, unitPrice: med.unitPrice,
      expiryDate: med.expiryDate, isActive: med.isActive
    });
    setShowModal(true);
  };

  const handleSave = async () => {
    const method  = editMedicine ? 'PUT' : 'POST';
    const url     = editMedicine
      ? `${API_BASE}/medicines/${editMedicine.id}`
      : `${API_BASE}/medicines`;
    try {
      const res = await fetch(url, {
        method,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(formData)
      });
      if (!res.ok) throw new Error('Save failed');
      setShowModal(false);
      fetchMedicines();
    } catch (e) { alert('Error: ' + e.message); }
  };

  const handleDeactivate = async (med) => {
    if (!window.confirm(`Deactivate "${med.name}"?`)) return;
    await fetch(`${API_BASE}/medicines/${med.id}`, { method: 'DELETE' });
    fetchMedicines();
  };

  const handleRestock = async (id) => {
    try {
      const res = await fetch(`${API_BASE}/medicines/${id}/restock`, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ quantityToAdd: Number(restockQty) })
      });
      if (!res.ok) throw new Error('Restock failed');
      setRestockId(null);
      setRestockQty(1);
      fetchMedicines();
    } catch (e) { alert('Error: ' + e.message); }
  };

  const toggleSort = (col) => {
    if (sortBy === col) setSortDir(d => d === 'asc' ? 'desc' : 'asc');
    else { setSortBy(col); setSortDir('asc'); }
    setPage(1);
  };

  const SortIcon = ({ col }) => {
    if (sortBy !== col) return <span style={{ opacity: 0.3 }}> ↕</span>;
    return <span style={{ color: '#6c63ff' }}> {sortDir === 'asc' ? '↑' : '↓'}</span>;
  };

  // ── Styles ──────────────────────────────────────────────────────────────────
  const s = {
    page:    { padding: '28px 32px', fontFamily: "'Segoe UI', sans-serif", background: '#f8f9ff', minHeight: '100vh' },
    header:  { display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 24 },
    title:   { margin: 0, fontSize: 26, fontWeight: 700, color: '#1a1a2e' },
    sub:     { margin: '4px 0 0', fontSize: 13, color: '#666' },
    addBtn:  { background: 'linear-gradient(135deg,#6c63ff,#8b5cf6)', color: '#fff', border: 'none',
               padding: '10px 22px', borderRadius: 10, fontSize: 14, fontWeight: 600, cursor: 'pointer' },
    toolbar: { display: 'flex', gap: 12, marginBottom: 20, flexWrap: 'wrap', alignItems: 'center' },
    input:   { padding: '9px 14px', borderRadius: 8, border: '1px solid #ddd', fontSize: 13,
               outline: 'none', minWidth: 200 },
    select:  { padding: '9px 14px', borderRadius: 8, border: '1px solid #ddd', fontSize: 13,
               background: '#fff', cursor: 'pointer' },
    table:   { width: '100%', borderCollapse: 'collapse', background: '#fff',
               borderRadius: 12, overflow: 'hidden', boxShadow: '0 2px 12px rgba(0,0,0,.07)' },
    th:      { padding: '12px 16px', textAlign: 'left', background: '#f0efff',
               color: '#3d3a8f', fontSize: 12, fontWeight: 700, textTransform: 'uppercase',
               letterSpacing: '0.5px', cursor: 'pointer', userSelect: 'none' },
    td:      { padding: '12px 16px', borderBottom: '1px solid #f0f0f0', fontSize: 13, color: '#333' },
    actionBtn:(bg) => ({
      padding: '5px 12px', borderRadius: 6, border: 'none', cursor: 'pointer',
      fontSize: 12, fontWeight: 600, background: bg, color: '#fff', marginRight: 6
    }),
    modal:   { position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.45)', zIndex: 999,
               display: 'flex', alignItems: 'center', justifyContent: 'center' },
    modalBox:{ background: '#fff', borderRadius: 16, padding: 32, width: 480, maxHeight: '90vh',
               overflowY: 'auto', boxShadow: '0 20px 60px rgba(0,0,0,.25)' },
    label:   { display: 'block', fontSize: 12, fontWeight: 600, color: '#555', marginBottom: 4 },
    field:   { width: '100%', padding: '9px 12px', borderRadius: 8, border: '1px solid #ddd',
               fontSize: 13, marginBottom: 14, boxSizing: 'border-box' },
    saveBtn: { background: 'linear-gradient(135deg,#6c63ff,#8b5cf6)', color: '#fff', border: 'none',
               padding: '11px 0', width: '100%', borderRadius: 10, fontSize: 14, fontWeight: 700, cursor: 'pointer' },
    cancelBtn:{ background: '#f5f5f5', color: '#333', border: '1px solid #ddd',
                padding: '10px 0', width: '100%', borderRadius: 10, fontSize: 14, cursor: 'pointer', marginTop: 8 },
    pager:   { display: 'flex', justifyContent: 'center', alignItems: 'center', gap: 12, marginTop: 20 },
    pgBtn:   (active) => ({
      padding: '7px 14px', borderRadius: 8, border: 'none', cursor: 'pointer', fontSize: 13,
      background: active ? '#6c63ff' : '#f0f0f0', color: active ? '#fff' : '#555', fontWeight: active ? 700 : 400
    }),
  };

  // ── Render ────────────────────────────────────────────────────────────────────
  return (
    <div style={s.page}>
      {/* Header */}
      <div style={s.header}>
        <div>
          <h1 style={s.title}>💊 Pharmacy Inventory</h1>
          <p style={s.sub}>Medicine catalog management — {total} medicines total</p>
        </div>
        <button id="btn-add-medicine" style={s.addBtn} onClick={openAddModal}>+ Add Medicine</button>
      </div>

      {/* Toolbar */}
      <div style={s.toolbar}>
        <input
          id="input-medicine-search"
          style={s.input}
          placeholder="🔍  Search name, category, manufacturer..."
          value={search}
          onChange={e => { setSearch(e.target.value); setPage(1); }}
        />
        <select id="select-medicine-filter" style={s.select} value={filter}
          onChange={e => { setFilter(e.target.value); setPage(1); }}>
          <option value="all">All Active</option>
          <option value="lowstock">⚠ Low Stock</option>
          <option value="expiring">⏰ Expiring Soon</option>
          <option value="inactive">❌ Inactive</option>
        </select>
      </div>

      {/* Error / Loading */}
      {error   && <p style={{ color: '#ff4d4f', padding: '12px 0' }}>⚠ {error} — Is the API running?</p>}
      {loading && <p style={{ color: '#6c63ff', padding: '12px 0' }}>Loading medicines...</p>}

      {/* Table */}
      {!loading && !error && (
        <table style={s.table}>
          <thead>
            <tr>
              <th style={s.th} onClick={() => toggleSort('name')}>Name <SortIcon col="name"/></th>
              <th style={s.th}>Category</th>
              <th style={s.th} onClick={() => toggleSort('stock')}>Stock <SortIcon col="stock"/></th>
              <th style={s.th} onClick={() => toggleSort('price')}>Price (LKR) <SortIcon col="price"/></th>
              <th style={s.th} onClick={() => toggleSort('expiry')}>Expiry <SortIcon col="expiry"/></th>
              <th style={s.th}>Status</th>
              <th style={s.th}>Actions</th>
            </tr>
          </thead>
          <tbody>
            {medicines.length === 0 ? (
              <tr><td colSpan={7} style={{ ...s.td, textAlign: 'center', color: '#999', padding: 32 }}>
                No medicines found.
              </td></tr>
            ) : medicines.map(med => (
              <tr key={med.id} style={{ background: med.stockQuantity === 0 ? '#fff2f0' : 'transparent' }}>
                <td style={s.td}>
                  <strong>{med.name}</strong>
                  {med.isExpiringSoon && <span style={{ color: '#fa8c16', fontSize: 11, marginLeft: 6 }}>⏰ Expiring</span>}
                </td>
                <td style={s.td}>{badge(med.category, '#6c63ff')}</td>
                <td style={s.td}>
                  <span style={{ color: stockColor(med), fontWeight: 700 }}>{med.stockQuantity}</span>
                  <span style={{ color: '#aaa', fontSize: 11, marginLeft: 4 }}>/ {med.reorderLevel} min</span>
                  {med.isLowStock && badge(' Low', '#fa8c16')}
                </td>
                <td style={s.td}>LKR {Number(med.unitPrice).toFixed(2)}</td>
                <td style={s.td}>{med.expiryDate}</td>
                <td style={s.td}>{med.isActive ? badge('Active', '#52c41a') : badge('Inactive', '#ff4d4f')}</td>
                <td style={s.td}>
                  <button style={s.actionBtn('#6c63ff')} onClick={() => openEditModal(med)}>Edit</button>
                  <button style={s.actionBtn('#13c2c2')} onClick={() => { setRestockId(med.id); setRestockQty(10); }}>Restock</button>
                  <button style={s.actionBtn('#ff7875')} onClick={() => handleDeactivate(med)}>Deactivate</button>

                  {/* Inline restock form */}
                  {restockId === med.id && (
                    <div style={{ marginTop: 8, display: 'flex', gap: 6, alignItems: 'center' }}>
                      <input type="number" min="1" value={restockQty}
                        onChange={e => setRestockQty(e.target.value)}
                        style={{ width: 70, padding: '5px 8px', borderRadius: 6, border: '1px solid #ddd', fontSize: 13 }} />
                      <button style={s.actionBtn('#52c41a')} onClick={() => handleRestock(med.id)}>✓ Add</button>
                      <button style={s.actionBtn('#aaa')} onClick={() => setRestockId(null)}>✗</button>
                    </div>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}

      {/* Pagination */}
      {totalPages > 1 && (
        <div style={s.pager}>
          <button style={s.pgBtn(false)} disabled={page <= 1} onClick={() => setPage(p => p - 1)}>‹ Prev</button>
          {[...Array(totalPages)].map((_, i) => (
            <button key={i} style={s.pgBtn(page === i + 1)} onClick={() => setPage(i + 1)}>{i + 1}</button>
          ))}
          <button style={s.pgBtn(false)} disabled={page >= totalPages} onClick={() => setPage(p => p + 1)}>Next ›</button>
        </div>
      )}

      {/* Add / Edit Modal */}
      {showModal && (
        <div style={s.modal}>
          <div style={s.modalBox}>
            <h2 style={{ margin: '0 0 20px', color: '#1a1a2e', fontSize: 20 }}>
              {editMedicine ? '✏️ Edit Medicine' : '➕ Add New Medicine'}
            </h2>
            {[
              ['Name',         'name',          'text',   'e.g. Amoxicillin 500mg'],
              ['Category',     'category',      'text',   'e.g. Antibiotic'],
              ['Manufacturer', 'manufacturer',  'text',   'Pharma company'],
              ['Stock Qty',    'stockQuantity', 'number', '0'],
              ['Reorder Level','reorderLevel',  'number', '10'],
              ['Unit Price',   'unitPrice',     'number', '0.00'],
              ['Expiry Date',  'expiryDate',    'date',   ''],
            ].map(([label, key, type, placeholder]) => (
              <div key={key}>
                <label style={s.label}>{label}</label>
                <input id={`field-med-${key}`} type={type} style={s.field}
                  placeholder={placeholder}
                  value={formData[key] || ''}
                  onChange={e => setFormData(f => ({ ...f, [key]: e.target.value }))} />
              </div>
            ))}
            <label style={s.label}>Description</label>
            <textarea style={{ ...s.field, minHeight: 70, resize: 'vertical' }}
              value={formData.description}
              onChange={e => setFormData(f => ({ ...f, description: e.target.value }))} />

            <button id="btn-save-medicine" style={s.saveBtn} onClick={handleSave}>
              {editMedicine ? 'Save Changes' : 'Add Medicine'}
            </button>
            <button style={s.cancelBtn} onClick={() => setShowModal(false)}>Cancel</button>
          </div>
        </div>
      )}
    </div>
  );
};

export default InventoryManagement;
