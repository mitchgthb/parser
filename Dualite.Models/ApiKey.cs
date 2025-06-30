namespace Dualite.Models
{
    public class ApiKey
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ClientId { get; set; }
        public required string KeyHash { get; set; }  // Store the hashed API key, never the raw value
        public string? KeyName { get; set; }
        public List<string> Permissions { get; set; } = new List<string> { "email:extract", "invoice:parse" };
        public int RateLimitPerMinute { get; set; } = 60;
        public bool IsActive { get; set; } = true;
        public DateTime? LastUsedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public Client? Client { get; set; }
        public List<ProcessingJob> ProcessingJobs { get; set; } = [];
    }
}
