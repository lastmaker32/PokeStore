namespace PokeStore.Api.Presentation.Extensions;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PokeStore.Api.Application.Services;
using PokeStore.Api.Core.Interfaces;
using PokeStore.Api.Infrastructure.Data;
using PokeStore.Api.Infrastructure.External;
using PokeStore.Api.Infrastructure.Repositories;
using PokeStore.Api.Infrastructure.Security;
using System.Text;

/// <summary>
/// Extension methods for service collection
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<PokestoreDbContext>(options =>
            options.UseSqlServer(connectionString,
                sqlOptions => sqlOptions.MigrationsAssembly(typeof(PokestoreDbContext).Assembly.FullName)));

        // Repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IInventoryRepository, InventoryRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IProcessedWebhookRepository, ProcessedWebhookRepository>();

        // Security Services
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHashService, PasswordHashService>();
        services.AddScoped<IWebhookSignatureValidator, WebhookSignatureValidator>();

        // Application Services
        services.AddScoped<ProductService>();
        services.AddScoped<UserService>();
        services.AddScoped<CartService>();
        services.AddScoped<CheckoutService>();
        services.AddScoped<OrderService>();
        services.AddScoped<PaymentService>();

        // External Services
        services.AddHttpClient<IPaymentGatewayClient, PayPalPaymentGatewayClient>();

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("Jwt");
        var secretKey = jwtSection["SecretKey"];

        if (string.IsNullOrEmpty(secretKey))
            throw new InvalidOperationException("JWT:SecretKey is not configured");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateIssuer = true,
                ValidIssuer = jwtSection["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSection["Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        return services;
    }

    public static IServiceCollection AddSwaggerGeneration(this IServiceCollection services)
    {
        services.AddSwaggerGen();
        return services;
    }
}
