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
        localStorage.removeItem('jwtToken');
        window.location.href = '/Auth/Login';
        throw new Error('No autorizado');
    }

    return response;
}

async function loadProperties() {
    const container = document.getElementById('propertiesContainer');
    const city = document.getElementById('cityFilter')?.value?.trim();

    container.innerHTML = '<div class="text-center py-5">Cargando propiedades...</div>';

    try {
        const endpoint = city ? '/properties/search' : '/properties';
        const options = city ? {
            method: 'POST',
            body: JSON.stringify({ city })
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

    if (!properties || properties.length === 0) {
        container.innerHTML = '<div class="text-center py-5">No hay propiedades disponibles.</div>';
        return;
    }

    container.innerHTML = properties.map(property => `
        <div class="card mb-4 shadow-sm">
            <img src="${property.imageUrls?.[0] || '/images/default-house.jpg'}" class="card-img-top" style="height: 240px; object-fit: cover;" alt="${property.title}">
            <div class="card-body">
                <h5 class="card-title">${property.title}</h5>
                <p class="card-text text-muted">${property.city}</p>
                <p class="card-text">${property.description || ''}</p>
                <p class="card-text"><strong>$${property.pricePerNight}</strong> / noche</p>
                <a href="/Property/Details/${property.id}" class="btn btn-primary">Ver detalle</a>
            </div>
        </div>
    `).join('');
}

function populateCityFilter(properties) {
    const select = document.getElementById('cityFilter');
    if (!select || !properties) return;

    const cities = [...new Set(properties.map(p => p.city).filter(Boolean))].sort();
    const options = ['<option value="">Todas las ciudades</option>', ...cities.map(city => `<option value="${city}">${city}</option>`)];
    select.innerHTML = options.join('');
}

document.addEventListener('DOMContentLoaded', loadProperties);
