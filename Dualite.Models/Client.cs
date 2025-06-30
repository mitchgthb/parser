namespace Dualite.Models
{
    public class Client
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public required string Name { get; set; }
        public string? CompanyName { get; set; }
        public string? ContactEmail { get; set; }
        public string ServiceTier { get; set; } = "standard"; // 'basic', 'standard', 'premium'
        public int MonthlyQuota { get; set; } = 5000;
        public int QuotaUsed { get; set; } = 0;
        public DateTime QuotaResetDate { get; set; } = DateTime.UtcNow.AddMonths(1);
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public List<ApiKey> ApiKeys { get; set; } = [];
        public List<ProcessingJob> ProcessingJobs { get; set; } = [];
    }
}
