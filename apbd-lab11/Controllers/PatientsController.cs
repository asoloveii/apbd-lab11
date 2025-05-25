using apbd_lab11.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace apbd_lab11.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IPatientsService _service;

    public PatientsController(IPatientsService service)
    {
        _service = service;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPatientDetails(int id)
    {
        var result = await _service.GetPatientDetailsAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }
}
