import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { apiFetch } from '../services/api';
import './staff/ManagePatients.css';

const AdminDashboard = () => {
  const [patients, setPatients] = useState([]);
  const [wards, setWards] = useState([]);

  useEffect(() => {
    const load = async () => {
      try {
        const [pRes, wRes] = await Promise.all([
          apiFetch('/api/PatientProfiles'),
          apiFetch('/api/Wards')
        ]);
        if (pRes.ok) setPatients(await pRes.json());
        if (wRes.ok) setWards(await wRes.json());
      } catch (e) {
        console.error(e);
      }
    };
    load();
  }, []);

  const occupied = wards.reduce((sum, w) => sum + (w.occupiedBeds || 0), 0);
  const capacity = wards.reduce((sum, w) => sum + (w.capacity || 0), 0);

  return (
    <div className="manage-patients-container">
      <h2 className="page-title">Admin dashboard</h2>
      <p className="records-hint">Live counts from the hospital database.</p>
      <div className="admin-stat-grid">
        <div className="admin-stat-card">
          <span>Patients on file</span>
          <strong>{patients.length}</strong>
        </div>
        <div className="admin-stat-card">
          <span>Beds occupied</span>
          <strong>{occupied} / {capacity || '—'}</strong>
        </div>
        <div className="admin-stat-card">
          <span>Wards</span>
          <strong>{wards.length}</strong>
        </div>
      </div>
    </div>
  );
};

export default AdminDashboard;
