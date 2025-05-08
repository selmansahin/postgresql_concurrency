using ConcurrencyApi.Infrastructure.Persistence;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")));

// Add FastEndpoints
builder.Services.AddFastEndpoints();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => 
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Concurrency API", Version = "v1" });
});

// Add OpenAPI/Swagger
//builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline
// if (app.Environment.IsDevelopment())
// {
//     app.MapOpenApi();
// }

// Use Swagger in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Concurrency API v1"));
}

// Use FastEndpoints
app.UseFastEndpoints();

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();
