import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import './Login.css';
import logo from '../assets/logo.png';
import loginPic from '../assets/login_pic.png';

const Login = () => {
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleLogin = async (e) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const data = await login(username, password);
      const role = data.user?.role;

      if (role === 'Admin') {
        navigate('/admin');
      } else if (role === 'Doctor') {
        navigate('/doctor/dashboard');
      } else if (role === 'Staff') {
        navigate('/staff/patients');
      } else {
        setError('This portal is reserved for clinical staff, doctors, and administrators.');
      }
    } catch (err) {
      setError(err.message || 'Invalid username or password.');
    } finally {
      setLoading(false);
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
          
          {error && (
            <div style={{
              backgroundColor: '#fee2e2',
              color: '#b91c1c',
              padding: '10px 14px',
              borderRadius: '8px',
              marginBottom: '16px',
              fontSize: '14px'
            }}>
              {error}
            </div>
          )}

          <form onSubmit={handleLogin} className="login-form">
            <div className="input-group">
              <span className="input-icon">👤</span>
              <input 
                type="text" 
                placeholder="Username or Email" 
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                required
                disabled={loading}
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
                disabled={loading}
              />
            </div>
            <button type="submit" className="login-btn" disabled={loading}>
              {loading ? 'Signing in...' : 'Login'}
            </button>
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
