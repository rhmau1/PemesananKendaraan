using Microsoft.EntityFrameworkCore;
using monitorKendaraan.Models;
using System.Reflection.Metadata;

namespace PemesananKendaraan.Models
{
    public class MyDbContext : DbContext
    {
        public MyDbContext(DbContextOptions<MyDbContext> options)
        : base(options)
        {
        }

        public DbSet<User> User { get; set; } = null!;
        public DbSet<Driver> Driver { get; set; } = null!;
        public DbSet<Vehicle> Vehicle { get; set; } = null!;
        public DbSet<Booking> Booking { get; set; } = null!;
        public DbSet<Approval> Approval { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.user) 
                .WithMany(u => u.Bookings) 
                .HasForeignKey(b => b.user_id); 

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.vehicle) 
                .WithMany(v => v.Bookings) 
                .HasForeignKey(b => b.vehicle_id); 

            modelBuilder.Entity<Booking>()
                .HasOne(b => b.driver) 
                .WithMany(d => d.Bookings) 
                .HasForeignKey(b => b.driver_id);

            modelBuilder.Entity<Approval>()
                .HasOne(a => a.booking) 
                .WithMany(b => b.Approvals) 
                .HasForeignKey(a => a.booking_id) 
                .OnDelete(DeleteBehavior.Restrict); 

            modelBuilder.Entity<Approval>()
                .HasOne(a => a.user) 
                .WithMany(u => u.Approvals) 
                .HasForeignKey(a => a.user_id) 
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
