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
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add background services
builder.Services.AddHostedService<InventoryCleanupService>();

// Add logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Apply migrations automatically on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PokestoreDbContext>();
    dbContext.Database.Migrate();
    
    // Seed initial data
    SeedData.Initialize(dbContext);
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "POKESTORE API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Add custom middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
