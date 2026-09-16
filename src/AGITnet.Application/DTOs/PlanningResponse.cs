namespace AGITnet.Application.DTOs;

public class PlanningResponse
{
    public int PlanningId { get; set; }
    public string RequestCode { get; set; } = string.Empty;
    public string CandidateToken { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<PlanningSlotResponse> Slots { get; set; } = new();
}

public class PlanningSlotResponse
{
    public int PlanningSlotId { get; set; }
    public int PlanningId { get; set; }
    public int SlotOrder { get; set; }
    public string SlotName { get; set; } = string.Empty;
    public int OriginalQuantity { get; set; }
    public int BalancedQuantity { get; set; }
    public bool IsActive { get; set; }
}
