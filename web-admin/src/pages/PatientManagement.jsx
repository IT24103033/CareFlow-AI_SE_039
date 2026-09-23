import React, { useState } from 'react';

const PatientManagement = () => {
    const [formData, setFormData] = useState({
        fullName: '',
        dateOfBirth: '',
        bloodGroup: '',
        medicalHistorySummary: ''
    });
    const [message, setMessage] = useState('');
    const [dobError, setDobError] = useState('');

    const handleChange = (e) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        // Validate DOB
        const pattern = /^\d{4}-\d{2}-\d{2}$/;
        if (!pattern.test(formData.dateOfBirth)) {
            setDobError('Format must be YYYY-MM-DD');
            return;
        }
        const entered = new Date(formData.dateOfBirth);
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        if (entered > today) {
            setDobError('Date of birth cannot be in the future');
            return;
        }
        setDobError('');
        try {
            // Connects to your running C# backend!
            const response = await fetch('http://localhost:5241/api/PatientProfiles', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(formData)
            });

            if (response.ok) {
                setMessage('Patient registered successfully!');
                setFormData({ fullName: '', dateOfBirth: '', bloodGroup: '', medicalHistorySummary: '' }); // Clear form
            } else {
                setMessage('Error registering patient.');
            }
        } catch (error) {
            setMessage('Network error. Is the backend running?');
        }
    };

    return (
        <div style={{ padding: '20px', maxWidth: '500px' }}>
            <h2>Register New Patient</h2>
            <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '15px' }}>
                <div style={{ display: 'flex', flexDirection: 'column', gap: '5px' }}>
                    <label style={{ fontSize: '14px', fontWeight: 'bold' }}>Full Name</label>
                    <input type="text" name="fullName" placeholder="John Doe" value={formData.fullName} onChange={handleChange} required style={{ padding: '10px', borderRadius: '6px', border: '1px solid #ccc' }} />
                </div>
                <div style={{ display: 'flex', flexDirection: 'column', gap: '5px' }}>
                    <label style={{ fontSize: '14px', fontWeight: 'bold' }}>Date of Birth (YYYY-MM-DD)</label>
                    <input
                      type="text"
                      name="dateOfBirth"
                      placeholder="e.g., 1990-05-24"
                      value={formData.dateOfBirth}
                      onChange={(e) => { handleChange(e); setDobError(''); }}
                      required
                      style={{ padding: '10px', borderRadius: '6px', border: dobError ? '1px solid red' : '1px solid #ccc' }}
                    />
                    {dobError && <span style={{ color: 'red', fontSize: '12px' }}>{dobError}</span>}
                </div>
                <div style={{ display: 'flex', flexDirection: 'column', gap: '5px' }}>
                    <label style={{ fontSize: '14px', fontWeight: 'bold' }}>Blood Group</label>
                    <input type="text" name="bloodGroup" placeholder="e.g., O+" value={formData.bloodGroup} onChange={handleChange} required style={{ padding: '10px', borderRadius: '6px', border: '1px solid #ccc' }} />
                </div>
                <div style={{ display: 'flex', flexDirection: 'column', gap: '5px' }}>
                    <label style={{ fontSize: '14px', fontWeight: 'bold' }}>Medical History Summary</label>
                    <textarea name="medicalHistorySummary" placeholder="List any known allergies, chronic conditions, etc." value={formData.medicalHistorySummary} onChange={handleChange} rows="4" required style={{ padding: '10px', borderRadius: '6px', border: '1px solid #ccc', fontFamily: 'inherit' }} />
                </div>
                <button type="submit" style={{ padding: '12px', background: '#0ab39c', color: 'white', border: 'none', borderRadius: '6px', fontWeight: 'bold', cursor: 'pointer', marginTop: '10px' }}>Register Patient</button>
            </form>
            {message && <p style={{ marginTop: '15px', fontWeight: 'bold' }}>{message}</p>}
        </div>
    );
};

export default PatientManagement;