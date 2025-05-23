namespace TapcatAPI.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public int PetId { get; set; }
        public DateTime ScheduledAt { get; set; }
        public bool IsHomePickup { get; set; } 
        public bool IsConcluded { get; set; } = false;
        public bool IsPaidInCash { get; set; } = false;
        public decimal TotalPrice { get; set; } 
        
        public Pet Pet { get; set; } = null!;
        public ICollection<AppointmentService> AppointmentServices { get; set; } = new List<AppointmentService>(); 
    }
}