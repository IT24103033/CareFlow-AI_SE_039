import React, { useEffect, useState } from 'react';
import { apiFetch } from '../api';
import './staff/ManagePatients.css';

const emptyForm = {
  username: '',
  password: '',
  role: 'Staff'
};

const AdminUsers = () => {
  const [users, setUsers] = useState([]);
  const [roleFilter, setRoleFilter] = useState('');
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editing, setEditing] = useState(null);
  const [formData, setFormData] = useState(emptyForm);
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);

  const fetchUsers = async () => {
    try {
      const query = roleFilter ? `?role=${encodeURIComponent(roleFilter)}` : '';
      const res = await apiFetch(`/api/Users${query}`);
      if (res.ok) setUsers(await res.json());
      else setError(await res.text() || 'Could not load users.');
    } catch {
      setError('Network error loading users.');
    }
  };

  useEffect(() => {
    fetchUsers();
  }, [roleFilter]);

  const openCreate = () => {
    setEditing(null);
    setFormData(emptyForm);
    setError('');
    setIsFormOpen(true);
  };

  const openEdit = (user) => {
    setEditing(user);
    setFormData({
      username: user.username || '',
      password: '',
      role: user.role || 'Staff'
    });
    setError('');
    setIsFormOpen(true);
  };

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSaving(true);
    setError('');

    const payload = {
      username: formData.username.trim(),
      role: formData.role,
      password: formData.password || null
    };

    if (!editing && (!payload.password || payload.password.length < 6)) {
      setError('Password must be at least 6 characters.');
      setSaving(false);
      return;
    }

    try {
      const res = await apiFetch(editing ? `/api/Users/${editing.id}` : '/api/Users', {
        method: editing ? 'PUT' : 'POST',
        body: JSON.stringify(payload)
      });

      if (!res.ok) {
        setError((await res.text()) || 'Could not save user.');
        return;
      }

      setIsFormOpen(false);
      setEditing(null);
      setFormData(emptyForm);
      fetchUsers();
    } catch {
      setError('Network error saving user.');
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (user) => {
    if (!window.confirm(`Remove account "${user.username}" (${user.role})? They will no longer be able to log in.`)) {
      return;
    }
    setError('');
    try {
      const res = await apiFetch(`/api/Users/${user.id}`, { method: 'DELETE' });
      if (!res.ok) {
        setError(await res.text() || 'Could not delete user.');
        return;
      }
      fetchUsers();
    } catch {
      setError('Network error deleting user.');
    }
  };

  return (
    <div className="manage-patients-container">
      <div className="manage-header">
        <h2 className="page-title">Staff &amp; doctors</h2>
        <button className="register-btn" onClick={openCreate}>Add account</button>
      </div>
      <p className="records-hint">
        Accounts live in the Users table (Username, Password, Role). Staff and doctors cannot self-register — create login credentials here.
      </p>
      {error && !isFormOpen && <p className="ai-error">{error}</p>}

      <div className="search-bar-container" style={{ maxWidth: 260 }}>
        <select
          className="search-input"
          value={roleFilter}
          onChange={(e) => setRoleFilter(e.target.value)}
          style={{ marginLeft: 0 }}
        >
          <option value="">All roles</option>
          <option value="Admin">Admin</option>
          <option value="Doctor">Doctor</option>
          <option value="Staff">Staff</option>
        </select>
      </div>

      <div className="patients-grid">
        {users.map((u) => (
          <div className="patient-tile" key={u.id}>
            <div className="patient-tile-header">
              <div className="avatar"></div>
              <h3 className="patient-name">{u.username}</h3>
            </div>
            <div className="patient-tile-body">
              <p><strong>Role:</strong> {u.role}</p>
              <p><strong>Login:</strong> use username + password at the login page</p>
            </div>
            <div className="patient-tile-actions">
              <button className="admit-btn" onClick={() => openEdit(u)}>Edit</button>
              <button className="danger-btn" onClick={() => handleDelete(u)}>Remove</button>
            </div>
          </div>
        ))}
        {users.length === 0 && <p className="records-hint">No accounts found for this filter.</p>}
      </div>

      {isFormOpen && (
        <div className="modal-overlay">
          <div className="modal-content">
            <button className="close-btn" onClick={() => setIsFormOpen(false)}>×</button>
            <div style={{ padding: '8px 0', maxWidth: 500 }}>
              <h2 style={{ marginTop: 0 }}>{editing ? 'Edit account' : 'Add account'}</h2>
              <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 15 }}>
                <div style={{ display: 'flex', flexDirection: 'column', gap: 5 }}>
                  <label style={{ fontSize: 14, fontWeight: 'bold' }}>Username</label>
                  <input
                    type="text"
                    name="username"
                    placeholder="e.g., nurse.kamala"
                    value={formData.username}
                    onChange={handleChange}
                    required
                    style={{ padding: 10, borderRadius: 6, border: '1px solid #ccc' }}
                  />
                </div>
                <div style={{ display: 'flex', flexDirection: 'column', gap: 5 }}>
                  <label style={{ fontSize: 14, fontWeight: 'bold' }}>
                    Password {editing ? '(leave blank to keep current)' : ''}
                  </label>
                  <input
                    type="password"
                    name="password"
                    placeholder={editing ? 'Optional new password' : 'At least 6 characters'}
                    value={formData.password}
                    onChange={handleChange}
                    required={!editing}
                    minLength={editing ? undefined : 6}
                    style={{ padding: 10, borderRadius: 6, border: '1px solid #ccc' }}
                  />
                </div>
                <div style={{ display: 'flex', flexDirection: 'column', gap: 5 }}>
                  <label style={{ fontSize: 14, fontWeight: 'bold' }}>Role</label>
                  <select
                    name="role"
                    value={formData.role}
                    onChange={handleChange}
                    required
                    style={{ padding: 10, borderRadius: 6, border: '1px solid #ccc' }}
                  >
                    <option value="Staff">Staff</option>
                    <option value="Doctor">Doctor</option>
                    <option value="Admin">Admin</option>
                  </select>
                </div>
                {error && <p className="ai-error">{error}</p>}
                <button
                  type="submit"
                  disabled={saving}
                  style={{ padding: 12, background: '#0ab39c', color: 'white', border: 'none', borderRadius: 6, fontWeight: 'bold', cursor: 'pointer', marginTop: 10 }}
                >
                  {saving ? 'Saving...' : editing ? 'Save changes' : 'Create account'}
                </button>
              </form>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default AdminUsers;
