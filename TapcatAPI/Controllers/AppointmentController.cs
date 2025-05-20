using Microsoft.AspNetCore.Mvc;
using TapcatAPI.DTOs;
using TapcatAPI.Services;

namespace TapcatAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AppointmentController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly ILogger<AppointmentController> _logger;

    public AppointmentController(IAppointmentService appointmentService, ILogger<AppointmentController> logger)
    {
        _appointmentService = appointmentService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppointmentDTO>>> GetAll()
    {
        try
        {
            var result = await _appointmentService.GetAll();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar agendamentos.");
            return StatusCode(500, "Erro interno no servidor.");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AppointmentDTO>> GetById(int id)
    {
        try
        {
            var appointment = await _appointmentService.GetById(id);
            if (appointment == null)
                return NotFound();

            return Ok(appointment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao buscar agendamento ID {id}.");
            return StatusCode(500, "Erro interno no servidor.");
        }
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentDTO>> Create([FromBody] CreateAppointmentDTO dto)
    {
        try
        {
            var created = await _appointmentService.Create(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar agendamento.");
            return StatusCode(500, "Erro interno ao criar agendamento.");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateAppointmentDTO dto)
    {
        try
        {
            await _appointmentService.Update(id, dto);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao atualizar agendamento ID {id}.");
            return StatusCode(500, "Erro interno ao atualizar agendamento.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _appointmentService.Delete(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao deletar agendamento ID {id}.");
            return StatusCode(500, "Erro interno ao deletar agendamento.");
        }
    }
}
