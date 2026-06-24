const API_BASE_URL = `${window.location.origin}/api`;

async function fetchApi(endpoint, options = {}) {
    const token = localStorage.getItem('jwtToken');
    const headers = {
        'Content-Type': 'application/json',
        ...(token && { 'Authorization': `Bearer ${token}` }),
        ...options.headers
    };

    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
        ...options,
        headers
    });

    if (response.status === 401) {
        logout();
        window.location.href = '/Auth/Login';
        throw new Error('No autorizado');
    }

    return response;
}

function parseJwt(token) {
    try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function(c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));
        return JSON.parse(jsonPayload);
    } catch (e) {
        return null;
    }
}

function getUserRole() {
    const token = localStorage.getItem('jwtToken');
    if (!token) return null;
    const payload = parseJwt(token);
    return payload?.role || payload?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
}

function getUserName() {
    const token = localStorage.getItem('jwtToken');
    if (!token) return null;
    const payload = parseJwt(token);
    return payload?.name || payload?.unique_name || payload?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'];
}

function logout() {
    localStorage.removeItem('jwtToken');
    window.location.href = '/';
}

window.fetchApi = fetchApi;
window.getUserRole = getUserRole;
window.getUserName = getUserName;
window.logout = logout;