using apbd_lab11.DTOs;
using apbd_lab11.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace apbd_lab11.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrescriptionsController : ControllerBase
{
    private readonly IPrescriptionsService _service;

    public PrescriptionsController(IPrescriptionsService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> AddPrescription([FromBody] CreatePrescriptionRequest request)
    {
        try
        {
            await _service.AddPrescriptionAsync(request);
            return Ok("Prescription created.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
