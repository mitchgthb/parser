using System;
using System.Collections.Generic;

namespace Dualite.Models.Responses
{
    public class ApiKeyResponse
    {
        /// <summary>
        /// The API key value (only returned at creation time)
        /// </summary>
        public string? ApiKey { get; set; }

        /// <summary>
        /// A descriptive name for the API key
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// When the API key was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// When the API key expires (null means no expiration)
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// List of permissions for the API key
        /// </summary>
        public List<string>? Permissions { get; set; }
    }

    public class ApiKeyInfoResponse
    {
        /// <summary>
        /// Unique identifier for the API key
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// A descriptive name for the API key
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// When the API key was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When the API key was last used
        /// </summary>
        public DateTime? LastUsedAt { get; set; }

        /// <summary>
        /// When the API key expires (null means no expiration)
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Whether the API key is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// List of permissions for the API key
        /// </summary>
        public List<string>? Permissions { get; set; }
    }
}
