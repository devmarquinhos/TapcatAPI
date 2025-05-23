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
    private readonly ILogger<AppointmentAppService> _logger;

    public AppointmentAppService(AppDbContext context, IMapper mapper, ILogger<AppointmentAppService> logger)
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
        var pet = await _context.Pets
            .Include(p => p.Customer)
            .FirstOrDefaultAsync(p => p.Id == dto.PetId)
            ?? throw new ArgumentException("Pet não encontrado.");

        if (dto.ServiceIds == null || dto.ServiceIds.Count == 0)
            throw new ArgumentException("Selecione pelo menos um serviço.");

        var appointment = _mapper.Map<Appointment>(dto);
        appointment.IsPaidInCash = dto.IsPaidInCash;
        appointment.TotalPrice = await CalculateTotalPrice(
            dto.ServiceIds,
            dto.IsHomePickup,
            dto.IsPaidInCash,
            pet.Id,
            pet.Species.ToLower(),
            pet.Weight,
            pet.Customer.Id
        );

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
            .Include(a => a.Pet)
                .ThenInclude(p => p.Customer)
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

                appointment.TotalPrice = await CalculateTotalPrice(
                    dto.ServiceIds,
                    dto.IsHomePickup ?? appointment.IsHomePickup,
                    dto.IsPaidInCash ?? appointment.IsPaidInCash,
                    appointment.Pet.Id,
                    appointment.Pet.Species.ToLower(),
                    appointment.Pet.Weight,
                    appointment.Pet.Customer.Id
                );
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

    private async Task<decimal> CalculateTotalPrice(
        List<int> serviceIds,
        bool isHomePickup,
        bool isPaidInCash,
        int petId,
        string petType,
        float petWeight,
        int customerId)
    {
        var services = await _context.Services
            .Where(s => serviceIds.Contains(s.Id))
            .ToListAsync();

        decimal total = 0;

        foreach (var service in services)
        {
            var name = service.Name.ToLower();

            if (name.Contains("banho"))
            {
                if (petType == "gato") total += 15;
                else total += petWeight <= 10 ? 30 : 50;
            }
            else if (name.Contains("tosa"))
            {
                if (petType == "gato") total += 25;
                else total += petWeight <= 10 ? 75 : 95;
            }
            else
            {
                total += service.Price;
            }
        }
        
        var totalVisits = await _context.Appointments
            .Include(a => a.Pet)
            .ThenInclude(p => p.Customer)
            .CountAsync(a => a.Pet.Customer.Id == customerId);

        if ((totalVisits + 1) % 10 == 0)
        {
            var banho = services.FirstOrDefault(s => s.Name.ToLower().Contains("banho"));
            if (banho != null)
            {
                if (petType == "gato") total -= 15;
                else total -= petWeight <= 10 ? 30 : 50;
            }
        }

        if (isHomePickup)
            total += 10;

        if (isPaidInCash)
            total *= 0.95m;

        return total;
    }
}
