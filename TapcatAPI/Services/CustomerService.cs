using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using TapcatAPI.Data;
using TapcatAPI.DTOs;
using TapcatAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace TapcatAPI.Services;

public class CustomerService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public CustomerService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<AuthDTO.AuthResponseDTO?> Register(RegisterDTO dto)
    {
        if (await _context.Customers.AnyAsync(c => c.Email == dto.Email))
            return null;

        var customer = new Customer
        {
            Name = dto.Name,
            Email = dto.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(dto.Password), // Senha criptografada
            Phone = dto.Phone,
            Address = dto.Address
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(customer);
        return new AuthDTO.AuthResponseDTO(customer.Id, customer.Name, customer.Email, token);
    }

    public async Task<AuthDTO.AuthResponseDTO?> Login(LoginDTO dto)
    {
        var customer = await _context.Customers
            .FirstOrDefaultAsync(c => c.Email == dto.Email);

        if (customer == null || !BCrypt.Net.BCrypt.Verify(dto.Password, customer.Password))
            return null;

        var token = GenerateJwtToken(customer);
        return new AuthDTO.AuthResponseDTO(customer.Id, customer.Name, customer.Email, token);
    }

    private string GenerateJwtToken(Customer customer)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, customer.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, customer.Email)
            },
            expires: DateTime.Now.AddHours(2),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}