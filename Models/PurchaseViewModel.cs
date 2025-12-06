using System.ComponentModel.DataAnnotations;

namespace VirtualEventTicketing.Models
{
    public class PurchaseViewModel
    {
        public int EventId { get; set; }
        public Event Event { get; set; } = null!;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Your Name")]
        public string GuestName { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [EmailAddress]
        [Display(Name = "Your Email")]
        public string GuestEmail { get; set; } = string.Empty;
    }
}

