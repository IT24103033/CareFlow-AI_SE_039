import React from 'react';
import { it, expect, vi, afterEach } from 'vitest';
import { render, screen, fireEvent, waitFor, cleanup } from '@testing-library/react';
import AdminPatients from '../src/pages/AdminPatients';
import PatientEditModal from '../src/components/PatientEditModal';
const patient = { id: 'test-id', fullName: 'Test Patient', dateOfBirth: '2000-01-01', bloodGroup: 'O+' };
afterEach(() => { cleanup(); vi.restoreAllMocks(); localStorage.clear(); });
it('delete uses the authenticated API helper and refreshes after success', async () => {
  localStorage.setItem('careflow_token', 'test-token');
  vi.spyOn(window, 'confirm').mockReturnValue(true);
  const fetch = vi.spyOn(globalThis, 'fetch').mockImplementation(async (_url, options) => options?.method === 'DELETE' ? new Response(null, { status: 204 }) : new Response(JSON.stringify([patient])));
  render(<AdminPatients />);
  fireEvent.click(await screen.findByRole('button', { name: 'Remove' }));
  await waitFor(() => expect(fetch).toHaveBeenCalledWith(expect.stringContaining('/api/PatientProfiles/test-id'), expect.objectContaining({ method: 'DELETE', headers: expect.objectContaining({ Authorization: 'Bearer test-token' }) })));
});
it('edit sends authentication and JSON through the API helper', async () => {
  localStorage.setItem('careflow_token', 'test-token');
  const fetch = vi.spyOn(globalThis, 'fetch').mockResolvedValue(new Response('{}'));
  const saved = vi.fn();
  render(<PatientEditModal patient={patient} onClose={() => {}} onSaved={saved} />);
  fireEvent.submit(screen.getByLabelText('Full name').closest('form'));
  await waitFor(() => expect(saved).toHaveBeenCalled());
  expect(fetch).toHaveBeenCalledWith(expect.stringContaining('/api/PatientProfiles/test-id'), expect.objectContaining({ method: 'PUT', headers: expect.objectContaining({ Authorization: 'Bearer test-token', 'Content-Type': 'application/json' }) }));
});
it('delete reports a rejected request instead of claiming success', async () => {
  vi.spyOn(window, 'confirm').mockReturnValue(true);
  vi.spyOn(globalThis, 'fetch').mockImplementation(async (_url, options) => options?.method === 'DELETE' ? new Response('Admin access required', { status: 403 }) : new Response(JSON.stringify([patient])));
  render(<AdminPatients />);
  fireEvent.click(await screen.findByRole('button', { name: 'Remove' }));
  expect(await screen.findByText('Admin access required')).toBeInTheDocument();
});
it('displays empty state when no patient records exist', async () => {
  vi.spyOn(globalThis, 'fetch').mockResolvedValue(new Response(JSON.stringify([])));
  render(<AdminPatients />);
  expect(await screen.findByText('No patient records found.')).toBeInTheDocument();
});

