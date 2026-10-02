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
import PatientManagement from './pages/PatientManagement';
import AiAnalysis from './pages/AiAnalysis';
import PatientHistorySearch from './pages/PatientHistorySearch';
import TriageReview from './pages/TriageReview';

// Component C: Appointments & Resource Scheduling
import AppointmentCalendar from './pages/AppointmentCalendar';

// Component D: Pharmacy
import InventoryManagement from './pages/InventoryManagement';
import PrescriptionManagement from './pages/PrescriptionManagement';

import './App.css';

// Navigation links for the sidebar based on role
const staffLinks = [
  { path: '/staff/patients', label: 'Patient Details', icon: '👤' },
  { path: '/staff/wards', label: 'Wards', icon: '🛏️' },
  { path: '/staff/pharmacy', label: 'Pharmacy & Inventory', icon: '💊' },
  { path: '/staff/prescriptions', label: 'Prescriptions', icon: '📋' },
  { path: '/staff/history-search', label: 'History Search', icon: '🔍' },
  { path: '/staff/ai-analysis', label: 'AI Analysis', icon: '🤖' },
  { path: '/staff/appointments', label: 'Appointments', icon: '📅' }
];

const adminLinks = [
  { path: '/admin', label: 'Dashboard', icon: '📊' },
];

const doctorLinks = [
  { path: '/doctor/dashboard', label: 'Dashboard', icon: '👥' },
  { path: '/doctor/triage', label: 'Triage Review', icon: '🩺' },
  { path: '/doctor/prescriptions', label: 'Prescriptions', icon: '📋' },
];

function App() {
  return (
    <AuthProvider>
      <Router>
        <Routes>
          <Route path="/login" element={<Login />} />

          {/* Admin Routes */}
          <Route
            path="/admin"
            element={
              <ProtectedRoute allowedRoles={['Admin']}>
                <SidebarLayout role="Admin" links={adminLinks} />
              </ProtectedRoute>
            }
          >
            <Route index element={<AdminDashboard />} />
          </Route>

          {/* Doctor Routes */}
          <Route
            path="/doctor"
            element={
              <ProtectedRoute allowedRoles={['Doctor']}>
                <SidebarLayout role="Doctor Staff" links={doctorLinks} />
              </ProtectedRoute>
            }
          >
            <Route index element={<Navigate to="/doctor/dashboard" replace />} />
            <Route path="dashboard" element={<DoctorDashboard />} />
            <Route path="triage" element={<TriageReview />} />
            <Route path="prescriptions" element={<PrescriptionManagement />} />
          </Route>

          {/* Staff Routes */}
          <Route
            path="/staff"
            element={
              <ProtectedRoute allowedRoles={['Staff', 'Admin']}>
                <SidebarLayout role="Hospital Staff" links={staffLinks} />
              </ProtectedRoute>
            }
          >
            <Route
              index
              element={<Navigate to="/staff/patients" replace />}
            />

            <Route
              path="patients"
              element={<ManagePatients />}
            />

            <Route
              path="wards"
              element={<WardManagement />}
            />

            <Route
              path="pharmacy"
              element={<InventoryManagement />}
            />

            <Route
              path="prescriptions"
              element={<PrescriptionManagement />}
            />

            <Route
              path="history-search"
              element={<PatientHistorySearch />}
            />

            <Route
              path="ai-analysis"
              element={<AiAnalysis />}
            />

            {/* Component C: Appointments & Resource Scheduling */}
            <Route
              path="appointments"
              element={<AppointmentCalendar />}
            />

            <Route
              path="*"
              element={
                <div className="placeholder-view">
                  Feature Coming Soon
                </div>
              }
            />
          </Route>

          {/* Direct routes for backward compatibility */}
          <Route
            path="/triage"
            element={
              <ProtectedRoute allowedRoles={['Doctor']}>
                <TriageReview />
              </ProtectedRoute>
            }
          />

          <Route
            path="/inventory"
            element={
              <ProtectedRoute allowedRoles={['Staff', 'Admin']}>
                <InventoryManagement />
              </ProtectedRoute>
            }
          />

          <Route
            path="/prescriptions"
            element={
              <ProtectedRoute allowedRoles={['Doctor', 'Staff', 'Admin']}>
                <PrescriptionManagement />
              </ProtectedRoute>
            }
          />

          <Route
            path="/wards"
            element={
              <ProtectedRoute allowedRoles={['Doctor', 'Staff', 'Admin']}>
                <WardManagement />
              </ProtectedRoute>
            }
          />

          <Route
            path="/patients"
            element={
              <ProtectedRoute allowedRoles={['Doctor', 'Staff', 'Admin']}>
                <PatientManagement />
              </ProtectedRoute>
            }
          />

          <Route
            path="/ai-analysis"
            element={
              <ProtectedRoute allowedRoles={['Doctor', 'Staff', 'Admin']}>
                <AiAnalysis />
              </ProtectedRoute>
            }
          />

          <Route
            path="/history-search"
            element={
              <ProtectedRoute allowedRoles={['Doctor', 'Staff', 'Admin']}>
                <PatientHistorySearch />
              </ProtectedRoute>
            }
          />

          {/* Fallback routing */}
          <Route
            path="/"
            element={<Navigate to="/login" replace />}
          />

          <Route
            path="*"
            element={<Navigate to="/login" replace />}
          />
        </Routes>
      </Router>
    </AuthProvider>
  );
}

export default App;