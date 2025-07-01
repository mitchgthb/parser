using System.Collections.Generic;

namespace Dualite.Models.Requests
{
    public class EmailProcessRequest
    {
        /// <summary>
        /// Email subject
        /// </summary>
        public string? Subject { get; set; }

        /// <summary>
        /// Email sender address
        /// </summary>
        public string? SenderEmail { get; set; }

        /// <summary>
        /// Email sender name
        /// </summary>
        public string? SenderName { get; set; }

        /// <summary>
        /// List of recipient email addresses
        /// </summary>
        public List<string>? RecipientEmails { get; set; }

        /// <summary>
        /// List of CC email addresses
        /// </summary>
        public List<string>? CcEmails { get; set; }

        /// <summary>
        /// The full content of the email body
        /// </summary>
        public string? EmailContent { get; set; }

        /// <summary>
        /// Whether to extract named entities from the email
        /// </summary>
        public bool? ExtractEntities { get; set; }

        /// <summary>
        /// Whether to classify email intent
        /// </summary>
        public bool? ClassifyIntent { get; set; }

        /// <summary>
        /// Whether to calculate email urgency
        /// </summary>
        public bool? CalculateUrgency { get; set; }

        /// <summary>
        /// Whether to estimate response effort
        /// </summary>
        public bool? EstimateEffort { get; set; }

        /// <summary>
        /// Language code for NLP processing (e.g., en, fr, de)
        /// </summary>
        public string? Language { get; set; }
    }
}
