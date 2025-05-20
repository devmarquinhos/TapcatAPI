using AutoMapper;
using TapcatAPI.Data;
using TapcatAPI.DTOs;
using TapcatAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace TapcatAPI.Services;

public class PetService : IPetService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public PetService(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PetDTO>> GetAll()
    {
        var pets = await _context.Pets.Include(p => p.Customer).ToListAsync();
        return _mapper.Map<IEnumerable<PetDTO>>(pets);
    }

    public async Task<PetDTO?> GetById(int id)
    {
        var pet = await _context.Pets.Include(p => p.Customer).FirstOrDefaultAsync(p => p.Id == id);
        return pet == null ? null : _mapper.Map<PetDTO>(pet);
    }

    public async Task<PetDTO> Create(CreatePetDTO dto)
    {
        if (!await _context.Customers.AnyAsync(c => c.Id == dto.CustomerId))
            throw new Exception("Cliente não encontrado");

        var pet = _mapper.Map<Pet>(dto);
        _context.Pets.Add(pet);
        await _context.SaveChangesAsync();
        return _mapper.Map<PetDTO>(pet);
    }

    public async Task Update(int id, UpdatePetDTO dto)
    {
        var pet = await _context.Pets.FindAsync(id) ?? throw new Exception("Pet não encontrado");
        _mapper.Map(dto, pet);
        await _context.SaveChangesAsync();
    }

    public async Task Delete(int id)
    {
        var pet = await _context.Pets.FindAsync(id) ?? throw new Exception("Pet não encontrado");
        _context.Pets.Remove(pet);
        await _context.SaveChangesAsync();
    }
}