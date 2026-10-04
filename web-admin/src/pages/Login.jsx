import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { API_BASE } from '../api';
import './Login.css';
import logo from '../assets/logo.png';
import loginPic from '../assets/login_pic.png';

const Login = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleLogin = async (e) => {
    e.preventDefault();
    setError('');
    try {
      const res = await fetch(`${API_BASE}/api/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username: email, password })
      });
      const data = await res.json().catch(() => null);
      if (!res.ok) {
        setError(data?.error || 'Invalid username or password.');
        return;
      }

      login({ token: data.token, role: data.role, username: data.username });

      if (data.role === 'Admin') navigate('/admin');
      else if (data.role === 'Doctor') navigate('/doctor');
      else navigate('/staff/patients');
    } catch {
      setError('Could not reach the API. Is the backend running?');
    }
  };

  return (
    <div className="login-container">
      <div className="login-card">
        <div className="login-left">
          <div className="login-logo">
            <img src={logo} alt="CareFlow Logo" style={{ width: '40px', height: '40px', objectFit: 'contain' }} />
            <div>
              <h2 className="logo-title">CareFlow AI</h2>
              <p className="logo-subtitle">Hospital Management System</p>
            </div>
          </div>
          
          <h1 className="login-heading">Login</h1>
          
          <form onSubmit={handleLogin} className="login-form">
            <div className="input-group">
              <span className="input-icon">✉</span>
              <input 
                type="text" 
                placeholder="Username (admin, doctor, or staff)" 
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />
            </div>
            <div className="input-group">
              <span className="input-icon">🔑</span>
              <input 
                type="password" 
                placeholder="Password" 
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
              />
            </div>
            {error && <p className="ai-error">{error}</p>}
            <button type="submit" className="login-btn">Login</button>
          </form>
        </div>
        <div className="login-right">
          <img src={loginPic} alt="Illustration" style={{ width: '100%', height: '100%', objectFit: 'cover', borderTopLeftRadius: '30px', borderBottomLeftRadius: '30px' }} />
        </div>
      </div>
    </div>
  );
};

export default Login;
