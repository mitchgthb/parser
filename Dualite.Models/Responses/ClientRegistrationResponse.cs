using System;

namespace Dualite.Models.Responses
{
    public class ClientRegistrationResponse
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
        /// When the client was registered
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Subscription level
        /// </summary>
        public string? SubscriptionLevel { get; set; }

        /// <summary>
        /// Initial API key issued to the client
        /// </summary>
        public ApiKeyResponse? ApiKey { get; set; }

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
