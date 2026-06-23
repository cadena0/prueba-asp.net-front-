const API_BASE_URL = `${window.location.origin}/api`;

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

document.addEventListener('DOMContentLoaded', () => {
    const loginForm = document.getElementById('loginForm');
    if (loginForm) {
        loginForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            showMessage('');

            const email = document.getElementById('email').value.trim();
            const password = document.getElementById('password').value.trim();

            try {
                const res = await fetch(`${API_BASE_URL}/auth/login`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ email, password })
                });

                const body = await res.json();
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

            try {
                const res = await fetch(`${API_BASE_URL}/auth/register`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ fullName, email, password })
                });

                const body = await res.json();
                if (!res.ok) {
                    showMessage(body.message || 'Error al registrarse');
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