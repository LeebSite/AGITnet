using AGITnet.Application.DTOs;
using AGITnet.Application.Exceptions;
using AGITnet.Application.Interfaces;
using AGITnet.Domain.Entities;
using AGITnet.Domain.Services;

namespace AGITnet.Application.Services;

public class PlanningService : IPlanningService
{
    private const string CANDIDATE_TOKEN = "VEH-GHALIBCANDIDATE";
    private readonly IPlanningRepository _repository;

    public PlanningService(IPlanningRepository repository)
    {
        _repository = repository;
    }

    public async Task<PlanningResponse> CreatePlanningAsync(CreatePlanningRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.RequestCode))
        {
            throw new ArgumentException("RequestCode tidak boleh kosong.", nameof(request));
        }

        if (request.Slots == null || request.Slots.Count == 0)
        {
            throw new ArgumentException("Aturan slot minimal 1 item.", nameof(request));
        }

        // Pre-check unique RequestCode
        var existing = await _repository.GetByRequestCodeAsync(request.RequestCode);
        if (existing != null)
        {
            throw new DuplicateRequestCodeException(request.RequestCode);
        }

        // Extract original quantities for balancing
        var originalQuantities = request.Slots.Select(s => s.Quantity).ToArray();

        // Perform balancing using Domain core logic (Case 1)
        var balancedQuantities = ProductionBalancer.Balance(originalQuantities);

        var planning = new Planning
        {
            RequestCode = request.RequestCode.Trim(),
            CandidateToken = CANDIDATE_TOKEN,
            CreatedAt = DateTime.UtcNow,
            Status = "Balanced",
            Slots = new List<PlanningSlot>()
        };

        for (int i = 0; i < request.Slots.Count; i++)
        {
            var slotRequest = request.Slots[i];
            planning.Slots.Add(new PlanningSlot
            {
                SlotOrder = i + 1,
                SlotName = slotRequest.SlotName.Trim(),
                OriginalQuantity = slotRequest.Quantity,
                BalancedQuantity = balancedQuantities[i],
                IsActive = slotRequest.Quantity > 0
            });
        }

        var savedPlanning = await _repository.CreateAsync(planning);
        return MapToResponse(savedPlanning);
    }

    public async Task<PlanningResponse?> GetByIdAsync(int id)
    {
        var planning = await _repository.GetByIdAsync(id);
        return planning == null ? null : MapToResponse(planning);
    }

    public async Task<PlanningResponse?> GetByRequestCodeAsync(string requestCode)
    {
        if (string.IsNullOrWhiteSpace(requestCode))
            return null;

        var planning = await _repository.GetByRequestCodeAsync(requestCode);
        return planning == null ? null : MapToResponse(planning);
    }

    public async Task<List<PlanningResponse>> GetAllAsync()
    {
        var plannings = await _repository.GetAllAsync();
        return plannings.Select(MapToResponse).ToList();
    }

    private static PlanningResponse MapToResponse(Planning planning)
    {
        return new PlanningResponse
        {
            PlanningId = planning.PlanningId,
            RequestCode = planning.RequestCode,
            CandidateToken = planning.CandidateToken,
            CreatedAt = planning.CreatedAt,
            Status = planning.Status,
            Slots = planning.Slots.OrderBy(s => s.SlotOrder).Select(s => new PlanningSlotResponse
            {
                PlanningSlotId = s.PlanningSlotId,
                PlanningId = s.PlanningId,
                SlotOrder = s.SlotOrder,
                SlotName = s.SlotName,
                OriginalQuantity = s.OriginalQuantity,
                BalancedQuantity = s.BalancedQuantity,
                IsActive = s.IsActive
            }).ToList()
        };
    }
}
