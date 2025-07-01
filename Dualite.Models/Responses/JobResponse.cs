using System;

namespace Dualite.Models.Responses
{
    public class JobResponse
    {
        /// <summary>
        /// Unique identifier for the job
        /// </summary>
        public Guid JobId { get; set; }

        /// <summary>
        /// Current status of the job
        /// </summary>
        public string Status { get; set; } = "pending";

        /// <summary>
        /// Timestamp when the job was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Estimated completion time
        /// </summary>
        public DateTime? EstimatedCompletionTime { get; set; }
    }
}
