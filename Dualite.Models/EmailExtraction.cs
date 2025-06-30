namespace Dualite.Models
{
    public class EmailExtraction
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid JobId { get; set; }
        public string? SenderName { get; set; }
        public string? SenderEmail { get; set; }
        public string? SenderCompany { get; set; }
        public string? SubjectLine { get; set; }
        public string? DetectedIntent { get; set; }
        public int? EstimatedEffortMinutes { get; set; }
        public decimal? UrgencyScore { get; set; }
        public Dictionary<string, object>? ExtractedEntities { get; set; }
        public Dictionary<string, decimal>? ConfidenceScores { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public ProcessingJob? ProcessingJob { get; set; }
    }
}
