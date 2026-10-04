import React, { useEffect, useState } from 'react';
import { apiFetch } from '../api';
import './staff/ManagePatients.css';

const WARD_TYPES = ['General', 'ICU', 'Maternity', 'Pediatrics', 'Surgical', 'Emergency'];

const emptyForm = {
  wardNumber: '',
  wardType: 'General',
  capacity: 20
};

const AdminWards = () => {
  const [wards, setWards] = useState([]);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editing, setEditing] = useState(null);
  const [formData, setFormData] = useState(emptyForm);
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);

  const fetchWards = async () => {
    try {
      const res = await apiFetch('/api/Wards');
      if (res.ok) setWards(await res.json());
      else setError(await res.text() || 'Could not load wards.');
    } catch {
      setError('Network error loading wards.');
    }
  };

  useEffect(() => {
    fetchWards();
  }, []);

  const openCreate = () => {
    setEditing(null);
    setFormData(emptyForm);
    setError('');
    setIsFormOpen(true);
  };

  const openEdit = (ward) => {
    setEditing(ward);
    setFormData({
      wardNumber: ward.wardNumber || '',
      wardType: ward.wardType || 'General',
      capacity: ward.capacity || 1
    });
    setError('');
    setIsFormOpen(true);
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData({
      ...formData,
      [name]: name === 'capacity' ? Number(value) : value
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSaving(true);
    setError('');

    try {
      const res = await apiFetch(editing ? `/api/Wards/${editing.id}` : '/api/Wards', {
        method: editing ? 'PUT' : 'POST',
        body: JSON.stringify({
          wardNumber: formData.wardNumber.trim(),
          wardType: formData.wardType.trim(),
          capacity: formData.capacity
        })
      });

      if (!res.ok) {
        setError(await res.text() || 'Could not save ward.');
        return;
      }

      setIsFormOpen(false);
      setEditing(null);
      setFormData(emptyForm);
      fetchWards();
    } catch {
      setError('Network error saving ward.');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (ward) => {
    if (!window.confirm(`Delete ward ${ward.wardNumber}? This is only allowed when no beds are occupied.`)) {
      return;
    }
    setError('');
    try {
      const res = await apiFetch(`/api/Wards/${ward.id}`, { method: 'DELETE' });
      if (!res.ok) {
        setError(await res.text() || 'Could not delete ward.');
        return;
      }
      fetchWards();
    } catch {
      setError('Network error deleting ward.');
    }
  };

  return (
    <div className="manage-patients-container">
      <div className="manage-header">
        <h2 className="page-title">Ward management</h2>
        <button className="register-btn" onClick={openCreate}>Add ward</button>
      </div>
      <p className="records-hint">
        Wards use WardNumber, WardType, and Capacity. Occupied beds update when patients are admitted.
      </p>
      {error && !isFormOpen && <p className="ai-error">{error}</p>}

      <div className="patients-grid">
        {wards.map((ward) => {
          const percentFull = ward.capacity > 0 ? (ward.occupiedBeds / ward.capacity) * 100 : 0;
          const isFull = percentFull >= 100;
          return (
            <div className="patient-tile" key={ward.id}>
              <div className="patient-tile-header">
                <div className="avatar"></div>
                <h3 className="patient-name">Ward {ward.wardNumber}</h3>
              </div>
              <div className="patient-tile-body">
                <p><strong>Type:</strong> {ward.wardType}</p>
                <p><strong>Capacity:</strong> {ward.capacity} beds</p>
                <p><strong>Occupied:</strong> {ward.occupiedBeds} beds</p>
                <div style={{ background: '#e0e0e0', height: 10, borderRadius: 6, marginTop: 10, overflow: 'hidden' }}>
                  <div style={{
                    background: isFull ? '#ff4d4f' : '#52c41a',
                    height: 10,
                    width: `${Math.min(percentFull, 100)}%`
                  }} />
                </div>
              </div>
              <div className="patient-tile-actions">
                <button className="admit-btn" onClick={() => openEdit(ward)}>Edit</button>
                <button className="danger-btn" onClick={() => handleDelete(ward)}>Remove</button>
              </div>
            </div>
          );
        })}
      </div>

      {isFormOpen && (
        <div className="modal-overlay">
          <div className="modal-content">
            <button className="close-btn" onClick={() => setIsFormOpen(false)}>×</button>
            <div style={{ padding: '8px 0', maxWidth: 500 }}>
              <h2 style={{ marginTop: 0 }}>{editing ? 'Edit ward' : 'Add ward'}</h2>
              <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 15 }}>
                <div style={{ display: 'flex', flexDirection: 'column', gap: 5 }}>
                  <label style={{ fontSize: 14, fontWeight: 'bold' }}>Ward number</label>
                  <input
                    type="text"
                    name="wardNumber"
                    placeholder="e.g., G-02"
                    value={formData.wardNumber}
                    onChange={handleChange}
                    required
                    style={{ padding: 10, borderRadius: 6, border: '1px solid #ccc' }}
                  />
                </div>
                <div style={{ display: 'flex', flexDirection: 'column', gap: 5 }}>
                  <label style={{ fontSize: 14, fontWeight: 'bold' }}>Ward type</label>
                  <select
                    name="wardType"
                    value={formData.wardType}
                    onChange={handleChange}
                    required
                    style={{ padding: 10, borderRadius: 6, border: '1px solid #ccc' }}
                  >
                    {WARD_TYPES.map((t) => (
                      <option key={t} value={t}>{t}</option>
                    ))}
                  </select>
                </div>
                <div style={{ display: 'flex', flexDirection: 'column', gap: 5 }}>
                  <label style={{ fontSize: 14, fontWeight: 'bold' }}>Capacity (beds)</label>
                  <input
                    type="number"
                    name="capacity"
                    min={1}
                    max={500}
                    value={formData.capacity}
                    onChange={handleChange}
                    required
                    style={{ padding: 10, borderRadius: 6, border: '1px solid #ccc' }}
                  />
                  {editing && (
                    <span style={{ fontSize: 12, color: '#6c757d' }}>
                      Cannot set capacity below current occupancy ({editing.occupiedBeds}).
                    </span>
                  )}
                </div>
                {error && <p className="ai-error">{error}</p>}
                <button
                  type="submit"
                  disabled={saving}
                  style={{ padding: 12, background: '#0ab39c', color: 'white', border: 'none', borderRadius: 6, fontWeight: 'bold', cursor: 'pointer', marginTop: 10 }}
                >
                  {saving ? 'Saving...' : editing ? 'Save changes' : 'Create ward'}
                </button>
              </form>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default AdminWards;
