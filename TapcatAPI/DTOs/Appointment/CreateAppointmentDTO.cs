namespace TapcatAPI.DTOs;

public class CreateAppointmentDTO
{
    public DateTime ScheduledAt { get; set; }
    public bool IsHomePickup { get; set; }
    public int PetId { get; set; }
    public bool IsPaidInCash { get; set; }
    public List<int> ServiceIds { get; set; } = new();
}