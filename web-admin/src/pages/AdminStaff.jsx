import React, { useEffect, useMemo, useState } from 'react';
import { apiFetch } from '../services/api';
import './staff/ManagePatients.css';

const PAGE_SIZE = 8;

const AdminStaff = () => {
  const [staff, setStaff] = useState([]);
  const [isRegisterOpen, setIsRegisterOpen] = useState(false);
  const [formData, setFormData] = useState({
    fullName: '',
    username: '',
    email: '',
    password: '',
    role: 'Staff',
    specialization: ''
  });
  const [error, setError] = useState('');
  const [searchTerm, setSearchTerm] = useState('');
  const [roleFilter, setRoleFilter] = useState('All');
  const [specFilter, setSpecFilter] = useState('All');
  const [page, setPage] = useState(1);

  const fetchStaff = async () => {
    try {
      const res = await apiFetch('/api/auth/staff');
      if (res.ok) setStaff(await res.json());
    } catch (e) {
      console.error('Failed to fetch staff', e);
    }
  };

  useEffect(() => {
    fetchStaff();
  }, []);

  const specializations = useMemo(() => {
    const values = staff
      .map((s) => s.specialization)
      .filter((v) => v && String(v).trim());
    return [...new Set(values)].sort();
  }, [staff]);

  const filtered = useMemo(() => {
    const q = searchTerm.trim().toLowerCase();
    return staff.filter((s) => {
      if (roleFilter !== 'All' && s.role !== roleFilter) return false;
      if (specFilter !== 'All' && (s.specialization || '') !== specFilter) return false;
      if (!q) return true;
      const haystack = [s.fullName, s.username, s.email, s.specialization, s.role]
        .filter(Boolean)
        .join(' ')
        .toLowerCase();
      return haystack.includes(q);
    });
  }, [staff, searchTerm, roleFilter, specFilter]);

  const totalPages = Math.max(1, Math.ceil(filtered.length / PAGE_SIZE));
  const currentPage = Math.min(page, totalPages);
  const paged = filtered.slice((currentPage - 1) * PAGE_SIZE, currentPage * PAGE_SIZE);

  useEffect(() => {
    setPage(1);
  }, [searchTerm, roleFilter, specFilter]);

  const handleRegister = async (e) => {
    e.preventDefault();
    setError('');
    try {
      const res = await apiFetch('/api/auth/register-staff', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(formData)
      });
      if (!res.ok) {
        const text = await res.text();
        setError(text || 'Failed to register staff');
        return;
      }
      setIsRegisterOpen(false);
      setFormData({
        fullName: '',
        username: '',
        email: '',
        password: '',
        role: 'Staff',
        specialization: ''
      });
      fetchStaff();
    } catch (e) {
      console.error(e);
      setError('An error occurred while registering');
    }
  };

  return (
    <div className="manage-patients-container">
      <div className="manage-header">
        <h2 className="page-title">Staff Management</h2>
        <button className="register-btn" onClick={() => setIsRegisterOpen(true)}>
          + Add Staff Member
        </button>
      </div>
      <p className="records-hint">View and manage hospital staff and doctor accounts.</p>

      <div className="staff-toolbar">
        <div className="search-bar-container" style={{ marginBottom: 0, maxWidth: 320 }}>
          <span className="search-icon">🔍</span>
          <input
            type="text"
            className="search-input"
            placeholder="Search name, username, email..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>
        <select
          className="staff-filter"
          value={roleFilter}
          onChange={(e) => setRoleFilter(e.target.value)}
        >
          <option value="All">All roles</option>
          <option value="Admin">Admin</option>
          <option value="Doctor">Doctor</option>
          <option value="Staff">Staff</option>
        </select>
        <select
          className="staff-filter"
          value={specFilter}
          onChange={(e) => setSpecFilter(e.target.value)}
        >
          <option value="All">All specializations</option>
          {specializations.map((spec) => (
            <option key={spec} value={spec}>{spec}</option>
          ))}
        </select>
        <span className="staff-count">{filtered.length} account{filtered.length === 1 ? '' : 's'}</span>
      </div>

      <div className="table-wrapper">
        <table className="patients-table">
          <thead>
            <tr>
              <th>Full Name</th>
              <th>Username</th>
              <th>Email</th>
              <th>Role</th>
              <th>Specialization</th>
              <th>Linked Doctor ID</th>
            </tr>
          </thead>
          <tbody>
            {paged.map((s) => (
              <tr key={s.id}>
                <td style={{ fontWeight: 500 }}>{s.fullName || '—'}</td>
                <td>{s.username}</td>
                <td>{s.email || '—'}</td>
                <td>
                  <span className={`status-badge ${s.role === 'Admin' ? 'completed' : s.role === 'Doctor' ? 'review' : 'pending'}`}>
                    {s.role}
                  </span>
                </td>
                <td>{s.role === 'Doctor' ? (s.specialization || '—') : '—'}</td>
                <td style={{ fontSize: '0.8rem', color: '#666' }}>
                  {s.doctorId ? `${String(s.doctorId).substring(0, 8)}...` : '—'}
                </td>
              </tr>
            ))}
            {paged.length === 0 && (
              <tr>
                <td colSpan="6" style={{ textAlign: 'center', padding: '2rem', color: '#888' }}>
                  No staff accounts match your search.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      <div className="pagination-bar">
        <button
          type="button"
          className="page-btn"
          disabled={currentPage <= 1}
          onClick={() => setPage((p) => Math.max(1, p - 1))}
        >
          Previous
        </button>
        <span>Page {currentPage} of {totalPages}</span>
        <button
          type="button"
          className="page-btn"
          disabled={currentPage >= totalPages}
          onClick={() => setPage((p) => Math.min(totalPages, p + 1))}
        >
          Next
        </button>
      </div>

      {isRegisterOpen && (
        <div className="modal-overlay">
          <div className="modal-content" style={{ maxWidth: '400px' }}>
            <h3>Register New Staff</h3>
            {error && <div className="error-message" style={{ color: 'red', marginBottom: '1rem', fontSize: '0.9rem' }}>{error}</div>}
            <form onSubmit={handleRegister} className="patient-edit-form">
              <label>
                Full Name
                <input
                  type="text"
                  required
                  value={formData.fullName}
                  onChange={(e) => setFormData({ ...formData, fullName: e.target.value })}
                />
              </label>
              <label>
                Username
                <input
                  type="text"
                  required
                  value={formData.username}
                  onChange={(e) => setFormData({ ...formData, username: e.target.value })}
                />
              </label>
              <label>
                Email
                <input
                  type="email"
                  required
                  value={formData.email}
                  onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                />
              </label>
              <label>
                Password
                <input
                  type="password"
                  required
                  value={formData.password}
                  onChange={(e) => setFormData({ ...formData, password: e.target.value })}
                />
              </label>
              <label>
                Role
                <select
                  value={formData.role}
                  onChange={(e) => setFormData({ ...formData, role: e.target.value })}
                  style={{ width: '100%', padding: '0.5rem', borderRadius: '4px', border: '1px solid #ccc' }}
                >
                  <option value="Staff">Staff</option>
                  <option value="Doctor">Doctor</option>
                  <option value="Admin">Admin</option>
                </select>
              </label>

              {formData.role === 'Doctor' && (
                <label>
                  Specialization
                  <input
                    type="text"
                    required
                    placeholder="e.g., Cardiologist"
                    value={formData.specialization}
                    onChange={(e) => setFormData({ ...formData, specialization: e.target.value })}
                  />
                </label>
              )}

              <div className="modal-actions">
                <button type="button" className="cancel-btn" onClick={() => setIsRegisterOpen(false)}>Cancel</button>
                <button type="submit" className="save-btn">Register Account</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default AdminStaff;
