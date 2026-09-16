using System.Net;
using System.Net.Http.Json;
using AGITnet.Application.DTOs;
using AGITnet.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AGITnet.IntegrationTests;

public class PlanningApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public PlanningApiTests(WebApplicationFactory<Program> factory)
    {
        var dbName = "IntegrationTestDb_" + Guid.NewGuid();
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptors = services.Where(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                         d.ServiceType == typeof(DbContextOptions) ||
                         d.ServiceType == typeof(AppDbContext)).ToList();

                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseInMemoryDatabase(dbName);
                });
            });
        });
    }

    [Fact]
    public async Task PostPlanning_ValidRequest_ReturnsCreatedWithBalancedSlots()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new CreatePlanningRequest
        {
            RequestCode = "REQ-INT-001",
            Slots = new List<SlotRequest>
            {
                new SlotRequest { SlotName = "Slot 1", Quantity = 4 },
                new SlotRequest { SlotName = "Slot 2", Quantity = 5 },
                new SlotRequest { SlotName = "Slot 3", Quantity = 1 },
                new SlotRequest { SlotName = "Slot 4", Quantity = 7 },
                new SlotRequest { SlotName = "Slot 5", Quantity = 6 },
                new SlotRequest { SlotName = "Slot 6", Quantity = 4 },
                new SlotRequest { SlotName = "Slot 7", Quantity = 0 }
            }
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/planning", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var planning = await response.Content.ReadFromJsonAsync<PlanningResponse>();
        Assert.NotNull(planning);
        Assert.Equal("REQ-INT-001", planning.RequestCode);
        Assert.Equal("VEH-GHALIBCANDIDATE", planning.CandidateToken);
        Assert.Equal(7, planning.Slots.Count);
        Assert.Equal(new[] { 4, 5, 4, 5, 5, 4, 0 }, planning.Slots.Select(s => s.BalancedQuantity).ToArray());
    }

    [Fact]
    public async Task PostPlanning_DuplicateRequestCode_Returns409Conflict()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new CreatePlanningRequest
        {
            RequestCode = "REQ-INT-DUP",
            Slots = new List<SlotRequest>
            {
                new SlotRequest { SlotName = "Slot 1", Quantity = 5 }
            }
        };

        // First post
        var firstResponse = await client.PostAsJsonAsync("/api/planning", request);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        // Duplicate post
        var secondResponse = await client.PostAsJsonAsync("/api/planning", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [Fact]
    public async Task PostPlanning_NegativeQuantity_Returns400BadRequest()
    {
        // Arrange
        var client = _factory.CreateClient();
        var request = new CreatePlanningRequest
        {
            RequestCode = "REQ-BAD",
            Slots = new List<SlotRequest>
            {
                new SlotRequest { SlotName = "Slot 1", Quantity = -1 }
            }
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/planning", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetPlanningById_ExistingId_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        var createRequest = new CreatePlanningRequest
        {
            RequestCode = "REQ-INT-GET",
            Slots = new List<SlotRequest>
            {
                new SlotRequest { SlotName = "Slot 1", Quantity = 10 }
            }
        };

        var postResponse = await client.PostAsJsonAsync("/api/planning", createRequest);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var created = await postResponse.Content.ReadFromJsonAsync<PlanningResponse>();
        Assert.NotNull(created);

        // Act
        var getResponse = await client.GetAsync($"/api/planning/{created.PlanningId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var fetched = await getResponse.Content.ReadFromJsonAsync<PlanningResponse>();
        Assert.NotNull(fetched);
        Assert.Equal("REQ-INT-GET", fetched.RequestCode);
    }

    [Fact]
    public async Task GetPlanningById_NonExistingId_Returns404NotFound()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/planning/999999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPlanningByRequestCode_ExistingCode_ReturnsOk()
    {
        // Arrange
        var client = _factory.CreateClient();
        var createRequest = new CreatePlanningRequest
        {
            RequestCode = "REQ-FIND-ME",
            Slots = new List<SlotRequest>
            {
                new SlotRequest { SlotName = "Slot 1", Quantity = 3 }
            }
        };

        await client.PostAsJsonAsync("/api/planning", createRequest);

        // Act
        var getResponse = await client.GetAsync("/api/planning/by-code/REQ-FIND-ME");

        // Assert
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var fetched = await getResponse.Content.ReadFromJsonAsync<PlanningResponse>();
        Assert.NotNull(fetched);
        Assert.Equal("REQ-FIND-ME", fetched.RequestCode);
    }

    [Fact]
    public async Task GetAllPlannings_ReturnsList()
    {
        // Arrange
        var client = _factory.CreateClient();
        await client.PostAsJsonAsync("/api/planning", new CreatePlanningRequest
        {
            RequestCode = "REQ-ALL-1",
            Slots = new List<SlotRequest> { new SlotRequest { SlotName = "S", Quantity = 1 } }
        });

        // Act
        var getResponse = await client.GetAsync("/api/planning");

        // Assert
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        var list = await getResponse.Content.ReadFromJsonAsync<List<PlanningResponse>>();
        Assert.NotNull(list);
        Assert.NotEmpty(list);
    }
}
