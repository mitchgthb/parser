using System;
using System.Collections.Generic;

namespace Dualite.Models.Requests
{
    public class ApiKeyCreateRequest
    {
        /// <summary>
        /// A descriptive name for the API key
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Optional expiration date for the API key
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Optional dictionary of permissions for the API key
        /// Key is the permission name, value is whether it's allowed
        /// </summary>
        public Dictionary<string, bool> Permissions { get; set; } = new Dictionary<string, bool>();
    }
}
