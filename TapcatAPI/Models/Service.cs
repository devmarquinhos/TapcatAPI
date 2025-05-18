namespace TapcatAPI.Models
{
    public class Service
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public bool requiresBath { get; set; } = false;

        public ICollection<AppointmentService>? AppointmentServices { get; set; } = new List<AppointmentService>();
    }
}
