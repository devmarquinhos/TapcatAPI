namespace TapcatAPI.DTOs;

public class AppointmentDTO
{
    public int Id { get; set; }
    public DateTime ScheduledAt { get; set; }
    public bool IsHomePickup { get; set; }
    public bool IsConcluded { get; set; }
    public decimal TotalPrice { get; set; }
    public int PetId { get; set; }
    public string PetName { get; set; } = null!;
    public string CustomerName { get; set; } = null!;
    public List<ServiceDTO> Services { get; set; } = new();
}