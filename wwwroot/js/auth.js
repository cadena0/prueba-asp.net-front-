function parseJwt(token) {
    try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(window.atob(base64).split('').map(function(c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));
        return JSON.parse(jsonPayload);
    } catch {
        return null;
    }
}

function redirectByRole(token) {
    const payload = parseJwt(token);
    const role = payload?.role || payload?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
    localStorage.setItem('jwtToken', token);
    if (role === 'OWNER') {
        window.location.href = '/Owner/Dashboard';
    } else {
        window.location.href = '/';
    }
}

function showMessage(message, type = 'danger') {
    const alert = document.getElementById('authMessage');
    if (!alert) return;
    alert.innerHTML = `<div class="alert alert-${type}" role="alert">${message}</div>`;
}

async function authFetch(endpoint, options = {}) {
    const response = await fetch(`${window.location.origin}/api${endpoint}`, {
        ...options,
        headers: {
            'Content-Type': 'application/json',
            ...options.headers
        }
    });
    return response;
}

document.addEventListener('DOMContentLoaded', () => {
    const loginForm = document.getElementById('loginForm');
    if (loginForm) {
        loginForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            showMessage('');

            const email = document.getElementById('email').value.trim();
            const password = document.getElementById('password').value.trim();

            if (!email || !password) {
                showMessage('Por favor completa todos los campos.');
                return;
            }

            try {
                const res = await authFetch('/auth/login', {
                    method: 'POST',
                    body: JSON.stringify({ email, password })
                });

                const body = await res.json();
                console.log('Login response:', res.status, body);
                if (!res.ok) {
                    showMessage(body.message || 'Error al iniciar sesión');
                    return;
                }

                redirectByRole(body.token);
            } catch (error) {
                showMessage('No se pudo conectar con el servidor.');
                console.error(error);
            }
        });
    }

    const registerForm = document.getElementById('registerForm');
    if (registerForm) {
        registerForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            showMessage('');

            const firstName = document.getElementById('firstName').value.trim();
            const lastName = document.getElementById('lastName').value.trim();
            const email = document.getElementById('email').value.trim();
            const password = document.getElementById('password').value.trim();
            const fullName = `${firstName} ${lastName}`.trim();

            if (!firstName || !lastName) {
                showMessage('Nombre y apellido son obligatorios.');
                return;
            }

            if (!email) {
                showMessage('El correo es obligatorio.');
                return;
            }

            if (password.length < 6) {
                showMessage('La contraseña debe tener al menos 6 caracteres.');
                return;
            }

            try {
                const res = await authFetch('/auth/register', {
                    method: 'POST',
                    body: JSON.stringify({ fullName, email, password })
                });

                const body = await res.json();
                if (!res.ok) {
                    const message = body?.message || `Error ${res.status}: ${res.statusText}`;
                    showMessage(message);
                    return;
                }

                redirectByRole(body.token);
            } catch (error) {
                showMessage('No se pudo conectar con el servidor.');
                console.error(error);
            }
        });
    }
});