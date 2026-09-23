import React, { useState, useEffect } from 'react';
import PatientManagement from '../PatientManagement';
import './ManagePatients.css'; // updated CSS

const ManagePatients = () => {
  const [patients, setPatients] = useState([]);
  const [wards, setWards] = useState([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [isRegisterModalOpen, setIsRegisterModalOpen] = useState(false);
  const [isAiOpen, setIsAiOpen] = useState(false);
  const [aiInput, setAiInput] = useState({ name: '', symptoms: '' });
  const [aiResponse, setAiResponse] = useState(null);

  // Admission Modal State
  const [isAdmitModalOpen, setIsAdmitModalOpen] = useState(false);
  const [selectedPatientId, setSelectedPatientId] = useState('');
  const [selectedWardId, setSelectedWardId] = useState('');

  useEffect(() => {
    fetchPatients();
    fetchWards();
  }, []);

  const fetchPatients = async () => {
    try {
      const res = await fetch('http://localhost:5241/api/PatientProfiles');
      if (res.ok) {
        const data = await res.json();
        setPatients(data);
      }
    } catch (e) {
      console.error('Failed to fetch patients', e);
    }
  };

  const fetchWards = async () => {
    try {
      const res = await fetch('http://localhost:5241/api/Wards');
      if (res.ok) {
        const data = await res.json();
        setWards(data);
      }
    } catch (e) {
      console.error('Failed to fetch wards', e);
    }
  };

  const handleSearch = async (e) => {
    const term = e.target.value;
    setSearchTerm(term);
    if (term.trim() === '') {
      fetchPatients();
      return;
    }
    try {
      const res = await fetch(`http://localhost:5241/api/PatientProfiles/search?name=${term}`);
      if (res.ok) {
        const data = await res.json();
        setPatients(data);
      }
    } catch (e) {
      console.error('Failed to search', e);
    }
  };

  const handleAiAnalyze = async () => {
    try {
      const res = await fetch('http://localhost:5241/api/Admissions/analyze-risk', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          patientName: aiInput.name,
          currentSymptoms: aiInput.symptoms
        })
      });
      if (res.ok) {
        const data = await res.json();
        setAiResponse(data);
      } else {
        setAiResponse({ recommendedWard: 'General', flaggedFactors: ['Backend error'] });
      }
    } catch (e) {
      setAiResponse({ recommendedWard: 'ICU (Mock)', flaggedFactors: ['High fever', 'Shortness of breath'] });
    }
  };

  const openAdmitModal = (patientId) => {
    setSelectedPatientId(patientId);
    setSelectedWardId('');
    setIsAdmitModalOpen(true);
  };

  // ------- DOB validation -------
  const validateDob = (val) => {
    const pattern = /^\d{4}-\d{2}-\d{2}$/;
    if (!pattern.test(val)) return 'Format must be YYYY-MM-DD';
    const entered = new Date(val);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    if (entered > today) return 'Date of birth cannot be in the future';
    return null; // valid
  };

  const handleAdmit = async () => {
    if (!selectedWardId) {
      alert('Please select a ward');
      return;
    }
    try {
      const res = await fetch('http://localhost:5241/api/Admissions/allocate-ward', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          patientProfileId: selectedPatientId,
          wardId: selectedWardId
        })
      });
      if (res.ok) {
        alert('Patient admitted successfully!');
        setIsAdmitModalOpen(false);
      } else {
        const errorText = await res.text();
        alert(`Error: ${errorText}`);
      }
    } catch (e) {
      console.error('Failed to admit', e);
      alert('Network error admitting patient');
    }
  };

  return (
    <div className="manage-patients-container">
      <div className="manage-header">
        <h2 className="page-title">Manage Patients</h2>
        <button className="register-btn" onClick={() => setIsRegisterModalOpen(true)}>Register New Patient</button>
      </div>

      <div className="search-bar-container">
        <span className="search-icon">🔍</span>
        <input 
          type="text" 
          placeholder="Search Here" 
          value={searchTerm}
          onChange={handleSearch}
          className="search-input"
        />
      </div>

      <div className="patients-grid">
        {patients.map((p, idx) => (
          <div className="patient-tile" key={idx}>
            <div className="patient-tile-header">
              <div className="avatar"></div>
              <h3 className="patient-name">{p.fullName || p.Name}</h3>
            </div>
            <div className="patient-tile-body">
              <p><strong>DOB:</strong> {p.dateOfBirth || p.DateOfBirth}</p>
              <p><strong>Blood Group:</strong> {p.bloodGroup || p.BloodGroup || 'N/A'}</p>
              <p><strong>History:</strong> {p.medicalHistorySummary || 'N/A'}</p>
            </div>
            <div className="patient-tile-actions">
              <button className="admit-btn" onClick={() => openAdmitModal(p.id || p.Id)}>Admit</button>
            </div>
          </div>
        ))}
      </div>

      {isRegisterModalOpen && (
        <div className="modal-overlay">
          <div className="modal-content">
            <button className="close-btn" onClick={() => setIsRegisterModalOpen(false)}>×</button>
            <PatientManagement />
          </div>
        </div>
      )}

      {isAdmitModalOpen && (
        <div className="modal-overlay">
          <div className="modal-content admit-modal">
            <button className="close-btn" onClick={() => setIsAdmitModalOpen(false)}>×</button>
            <h3>Admit Patient</h3>
            <p>Select a ward to admit this patient:</p>
            <select className="ward-select" value={selectedWardId} onChange={(e) => setSelectedWardId(e.target.value)}>
              <option value="">-- Select Ward --</option>
              {wards.map(w => (
                <option key={w.id} value={w.id}>{w.wardType} - {w.wardNumber}</option>
              ))}
            </select>
            <button className="submit-admit-btn" onClick={handleAdmit}>Confirm Admission</button>
          </div>
        </div>
      )}

      {/* AI FAB */}
      <div className="ai-fab-container">
        {isAiOpen && (
          <div className="ai-popup">
            <h4>AI Risk Analysis</h4>
            <input type="text" placeholder="Patient Name" value={aiInput.name} onChange={e => setAiInput({...aiInput, name: e.target.value})} />
            <textarea placeholder="Symptoms" value={aiInput.symptoms} onChange={e => setAiInput({...aiInput, symptoms: e.target.value})}></textarea>
            <button onClick={handleAiAnalyze}>Analyze</button>
            {aiResponse && (
              <div className="ai-results">
                <p><strong>Ward:</strong> {aiResponse.recommendedWard}</p>
                <p><strong>Flags:</strong> {aiResponse.flaggedFactors?.join(', ')}</p>
              </div>
            )}
          </div>
        )}
        <button className="ai-fab" onClick={() => setIsAiOpen(!isAiOpen)}>
          ✨
        </button>
      </div>
    </div>
  );
};

export default ManagePatients;
