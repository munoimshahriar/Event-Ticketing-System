using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VirtualEventTicketing.Models
{
    public class Event
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime Date { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0, 100000, ErrorMessage = "Ticket price must be between 0 and 100,000")]
        public decimal TicketPrice { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Available tickets must be 0 or greater")]
        public int AvailableTickets { get; set; }

        [Required]
        public int CategoryId { get; set; }

        // Organizer who created this event
        public string? OrganizerId { get; set; }

        // Navigation properties
        public Category Category { get; set; } = null!;
        public ApplicationUser? Organizer { get; set; }
        public ICollection<PurchaseItem> PurchaseItems { get; set; } = new List<PurchaseItem>();
    }
}

