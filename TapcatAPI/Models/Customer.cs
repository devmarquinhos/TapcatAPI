namespace TapcatAPI.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        
        public string Password { get; set; } = null!;

        public ICollection<Pet>? Pets { get; set; } = new List<Pet>();
    }
}
