using AGITnet.Application.DTOs;
using AGITnet.Application.Exceptions;
using AGITnet.Application.Services;
using AGITnet.Infrastructure.Persistence;
using AGITnet.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AGITnet.UnitTests;

public class PlanningServiceTests
{
    private AppDbContext CreateInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreatePlanningAsync_ShouldBalanceSlotsAndSaveToDatabase()
    {
        // Arrange
        using var context = CreateInMemoryDbContext(nameof(CreatePlanningAsync_ShouldBalanceSlotsAndSaveToDatabase));
        var repo = new PlanningRepository(context);
        var service = new PlanningService(repo);

        var request = new CreatePlanningRequest
        {
            RequestCode = "REQ-001",
            Slots = new List<SlotRequest>
            {
                new SlotRequest { SlotName = "Slot A", Quantity = 4 },
                new SlotRequest { SlotName = "Slot B", Quantity = 5 },
                new SlotRequest { SlotName = "Slot C", Quantity = 1 },
                new SlotRequest { SlotName = "Slot D", Quantity = 7 },
                new SlotRequest { SlotName = "Slot E", Quantity = 6 },
                new SlotRequest { SlotName = "Slot F", Quantity = 4 },
                new SlotRequest { SlotName = "Slot G", Quantity = 0 }
            }
        };

        // Act
        var result = await service.CreatePlanningAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("REQ-001", result.RequestCode);
        Assert.Equal("VEH-GHALIBCANDIDATE", result.CandidateToken);
        Assert.Equal(7, result.Slots.Count);

        // Expected balanced quantities: [4, 5, 4, 5, 5, 4, 0]
        var balancedQuantities = result.Slots.Select(s => s.BalancedQuantity).ToArray();
        Assert.Equal(new[] { 4, 5, 4, 5, 5, 4, 0 }, balancedQuantities);

        // Verify inactive slot
        Assert.False(result.Slots.Last().IsActive);
        Assert.Equal(0, result.Slots.Last().BalancedQuantity);
    }

    [Fact]
    public async Task CreatePlanningAsync_DuplicateRequestCode_ShouldThrowDuplicateRequestCodeException()
    {
        // Arrange
        using var context = CreateInMemoryDbContext(nameof(CreatePlanningAsync_DuplicateRequestCode_ShouldThrowDuplicateRequestCodeException));
        var repo = new PlanningRepository(context);
        var service = new PlanningService(repo);

        var request = new CreatePlanningRequest
        {
            RequestCode = "REQ-DUP",
            Slots = new List<SlotRequest>
            {
                new SlotRequest { SlotName = "Slot 1", Quantity = 10 }
            }
        };

        await service.CreatePlanningAsync(request);

        // Act & Assert
        await Assert.ThrowsAsync<DuplicateRequestCodeException>(() => service.CreatePlanningAsync(request));
    }

    [Fact]
    public async Task CreatePlanningAsync_EmptyRequestCode_ShouldThrowArgumentException()
    {
        // Arrange
        using var context = CreateInMemoryDbContext(nameof(CreatePlanningAsync_EmptyRequestCode_ShouldThrowArgumentException));
        var repo = new PlanningRepository(context);
        var service = new PlanningService(repo);

        var request = new CreatePlanningRequest
        {
            RequestCode = "   ",
            Slots = new List<SlotRequest>
            {
                new SlotRequest { SlotName = "Slot 1", Quantity = 10 }
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreatePlanningAsync(request));
    }

    [Fact]
    public async Task CreatePlanningAsync_NegativeQuantity_ShouldThrowArgumentExceptionFromBalancer()
    {
        // Arrange
        using var context = CreateInMemoryDbContext(nameof(CreatePlanningAsync_NegativeQuantity_ShouldThrowArgumentExceptionFromBalancer));
        var repo = new PlanningRepository(context);
        var service = new PlanningService(repo);

        var request = new CreatePlanningRequest
        {
            RequestCode = "REQ-NEG",
            Slots = new List<SlotRequest>
            {
                new SlotRequest { SlotName = "Slot 1", Quantity = -5 }
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => service.CreatePlanningAsync(request));
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ShouldReturnPlanning()
    {
        // Arrange
        using var context = CreateInMemoryDbContext(nameof(GetByIdAsync_ExistingId_ShouldReturnPlanning));
        var repo = new PlanningRepository(context);
        var service = new PlanningService(repo);

        var created = await service.CreatePlanningAsync(new CreatePlanningRequest
        {
            RequestCode = "REQ-GET-ID",
            Slots = new List<SlotRequest>
            {
                new SlotRequest { SlotName = "Slot 1", Quantity = 5 }
            }
        });

        // Act
        var found = await service.GetByIdAsync(created.PlanningId);

        // Assert
        Assert.NotNull(found);
        Assert.Equal("REQ-GET-ID", found.RequestCode);
    }

    [Fact]
    public async Task GetByRequestCodeAsync_ExistingCode_ShouldReturnPlanning()
    {
        // Arrange
        using var context = CreateInMemoryDbContext(nameof(GetByRequestCodeAsync_ExistingCode_ShouldReturnPlanning));
        var repo = new PlanningRepository(context);
        var service = new PlanningService(repo);

        await service.CreatePlanningAsync(new CreatePlanningRequest
        {
            RequestCode = "REQ-CODE-123",
            Slots = new List<SlotRequest>
            {
                new SlotRequest { SlotName = "Slot 1", Quantity = 5 }
            }
        });

        // Act
        var found = await service.GetByRequestCodeAsync("REQ-CODE-123");

        // Assert
        Assert.NotNull(found);
        Assert.Equal("REQ-CODE-123", found.RequestCode);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllPlannings()
    {
        // Arrange
        using var context = CreateInMemoryDbContext(nameof(GetAllAsync_ShouldReturnAllPlannings));
        var repo = new PlanningRepository(context);
        var service = new PlanningService(repo);

        await service.CreatePlanningAsync(new CreatePlanningRequest
        {
            RequestCode = "REQ-A",
            Slots = new List<SlotRequest> { new SlotRequest { SlotName = "S1", Quantity = 1 } }
        });
        await service.CreatePlanningAsync(new CreatePlanningRequest
        {
            RequestCode = "REQ-B",
            Slots = new List<SlotRequest> { new SlotRequest { SlotName = "S2", Quantity = 2 } }
        });

        // Act
        var list = await service.GetAllAsync();

        // Assert
        Assert.Equal(2, list.Count);
    }
}
