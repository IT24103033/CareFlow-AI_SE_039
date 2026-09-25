import React from 'react';
import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';
import WardManagement from './pages/WardManagement';
import PatientManagement from './pages/PatientManagement';
// ── Component D: Pharmacy (Amodhya) ──────────────────────────────────────────
import InventoryManagement from './pages/InventoryManagement';
import PrescriptionManagement from './pages/PrescriptionManagement';
import './App.css';

const navLink = { textDecoration: 'none', color: '#0066cc', fontWeight: 'bold', marginRight: '20px' };

function App() {
  return (
    <Router>
      {/* Top navigation bar */}
      <nav style={{ padding: '15px 24px', background: '#1a1a2e', borderBottom: '3px solid #6c63ff', display: 'flex', alignItems: 'center', gap: 4, flexWrap: 'wrap' }}>
        <Link to="/" style={{ ...navLink, color: '#fff', fontSize: 17, marginRight: 28 }}>
          🏥 CareFlow AI
        </Link>
        <Link to="/wards"         style={{ ...navLink, color: '#a9b4d4' }}>Ward Management</Link>
        <Link to="/patients"      style={{ ...navLink, color: '#a9b4d4' }}>Patient Registration</Link>
        {/* Component D links */}
        <Link to="/inventory"     style={{ ...navLink, color: '#c3b1e1' }}>💊 Inventory</Link>
        <Link to="/prescriptions" style={{ ...navLink, color: '#c3b1e1' }}>📋 Prescriptions</Link>
      </nav>

      {/* Page routes */}
      <Routes>
        <Route path="/" element={
          <div style={{ padding: '20px' }}>
            <h2>Welcome to CareFlow AI</h2>
            <p>Select a module from the navigation above.</p>
          </div>
        } />
        <Route path="/wards"         element={<WardManagement />} />
        <Route path="/patients"      element={<PatientManagement />} />
        {/* ── Component D: Pharmacy ─────────────────────────────────────── */}
        <Route path="/inventory"     element={<InventoryManagement />} />
        <Route path="/prescriptions" element={<PrescriptionManagement />} />
      </Routes>
    </Router>
  );
}

export default App;