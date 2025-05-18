using Microsoft.EntityFrameworkCore;
using TapcatAPI.Models;

namespace TapcatAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<AppointmentService> AppointmentServices => Set<AppointmentService>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // AppointmentService (N:N)
        modelBuilder.Entity<AppointmentService>()
            .HasKey(x => new { x.AppointmentId, x.ServiceId });

        modelBuilder.Entity<AppointmentService>()
            .HasOne(x => x.Appointment)
            .WithMany(a => a.AppointmentServices)
            .HasForeignKey(x => x.AppointmentId);

        modelBuilder.Entity<AppointmentService>()
            .HasOne(x => x.Service)
            .WithMany(s => s.AppointmentServices)
            .HasForeignKey(x => x.ServiceId);

        // Customer → Pets (1:N)
        modelBuilder.Entity<Customer>()
            .HasMany(c => c.Pets)
            .WithOne(p => p.Customer)
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Pet → Appointments (1:N)
        modelBuilder.Entity<Pet>()
            .HasMany(p => p.Appointments)
            .WithOne(a => a.Pet)
            .HasForeignKey(a => a.PetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}