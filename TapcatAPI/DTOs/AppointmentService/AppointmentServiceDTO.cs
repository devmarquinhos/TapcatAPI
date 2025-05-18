namespace TapcatAPI.DTOs;

public class AppointmentServiceDTO
{
    public int AppointmentId { get; set; }
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = null!;
    public decimal ServicePrice { get; set; }
}