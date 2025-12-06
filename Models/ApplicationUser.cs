using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace VirtualEventTicketing.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Display(Name = "Phone Number")]
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Profile Picture")]
        [StringLength(500)]
        public string? ProfilePictureUrl { get; set; }

        // Navigation properties
        public ICollection<Event> CreatedEvents { get; set; } = new List<Event>();
        public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
        public ICollection<EventRating> EventRatings { get; set; } = new List<EventRating>();
    }
}

