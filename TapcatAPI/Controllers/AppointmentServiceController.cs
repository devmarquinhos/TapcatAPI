using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using TapcatAPI.Data;
using TapcatAPI.DTOs;
using TapcatAPI.Models;

namespace TapcatAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AppointmentServiceController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<AppointmentServiceController> _logger;

    public AppointmentServiceController(
        AppDbContext context,
        IMapper mapper,
        ILogger<AppointmentServiceController> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentServiceDTO>> Create([FromBody] CreateAppointmentServiceDTO createDto)
    {
        try
        {
            // Verifica existência de agendamento e serviço
            var appointmentExists = await _context.Appointments.AnyAsync(a => a.Id == createDto.AppointmentId);
            var serviceExists = await _context.Services.AnyAsync(s => s.Id == createDto.ServiceId);

            if (!appointmentExists)
                return BadRequest("Agendamento não encontrado.");
            if (!serviceExists)
                return BadRequest("Serviço não encontrado.");

            var appointmentService = _mapper.Map<AppointmentService>(createDto);

            _context.AppointmentServices.Add(appointmentService);
            await _context.SaveChangesAsync();

            // Recupera a entidade completa para retorno
            var result = await _context.AppointmentServices
                .Where(a => a.AppointmentId == createDto.AppointmentId && a.ServiceId == createDto.ServiceId)
                .Include(a => a.Service)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (result == null)
                return StatusCode(500, "Erro ao recuperar a relação criada.");

            return CreatedAtAction(
                nameof(GetByCompositeKey),
                new { appointmentId = result.AppointmentId, serviceId = result.ServiceId },
                _mapper.Map<AppointmentServiceDTO>(result)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar vínculo entre agendamento e serviço.");
            return StatusCode(500, "Erro interno no servidor.");
        }
    }

    [HttpGet("{appointmentId}/{serviceId}")]
    public async Task<ActionResult<AppointmentServiceDTO>> GetByCompositeKey(int appointmentId, int serviceId)
    {
        try
        {
            var result = await _context.AppointmentServices
                .Where(a => a.AppointmentId == appointmentId && a.ServiceId == serviceId)
                .Include(a => a.Service)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (result == null)
                return NotFound();

            return Ok(_mapper.Map<AppointmentServiceDTO>(result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao buscar serviço {serviceId} do agendamento {appointmentId}.");
            return StatusCode(500, "Erro interno ao buscar relação.");
        }
    }

    [HttpDelete("{appointmentId}/{serviceId}")]
    public async Task<IActionResult> Delete(int appointmentId, int serviceId)
    {
        try
        {
            var appointmentService = await _context.AppointmentServices
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId && a.ServiceId == serviceId);

            if (appointmentService == null)
                return NotFound();

            _context.AppointmentServices.Remove(appointmentService);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao deletar serviço {serviceId} do agendamento {appointmentId}.");
            return StatusCode(500, "Erro interno ao processar requisição.");
        }
    }
}
