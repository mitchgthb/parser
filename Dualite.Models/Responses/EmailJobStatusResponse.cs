using System;
using System.Collections.Generic;

namespace Dualite.Models.Responses
{
    public class EmailJobStatusResponse
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
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp when the job was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Error message if the job failed
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Progress percentage (0-100)
        /// </summary>
        public int Progress { get; set; } = 0;

        /// <summary>
        /// Extracted email data
        /// </summary>
        public EmailAnalysisData? Data { get; set; }
    }

    public class EmailAnalysisData
    {
        /// <summary>
        /// Email subject
        /// </summary>
        public string? Subject { get; set; }

        /// <summary>
        /// Email content excerpt
        /// </summary>
        public string? ContentExcerpt { get; set; }

        /// <summary>
        /// Named entities extracted from the email
        /// </summary>
        public List<Entity>? Entities { get; set; }

        /// <summary>
        /// The intent classification of the email
        /// </summary>
        public Intent? Intent { get; set; }

        /// <summary>
        /// Urgency score (0-100) where 100 is most urgent
        /// </summary>
        public int? UrgencyScore { get; set; }

        /// <summary>
        /// The estimated effort to respond or complete the task in the email
        /// </summary>
        public EffortEstimate? EffortEstimate { get; set; }

        /// <summary>
        /// Email sentiment analysis
        /// </summary>
        public Sentiment? Sentiment { get; set; }

        /// <summary>
        /// Extracted action items
        /// </summary>
        public List<ActionItem>? ActionItems { get; set; }

        /// <summary>
        /// Confidence score for the analysis (0-100)
        /// </summary>
        public int ConfidenceScore { get; set; }
    }

    public class Entity
    {
        /// <summary>
        /// Text of the entity
        /// </summary>
        public string Text { get; set; } = "";

        /// <summary>
        /// Entity type (e.g., PERSON, ORGANIZATION, DATE, etc.)
        /// </summary>
        public string Type { get; set; } = "";

        /// <summary>
        /// Confidence score for the entity (0-100)
        /// </summary>
        public int ConfidenceScore { get; set; }
    }

    public class Intent
    {
        /// <summary>
        /// Primary intent of the email
        /// </summary>
        public string Label { get; set; } = "";

        /// <summary>
        /// Confidence score for the intent (0-100)
        /// </summary>
        public int ConfidenceScore { get; set; }

        /// <summary>
        /// Secondary intents with scores
        /// </summary>
        public Dictionary<string, int>? SecondaryIntents { get; set; }
    }

    public class EffortEstimate
    {
        /// <summary>
        /// Estimated time to complete (in minutes)
        /// </summary>
        public int? MinutesToComplete { get; set; }

        /// <summary>
        /// Difficulty level (e.g., Low, Medium, High)
        /// </summary>
        public string? DifficultyLevel { get; set; }

        /// <summary>
        /// Reasoning for the estimate
        /// </summary>
        public string? Reasoning { get; set; }
    }

    public class Sentiment
    {
        /// <summary>
        /// Overall sentiment (e.g., Positive, Negative, Neutral)
        /// </summary>
        public string? Overall { get; set; }

        /// <summary>
        /// Sentiment score (-1.0 to 1.0) where 1.0 is most positive
        /// </summary>
        public double? Score { get; set; }

        /// <summary>
        /// Positive score component (0-1.0)
        /// </summary>
        public double? Positive { get; set; }

        /// <summary>
        /// Negative score component (0-1.0)
        /// </summary>
        public double? Negative { get; set; }

        /// <summary>
        /// Neutral score component (0-1.0)
        /// </summary>
        public double? Neutral { get; set; }
    }

    public class ActionItem
    {
        /// <summary>
        /// Action item text
        /// </summary>
        public string Text { get; set; } = "";

        /// <summary>
        /// Action due date if specified
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Priority (e.g., Low, Medium, High)
        /// </summary>
        public string? Priority { get; set; }

        /// <summary>
        /// Confidence score for the action item (0-100)
        /// </summary>
        public int ConfidenceScore { get; set; }
    }
}
