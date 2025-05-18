namespace TapcatAPI.DTOs;

public class UpdatePetDTO
{
    public string? Name { get; set; }
    public double? Weight { get; set; }
    public string? Species { get; set; }
    public int? CustomerId { get; set; }
}