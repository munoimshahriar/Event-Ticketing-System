using System.ComponentModel.DataAnnotations;

namespace VirtualEventTicketing.Models
{
    public class PurchaseItem
    {
        public int Id { get; set; }

        [Required]
        public int PurchaseId { get; set; }

        [Required]
        public int EventId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        // Navigation properties
        public Purchase Purchase { get; set; } = null!;
        public Event Event { get; set; } = null!;
        public ICollection<EventRating> Ratings { get; set; } = new List<EventRating>();
    }
}

