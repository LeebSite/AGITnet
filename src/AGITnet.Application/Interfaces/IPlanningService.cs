using AGITnet.Application.DTOs;

namespace AGITnet.Application.Interfaces;

public interface IPlanningService
{
    Task<PlanningResponse> CreatePlanningAsync(CreatePlanningRequest request);
    Task<PlanningResponse?> GetByIdAsync(int id);
    Task<PlanningResponse?> GetByRequestCodeAsync(string requestCode);
    Task<List<PlanningResponse>> GetAllAsync();
}
