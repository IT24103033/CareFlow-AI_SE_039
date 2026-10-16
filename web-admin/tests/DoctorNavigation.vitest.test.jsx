import React from 'react';
import { it, expect, vi, afterEach } from 'vitest';
import { render, screen, fireEvent, cleanup } from '@testing-library/react';
import App from '../src/App';
import { useAuth } from '../src/context/AuthContext';
vi.mock('../src/context/AuthContext', () => ({ AuthProvider: ({ children }) => children, useAuth: vi.fn() }));
vi.mock('../src/pages/DoctorDashboard', () => ({ default: () => <h1>Doctor overview</h1> }));
vi.mock('../src/pages/TriageReview', () => ({ default: () => <h1>Triage queue</h1> }));
vi.mock('../src/pages/PrescriptionManagement', () => ({ default: () => <h1>Prescription page</h1> }));
vi.mock('../src/pages/AppointmentCalendar', () => ({ default: () => <h1>Appointment availability</h1> }));
afterEach(cleanup);
it('doctor links open four distinct routes', () => {
  useAuth.mockReturnValue({ user: { role: 'Doctor' }, logout: vi.fn() });
  window.history.replaceState({}, '', '/doctor/dashboard');
  render(<App />);
  expect(screen.getByRole('heading', { name: 'Doctor overview' })).toBeInTheDocument();
  for (const [link, heading] of [['Triage Review', 'Triage queue'], ['Prescriptions', 'Prescription page'], ['Appointments', 'Appointment availability'], ['Dashboard', 'Doctor overview']]) {
    fireEvent.click(screen.getByRole('link', { name: new RegExp(link) }));
    expect(screen.getByRole('heading', { name: heading })).toBeInTheDocument();
  }
});
it('staff retains the existing availability route', () => {
  useAuth.mockReturnValue({ user: { role: 'Staff' }, logout: vi.fn() });
  window.history.replaceState({}, '', '/staff/appointments');
  render(<App />);
  expect(screen.getByRole('heading', { name: 'Appointment availability' })).toBeInTheDocument();
});
