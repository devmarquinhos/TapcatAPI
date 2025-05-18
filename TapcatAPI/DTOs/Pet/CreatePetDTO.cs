namespace TapcatAPI.DTOs;

public class CreatePetDTO
{
    public string Name { get; set; } = null!;
    public double Weight { get; set; }
    public string Species { get; set; } = null!;
    public int CustomerId { get; set; }
}