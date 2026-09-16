using AGITnet.Application.DTOs;
using AGITnet.Application.Exceptions;
using AGITnet.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AGITnet.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PlanningController : ControllerBase
{
    private readonly IPlanningService _planningService;

    public PlanningController(IPlanningService planningService)
    {
        _planningService = planningService;
    }

    /// <summary>
    /// Buat rencana produksi baru dan lakukan balancing slot.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PlanningResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PlanningResponse>> Create([FromBody] CreatePlanningRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _planningService.CreatePlanningAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.PlanningId }, result);
        }
        catch (DuplicateRequestCodeException ex)
        {
            return Conflict(new { message = ex.Message, requestCode = ex.RequestCode });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Ambil detail rencana produksi berdasarkan ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PlanningResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlanningResponse>> GetById(int id)
    {
        var result = await _planningService.GetByIdAsync(id);
        if (result == null)
        {
            return NotFound(new { message = $"Planning dengan ID {id} tidak ditemukan." });
        }

        return Ok(result);
    }

    /// <summary>
    /// Ambil seluruh daftar rencana produksi.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<PlanningResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PlanningResponse>>> GetAll()
    {
        var result = await _planningService.GetAllAsync();
        return Ok(result);
    }

    /// <summary>
    /// Ambil detail rencana produksi berdasarkan RequestCode.
    /// </summary>
    [HttpGet("by-code/{requestCode}")]
    [ProducesResponseType(typeof(PlanningResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PlanningResponse>> GetByRequestCode(string requestCode)
    {
        var result = await _planningService.GetByRequestCodeAsync(requestCode);
        if (result == null)
        {
            return NotFound(new { message = $"Planning dengan RequestCode '{requestCode}' tidak ditemukan." });
        }

        return Ok(result);
    }
}
