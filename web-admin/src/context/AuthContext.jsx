import React, { createContext, useState, useContext, useEffect, useCallback } from 'react';
import { authApi, getStoredToken, getStoredUser, setStoredToken, setStoredUser, clearAuthStorage } from '../services/api';

const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
  const [token, setToken] = useState(() => getStoredToken());
  const [user, setUser] = useState(() => getStoredUser());
  const [loading, setLoading] = useState(false);

  const logout = useCallback(() => {
    clearAuthStorage();
    setToken(null);
    setUser(null);
  }, []);

  // Listen for unauthorized 401 events from apiFetch
  useEffect(() => {
    const handleUnauthorized = () => {
      logout();
    };
    window.addEventListener('careflow:unauthorized', handleUnauthorized);
    return () => {
      window.removeEventListener('careflow:unauthorized', handleUnauthorized);
    };
  }, [logout]);

  const login = async (username, password) => {
    setLoading(true);
    try {
      const data = await authApi.login(username, password);
      setStoredToken(data.token);
      setStoredUser(data.user);
      setToken(data.token);
      setUser(data.user);
      return data;
    } finally {
      setLoading(false);
    }
  };

  const getAccessToken = useCallback(() => {
    return token || getStoredToken();
  }, [token]);

  return (
    <AuthContext.Provider value={{
      user,
      token,
      isAuthenticated: Boolean(token && user),
      loading,
      login,
      logout,
      getAccessToken
    }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  return useContext(AuthContext);
};
