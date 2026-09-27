const API_BASE_URL = (import.meta.env?.VITE_API_BASE_URL || 'http://localhost:5241').replace(/\/$/, '');

export const TOKEN_KEY = 'careflow_token';
export const USER_KEY = 'careflow_user';

export function getStoredToken() {
  return localStorage.getItem(TOKEN_KEY);
}

export function setStoredToken(token) {
  if (token) {
    localStorage.setItem(TOKEN_KEY, token);
  } else {
    localStorage.removeItem(TOKEN_KEY);
  }
}

export function getStoredUser() {
  const raw = localStorage.getItem(USER_KEY);
  if (!raw) return null;
  try {
    return JSON.parse(raw);
  } catch {
    return null;
  }
}

export function setStoredUser(user) {
  if (user) {
    localStorage.setItem(USER_KEY, JSON.stringify(user));
  } else {
    localStorage.removeItem(USER_KEY);
  }
}

export function clearAuthStorage() {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(USER_KEY);
}

/**
 * Authenticated fetch helper that automatically prepends API_BASE_URL
 * (for relative paths starting with /api or /) and attaches Bearer authorization.
 * Handles 401 Unauthorized by broadcasting 'careflow:unauthorized' event.
 */
export async function apiFetch(url, options = {}) {
  let fullUrl = url;
  if (!url.startsWith('http://') && !url.startsWith('https://')) {
    const cleanPath = url.startsWith('/') ? url : `/${url}`;
    fullUrl = `${API_BASE_URL}${cleanPath}`;
  }

  const token = getStoredToken();
  const headers = {
    ...(options.headers || {})
  };

  if (token && !headers['Authorization'] && !headers['authorization']) {
    headers['Authorization'] = `Bearer ${token}`;
  }

  if (options.body && typeof options.body === 'string' && !headers['Content-Type'] && !headers['content-type']) {
    headers['Content-Type'] = 'application/json';
  }

  const response = await fetch(fullUrl, {
    ...options,
    headers
  });

  if (response.status === 401) {
    window.dispatchEvent(new CustomEvent('careflow:unauthorized', { detail: { url: fullUrl } }));
  }

  return response;
}

export const authApi = {
  async login(username, password) {
    const res = await fetch(`${API_BASE_URL}/api/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, password })
    });

    const data = await res.json().catch(() => null);
    if (!res.ok) {
      const message = typeof data === 'string' ? data : data?.message || data?.title || 'Invalid credentials.';
      const err = new Error(message);
      err.status = res.status;
      throw err;
    }
    return data;
  },

  async getMe() {
    const res = await apiFetch('/api/auth/me');
    if (!res.ok) {
      throw new Error('Failed to get current user.');
    }
    return res.json();
  }
};

export { API_BASE_URL };
