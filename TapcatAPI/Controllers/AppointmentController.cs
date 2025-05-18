using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using TapcatAPI.Data;
using TapcatAPI.DTOs;
using TapcatAPI.Models;

namespace TapcatAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AppointmentController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<AppointmentController> _logger;

    public AppointmentController(AppDbContext context, IMapper mapper, ILogger<AppointmentController> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AppointmentDTO>>> GetAll()
    {
        try
        {
            var appointments = await _context.Appointments
                .Include(a => a.Pet)
                    .ThenInclude(p => p.Customer)
                .Include(a => a.AppointmentServices)
                    .ThenInclude(a => a.Service)
                .AsNoTracking()
                .ToListAsync();

            var dtoList = _mapper.Map<IEnumerable<AppointmentDTO>>(appointments);
            return Ok(dtoList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar agendamentos");
            return StatusCode(500, "Erro interno no servidor.");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AppointmentDTO>> GetById(int id)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Pet)
                .ThenInclude(p => p.Customer)
            .Include(a => a.AppointmentServices)
                .ThenInclude(a => a.Service)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null)
            return NotFound();

        var dto = _mapper.Map<AppointmentDTO>(appointment);
        return Ok(dto);
    }

    [HttpPost]
    public async Task<ActionResult<AppointmentDTO>> Create([FromBody] CreateAppointmentDTO dto)
    {
        if (!await _context.Pets.AnyAsync(p => p.Id == dto.PetId))
            return BadRequest("Pet não encontrado.");

        if (dto.ServiceIds.Count == 0)
            return BadRequest("Selecione pelo menos um serviço.");

        var appointment = _mapper.Map<Appointment>(dto);
        appointment.TotalPrice = await CalculateTotalPrice(dto.PetId, dto.ServiceIds, dto.IsHomePickup);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            foreach (var serviceId in dto.ServiceIds)
            {
                _context.AppointmentServices.Add(new AppointmentService
                {
                    AppointmentId = appointment.Id,
                    ServiceId = serviceId
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var created = await _context.Appointments
                .Include(a => a.Pet).ThenInclude(p => p.Customer)
                .Include(a => a.AppointmentServices).ThenInclude(a => a.Service)
                .AsNoTracking()
                .FirstAsync(a => a.Id == appointment.Id);

            return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, _mapper.Map<AppointmentDTO>(created));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Erro ao criar agendamento.");
            return StatusCode(500, "Erro interno ao criar agendamento.");
        }
    }

    private async Task<decimal> CalculateTotalPrice(int petId, List<int> serviceIds, bool isHomePickup)
    {
        var pet = await _context.Pets.FindAsync(petId)
            ?? throw new Exception("Pet não encontrado");

        var services = await _context.Services
            .Where(s => serviceIds.Contains(s.Id))
            .ToListAsync();

        decimal total = services.Sum(s => s.Price);

        if (isHomePickup)
            total += 10;

        return total;
    }
}
