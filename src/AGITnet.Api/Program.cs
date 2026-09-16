using AGITnet.Application.Interfaces;
using AGITnet.Application.Services;
using AGITnet.Infrastructure.Persistence;
using AGITnet.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure DbContext with PostgreSQL connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (!string.IsNullOrEmpty(connectionString))
    {
        options.UseNpgsql(connectionString);
    }
    else
    {
        options.UseInMemoryDatabase("AGITnet_DevDb");
    }
});

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

// Ensure Database & Tables exist automatically at startup, fallback to InMemory if Postgres is unavailable
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var dbContext = services.GetRequiredService<AppDbContext>();
        // Ensure database agitnet_db and tables are created automatically
        dbContext.Database.EnsureCreated();
        logger.LogInformation("Database PostgreSQL 'agitnet_db' berhasil di-verify/dibuat.");
    }
    catch (Exception ex)
    {
        logger.LogWarning("PostgreSQL tidak tersedia ({Message}). Menggunakan database sementara In-Memory untuk pengembangan.", ex.Message);
        
        // Dynamic fallback to In-Memory DbContext if PostgreSQL is not running
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("AGITnet_FallbackDb");
        
        builder.Services.AddScoped(_ => new AppDbContext(optionsBuilder.Options));
    }
}

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
