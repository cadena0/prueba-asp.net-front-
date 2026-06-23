using pruebaAsp.Models;

namespace pruebaAsp.Services
{
    public sealed class InMemoryDataStore
    {
        private readonly object _lock = new();
        private int _nextUserId = 3;
        private int _nextPropertyId = 3;
        private int _nextFavoriteId = 1;
        private int _nextReservationId = 1;

        public List<User> Users { get; } = new();
        public List<Property> Properties { get; } = new();
        public List<Favorite> Favorites { get; } = new();
        public List<Reservation> Reservations { get; } = new();

        public InMemoryDataStore()
        {
            Users.Add(new User
            {
                Id = 1,
                FullName = "Carlos Ruiz",
                Email = "owner@example.com",
                Password = "Owner123!",
                Role = "OWNER"
            });

            Users.Add(new User
            {
                Id = 2,
                FullName = "Laura Pérez",
                Email = "guest@example.com",
                Password = "Guest123!",
                Role = "GUEST"
            });

            Properties.Add(new Property
            {
                Id = 1,
                Title = "Apartamento Centro",
                Description = "Hermoso apartamento con vista",
                City = "Bogotá",
                PricePerNight = 150000m,
                OwnerId = 1,
                OwnerName = "Carlos Ruiz",
                ImageUrls = new List<string>
                {
                    "https://picsum.photos/seed/prop1/800/600",
                    "https://picsum.photos/seed/prop2/800/600"
                }
            });

            Properties.Add(new Property
            {
                Id = 2,
                Title = "Casa en la Playa",
                Description = "Casa con piscina y vista al mar",
                City = "Santa Marta",
                PricePerNight = 250000m,
                OwnerId = 1,
                OwnerName = "Carlos Ruiz",
                ImageUrls = new List<string>
                {
                    "https://picsum.photos/seed/prop3/800/600",
                    "https://picsum.photos/seed/prop4/800/600"
                }
            });
        }

        public int GetNextUserId()
        {
            lock (_lock)
            {
                return _nextUserId++;
            }
        }

        public int GetNextPropertyId()
        {
            lock (_lock)
            {
                return _nextPropertyId++;
            }
        }

        public int GetNextFavoriteId()
        {
            lock (_lock)
            {
                return _nextFavoriteId++;
            }
        }

        public int GetNextReservationId()
        {
            lock (_lock)
            {
                return _nextReservationId++;
            }
        }
    }
}
