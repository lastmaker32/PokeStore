namespace PokeStore.Api.Infrastructure.Data;

using PokeStore.Api.Core.Entities;

/// <summary>
/// Seed data for initial database population
/// </summary>
public static class SeedData
{
    public static void Initialize(PokestoreDbContext context)
    {
        // Check if data already exists
        if (context.Categories.Any())
        {
            return; // Database already seeded
        }

        // Add categories
        var categories = new[]
        {
            new Category { Name = "Booster Packs", Slug = "booster-packs", Description = "Official booster packs from various sets" },
            new Category { Name = "Theme Decks", Slug = "theme-decks", Description = "Pre-constructed theme decks for beginners" },
            new Category { Name = "Elite Trainer Boxes", Slug = "elite-trainer-boxes", Description = "Deluxe collections with boosters and accessories" },
            new Category { Name = "Singles", Slug = "singles", Description = "Individual cards for deck building" },
            new Category { Name = "Accessories", Slug = "accessories", Description = "Sleeves, playmats, and other accessories" }
        };

        context.Categories.AddRange(categories);
        context.SaveChanges();

        // Add products
        var products = new[]
        {
            new Product 
            { 
                Name = "Pokémon TCG Scarlet & Violet Booster Pack", 
                Slug = "sv-booster-pack",
                Description = "Scarlet & Violet booster pack containing 10 random cards.",
                Price = 4.99m,
                StockQuantity = 100,
                ImageUrl = "https://via.placeholder.com/300x400?text=SV+Booster",
                CategoryId = categories[0].Id
            },
            new Product 
            { 
                Name = "Pokémon TCG Sword & Shield Theme Deck", 
                Slug = "ss-theme-deck",
                Description = "Complete theme deck with 60 cards ready to play.",
                Price = 11.99m,
                StockQuantity = 50,
                ImageUrl = "https://via.placeholder.com/300x400?text=SS+Theme",
                CategoryId = categories[1].Id
            },
            new Product 
            { 
                Name = "Elite Trainer Box: Scarlet & Violet", 
                Slug = "etb-sv",
                Description = "Contains 8 booster packs, sleeves, and a playmat.",
                Price = 39.99m,
                StockQuantity = 30,
                ImageUrl = "https://via.placeholder.com/300x400?text=ETB+SV",
                CategoryId = categories[2].Id
            },
            new Product 
            { 
                Name = "Pikachu Promo Card (Graded PSA 9)", 
                Slug = "pikachu-promo-psa9",
                Description = "Rare Pikachu promotional card, graded PSA 9.",
                Price = 149.99m,
                StockQuantity = 5,
                ImageUrl = "https://via.placeholder.com/300x400?text=Pikachu",
                CategoryId = categories[3].Id
            },
            new Product 
            { 
                Name = "Card Sleeves Pack (100)", 
                Slug = "card-sleeves-100",
                Description = "Premium quality card sleeves, pack of 100.",
                Price = 5.99m,
                StockQuantity = 200,
                ImageUrl = "https://via.placeholder.com/300x400?text=Sleeves",
                CategoryId = categories[4].Id
            },
            new Product 
            { 
                Name = "Pokémon Playmat - Grass Type", 
                Slug = "playmat-grass",
                Description = "Official playmat with grass-type Pokémon design.",
                Price = 14.99m,
                StockQuantity = 40,
                ImageUrl = "https://via.placeholder.com/300x400?text=Playmat",
                CategoryId = categories[4].Id
            }
        };

        context.Products.AddRange(products);
        context.SaveChanges();

        // Add a test user
        var testUser = new User
        {
            Email = "test@pokestore.com",
            FirstName = "Test",
            LastName = "User",
            PhoneNumber = "555-0123",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@12345"), // Password: Test@12345
            Role = "User",
            IsActive = true
        };

        context.Users.Add(testUser);
        context.SaveChanges();
    }
}
