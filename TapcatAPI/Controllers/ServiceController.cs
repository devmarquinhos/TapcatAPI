using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.JsonPatch;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TapcatAPI.Data;
using TapcatAPI.DTOs;
using TapcatAPI.Models;

namespace TapcatAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ServiceController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public ServiceController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    // GET: api/v1/service
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServiceDTO>>> GetAll()
    {
        var services = await _context.Services
            .Select(s => _mapper.Map<ServiceDTO>(s))
            .ToListAsync();
        return Ok(services);
    }
    
    // GET: api/v1/service/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceDTO>> GetById(int id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service == null)
            return NotFound();
            
        return Ok(_mapper.Map<ServiceDTO>(service));
    }
    
    // POST: api/v1/service
    [HttpPost]
    public async Task<ActionResult<ServiceDTO>> Create([FromBody] CreateServiceDTO createDto)
    {
        var service = _mapper.Map<Service>(createDto);
        
        _context.Services.Add(service);
        await _context.SaveChangesAsync();
        
        var serviceDto = _mapper.Map<ServiceDTO>(service);
        return CreatedAtAction(nameof(GetById), new { id = service.Id }, serviceDto);
    }
    
    // PUT: api/v1/service/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateServiceDTO updateDto)
    {
        var service = await _context.Services.FindAsync(id);
        if (service == null)
            return NotFound();

        _mapper.Map(updateDto, service);
        await _context.SaveChangesAsync();
        
        return NoContent();
    }
    
    // PATCH: api/v1/service/{id}
    [HttpPatch("{id}")]
    public async Task<IActionResult> PatchService(
        int id,
        [FromBody] UpdateServiceDTO updateDto)
    {
        if (updateDto == null)
            return BadRequest("O corpo da requisição não pode ser nulo.");
        
        var service = await _context.Services.FindAsync(id);
        if (service == null)
            return NotFound();
        
        if (updateDto.Name != null)
            service.Name = updateDto.Name;
    
        if (updateDto.Price.HasValue)
            service.Price = updateDto.Price.Value;
    
        if (updateDto.requiresBath.HasValue)
            service.requiresBath = updateDto.requiresBath.Value;
        
        if (service.Price <= 0)
            ModelState.AddModelError("Price", "O preço deve ser maior que zero.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        await _context.SaveChangesAsync();

        return NoContent();
    }
    
    // DELETE: api/v1/service/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service == null)
            return NotFound();

        _context.Services.Remove(service);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}