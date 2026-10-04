import React, { useEffect, useState } from 'react';
import PatientManagement from './PatientManagement';
import PatientEditModal from '../components/PatientEditModal';
import { apiFetch } from '../services/api';
import './staff/ManagePatients.css';

const AdminPatients = () => {
  const [patients, setPatients] = useState([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [isRegisterOpen, setIsRegisterOpen] = useState(false);
  const [editing, setEditing] = useState(null);
  const [error, setError] = useState('');

  const fetchPatients = async () => {
    try {
      const res = await apiFetch('/api/PatientProfiles');
      if (res.ok) setPatients(await res.json());
    } catch (e) {
      console.error('Failed to fetch patients', e);
    }
  };

  useEffect(() => {
    fetchPatients();
  }, []);

  const handleSearch = async (e) => {
    const term = e.target.value;
    setSearchTerm(term);
    if (term.trim() === '') {
      fetchPatients();
      return;
    }
    try {
      const res = await apiFetch(`/api/PatientProfiles/search?name=${encodeURIComponent(term)}`);
      if (res.ok) setPatients(await res.json());
    } catch (e) {
      console.error('Failed to search', e);
    }
  };

  const handleDelete = async (patient) => {
    const name = patient.fullName || 'this patient';
    if (!window.confirm(`Remove ${name} from hospital records? This also clears their admissions.`)) {
      return;
    }
    setError('');
    try {
      const res = await fetch(`http://localhost:5241/api/PatientProfiles/${patient.id}`, { method: 'DELETE' });
      if (!res.ok) {
        setError(await res.text() || 'Could not delete patient.');
        return;
      }
      fetchPatients();
    } catch {
      setError('Network error deleting patient.');
    }
  };

  return (
    <div className="manage-patients-container">
      <div className="manage-header">
        <h2 className="page-title">Patient records</h2>
        <button className="register-btn" onClick={() => setIsRegisterOpen(true)}>Register patient</button>
      </div>
      <p className="records-hint">Update demographics and medical history, or remove a record if it was created in error.</p>
      {error && <p className="ai-error">{error}</p>}

      <div className="search-bar-container">
        <span className="search-icon">🔍</span>
        <input
          type="text"
          placeholder="Search by name"
          value={searchTerm}
          onChange={handleSearch}
          className="search-input"
        />
      </div>

      <div className="patients-grid">
        {patients.map((p) => (
          <div className="patient-tile" key={p.id}>
            <div className="patient-tile-header">
              <div className="avatar"></div>
              <h3 className="patient-name">{p.fullName}</h3>
            </div>
            <div className="patient-tile-body">
              <p><strong>DOB:</strong> {String(p.dateOfBirth || '').slice(0, 10)}</p>
              <p><strong>Blood Group:</strong> {p.bloodGroup || 'N/A'}</p>
              <p><strong>History:</strong> {p.medicalHistorySummary || 'N/A'}</p>
            </div>
            <div className="patient-tile-actions">
              <button className="admit-btn" onClick={() => setEditing(p)}>Edit</button>
              <button className="danger-btn" onClick={() => handleDelete(p)}>Remove</button>
            </div>
          </div>
        ))}
      </div>

      {isRegisterOpen && (
        <div className="modal-overlay">
          <div className="modal-content">
            <button className="close-btn" onClick={() => setIsRegisterOpen(false)}>×</button>
            <PatientManagement
              onRegistered={() => {
                setIsRegisterOpen(false);
                fetchPatients();
              }}
            />
          </div>
        </div>
      )}

      {editing && (
        <PatientEditModal
          patient={editing}
          onClose={() => setEditing(null)}
          onSaved={() => {
            setEditing(null);
            fetchPatients();
          }}
        />
      )}
    </div>
  );
};

export default AdminPatients;
