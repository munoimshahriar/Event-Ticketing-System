using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VirtualEventTicketing.Models;

namespace VirtualEventTicketing.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        public DbSet<PurchaseItem> PurchaseItems { get; set; }
        public DbSet<EventRating> EventRatings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure decimal precision
            modelBuilder.Entity<Event>()
                .Property(e => e.TicketPrice)
                .HasColumnType("decimal(10,2)");

            modelBuilder.Entity<Purchase>()
                .Property(p => p.TotalCost)
                .HasColumnType("decimal(10,2)");

            // Configure relationships
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Category)
                .WithMany(c => c.Events)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PurchaseItem>()
                .HasOne(pi => pi.Purchase)
                .WithMany(p => p.PurchaseItems)
                .HasForeignKey(pi => pi.PurchaseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PurchaseItem>()
                .HasOne(pi => pi.Event)
                .WithMany(e => e.PurchaseItems)
                .HasForeignKey(pi => pi.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure Event-Organizer relationship
            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organizer)
                .WithMany(u => u.CreatedEvents)
                .HasForeignKey(e => e.OrganizerId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure Purchase-User relationship
            modelBuilder.Entity<Purchase>()
                .HasOne(p => p.User)
                .WithMany(u => u.Purchases)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure EventRating relationships
            modelBuilder.Entity<EventRating>()
                .HasOne(er => er.PurchaseItem)
                .WithMany(pi => pi.Ratings)
                .HasForeignKey(er => er.PurchaseItemId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EventRating>()
                .HasOne(er => er.User)
                .WithMany(u => u.EventRatings)
                .HasForeignKey(er => er.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

