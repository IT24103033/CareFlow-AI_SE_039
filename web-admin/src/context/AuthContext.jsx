import React, { createContext, useState, useContext } from 'react';
import { getAuth } from '../api';

const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(() => getAuth());

  const login = (data) => {
    localStorage.setItem('careflow_auth', JSON.stringify(data));
    setUser(data);
  };

  const logout = () => {
    localStorage.removeItem('careflow_auth');
    setUser(null);
  };

  return (
    <AuthContext.Provider value={{ user, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  return useContext(AuthContext);
};
