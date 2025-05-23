namespace TapcatAPI.DTOs;

public class UpdateAppointmentDTO
{
    public DateTime? ScheduledAt { get; set; }
    public bool? IsHomePickup { get; set; }
    public bool? IsConcluded { get; set; }
    public int? PetId { get; set; }
    public bool? IsPaidInCash { get; set; }
    
    public List<int>? ServiceIds { get; set; }
}