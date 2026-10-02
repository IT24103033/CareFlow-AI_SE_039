import React, { useEffect, useState } from 'react';
import { apiFetch } from '../services/api';
import './AppointmentCalendar.css';

export default function AppointmentCalendar() {
  const [availability, setAvailability] = useState([]);
  const [date, setDate] = useState('');
  const [specialization, setSpecialization] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const loadAvailability = async () => {
    setLoading(true);
    setError('');

    try {
      const params = new URLSearchParams();

      if (specialization.trim()) {
        params.append('specialization', specialization.trim());
      }

      if (date) {
        params.append('date', date);
      }

      const query = params.toString();
      const url = query
        ? `/api/DoctorAvailability/search?${query}`
        : '/api/DoctorAvailability/search';

      const response = await apiFetch(url);

      if (!response.ok) {
        throw new Error('Failed to load doctor availability.');
      }

      const data = await response.json();
      setAvailability(Array.isArray(data) ? data : []);
    } catch (err) {
      setError(err.message || 'Unable to load availability.');
      setAvailability([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadAvailability();
  }, []);

  const clearFilters = () => {
    setDate('');
    setSpecialization('');
    setTimeout(() => loadAvailability(), 0);
  };

  return (
    <div className="appointment-page">
      <div className="appointment-header">
        <div>
          <h1>Appointment Availability</h1>
          <p>View doctor schedules and available appointment times.</p>
        </div>

        <button
          type="button"
          className="refresh-button"
          onClick={loadAvailability}
          disabled={loading}
        >
          {loading ? 'Loading...' : 'Refresh'}
        </button>
      </div>

      <div className="appointment-filters">
        <div className="filter-group">
          <label htmlFor="appointment-date">Date</label>
          <input
            id="appointment-date"
            type="date"
            value={date}
            onChange={(event) => setDate(event.target.value)}
          />
        </div>

        <div className="filter-group">
          <label htmlFor="specialization">Specialization</label>
          <input
            id="specialization"
            type="text"
            placeholder="e.g. Cardiology"
            value={specialization}
            onChange={(event) => setSpecialization(event.target.value)}
          />
        </div>

        <button
          type="button"
          className="search-button"
          onClick={loadAvailability}
          disabled={loading}
        >
          Search
        </button>

        <button
          type="button"
          className="clear-button"
          onClick={clearFilters}
          disabled={loading}
        >
          Clear
        </button>
      </div>

      {error && (
        <div className="appointment-error">
          {error}
        </div>
      )}

      <div className="availability-summary">
        <strong>{availability.length}</strong>
        <span>availability record{availability.length === 1 ? '' : 's'} found</span>
      </div>

      {loading ? (
        <div className="appointment-empty">
          Loading doctor availability...
        </div>
      ) : availability.length === 0 ? (
        <div className="appointment-empty">
          <h3>No availability found</h3>
          <p>
            Try selecting a different date or specialization.
          </p>
        </div>
      ) : (
        <div className="availability-grid">
          {availability.map((item) => (
            <div className="availability-card" key={item.id}>
              <div className="availability-card-header">
                <div>
                  <h3>{item.doctorName}</h3>
                  <span>{item.specialization || 'General'}</span>
                </div>

                <div className="doctor-badge">
                  Doctor
                </div>
              </div>

              <div className="availability-details">
                <div className="detail-item">
                  <span className="detail-label">Date</span>
                  <strong>{item.date}</strong>
                </div>

                <div className="detail-item">
                  <span className="detail-label">Available Time</span>
                  <strong>
                    {item.startTime} - {item.endTime}
                  </strong>
                </div>
              </div>

              <div className="availability-footer">
                <span>Doctor ID</span>
                <small>{item.doctorId}</small>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}