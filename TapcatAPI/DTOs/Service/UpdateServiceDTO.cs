namespace TapcatAPI.DTOs;

public class UpdateServiceDTO
{
    public string? Name { get; set; }
    public decimal? Price { get; set; }
    public bool? requiresBath { get; set; }
}