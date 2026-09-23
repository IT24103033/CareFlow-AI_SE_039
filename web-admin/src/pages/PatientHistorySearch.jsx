import React, { useState } from 'react';

const PatientHistorySearch = () => {
    const [searchTerm, setSearchTerm] = useState('');
    const [results, setResults] = useState([]);
    const [loading, setLoading] = useState(false);
    const [hasSearched, setHasSearched] = useState(false);

    const handleSearch = async () => {
        if (!searchTerm.trim()) return;

        setLoading(true);
        setHasSearched(true);

        try {
            const response = await fetch(`http://localhost:5241/api/PatientProfiles/search?name=${searchTerm}`);
            const data = await response.json();

            // The API returns an array of matches
            setResults(data);
        } catch (error) {
            console.error("Error fetching patient history:", error);
            setResults([]);
        }

        setLoading(false);
    };

    return (
        <div style={{ padding: '20px', maxWidth: '800px' }}>
            <h2>Patient History Search</h2>
            <p style={{ color: '#555', marginBottom: '20px' }}>Search the hospital database to review patient medical records.</p>

            <div style={{ display: 'flex', gap: '10px', marginBottom: '30px' }}>
                <input
                    type="text"
                    placeholder="Enter patient name (e.g., Jane Doe)..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    style={{ flex: 1, padding: '10px', borderRadius: '4px', border: '1px solid #ccc' }}
                    onKeyDown={(e) => e.key === 'Enter' && handleSearch()}
                />
                <button
                    onClick={handleSearch}
                    disabled={loading}
                    style={{ padding: '10px 20px', background: '#0056b3', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer', fontWeight: 'bold' }}>
                    {loading ? 'Searching...' : 'Search Records'}
                </button>
            </div>

            <div>
                {loading && <p>Loading records...</p>}

                {!loading && hasSearched && results.length === 0 && (
                    <div style={{ padding: '15px', background: '#ffeeba', borderLeft: '5px solid #ffc107', borderRadius: '4px' }}>
                        No patient records found matching "{searchTerm}".
                    </div>
                )}

                {!loading && results.length > 0 && (
                    <div style={{ display: 'flex', flexDirection: 'column', gap: '15px' }}>
                        {results.map((patient) => (
                            <div key={patient.id} style={{ border: '1px solid #ddd', padding: '20px', borderRadius: '8px', boxShadow: '0 2px 4px rgba(0,0,0,0.05)' }}>
                                <h3 style={{ marginTop: 0, color: '#0056b3' }}>{patient.fullName}</h3>
                                <div style={{ display: 'grid', gridTemplateColumns: '120px 1fr', gap: '10px', marginBottom: '15px' }}>
                                    <strong>Blood Group:</strong>
                                    <span style={{ color: '#d9534f', fontWeight: 'bold' }}>{patient.bloodGroup}</span>

                                    <strong>Date of Birth:</strong>
                                    <span>{patient.dateOfBirth}</span>
                                </div>

                                <strong>Medical History Summary:</strong>
                                <p style={{ background: '#f8f9fa', padding: '15px', borderRadius: '4px', border: '1px solid #eee', marginTop: '5px' }}>
                                    {patient.medicalHistorySummary}
                                </p>
                            </div>
                        ))}
                    </div>
                )}
            </div>
        </div>
    );
};

export default PatientHistorySearch;