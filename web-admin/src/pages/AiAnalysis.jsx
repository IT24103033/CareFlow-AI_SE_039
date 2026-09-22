import React, { useState } from 'react';

const AiAnalysis = () => {
    const [patientName, setPatientName] = useState('');
    const [symptoms, setSymptoms] = useState('');
    const [analysis, setAnalysis] = useState(null);
    const [loading, setLoading] = useState(false);

    const runAnalysis = async () => {
        setLoading(true);
        try {
            const response = await fetch('http://localhost:5241/api/Admissions/analyze-risk', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ patientName: patientName, currentSymptoms: symptoms })
            });
            const data = await response.json();
            setAnalysis(data);
        } catch (error) {
            console.error("Analysis failed", error);
        }
        setLoading(false);
    };

    return (
        <div style={{ padding: '20px', maxWidth: '600px' }}>
            <h2>AI Risk Analysis</h2>
            <p style={{ color: '#555' }}>Enter a patient's name and symptoms to generate a ward recommendation.</p>

            <div style={{ display: 'flex', flexDirection: 'column', gap: '10px', marginBottom: '20px' }}>
                <input
                    type="text"
                    placeholder="Patient Name (e.g., Test User)"
                    value={patientName}
                    onChange={(e) => setPatientName(e.target.value)}
                    style={{ padding: '10px', borderRadius: '4px', border: '1px solid #ccc' }}
                />
                <textarea
                    placeholder="Enter current symptoms (e.g., severe chest pain)..."
                    value={symptoms}
                    onChange={(e) => setSymptoms(e.target.value)}
                    rows="4"
                    style={{ padding: '10px', borderRadius: '4px', border: '1px solid #ccc' }}
                />
                <button
                    onClick={runAnalysis}
                    disabled={loading}
                    style={{ padding: '12px', background: '#6f42c1', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer', fontWeight: 'bold' }}>
                    {loading ? 'Analyzing with Gemini...' : 'Run AI Analysis'}
                </button>
            </div>

            {analysis && (
                <div style={{ padding: '20px', background: '#f8f9fa', borderLeft: '5px solid #6f42c1', borderRadius: '4px' }}>
                    <h3 style={{ marginTop: 0, color: '#6f42c1' }}>Recommended Ward: {analysis.recommendedWardType}</h3>
                    <p><strong>Risk Level:</strong> {analysis.riskLevel}</p>
                    <p><strong>Flagged Factors:</strong></p>
                    <ul style={{ margin: 0, paddingLeft: '20px' }}>
                        {analysis.flaggedFactors.map((factor, i) => <li key={i}>{factor}</li>)}
                    </ul>
                </div>
            )}
        </div>
    );
};

export default AiAnalysis;