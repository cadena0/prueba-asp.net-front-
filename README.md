# Rentas Frontend

Frontend application for the Rentas rental management system. Connects to the RentasApi backend to allow users to search for properties, make reservations, manage favorites, and view dashboard metrics for property owners.

## Project Structure

```
RentasFrontend/
├── public/
│   └── index.html
├── src/
│   ├── components/
│   │   ├── Navbar.jsx
│   │   ├── PropertyCard.jsx
│   │   ├── PropertyForm.jsx
│   │   ├── ReservationForm.jsx
│   │   ├── AuthForms.jsx
│   │   └── Dashboard.jsx
│   ├── pages/
│   │   ├── Home.jsx
│   │   ├── PropertyDetails.jsx
│   │   ├── SearchResults.jsx
│   │   ├── Login.jsx
│   │   ├── Register.jsx
│   │   ├── MyReservations.jsx
│   │   ├── MyFavorites.jsx
│   │   └── OwnerDashboard.jsx
│   ├── services/
│   │   ├── api.js
│   │   └── auth.js
│   ├── hooks/
│   │   ├── useAuth.js
│   │   └── useProperties.js
│   ├── utils/
│   │   └── tokenStorage.js
│   ├── App.jsx
│   └── main.jsx
├── package.json
└── README.md
```

## Tech Stack

- React 18+ (or Vue 3 / Angular 17)
- Axios for HTTP requests
- React Router for routing
- Context API or Redux for global auth state
- jwt-decode for reading JWT claims

## API Base URL

Development: `http://localhost:5000/api`

The backend must be running before starting the frontend.

## How It Works

### Public Flows (No Authentication Required)

#### 1. Browse Properties

The homepage displays all available properties. The frontend calls:

```
GET /api/properties
```

Response: Array of property objects with id, title, city, pricePerNight, ownerName, and imageUrls.

#### 2. Search Properties

Users can filter by city and date range to check availability. The frontend sends:

```
POST /api/properties/search
Body: { city?, startDate?, endDate? }
```

The backend returns only properties that do not have overlapping reservations in the requested date range.

#### 3. View Property Details

Clicking a property card shows full details:

```
GET /api/properties/{id}
```

Response: Complete property info including all image URLs.

### Authentication Flow

#### Registration

New users register with full name, email, and password:

```
POST /api/auth/register
Body: { fullName, email, password }
```

Response: `{ token: "jwt_token_here" }`

The token must be stored in localStorage or sessionStorage and sent in the Authorization header on every protected request.

The backend assigns the OWNER role only if the email matches the configured seed email. All other users receive the GUEST role.

#### Login

Existing users authenticate with email and password:

```
POST /api/auth/login
Body: { email, password }
```

Response: `{ token: "jwt_token_here" }`

#### Using the Token

All protected endpoints require the header:

```
Authorization: Bearer <token>
```

The frontend should use an Axios interceptor to automatically attach the token:

```javascript
api.interceptors.request.use(config => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});
```

A response interceptor should handle 401 errors by clearing the token and redirecting to login.

#### Role Based Access

- GUEST users can search properties, view details, create reservations, and manage favorites.
- OWNER users can additionally create, edit, and delete properties, and access the dashboard with business metrics.

The frontend can read the role from the JWT:

```javascript
import jwtDecode from 'jwt-decode';
const decoded = jwtDecode(token);
// decoded.role is "OWNER" or "GUEST"
```

### Protected Flows (Authentication Required)

#### Create Reservation

Authenticated users reserve a property for specific dates:

```
POST /api/reservations
Body: { propertyId, startDate, endDate }
Headers: Authorization: Bearer <token>
```

The backend validates that the property exists, checks for date overlaps, calculates the total price (pricePerNight * number of days), and creates the reservation.

Response: `{ id: reservation_id }`

The frontend should ensure the user is authenticated before showing the reservation form. If not authenticated, redirect to login first.

#### Cancel Reservation

Users can cancel their own reservations:

```
DELETE /api/reservations/{id}
Headers: Authorization: Bearer <token>
```

Only the user who created the reservation can cancel it. The frontend should only show the cancel button for the user's own reservations.

#### Manage Favorites

Authenticated users can save properties to a favorites list:

```
GET /api/favorites
Headers: Authorization: Bearer <token>
```

Response: Array of PropertyDto objects.

```
POST /api/favorites/{propertyId}
Headers: Authorization: Bearer <token>
```

```
DELETE /api/favorites/{propertyId}
Headers: Authorization: Bearer <token>
```

#### Owner Features

##### Create Property

Only OWNER users can create properties:

```
POST /api/properties
Body: { title, description, city, pricePerNight, imageUrls? }
Headers: Authorization: Bearer <token>
```

If imageUrls is not provided, the backend assigns a default placeholder image automatically.

##### Edit Property

Only the owner of a specific property can edit it:

```
PUT /api/properties/{id}
Body: { title?, description?, city?, pricePerNight? }
Headers: Authorization: Bearer <token>
```

The frontend should only render the edit form if the logged-in user is the owner of the property.

##### Delete Property

Only the owner can delete:

```
DELETE /api/properties/{id}
Headers: Authorization: Bearer <token>
```

A confirmation dialog should appear before deletion.

##### Dashboard Metrics

The owner can view business metrics:

```
GET /api/dashboard/metrics
Headers: Authorization: Bearer <token>
```

Response:

```json
{
  "totalProperties": 5,
  "totalEarnings": 1500000,
  "occupancyRate": 45.5,
  "totalReservations": 30,
  "properties": [
    {
      "id": 1,
      "title": "Casa en la playa",
      "reservationCount": 10,
      "earnings": 500000,
      "occupancyRate": 60.0
    }
  ]
}
```

The frontend should display these metrics with appropriate formatting (currency, percentages, charts if desired).

## Axios Configuration

Create a centralized API client in `src/services/api.js`:

```javascript
import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5000/api',
  headers: {
    'Content-Type': 'application/json'
  }
});

api.interceptors.request.use(config => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  response => response,
  error => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default api;
```

## JWT Token Handling

Store the token:

```javascript
localStorage.setItem('token', response.data.token);
```

Read the token and decode claims:

```javascript
import jwtDecode from 'jwt-decode';

const token = localStorage.getItem('token');
if (token) {
  const decoded = jwtDecode(token);
  const userId = decoded.sub;
  const userEmail = decoded.unique_name;
  const userRole = decoded.role;
}
```

Check expiration:

```javascript
const token = localStorage.getItem('token');
if (token) {
  const decoded = jwtDecode(token);
  const now = Date.now() / 1000;
  if (decoded.exp < now) {
    // Token expired, redirect to login
    localStorage.removeItem('token');
    window.location.href = '/login';
  }
}
```

## Common Errors and Solutions

### Error: "Email already registered"

Occurs during registration when the email already exists in the database.

Solution: Show a message to the user suggesting they log in instead, or use a different email.

### Error: "Invalid credentials"

Occurs during login when the email or password is incorrect.

Solution: Show a generic error message. Do not reveal whether the email or password was wrong for security reasons.

### Error: "Property not found"

Occurs when trying to reserve a property that no longer exists.

Solution: Redirect the user back to the search page with an error message.

### Error: "Dates overlap with existing reservation"

Occurs when trying to book dates that are already reserved.

Solution: Highlight the conflicting dates and ask the user to select different dates.

### Error: "Invalid date range"

Occurs when the end date is the same as or before the start date.

Solution: Validate dates on the frontend before sending the request. Ensure check-out is after check-in.

### Error: "Already in favorites"

Occurs when trying to add a property that is already saved as a favorite.

Solution: Ignore the error or update the UI to show the property is already favorited.

### Error: 401 Unauthorized

Occurs when calling a protected endpoint without a valid token, or with an expired token.

Solution: The Axios interceptor should handle this by clearing storage and redirecting to login.

### Error: 403 Forbidden

Occurs when a GUEST user tries to access an OWNER-only endpoint, or when a user tries to edit or delete a property they do not own.

Solution: Show an access denied message. Hide owner-only UI elements for GUEST users.

### Network Errors

Occurs when the backend is not running or the URL is wrong.

Solution: Show a friendly error page. Implement a retry button that checks the backend connection.

### CORS Errors

If the backend CORS policy does not include the frontend origin, the browser blocks the requests.

Ensure the backend allows requests from the frontend URL. The default backend allows:

- http://localhost:5173
- http://localhost:5247
- http://localhost:5000

## Routing Structure

```
/                    Home page (public, shows all properties)
/properties/:id       Property details (public)
/search              Search results (public)
/login               Login page (public, redirects to home if authenticated)
/register            Register page (public, redirects to home if authenticated)
/reservations         My reservations (protected)
/favorites           My favorites (protected)
/dashboard           Owner dashboard (protected, OWNER only)
```

## Guards and Redirects

- Unauthenticated users trying to access a protected route should be redirected to `/login`.
- Authenticated users trying to access `/login` or `/register` should be redirected to `/`.
- GUEST users trying to access `/dashboard` should see an access denied message.
- After login or registration, redirect the user to the page they originally requested, or to home.

## Date Handling

All dates in the API are ISO 8601 format (e.g., `2025-07-01T14:00:00Z`).

The backend uses UTC internally. The frontend should convert dates to the user's local timezone for display.

The search endpoint accepts optional `startDate` and `endDate`. The frontend should send these as date-only strings (e.g., `2025-07-01`) or full ISO timestamps.

## Payment Display

The API returns prices as numbers (e.g., `250000`). The frontend should format them as currency for display. If the app targets a specific country, use the appropriate locale:

```javascript
new Intl.NumberFormat('es-CO', {
  style: 'currency',
  currency: 'COP',
  minimumFractionDigits: 0
}).format(250000);
// Output: $250.000
```

## Testing the Integration

1. Start the backend on port 5000:

```bash
cd RentasApi
dotnet run --urls http://localhost:5000
```

2. Start the frontend:

```bash
cd RentasFrontend
npm install
npm run dev
```

3. Manual test flow:

- Open http://localhost:5173
- Browse properties on the home page
- Search for a property in a specific city
- Click on a property to view details
- Register a new account
- Log in and see the token stored
- Book a reservation
- Go to My Reservations and verify it appears
- Add the property to favorites
- Go to My Favorites and verify it appears

If running as OWNER:
- Create a new property
- Go to Dashboard and verify metrics display

## Environment Variables

Create a `.env` file in the frontend root:

```
VITE_API_BASE_URL=http://localhost:5000/api
```

Use this variable in the API client instead of hardcoding the URL.

## Build and Deployment

Development:

```bash
npm run dev
```

Production build:

```bash
npm run build
```

The build output goes to the `dist/` folder. Serve it with any static file server and configure it to proxy API requests to the backend, or deploy the frontend and backend separately and update the `VITE_API_BASE_URL` for production.

## Backend API Reference

See `API_CHEATSHEET.md` in the backend directory for the complete endpoint reference.
