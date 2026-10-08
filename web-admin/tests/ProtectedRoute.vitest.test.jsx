import React from 'react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import {
  MemoryRouter,
  Route,
  Routes
} from 'react-router-dom';
import { render, screen } from '@testing-library/react';

import ProtectedRoute from '../src/components/ProtectedRoute';
import { useAuth } from '../src/context/AuthContext';

vi.mock('../src/context/AuthContext', () => ({
  useAuth: vi.fn()
}));

const renderProtectedRoute = ({
  user,
  allowedRoles
}) => {
  useAuth.mockReturnValue({
    user
  });

  return render(
    <MemoryRouter initialEntries={['/protected']}>
      <Routes>
        <Route
          path="/protected"
          element={
            <ProtectedRoute allowedRoles={allowedRoles}>
              <div>Protected Content</div>
            </ProtectedRoute>
          }
        />

        <Route
          path="/login"
          element={<div>Login Page</div>}
        />
      </Routes>
    </MemoryRouter>
  );
};

describe('ProtectedRoute component', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('redirects an unauthenticated user to the login page', () => {
    renderProtectedRoute({
      user: null
    });

    expect(
      screen.getByText('Login Page')
    ).toBeInTheDocument();

    expect(
      screen.queryByText('Protected Content')
    ).not.toBeInTheDocument();
  });

  it('allows an authenticated user when their role is permitted', () => {
    renderProtectedRoute({
      user: {
        role: 'Admin'
      },
      allowedRoles: ['Admin']
    });

    expect(
      screen.getByText('Protected Content')
    ).toBeInTheDocument();

    expect(
      screen.queryByText('Login Page')
    ).not.toBeInTheDocument();
  });

  it('redirects an authenticated user when their role is not permitted', () => {
    renderProtectedRoute({
      user: {
        role: 'Patient'
      },
      allowedRoles: ['Admin', 'Doctor', 'Staff']
    });

    expect(
      screen.getByText('Login Page')
    ).toBeInTheDocument();

    expect(
      screen.queryByText('Protected Content')
    ).not.toBeInTheDocument();
  });

  it('allows any authenticated user when no allowedRoles restriction is provided', () => {
    renderProtectedRoute({
      user: {
        role: 'Patient'
      }
    });

    expect(
      screen.getByText('Protected Content')
    ).toBeInTheDocument();

    expect(
      screen.queryByText('Login Page')
    ).not.toBeInTheDocument();
  });
});