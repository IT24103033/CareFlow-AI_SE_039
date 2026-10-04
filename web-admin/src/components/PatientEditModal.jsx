import React, { useState } from 'react';

const BLOOD_GROUPS = ['O+', 'O-', 'A+', 'A-', 'B+', 'B-', 'AB+', 'AB-'];

const toDateInput = (value) => {
  if (!value) return '';
  return String(value).slice(0, 10);
};

const PatientEditModal = ({ patient, onClose, onSaved }) => {
  const [formData, setFormData] = useState({
    fullName: patient.fullName || '',
    dateOfBirth: toDateInput(patient.dateOfBirth),
    bloodGroup: patient.bloodGroup || '',
    medicalHistorySummary: patient.medicalHistorySummary || ''
  });
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);

  const handleChange = (e) => {
    setFormData({ ...formData, [e.target.name]: e.target.value });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    const pattern = /^\d{4}-\d{2}-\d{2}$/;
    if (!pattern.test(formData.dateOfBirth)) {
      setError('Date of birth must be YYYY-MM-DD');
      return;
    }
    const entered = new Date(formData.dateOfBirth);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    if (entered > today) {
      setError('Date of birth cannot be in the future');
      return;
    }

    setSaving(true);
    setError('');
    try {
      const res = await fetch(`http://localhost:5241/api/PatientProfiles/${patient.id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(formData)
      });
      if (!res.ok) {
        setError(await res.text() || 'Could not update patient.');
        return;
      }
      onSaved();
    } catch {
      setError('Network error. Is the API running?');
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="modal-overlay">
      <div className="modal-content">
        <button className="close-btn" onClick={onClose}>×</button>
        <h3>Edit patient record</h3>
        <form onSubmit={handleSubmit} className="patient-edit-form">
          <label>
            Full name
            <input name="fullName" value={formData.fullName} onChange={handleChange} required />
          </label>
          <label>
            Date of birth
            <input type="date" name="dateOfBirth" value={formData.dateOfBirth} onChange={handleChange} required />
          </label>
          <label>
            Blood group
            <select name="bloodGroup" value={formData.bloodGroup} onChange={handleChange} required>
              <option value="">Select</option>
              {BLOOD_GROUPS.map((g) => (
                <option key={g} value={g}>{g}</option>
              ))}
            </select>
          </label>
          <label>
            Medical history
            <textarea name="medicalHistorySummary" rows="4" value={formData.medicalHistorySummary} onChange={handleChange} />
          </label>
          {error && <p className="ai-error">{error}</p>}
          <button type="submit" className="submit-admit-btn" disabled={saving}>
            {saving ? 'Saving...' : 'Save changes'}
          </button>
        </form>
      </div>
    </div>
  );
};

export default PatientEditModal;
