using AGITnet.Application.Exceptions;
using AGITnet.Application.Interfaces;
using AGITnet.Domain.Entities;
using AGITnet.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AGITnet.Infrastructure.Repositories;

public class PlanningRepository : IPlanningRepository
{
    private readonly AppDbContext _context;

    public PlanningRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Planning?> GetByIdAsync(int id)
    {
        return await _context.Plannings
            .Include(p => p.Slots)
            .FirstOrDefaultAsync(p => p.PlanningId == id);
    }

    public async Task<Planning?> GetByRequestCodeAsync(string requestCode)
    {
        return await _context.Plannings
            .Include(p => p.Slots)
            .FirstOrDefaultAsync(p => p.RequestCode == requestCode);
    }

    public async Task<List<Planning>> GetAllAsync()
    {
        return await _context.Plannings
            .Include(p => p.Slots)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Planning> CreateAsync(Planning planning)
    {
        try
        {
            await _context.Plannings.AddAsync(planning);
            await _context.SaveChangesAsync();
            return planning;
        }
        catch (DbUpdateException)
        {
            // Concurrent race condition handling for unique RequestCode
            var existing = await _context.Plannings.FirstOrDefaultAsync(p => p.RequestCode == planning.RequestCode);
            if (existing != null)
            {
                throw new DuplicateRequestCodeException(planning.RequestCode);
            }
            throw;
        }
    }
}
