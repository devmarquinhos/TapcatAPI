using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TapcatAPI.Data;
using TapcatAPI.DTOs;
using TapcatAPI.Models;

namespace TapcatAPI.Services;

public class AppointmentAppService : IAppointmentService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentAppService(AppDbContext context, IMapper mapper, ILogger<AppointmentService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<AppointmentDTO>> GetAll()
    {
        var appointments = await _context.Appointments
            .Include(a => a.Pet)
                .ThenInclude(p => p.Customer)
            .Include(a => a.AppointmentServices)
                .ThenInclude(a => a.Service)
            .AsNoTracking()
            .ToListAsync();

        return _mapper.Map<IEnumerable<AppointmentDTO>>(appointments);
    }

    public async Task<AppointmentDTO?> GetById(int id)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Pet)
                .ThenInclude(p => p.Customer)
            .Include(a => a.AppointmentServices)
                .ThenInclude(a => a.Service)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        return appointment == null ? null : _mapper.Map<AppointmentDTO>(appointment);
    }

    public async Task<AppointmentDTO> Create(CreateAppointmentDTO dto)
    {
        if (!await _context.Pets.AnyAsync(p => p.Id == dto.PetId))
            throw new ArgumentException("Pet não encontrado.");

        if (dto.ServiceIds == null || dto.ServiceIds.Count == 0)
            throw new ArgumentException("Selecione pelo menos um serviço.");

        var appointment = _mapper.Map<Appointment>(dto);
        appointment.TotalPrice = await CalculateTotalPrice(dto.ServiceIds, dto.IsHomePickup);

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

            return _mapper.Map<AppointmentDTO>(created);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Erro ao criar agendamento.");
            throw new Exception("Erro interno ao criar agendamento.");
        }
    }

    public async Task Update(int id, UpdateAppointmentDTO dto)
    {
        var appointment = await _context.Appointments
            .Include(a => a.AppointmentServices)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (appointment == null)
            throw new KeyNotFoundException("Agendamento não encontrado.");

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            _mapper.Map(dto, appointment);

            if (dto.ServiceIds != null)
            {
                _context.AppointmentServices.RemoveRange(appointment.AppointmentServices);

                foreach (var serviceId in dto.ServiceIds)
                {
                    _context.AppointmentServices.Add(new AppointmentService
                    {
                        AppointmentId = appointment.Id,
                        ServiceId = serviceId
                    });
                }

                appointment.TotalPrice = await CalculateTotalPrice(dto.ServiceIds, dto.IsHomePickup ?? appointment.IsHomePickup);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, $"Erro ao atualizar agendamento ID {id}");
            throw new Exception("Erro interno ao atualizar agendamento.");
        }
    }

    public async Task Delete(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null)
            throw new KeyNotFoundException("Agendamento não encontrado.");

        _context.Appointments.Remove(appointment);
        await _context.SaveChangesAsync();
    }

    private async Task<decimal> CalculateTotalPrice(List<int> serviceIds, bool isHomePickup)
    {
        var services = await _context.Services
            .Where(s => serviceIds.Contains(s.Id))
            .ToListAsync();

        decimal total = services.Sum(s => s.Price);
        if (isHomePickup)
            total += 10;

        return total;
    }
}
