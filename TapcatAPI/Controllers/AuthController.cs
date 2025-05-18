using Microsoft.AspNetCore.Mvc;
using TapcatAPI.DTOs;
using TapcatAPI.Services;

namespace TapcatAPI.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly CustomerService _customerService;

    public AuthController(CustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO dto)
    {
        var result = await _customerService.Register(dto);
        return result == null ? BadRequest("Email já cadastrado") : Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO dto)
    {
        var result = await _customerService.Login(dto);
        return result == null ? Unauthorized("Credenciais inválidas") : Ok(result);
    }
}