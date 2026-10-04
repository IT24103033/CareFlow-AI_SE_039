import React, { useState, useEffect } from 'react';
import PatientManagement from '../PatientManagement';
import { apiFetch } from '../../api';
import './ManagePatients.css';

const ManagePatients = () => {
  const [patients, setPatients] = useState([]);
  const [wards, setWards] = useState([]);
  const [searchTerm, setSearchTerm] = useState('');
  const [isRegisterModalOpen, setIsRegisterModalOpen] = useState(false);
  const [isAiOpen, setIsAiOpen] = useState(false);
  const [aiInput, setAiInput] = useState({ name: '', symptoms: '' });
  const [aiResponse, setAiResponse] = useState(null);
  const [aiLoading, setAiLoading] = useState(false);
  const [aiError, setAiError] = useState('');

  const [isAdmitModalOpen, setIsAdmitModalOpen] = useState(false);
  const [selectedPatientId, setSelectedPatientId] = useState('');
  const [selectedWardId, setSelectedWardId] = useState('');

  useEffect(() => {
    fetchPatients();
    fetchWards();
  }, []);

  const fetchPatients = async () => {
    try {
      const res = await apiFetch('/api/PatientProfiles');
      if (res.ok) setPatients(await res.json());
    } catch (e) {
      console.error('Failed to fetch patients', e);
    }
  };

  const fetchWards = async () => {
    try {
      const res = await apiFetch('/api/Wards');
      if (res.ok) setWards(await res.json());
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
      const res = await apiFetch(`/api/PatientProfiles/search?name=${encodeURIComponent(term)}`);
      if (res.ok) setPatients(await res.json());
    } catch (e) {
      console.error('Failed to search', e);
    }
  };

  const openAiForPatient = (patient) => {
    setAiInput({
      name: patient.fullName || patient.Name || '',
      symptoms: ''
    });
    setAiResponse(null);
    setAiError('');
    setIsAiOpen(true);
  };

  const handleAiAnalyze = async () => {
    if (!aiInput.name.trim() || !aiInput.symptoms.trim()) {
      setAiError('Enter the patient name and current symptoms.');
      return;
    }

    setAiLoading(true);
    setAiError('');
    setAiResponse(null);

    try {
      const res = await apiFetch('/api/Admissions/analyze-risk', {
        method: 'POST',
        body: JSON.stringify({
          patientName: aiInput.name,
          currentSymptoms: aiInput.symptoms
        })
      });

      const data = await res.json().catch(() => null);
      if (!res.ok || !data) {
        const message =
          (data && (data.detail || data.title || data.error || data.message)) ||
          (typeof data === 'string' ? data : null) ||
          `Backend returned ${res.status}. Check the API terminal for the error.`;
        setAiError(message);
        return;
      }

      setAiResponse(data);
    } catch {
      setAiError('Could not reach the API. Start the backend, then try again.');
    } finally {
      setAiLoading(false);
    }
  };

  const openAdmitModal = (patientId) => {
    setSelectedPatientId(patientId);
    setSelectedWardId('');
    setIsAdmitModalOpen(true);
  };

  const handleAdmit = async () => {
    if (!selectedWardId) {
      alert('Please select a ward');
      return;
    }
    try {
      const res = await apiFetch('/api/Admissions/allocate-ward', {
        method: 'POST',
        body: JSON.stringify({
          patientProfileId: selectedPatientId,
          wardId: selectedWardId
        })
      });
      if (res.ok) {
        alert('Patient admitted successfully!');
        setIsAdmitModalOpen(false);
        fetchWards();
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
      <p className="records-hint">Staff can register, admit, and run AI triage. Editing or removing records is limited to Admin.</p>

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
        {patients.map((p) => (
          <div className="patient-tile" key={p.id || p.Id}>
            <div className="patient-tile-header">
              <div className="avatar"></div>
              <h3 className="patient-name">{p.fullName || p.Name}</h3>
            </div>
            <div className="patient-tile-body">
              <p><strong>DOB:</strong> {String(p.dateOfBirth || p.DateOfBirth || '').slice(0, 10)}</p>
              <p><strong>Blood Group:</strong> {p.bloodGroup || p.BloodGroup || 'N/A'}</p>
              <p><strong>History:</strong> {p.medicalHistorySummary || 'N/A'}</p>
            </div>
            <div className="patient-tile-actions">
              <button className="admit-btn" onClick={() => openAdmitModal(p.id || p.Id)}>Admit</button>
              <button className="admit-btn" onClick={() => openAiForPatient(p)}>AI Analyze</button>
            </div>
          </div>
        ))}
      </div>

      {isRegisterModalOpen && (
        <div className="modal-overlay">
          <div className="modal-content">
            <button className="close-btn" onClick={() => setIsRegisterModalOpen(false)}>×</button>
            <PatientManagement
              onRegistered={() => {
                setIsRegisterModalOpen(false);
                fetchPatients();
              }}
            />
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
              {wards.map((w) => (
                <option key={w.id} value={w.id}>{w.wardType} - {w.wardNumber}</option>
              ))}
            </select>
            <button className="submit-admit-btn" onClick={handleAdmit}>Confirm Admission</button>
          </div>
        </div>
      )}

      <div className="ai-fab-container">
        {isAiOpen && (
          <div className="ai-popup">
            <h4>AI Risk Analysis</h4>
            <input type="text" placeholder="Patient Name" value={aiInput.name} onChange={(e) => setAiInput({ ...aiInput, name: e.target.value })} />
            <textarea placeholder="Symptoms" value={aiInput.symptoms} onChange={(e) => setAiInput({ ...aiInput, symptoms: e.target.value })} />
            <button onClick={handleAiAnalyze} disabled={aiLoading}>
              {aiLoading ? 'Analyzing...' : 'Analyze'}
            </button>
            {aiError && <p className="ai-error">{aiError}</p>}
            {aiResponse && (
              <div className="ai-results">
                <p><strong>Risk:</strong> {aiResponse.riskLevel}</p>
                <p><strong>Ward:</strong> {aiResponse.recommendedWardType}</p>
                <p><strong>Flags:</strong> {aiResponse.flaggedFactors?.join(', ')}</p>
                {aiResponse.patientHistoryUsed && (
                  <p><strong>History used:</strong> {aiResponse.patientHistoryUsed}</p>
                )}
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
