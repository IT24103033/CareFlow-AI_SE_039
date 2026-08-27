import React, { useState } from 'react';

const PatientManagement = () => {
    const [formData, setFormData] = useState({
        fullName: '',
        dateOfBirth: '',
        bloodGroup: '',
        medicalHistorySummary: ''
    });
    const [message, setMessage] = useState('');

    const handleChange = (e) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
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
            <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '10px' }}>
                <input type="text" name="fullName" placeholder="Full Name" value={formData.fullName} onChange={handleChange} required />
                <input type="date" name="dateOfBirth" value={formData.dateOfBirth} onChange={handleChange} required />
                <input type="text" name="bloodGroup" placeholder="Blood Group (e.g., O+)" value={formData.bloodGroup} onChange={handleChange} required />
                <textarea name="medicalHistorySummary" placeholder="Medical History Summary" value={formData.medicalHistorySummary} onChange={handleChange} rows="4" required />
                <button type="submit" style={{ padding: '10px', background: '#0066cc', color: 'white', border: 'none' }}>Register</button>
            </form>
            {message && <p style={{ marginTop: '15px', fontWeight: 'bold' }}>{message}</p>}
        </div>
    );
};

export default PatientManagement;