namespace TapcatAPI.Models
{
    public class Pet
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public float Weight { get; set; }
        public string Species { get; set; } = null!;
        public int CustomerId  { get; set; }
        
        public Customer Customer { get; set; } = null!;
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
