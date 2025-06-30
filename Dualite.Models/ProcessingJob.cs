namespace Dualite.Models
{
    public class ProcessingJob
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ClientId { get; set; }
        public Guid ApiKeyId { get; set; }
        public required string JobType { get; set; }  // 'email_extract', 'invoice_parse'
        public string Status { get; set; } = "pending"; // 'pending', 'processing', 'completed', 'failed'
        public string? InputHash { get; set; } // For deduplication
        public Dictionary<string, object>? InputMetadata { get; set; }
        public Dictionary<string, object>? OutputData { get; set; }
        public int? ProcessingTimeMs { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
        
        // Navigation properties
        public Client? Client { get; set; }
        public ApiKey? ApiKey { get; set; }
        public EmailExtraction? EmailExtraction { get; set; }
        public InvoiceExtraction? InvoiceExtraction { get; set; }
    }
}
