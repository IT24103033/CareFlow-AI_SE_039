import React, { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { apiFetch } from '../api';
import './staff/ManagePatients.css';

const AdminDashboard = () => {
  const [patients, setPatients] = useState([]);
  const [wards, setWards] = useState([]);
  const [users, setUsers] = useState([]);

  useEffect(() => {
    const load = async () => {
      try {
        const [pRes, wRes, uRes] = await Promise.all([
          apiFetch('/api/PatientProfiles'),
          apiFetch('/api/Wards'),
          apiFetch('/api/Users')
        ]);
        if (pRes.ok) setPatients(await pRes.json());
        if (wRes.ok) setWards(await wRes.json());
        if (uRes.ok) setUsers(await uRes.json());
      } catch (e) {
        console.error(e);
      }
    };
    load();
  }, []);

  const occupied = wards.reduce((sum, w) => sum + (w.occupiedBeds || 0), 0);
  const capacity = wards.reduce((sum, w) => sum + (w.capacity || 0), 0);
  const doctors = users.filter((u) => u.role === 'Doctor').length;
  const staff = users.filter((u) => u.role === 'Staff').length;

  return (
    <div className="manage-patients-container">
      <h2 className="page-title">Admin dashboard</h2>
      <p className="records-hint">Manage hospital accounts, patient records, and ward capacity from one place.</p>
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
        <div className="admin-stat-card">
          <span>Doctors</span>
          <strong>{doctors}</strong>
        </div>
        <div className="admin-stat-card">
          <span>Hospital staff</span>
          <strong>{staff}</strong>
        </div>
      </div>
      <div style={{ display: 'flex', flexWrap: 'wrap', gap: 12 }}>
        <Link to="/admin/patients" className="register-btn" style={{ display: 'inline-block', textDecoration: 'none' }}>
          Patient records
        </Link>
        <Link to="/admin/users" className="register-btn" style={{ display: 'inline-block', textDecoration: 'none' }}>
          Staff &amp; doctors
        </Link>
        <Link to="/admin/wards" className="register-btn" style={{ display: 'inline-block', textDecoration: 'none' }}>
          Manage wards
        </Link>
      </div>
    </div>
  );
};

export default AdminDashboard;
