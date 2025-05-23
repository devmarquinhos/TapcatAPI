using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using TapcatAPI.Data;
using TapcatAPI.DTOs;
using TapcatAPI.Models;

namespace TapcatAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PetController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public PetController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    // GET: api/v1/pet
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PetDTO>>> GetAll()
    {
        var pets = await _context.Pets
            .Include(p => p.Customer)
            .Select(p => _mapper.Map<PetDTO>(p))
            .ToListAsync();
        return Ok(pets);
    }

    // GET: api/v1/pet/"{id}"
    [HttpGet("{id}")]
    public async Task<ActionResult<PetDTO>> GetById(int id)
    {
        var pet = await _context.Pets
            .Include(p => p.Customer)
            .FirstOrDefaultAsync(p => p.Id == id);
            
        if (pet == null) return NotFound();
        
        return Ok(_mapper.Map<PetDTO>(pet));
    }

    // POST: api/v1/pet
    [HttpPost]
    public async Task<ActionResult<PetDTO>> Create([FromBody] CreatePetDTO createDto)
    {
        var pet = _mapper.Map<Pet>(createDto);
        
        if (!await _context.Customers.AnyAsync(c => c.Id == createDto.CustomerId))
            return BadRequest("Cliente não encontrado");
        
        _context.Pets.Add(pet);
        await _context.SaveChangesAsync();
        
        var createdPet = await _context.Pets
            .Include(p => p.Customer)
            .FirstAsync(p => p.Id == pet.Id);
            
        return CreatedAtAction(nameof(GetById), new { id = pet.Id }, _mapper.Map<PetDTO>(createdPet));
    }

    // PUT: api/v1/pet/"{id}"
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePetDTO updateDto)
    {
        var pet = await _context.Pets.FindAsync(id);
        if (pet == null) return NotFound();
        
        if (updateDto.CustomerId.HasValue && 
            !await _context.Customers.AnyAsync(c => c.Id == updateDto.CustomerId.Value))
            return BadRequest("Cliente não encontrado");

        _mapper.Map(updateDto, pet);
        await _context.SaveChangesAsync();
        
        return NoContent();
    }

    // PATCH: api/v1/pet/"{id}"
    [HttpPatch("{id}")]
    public async Task<IActionResult> PartialUpdate(int id, [FromBody] UpdatePetDTO updateDto)
    {
        var pet = await _context.Pets.FindAsync(id);
        if (pet == null) return NotFound();
        
        if (updateDto.Name != null) pet.Name = updateDto.Name;
        if (updateDto.Weight.HasValue) pet.Weight = (float)updateDto.Weight.Value;
        if (updateDto.Species != null) pet.Species = updateDto.Species;
        if (updateDto.CustomerId.HasValue) 
        {
            if (!await _context.Customers.AnyAsync(c => c.Id == updateDto.CustomerId.Value))
                return BadRequest("Cliente não encontrado");
            pet.CustomerId = updateDto.CustomerId.Value;
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/v1/pet/"{id}"
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var pet = await _context.Pets.FindAsync(id);
        if (pet == null) return NotFound();

        _context.Pets.Remove(pet);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}