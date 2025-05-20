using AutoMapper;
using TapcatAPI.Data;
using TapcatAPI.DTOs;
using TapcatAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace TapcatAPI.Services;

public class ServiceService : IServiceService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public ServiceService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ServiceDTO>> GetAll()
    {
        var services = await _context.Services.ToListAsync();
        return _mapper.Map<IEnumerable<ServiceDTO>>(services);
    }

    public async Task<ServiceDTO?> GetById(int id)
    {
        var service = await _context.Services.FindAsync(id);
        return service == null ? null : _mapper.Map<ServiceDTO>(service);
    }

    public async Task<ServiceDTO> Create(CreateServiceDTO dto)
    {
        var service = _mapper.Map<Service>(dto);
        _context.Services.Add(service);
        await _context.SaveChangesAsync();
        return _mapper.Map<ServiceDTO>(service);
    }

    public async Task Update(int id, UpdateServiceDTO dto)
    {
        var service = await _context.Services.FindAsync(id) ?? throw new Exception("Serviço não encontrado");
        _mapper.Map(dto, service);
        await _context.SaveChangesAsync();
    }

    public async Task Delete(int id)
    {
        var service = await _context.Services.FindAsync(id) ?? throw new Exception("Serviço não encontrado");
        _context.Services.Remove(service);
        await _context.SaveChangesAsync();
    }
}