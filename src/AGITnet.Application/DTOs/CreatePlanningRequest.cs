using System.ComponentModel.DataAnnotations;

namespace AGITnet.Application.DTOs;

public class CreatePlanningRequest
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "RequestCode wajib diisi.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "RequestCode harus diantara 1 dan 100 karakter.")]
    public string RequestCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Slots wajib diisi.")]
    [MinLength(1, ErrorMessage = "Minimal satu slot diperlukan.")]
    public List<SlotRequest> Slots { get; set; } = new();
}

public class SlotRequest
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "SlotName wajib diisi.")]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "SlotName harus diantara 1 dan 100 karakter.")]
    public string SlotName { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Quantity tidak boleh negatif.")]
    public int Quantity { get; set; }
}
