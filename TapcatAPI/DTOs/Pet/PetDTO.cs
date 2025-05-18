namespace TapcatAPI.DTOs;

public class PetDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public double Weight { get; set; }
    public string Species { get; set; } = null!;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = null!;
}