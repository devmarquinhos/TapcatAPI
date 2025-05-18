namespace TapcatAPI.DTOs;

public class AuthDTO
{
    public record RegisterDTO(
        string Name,
        string Email,
        string Password,
        string? Phone = null,
        string? Address = null);

    public record LoginDTO(
        string Email,
        string Password);

    public record AuthResponseDTO(
        int Id,
        string Name,
        string Email,
        string Token);
}