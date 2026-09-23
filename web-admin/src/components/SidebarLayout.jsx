import React from 'react';
import { Outlet, useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import './SidebarLayout.css'; // Create later
import logo from '../assets/logo.png';

const SidebarLayout = ({ role, links }) => {
  const { logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    if (window.confirm("Are you sure you want to log out?")) {
      logout();
      navigate('/login');
    }
  };

  return (
    <div className="layout-container">
      <aside className="sidebar">
        <div className="sidebar-header">
          <img src={logo} alt="CareFlow Logo" style={{ width: '36px', height: '36px', objectFit: 'contain' }} />
          <div className="sidebar-brand">
             <h3>CareFlow</h3>
             <span>{role}</span>
          </div>
        </div>
        <nav className="sidebar-nav">
          {links.map((link, idx) => (
             <Link key={idx} to={link.path} className="nav-link">
               <span className="nav-icon">{link.icon}</span>
               {link.label}
             </Link>
          ))}
        </nav>
      </aside>
      <main className="main-content">
        <header className="topbar">
          <div className="topbar-right">
             <button onClick={handleLogout} className="logout-btn">logout</button>
          </div>
        </header>
        <div className="content-area">
          <Outlet />
        </div>
      </main>
    </div>
  );
};

export default SidebarLayout;
