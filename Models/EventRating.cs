using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VirtualEventTicketing.Models
{
    public class EventRating
    {
        public int Id { get; set; }

        [Required]
        public int PurchaseItemId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [StringLength(1000)]
        public string? Comment { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public PurchaseItem PurchaseItem { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;
    }
}

