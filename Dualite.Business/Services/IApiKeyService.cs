using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dualite.Business.Services
{
    public class ApiKeyValidationResult
    {
        public bool IsValid { get; set; }
        public string? Error { get; set; }
        public Guid? ClientId { get; set; }
        public Guid? ApiKeyId { get; set; }
        public List<string>? Permissions { get; set; }
    }

    public interface IApiKeyService
    {
        /// <summary>
        /// Validates an API key
        /// </summary>
        /// <param name="apiKey">The API key to validate</param>
        /// <returns>Validation result with client information if valid</returns>
        Task<ApiKeyValidationResult> ValidateApiKeyAsync(string apiKey);

        /// <summary>
        /// Updates the last used timestamp for an API key
        /// </summary>
        /// <param name="apiKeyId">The ID of the API key</param>
        /// <returns>Task representing the asynchronous operation</returns>
        Task UpdateApiKeyUsageAsync(Guid? apiKeyId);

        /// <summary>
        /// Creates a new API key for a client
        /// </summary>
        /// <param name="clientId">The client ID</param>
        /// <param name="name">Name of the API key</param>
        /// <param name="expiresAt">Optional expiration date</param>
        /// <param name="permissions">Optional permissions dictionary</param>
        /// <returns>The newly created API key (unhashed)</returns>
        Task<string> CreateApiKeyAsync(Guid clientId, string name, DateTime? expiresAt = null, List<string>? permissions = null);

        /// <summary>
        /// Revokes an API key
        /// </summary>
        /// <param name="apiKeyId">The API key ID to revoke</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> RevokeApiKeyAsync(Guid apiKeyId);
        
        /// <summary>
        /// Checks if a client has permission for a specific action
        /// </summary>
        /// <param name="clientId">The client ID</param>
        /// <param name="permission">The permission to check</param>
        /// <returns>True if the client has permission, false otherwise</returns>
        Task<bool> HasPermissionAsync(Guid clientId, string permission);
    }
}
