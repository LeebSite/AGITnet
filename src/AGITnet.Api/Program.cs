using AGITnet.Application.Interfaces;
using AGITnet.Application.Services;
using AGITnet.Infrastructure.Persistence;
using AGITnet.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure PostgreSQL DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register Application & Infrastructure Services
builder.Services.AddScoped<IPlanningRepository, PlanningRepository>();
builder.Services.AddScoped<IPlanningService, PlanningService>();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "AGITnet API",
        Version = "v1",
        Description = "API for Production Plan Balancing - Candidate Token: VEH-GHALIBCANDIDATE"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AGITnet API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Partial Program class for WebApplicationFactory in integration tests
public partial class Program { }
