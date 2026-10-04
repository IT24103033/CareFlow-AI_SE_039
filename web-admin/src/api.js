export const API_BASE = 'http://localhost:5241';

export function getAuth() {
  try {
    return JSON.parse(localStorage.getItem('careflow_auth') || 'null');
  } catch {
    return null;
  }
}

export function authHeaders() {
  const auth = getAuth();
  const headers = { 'Content-Type': 'application/json' };
  if (auth?.token) headers.Authorization = `Bearer ${auth.token}`;
  return headers;
}

export function apiFetch(path, options = {}) {
  return fetch(`${API_BASE}${path}`, {
    ...options,
    headers: { ...authHeaders(), ...(options.headers || {}) }
  });
}
