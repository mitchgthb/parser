namespace Dualite.Models.Requests
{
    public class ClientRegistrationRequest
    {
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
        /// Initial subscription level
        /// </summary>
        public string? SubscriptionLevel { get; set; }
    }
}
