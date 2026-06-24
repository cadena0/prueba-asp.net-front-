async function loadProperties() {
    const container = document.getElementById('propertiesContainer');
    const city = document.getElementById('cityFilter')?.value?.trim();

    container.innerHTML = '<div class="text-center py-5">Cargando propiedades...</div>';

    try {
        const endpoint = city ? '/properties/search' : '/properties';
        const options = city ? {
            method: 'POST',
            body: JSON.stringify({ city: city, startDate: '', endDate: '' })
        } : {};

        const response = await fetchApi(endpoint, options);
        if (!response.ok) {
            throw new Error('Error al cargar propiedades');
        }
        const properties = await response.json();
        renderProperties(properties);
        populateCityFilter(properties);
    } catch (error) {
        container.innerHTML = '<div class="alert alert-danger">Error al cargar propiedades.</div>';
        console.error(error);
    }
}

function renderProperties(properties) {
    const container = document.getElementById('propertiesContainer');
    if (!container) return;

    if (!properties || properties.length === 0) {
        container.innerHTML = '<div class="text-center py-5">No hay propiedades disponibles.</div>';
        return;
    }

    const role = window.getUserRole ? window.getUserRole() : null;
    container.innerHTML = properties.map(property => `
        <div class="card mb-4 shadow-sm">
            <img src="${property.imageUrls?.[0] || '/images/default-house.jpg'}" class="card-img-top" style="height: 240px; object-fit: cover;" alt="${property.title}">
            <div class="card-body">
                <h5 class="card-title">${property.title}</h5>
                <p class="card-text text-muted">${property.city}</p>
                <p class="card-text">${property.description || ''}</p>
                <p class="card-text"><strong>$${property.pricePerNight}</strong> / noche</p>
                <div class="d-flex gap-2">
                    <a href="/Property/Details/${property.id}" class="btn btn-primary">Ver detalle</a>
                    ${role === 'GUEST' ? `<button type="button" class="btn btn-outline-primary btn-favorite" data-id="${property.id}">
                        <i class="bi bi-heart"></i> Favorito
                    </button>` : ''}
                </div>
            </div>
        </div>
    `).join('');

    document.querySelectorAll('.btn-favorite').forEach(btn => {
        btn.addEventListener('click', function() {
            const id = this.getAttribute('data-id');
            addToFavorite(id);
        });
    });
}

function populateCityFilter(properties) {
    const select = document.getElementById('cityFilter');
    if (!select || !properties) return;

    const cities = [...new Set(properties.map(p => p.city).filter(Boolean))].sort();
    const options = ['<option value="">Todas las ciudades</option>', ...cities.map(city => `<option value="${city}">${city}</option>`)];
    select.innerHTML = options.join('');
}

async function addToFavorite(propertyId) {
    try {
        const res = await fetchApi(`/favorites/${propertyId}`, { method: 'POST' });
        if (res.ok) {
            alert('Agregado a favoritos');
        } else {
            const error = await res.json();
            alert(error.message || 'Error al agregar favorito');
        }
    } catch (error) {
        alert('Error de conexión');
    }
}

function renderAuthNav() {
    const authNav = document.getElementById('authNav');
    if (!authNav) return;

    const userName = window.getUserName ? window.getUserName() : null;
    if (userName) {
        authNav.innerHTML = `
            <span class="nav-link text-white">Hola, ${userName}</span>
            <button type="button" class="btn btn-outline-light btn-sm" onclick="logout()">Salir</button>
        `;
    } else {
        authNav.innerHTML = `
            <a class="nav-link btn btn-outline-primary btn-sm" href="/Auth/Login">Iniciar sesión</a>
            <a class="nav-link btn btn-primary btn-sm text-white" href="/Auth/Register">Registrarse</a>
        `;
    }
}

document.addEventListener('DOMContentLoaded', () => {
    renderAuthNav();
    if (document.getElementById('propertiesContainer')) {
        loadProperties();
    }
});