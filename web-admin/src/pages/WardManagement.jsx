import React, { useState, useEffect } from 'react';

const WardManagement = () => {
    const [wards, setWards] = useState([]);
    const [loading, setLoading] = useState(true);

    // This automatically runs when the page loads to fetch the live data
    useEffect(() => {
        fetch('http://localhost:5241/api/Wards')
            .then(response => response.json())
            .then(data => {
                setWards(data);
                setLoading(false);
            })
            .catch(error => console.error("Error fetching wards:", error));
    }, []);

    return (
        <div style={{ padding: '20px' }}>
            <h2>Ward Capacity Dashboard</h2>
            <p style={{ color: '#555', marginBottom: '20px' }}>Live overview of all hospital wards and bed availability.</p>

            {loading ? (
                <p>Loading live ward data...</p>
            ) : (
                <div style={{ display: 'flex', gap: '20px', flexWrap: 'wrap' }}>
                    {wards.map(ward => {
                        // Calculate how full the ward is for the visual progress bar
                        const percentFull = ward.capacity > 0 ? (ward.occupiedBeds / ward.capacity) * 100 : 0;
                        const isFull = percentFull >= 100;

                        return (
                            <div key={ward.id} style={{ border: '1px solid #ccc', padding: '20px', borderRadius: '8px', width: '280px', boxShadow: '0 4px 8px rgba(0,0,0,0.1)' }}>
                                <h3 style={{ margin: '0 0 10px 0' }}>Ward {ward.wardNumber}</h3>
                                <p style={{ margin: '5px 0' }}><strong>Type:</strong> {ward.wardType}</p>
                                <p style={{ margin: '5px 0' }}><strong>Capacity:</strong> {ward.capacity} beds</p>
                                <p style={{ margin: '5px 0' }}><strong>Occupied:</strong> {ward.occupiedBeds} beds</p>

                                {/* Visual Progress Bar */}
                                <div style={{ background: '#e0e0e0', height: '12px', borderRadius: '6px', marginTop: '15px', overflow: 'hidden' }}>
                                    <div style={{
                                        background: isFull ? '#ff4d4f' : '#52c41a',
                                        height: '10px',
                                        width: `${percentFull}%`,
                                        transition: 'width 0.5s ease-in-out'
                                    }}></div>
                                </div>
                                <p style={{ fontSize: '12px', color: isFull ? 'red' : 'gray', marginTop: '8px', textAlign: 'center' }}>
                                    {isFull ? 'WARD FULL' : `${ward.capacity - ward.occupiedBeds} beds available`}
                                </p>
                            </div>
                        );
                    })}
                </div>
            )}
        </div>
    );
};

export default WardManagement;