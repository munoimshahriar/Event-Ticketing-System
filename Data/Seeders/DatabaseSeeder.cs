using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VirtualEventTicketing.Data;
using VirtualEventTicketing.Models;

namespace VirtualEventTicketing.Data.Seeders
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // 1. Seed Roles
            string[] roleNames = { "Admin", "Organizer", "Attendee" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Seed Admin User
            if (await userManager.FindByEmailAsync("admin@example.com") == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                    FullName = "System Admin",
                    EmailConfirmed = true,
                    DateOfBirth = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // 3. Seed Categories (only if they don't exist)
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Webinar", Description = "Educational webinars and online seminars" },
                    new Category { Name = "Concert", Description = "Live music performances and concerts" },
                    new Category { Name = "Workshop", Description = "Hands-on learning workshops" },
                    new Category { Name = "Conference", Description = "Professional conferences and summits" },
                    new Category { Name = "Sports", Description = "Sports events and competitions" }
                };

                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();

                // 4. Seed Events
                var events = new List<Event>
                {
                    new Event
                    {
                        Title = "Introduction to ASP.NET Core MVC",
                        Date = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(7), DateTimeKind.Utc),
                        TicketPrice = 25.00m,
                        AvailableTickets = 50,
                        CategoryId = categories[0].Id
                    },
                    new Event
                    {
                        Title = "Virtual Jazz Night",
                        Date = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(14), DateTimeKind.Utc),
                        TicketPrice = 35.00m,
                        AvailableTickets = 100,
                        CategoryId = categories[1].Id
                    },
                    new Event
                    {
                        Title = "Web Development Workshop",
                        Date = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(21), DateTimeKind.Utc),
                        TicketPrice = 45.00m,
                        AvailableTickets = 3,
                        CategoryId = categories[2].Id
                    },
                    new Event
                    {
                        Title = "Tech Innovation Summit 2025",
                        Date = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(30), DateTimeKind.Utc),
                        TicketPrice = 150.00m,
                        AvailableTickets = 200,
                        CategoryId = categories[3].Id
                    },
                    new Event
                    {
                        Title = "Virtual Marathon",
                        Date = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(10), DateTimeKind.Utc),
                        TicketPrice = 30.00m,
                        AvailableTickets = 2,
                        CategoryId = categories[4].Id
                    },
                    new Event
                    {
                        Title = "Data Science Webinar",
                        Date = DateTime.SpecifyKind(DateTime.UtcNow.AddDays(5), DateTimeKind.Utc),
                        TicketPrice = 20.00m,
                        AvailableTickets = 75,
                        CategoryId = categories[0].Id
                    }
                };

                context.Events.AddRange(events);
                await context.SaveChangesAsync();
            }
        }
    }
}

