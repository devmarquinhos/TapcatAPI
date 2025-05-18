using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TapcatAPI.Data;
using TapcatAPI.DTOs;
using TapcatAPI.Models;

namespace TapcatAPI.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public CustomerController(AppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    
    // GET: api/v1/customer
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerDTO>>> GetAll()
    {
        var customers = await _context.Customers
            .Select(c => _mapper.Map<CustomerDTO>(c))
            .ToListAsync();
        return Ok(customers);
    }
    
    // GET: api/v1/customer/"{id}"
    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerDTO>> GetById(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
            return NotFound();
            
        return Ok(_mapper.Map<CustomerDTO>(customer));
    }
    
    // POST: api/v1/customer/
    [HttpPost]
    public async Task<ActionResult<CustomerDTO>> Create([FromBody] CreateCustomerDTO createDto)
    {
        var customer = _mapper.Map<Customer>(createDto);
        
        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();
        
        var customerDto = _mapper.Map<CustomerDTO>(customer);
        return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customerDto);
    }

    // PUT: api/v1/customer/"{id}"
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCustomerDTO updateDto)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return NotFound();

        _mapper.Map(updateDto, customer);
        await _context.SaveChangesAsync();
        
        return NoContent();
    }

    // PATCH: api/v1/customer/"{id}"
    [HttpPatch("{id}")]
    public async Task<IActionResult> PartialUpdate(
        int id,
        [FromBody] UpdateCustomerDTO updateDto)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return NotFound();
        
        if (updateDto.Name != null) customer.Name = updateDto.Name;
        if (updateDto.Email != null) customer.Email = updateDto.Email;
        if (updateDto.Phone != null) customer.Phone = updateDto.Phone;
        if (updateDto.Address != null) customer.Address = updateDto.Address;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/v1/customer/"{id}"
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null) return NotFound();

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}