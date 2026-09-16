using AGITnet.Domain.Entities;

namespace AGITnet.Application.Interfaces;

public interface IPlanningRepository
{
    Task<Planning?> GetByIdAsync(int id);
    Task<Planning?> GetByRequestCodeAsync(string requestCode);
    Task<List<Planning>> GetAllAsync();
    Task<Planning> CreateAsync(Planning planning);
}
