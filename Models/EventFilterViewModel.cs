namespace VirtualEventTicketing.Models
{
    public class EventFilterViewModel
    {
        public string? SearchTitle { get; set; }
        public int? CategoryId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string Availability { get; set; } = "all"; // "all", "available", "soldout"
        public string SortBy { get; set; } = "date"; // "title", "date", "price"
        public List<Event> Events { get; set; } = new List<Event>();
        public List<Category> Categories { get; set; } = new List<Category>();
    }
}

