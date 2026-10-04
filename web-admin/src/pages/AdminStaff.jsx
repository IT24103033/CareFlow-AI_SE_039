import React, { useEffect, useState } from 'react';
import { apiFetch } from '../services/api';
import './staff/ManagePatients.css'; // Re-use the nice styling

const AdminStaff = () => {
  const [staff, setStaff] = useState([]);
  const [isRegisterOpen, setIsRegisterOpen] = useState(false);
  const [formData, setFormData] = useState({ username: '', password: '', role: 'Staff' });
  const [error, setError] = useState('');

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
      setFormData({ username: '', password: '', role: 'Staff' });
      fetchStaff();
    } catch (e) {
      console.error(e);
      setError('An error occurred while registering');
    }
  };

  return (
    <div className="manage-patients-container">
      <div className="header-row">
        <h2 className="page-title">Staff Management</h2>
        <button className="register-btn" onClick={() => setIsRegisterOpen(true)}>
          + Add Staff Member
        </button>
      </div>
      <p className="records-hint">View and manage hospital staff and doctor accounts.</p>

      {/* Staff Table */}
      <div className="table-wrapper">
        <table className="patients-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Username</th>
              <th>Email</th>
              <th>Role</th>
              <th>Linked Doctor ID</th>
            </tr>
          </thead>
          <tbody>
            {staff.map((s) => (
              <tr key={s.id}>
                <td style={{ fontSize: '0.8rem', color: '#666' }}>{s.id.substring(0, 8)}...</td>
                <td style={{ fontWeight: '500' }}>{s.username}</td>
                <td>{s.email || '—'}</td>
                <td>
                  <span className={`status-badge ${s.role === 'Admin' ? 'completed' : s.role === 'Doctor' ? 'review' : 'pending'}`}>
                    {s.role}
                  </span>
                </td>
                <td style={{ fontSize: '0.8rem', color: '#666' }}>
                  {s.doctorId ? `${s.doctorId.substring(0, 8)}...` : '—'}
                </td>
              </tr>
            ))}
            {staff.length === 0 && (
              <tr>
                <td colSpan="5" style={{ textAlign: 'center', padding: '2rem', color: '#888' }}>
                  No staff accounts found.
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      {/* Register Staff Modal */}
      {isRegisterOpen && (
        <div className="modal-overlay">
          <div className="modal-content" style={{ maxWidth: '400px' }}>
            <h3>Register New Staff</h3>
            {error && <div className="error-message" style={{ color: 'red', marginBottom: '1rem', fontSize: '0.9rem' }}>{error}</div>}
            <form onSubmit={handleRegister} className="patient-edit-form">
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
                  style={{ width: '100%', padding: '0.5rem', borderRadius: '4px', border: '1px solid #ccc', marginBottom: '1rem' }}
                >
                  <option value="Staff">Staff</option>
                  <option value="Doctor">Doctor</option>
                  <option value="Admin">Admin</option>
                </select>
              </label>
              
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
