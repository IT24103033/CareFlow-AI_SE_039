import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';

import Login from './pages/Login';
import SidebarLayout from './components/SidebarLayout';
import AdminDashboard from './pages/AdminDashboard';
import DoctorDashboard from './pages/DoctorDashboard';
import ManagePatients from './pages/staff/ManagePatients';
import WardManagement from './pages/WardManagement';
// Retain old pages just in case, but they are not used in new flow
import AiAnalysis from './pages/AiAnalysis';
import PatientHistorySearch from './pages/PatientHistorySearch';

import './App.css';

// Navigation links for the sidebar based on role
const staffLinks = [
  { path: '/staff/patients', label: 'Patient Details', icon: '👤' },
  { path: '/staff/triage', label: 'Triage & Intake Queue', icon: '🩺' },
  { path: '/staff/wards', label: 'Wards', icon: '🛏️' },
  { path: '/staff/pharmacy', label: 'Pharmacy & Inventory', icon: '💊' },
  { path: '/staff/appointments', label: 'Appointments', icon: '📅' }
];

const adminLinks = [
  { path: '/admin', label: 'Dashboard', icon: '📊' },
  // ...other admin links
];

const doctorLinks = [
  { path: '/doctor', label: 'My Patients', icon: '👥' },
  // ...other doctor links
];

function App() {
  return (
    <AuthProvider>
      <Router>
        <Routes>
          <Route path="/login" element={<Login />} />
          
          {/* Admin Routes */}
          <Route path="/admin" element={
            <ProtectedRoute allowedRoles={['Admin']}>
              <SidebarLayout role="Admin" links={adminLinks} />
            </ProtectedRoute>
          }>
            <Route index element={<AdminDashboard />} />
          </Route>

          {/* Doctor Routes */}
          <Route path="/doctor" element={
            <ProtectedRoute allowedRoles={['Doctor']}>
              <SidebarLayout role="Doctor Staff" links={doctorLinks} />
            </ProtectedRoute>
          }>
            <Route index element={<DoctorDashboard />} />
          </Route>

          {/* Staff Routes */}
          <Route path="/staff" element={
            <ProtectedRoute allowedRoles={['Staff']}>
              <SidebarLayout role="Hospital Staff" links={staffLinks} />
            </ProtectedRoute>
          }>
            <Route index element={<Navigate to="/staff/patients" replace />} />
            <Route path="patients" element={<ManagePatients />} />
            <Route path="wards" element={<WardManagement />} />
            <Route path="*" element={<div className="placeholder-view">Feature Coming Soon</div>} />
          </Route>

          {/* Fallback routing */}
          <Route path="/" element={<Navigate to="/login" replace />} />
          <Route path="*" element={<Navigate to="/login" replace />} />
        </Routes>
      </Router>
    </AuthProvider>
  );
}

export default App;