using pruebaAsp.Models;

namespace pruebaAsp.Services
{
    public class FavoriteService
    {
        private readonly InMemoryDataStore _store;

        public FavoriteService(InMemoryDataStore store)
        {
            _store = store;
        }

        public IEnumerable<Property> GetFavorites(int userId)
        {
            var propertyIds = _store.Favorites.Where(f => f.UserId == userId).Select(f => f.PropertyId).ToHashSet();
            return _store.Properties.Where(p => propertyIds.Contains(p.Id));
        }

        public bool AddFavorite(int userId, int propertyId)
        {
            var property = _store.Properties.FirstOrDefault(p => p.Id == propertyId);
            if (property == null)
            {
                return false;
            }

            if (_store.Favorites.Any(f => f.UserId == userId && f.PropertyId == propertyId))
            {
                return true;
            }

            _store.Favorites.Add(new Favorite
            {
                Id = _store.GetNextFavoriteId(),
                UserId = userId,
                PropertyId = propertyId
            });

            return true;
        }

        public bool RemoveFavorite(int userId, int propertyId)
        {
            var favorite = _store.Favorites.FirstOrDefault(f => f.UserId == userId && f.PropertyId == propertyId);
            if (favorite == null)
            {
                return false;
            }

            _store.Favorites.Remove(favorite);
            return true;
        }
    }
}
