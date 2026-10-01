const baseUrl = (import.meta.env?.VITE_API_BASE_URL || 'http://localhost:5241').replace(/\/$/, '');

// Component A can supply its in-memory access-token getter when mounting the queue.
export function createTriageApi(getAccessToken = () => null) {
  async function request(path, options = {}) {
    const token = getAccessToken();
    const response = await fetch(`${baseUrl}/api/triage${path}`, {
      ...options,
      headers: { 'Content-Type': 'application/json', ...(token ? { Authorization: `Bearer ${token}` } : {}) },
    });
    const raw = await response.text();
    let data;
    try { data = JSON.parse(raw); } catch { data = raw; }
    if (!response.ok) {
      const message = response.status === 401 ? 'Sign in with a doctor account to access triage review.'
        : response.status === 403 ? 'Your account does not have permission to review triage cases.'
        : typeof data === 'string' && data ? data
        : data?.errors ? Object.values(data.errors).flat().join(' ')
        : data?.detail || data?.title || 'The request failed. Please try again.';
      const error = new Error(message);
      error.status = response.status;
      throw error;
    }
    return data;
  }
  return {
    list: (filters, signal) => request(`/review-queue?${new URLSearchParams(Object.entries(filters).filter(([, value]) => value !== ''))}`, { signal }),
    detail: (id, signal) => request(`/review-queue/${encodeURIComponent(id)}`, { signal }),
    review: (id, decision) => request(`/${encodeURIComponent(id)}/review`, { method: 'PATCH', body: JSON.stringify(decision) }),
    retry: (id) => request(`/${encodeURIComponent(id)}/retry`, { method: 'POST' }),
  };
}
