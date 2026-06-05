using Microsoft.EntityFrameworkCore;
using PokeStore.Api.Infrastructure.BackgroundServices;
using PokeStore.Api.Infrastructure.Data;
using PokeStore.Api.Presentation.Extensions;
using PokeStore.Api.Presentation.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services
    .AddApplicationServices(builder.Configuration)
    .AddJwtAuthentication(builder.Configuration)
    .AddSwaggerGeneration();

builder.Services.AddControllers();

// CORS: solo orígenes específicos en producción
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            var origins = builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ?? [];

            policy.WithOrigins(origins)
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            policy.WithOrigins("https://tu-dominio-produccion.com")
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
    });
});

// Background services
builder.Services.AddHostedService<InventoryCleanupService>();

// Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Migraciones: solo automáticas en Development
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PokestoreDbContext>();

    if (app.Environment.IsDevelopment())
    {
        dbContext.Database.Migrate();
    }

    SeedData.Initialize(dbContext);
}

// HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "POKESTORE API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
