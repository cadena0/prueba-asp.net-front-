using pruebaAsp.Models;

namespace pruebaAsp.Services
{
    public class PropertyService
    {
        private readonly InMemoryDataStore _store;

        public PropertyService(InMemoryDataStore store)
        {
            _store = store;
        }

        public IEnumerable<Property> GetAllProperties()
        {
            return _store.Properties;
        }

        public IEnumerable<Property> SearchProperties(string? city, DateTime? startDate, DateTime? endDate)
        {
            var query = _store.Properties.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(city))
            {
                query = query.Where(p => p.City.Contains(city.Trim(), StringComparison.OrdinalIgnoreCase));
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                query = query.Where(p => !_store.Reservations.Any(r => r.PropertyId == p.Id && DatesOverlap(startDate.Value, endDate.Value, r.StartDate, r.EndDate)));
            }

            return query;
        }

        public Property? GetById(int id)
        {
            return _store.Properties.FirstOrDefault(p => p.Id == id);
        }

        public IEnumerable<Property> GetByOwner(int ownerId)
        {
            return _store.Properties.Where(p => p.OwnerId == ownerId);
        }

        public Property Create(PropertyCreateRequest request, int ownerId, string ownerName)
        {
            var property = new Property
            {
                Id = _store.GetNextPropertyId(),
                Title = request.Title,
                Description = request.Description,
                City = request.City,
                PricePerNight = request.PricePerNight,
                OwnerId = ownerId,
                OwnerName = ownerName,
                ImageUrls = request.ImageUrls?.Where(u => !string.IsNullOrWhiteSpace(u)).Select(u => u.Trim()).ToList() ?? new List<string>()
            };

            _store.Properties.Add(property);
            return property;
        }

        public bool Update(int id, PropertyUpdateRequest request, int ownerId)
        {
            var property = GetById(id);
            if (property == null || property.OwnerId != ownerId)
            {
                return false;
            }

            property.Title = request.Title ?? property.Title;
            property.Description = request.Description ?? property.Description;
            property.City = request.City ?? property.City;
            property.PricePerNight = request.PricePerNight ?? property.PricePerNight;
            if (request.ImageUrls != null)
            {
                property.ImageUrls = request.ImageUrls.Where(u => !string.IsNullOrWhiteSpace(u)).Select(u => u.Trim()).ToList();
            }

            return true;
        }

        public bool Delete(int id, int ownerId)
        {
            var property = GetById(id);
            if (property == null || property.OwnerId != ownerId)
            {
                return false;
            }

            _store.Properties.Remove(property);
            _store.Reservations.RemoveAll(r => r.PropertyId == id);
            _store.Favorites.RemoveAll(f => f.PropertyId == id);
            return true;
        }

        private static bool DatesOverlap(DateTime startA, DateTime endA, DateTime startB, DateTime endB)
        {
            return startA < endB && startB < endA;
        }
    }
}
