import React from 'react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import {
  MemoryRouter,
  Route,
  Routes
} from 'react-router-dom';

import Login from '../src/pages/Login';
import { useAuth } from '../src/context/AuthContext';

vi.mock('../src/context/AuthContext', () => ({
  useAuth: vi.fn()
}));

const renderLogin = (loginMock = vi.fn()) => {
  useAuth.mockReturnValue({
    login: loginMock
  });

  return render(
    <MemoryRouter initialEntries={['/login']}>
      <Routes>
        <Route
          path="/login"
          element={<Login />}
        />

        <Route
          path="/admin"
          element={<div>Admin Dashboard</div>}
        />

        <Route
          path="/doctor/dashboard"
          element={<div>Doctor Dashboard</div>}
        />

        <Route
          path="/staff/patients"
          element={<div>Staff Patients</div>}
        />
      </Routes>
    </MemoryRouter>
  );
};

describe('Login component', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    localStorage.clear();
  });

  it('renders the login page with required controls', () => {
    renderLogin();

    expect(
      screen.getByRole('heading', { name: 'Login' })
    ).toBeInTheDocument();

    expect(
      screen.getByPlaceholderText('Username or Email')
    ).toBeInTheDocument();

    expect(
      screen.getByPlaceholderText('Password')
    ).toBeInTheDocument();

    expect(
      screen.getByRole('button', { name: 'Login' })
    ).toBeInTheDocument();

    expect(
      screen.getByAltText('CareFlow Logo')
    ).toBeInTheDocument();
  });

  it('marks username and password as required and blocks empty submission', async () => {
    const user = userEvent.setup();
    const loginMock = vi.fn();

    renderLogin(loginMock);

    const username =
      screen.getByPlaceholderText('Username or Email');

    const password =
      screen.getByPlaceholderText('Password');

    expect(username).toBeRequired();
    expect(password).toBeRequired();

    await user.click(
      screen.getByRole('button', { name: 'Login' })
    );

    expect(username).toBeInvalid();
    expect(password).toBeInvalid();

    expect(loginMock).not.toHaveBeenCalled();
  });

  it('shows an error message when login fails', async () => {
    const user = userEvent.setup();

    const loginMock = vi
      .fn()
      .mockRejectedValue(
        new Error('Invalid username or password.')
      );

    renderLogin(loginMock);

    await user.type(
      screen.getByPlaceholderText('Username or Email'),
      'wrong_user'
    );

    await user.type(
      screen.getByPlaceholderText('Password'),
      'wrong_password'
    );

    await user.click(
      screen.getByRole('button', { name: 'Login' })
    );

    expect(
      await screen.findByText(
        'Invalid username or password.'
      )
    ).toBeInTheDocument();

    expect(loginMock).toHaveBeenCalledWith(
      'wrong_user',
      'wrong_password'
    );
  });

  it('navigates an Admin user to the admin dashboard', async () => {
    const user = userEvent.setup();

    const loginMock = vi.fn().mockResolvedValue({
      user: {
        role: 'Admin'
      }
    });

    renderLogin(loginMock);

    await user.type(
      screen.getByPlaceholderText('Username or Email'),
      'admin'
    );

    await user.type(
      screen.getByPlaceholderText('Password'),
      'AdminPass123!'
    );

    await user.click(
      screen.getByRole('button', { name: 'Login' })
    );

    expect(
      await screen.findByText('Admin Dashboard')
    ).toBeInTheDocument();
  });

  it('navigates a Doctor user to the doctor dashboard', async () => {
    const user = userEvent.setup();

    const loginMock = vi.fn().mockResolvedValue({
      user: {
        role: 'Doctor'
      }
    });

    renderLogin(loginMock);

    await user.type(
      screen.getByPlaceholderText('Username or Email'),
      'doctor_alice'
    );

    await user.type(
      screen.getByPlaceholderText('Password'),
      'DoctorPass123!'
    );

    await user.click(
      screen.getByRole('button', { name: 'Login' })
    );

    expect(
      await screen.findByText('Doctor Dashboard')
    ).toBeInTheDocument();
  });

  it('navigates a Staff user to the staff patients page', async () => {
    const user = userEvent.setup();

    const loginMock = vi.fn().mockResolvedValue({
      user: {
        role: 'Staff'
      }
    });

    renderLogin(loginMock);

    await user.type(
      screen.getByPlaceholderText('Username or Email'),
      'staff_member'
    );

    await user.type(
      screen.getByPlaceholderText('Password'),
      'StaffPass123!'
    );

    await user.click(
      screen.getByRole('button', { name: 'Login' })
    );

    expect(
      await screen.findByText('Staff Patients')
    ).toBeInTheDocument();
  });

  it('shows the portal error for an unsupported role', async () => {
    const user = userEvent.setup();

    const loginMock = vi.fn().mockResolvedValue({
      user: {
        role: 'Patient'
      }
    });

    renderLogin(loginMock);

    await user.type(
      screen.getByPlaceholderText('Username or Email'),
      'patient_user'
    );

    await user.type(
      screen.getByPlaceholderText('Password'),
      'PatientPass123!'
    );

    await user.click(
      screen.getByRole('button', { name: 'Login' })
    );

    expect(
      await screen.findByText(
        'This portal is reserved for clinical staff, doctors, and administrators.'
      )
    ).toBeInTheDocument();
  });

  it('shows the loading state and disables the form while signing in', async () => {
    const user = userEvent.setup();

    let resolveLogin;

    const loginMock = vi.fn(
      () =>
        new Promise((resolve) => {
          resolveLogin = resolve;
        })
    );

    renderLogin(loginMock);

    const username =
      screen.getByPlaceholderText('Username or Email');

    const password =
      screen.getByPlaceholderText('Password');

    const button =
      screen.getByRole('button', { name: 'Login' });

    await user.type(username, 'admin');
    await user.type(password, 'AdminPass123!');

    await user.click(button);

    expect(
      await screen.findByRole('button', {
        name: 'Signing in...'
      })
    ).toBeDisabled();

    expect(username).toBeDisabled();
    expect(password).toBeDisabled();

    resolveLogin({
      user: {
        role: 'Admin'
      }
    });

    await waitFor(() => {
      expect(
        screen.getByText('Admin Dashboard')
      ).toBeInTheDocument();
    });
  });
});