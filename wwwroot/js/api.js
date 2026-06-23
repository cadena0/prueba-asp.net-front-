const API_BASE_URL = `${window.location.origin}/api`;

// Función central para hacer peticiones con el JWT inyectado
async function fetchApi(endpoint, options = {}) {
    const token = localStorage.getItem('jwtToken');
    
    const headers = {
        'Content-Type': 'application/json',
        ...(token && { 'Authorization': `Bearer ${token}` }),
        ...options.headers
    };

    const config = {
        ...options,
        headers
    };

    const response = await fetch(`${API_BASE_URL}${endpoint}`, config);
    
    // Si el token expira o es inválido
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
    // Ajusta la key según cómo tu backend de .NET genere el claim de rol
    return payload['role'] || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']; 
}

function logout() {
    localStorage.removeItem('jwtToken');
    window.location.href = '/';
}

// Exponer funciones globalmente para evitar problemas de scope en la vista
window.fetchApi = fetchApi;
window.getUserRole = getUserRole;
window.logout = logout;
   