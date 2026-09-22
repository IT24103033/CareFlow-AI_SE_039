import React from 'react';
import { BrowserRouter as Router, Routes, Route, Link } from 'react-router-dom';
import WardManagement from './pages/WardManagement';
import PatientManagement from './pages/PatientManagement';
import AiAnalysis from './pages/AiAnalysis';
import './App.css';

function App() {
  return (
    <Router>
      {/* This is our top navigation bar */}
      <nav style={{ padding: '15px', background: '#f0f0f0', borderBottom: '2px solid #ccc' }}>
        <Link to="/" style={{ marginRight: '20px', textDecoration: 'none', color: '#333', fontWeight: 'bold' }}>
          CareFlow AI Home
        </Link>
        <Link to="/wards" style={{ textDecoration: 'none', color: '#0066cc', fontWeight: 'bold' }}>
          Ward Management
        </Link>
        <Link to="/patients" style={{ marginRight: '20px', textDecoration: 'none', color: '#0066cc', fontWeight: 'bold' }}>
          Patient Registration
        </Link>
        <Link to="/ai-analysis" style={{ marginRight: '20px', textDecoration: 'none', color: '#6f42c1', fontWeight: 'bold' }}>
          AI Analysis
        </Link>
      </nav>

      {/* These are the different pages we can navigate to */}
      <Routes>
        <Route path="/" element={<div style={{ padding: '20px' }}><h2>Welcome to CareFlow AI</h2><p>Select a module from the navigation above.</p></div>} />
        <Route path="/wards" element={<WardManagement />} />
        <Route path="/patients" element={<PatientManagement />} />
        <Route path="/ai-analysis" element={<AiAnalysis />} />
      </Routes>
    </Router>
  );
}

export default App;