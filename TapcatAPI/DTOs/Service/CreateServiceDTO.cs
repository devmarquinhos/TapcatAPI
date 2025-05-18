namespace TapcatAPI.DTOs;

public class CreateServiceDTO
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public bool requiresBath { get; set; }
}