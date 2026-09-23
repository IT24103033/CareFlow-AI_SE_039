import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import './Login.css'; // We will create this
import logo from '../assets/logo.png';
import loginPic from '../assets/login_pic.png';

const Login = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleLogin = (e) => {
    e.preventDefault();
    // Simulate login based on email. Real app would call backend.
    let role = 'Staff'; // Default
    if (email.includes('admin')) role = 'Admin';
    if (email.includes('doctor')) role = 'Doctor';
    
    login(role);

    if (role === 'Admin') navigate('/admin');
    else if (role === 'Doctor') navigate('/doctor');
    else navigate('/staff/patients');
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
                placeholder="Email Address" 
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
