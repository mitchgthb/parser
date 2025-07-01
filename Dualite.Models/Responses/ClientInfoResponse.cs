using System;

namespace Dualite.Models.Responses
{
    public class ClientInfoResponse
    {
        /// <summary>
        /// Unique client identifier
        /// </summary>
        public Guid ClientId { get; set; }

        /// <summary>
        /// Client name or organization name
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Client email address
        /// </summary>
        public string Email { get; set; } = "";

        /// <summary>
        /// Client website or URL
        /// </summary>
        public string? Website { get; set; }

        /// <summary>
        /// Client phone number
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// When the client was registered
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When the client information was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Current subscription level
        /// </summary>
        public string? SubscriptionLevel { get; set; }

        /// <summary>
        /// Whether the client account is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Number of API keys issued to this client
        /// </summary>
        public int ApiKeyCount { get; set; }

        /// <summary>
        /// Maximum number of API calls allowed for this client
        /// </summary>
        public int QuotaLimit { get; set; }

        /// <summary>
        /// Current number of API calls used by this client
        /// </summary>
        public int QuotaUsed { get; set; }
    }
}
