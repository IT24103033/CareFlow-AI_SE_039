import React from 'react';
import { Navigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

const ProtectedRoute = ({ children, allowedRoles }) => {
  const { user } = useAuth();

  if (!user) {
    return <Navigate to="/login" replace />;
  }

  if (allowedRoles && !allowedRoles.includes(user.role)) {
    // If the user doesn't have the right role, redirect them to login (or a designated unauthorized page)
    return <Navigate to="/login" replace />;
  }

  return children;
};

export default ProtectedRoute;
