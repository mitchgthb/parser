namespace Dualite.Models
{
    public class InvoiceExtraction
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid JobId { get; set; }
        public string? InvoiceNumber { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? VatAmount { get; set; }
        public decimal? VatRate { get; set; }
        public string Currency { get; set; } = "EUR";
        public string? SellerName { get; set; }
        public string? SellerKvk { get; set; } // Chamber of Commerce number (Netherlands)
        public string? SellerIban { get; set; }
        public string? BuyerName { get; set; }
        public string? BuyerKvk { get; set; }
        public Dictionary<string, object>? LineItems { get; set; }
        public Dictionary<string, object>? ExtractedFields { get; set; }
        public string ValidationStatus { get; set; } = "pending";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ProcessingJob? ProcessingJob { get; set; }
    }
}
