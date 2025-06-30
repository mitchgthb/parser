using Dualite.Data.Repositories;
using Dualite.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dualite.Business.Services
{
    public class ApiKeyService : IApiKeyService
    {
        private readonly IRepository<ApiKey> _apiKeyRepository;
        private readonly IRepository<Client> _clientRepository;
        private readonly ILogger<ApiKeyService> _logger;

        public ApiKeyService(
            IRepository<ApiKey> apiKeyRepository,
            IRepository<Client> clientRepository,
            ILogger<ApiKeyService> logger)
        {
            _apiKeyRepository = apiKeyRepository;
            _clientRepository = clientRepository;
            _logger = logger;
        }

        public async Task<ApiKeyValidationResult> ValidateApiKeyAsync(string apiKey)
        {
            try
            {
                // Hash the provided API key to compare with stored hashes
                string hashedKey = HashApiKey(apiKey);

                // Find the API key in the repository
                var keys = await _apiKeyRepository.FindAsync(k => k.KeyHash == hashedKey);
                var storedKey = keys.FirstOrDefault();

                if (storedKey == null)
                {
                    return new ApiKeyValidationResult
                    {
                        IsValid = false,
                        Error = "Invalid API key"
                    };
                }

                // Check if the key is active
                if (!storedKey.IsActive)
                {
                    return new ApiKeyValidationResult
                    {
                        IsValid = false,
                        Error = "API key has been revoked"
                    };
                }

                // Check if the key has expired
                if (storedKey.ExpiresAt.HasValue && storedKey.ExpiresAt.Value < DateTime.UtcNow)
                {
                    return new ApiKeyValidationResult
                    {
                        IsValid = false,
                        Error = "API key has expired"
                    };
                }

                // Get permissions directly as List<string>
                List<string>? permissions = storedKey.Permissions;

                return new ApiKeyValidationResult
                {
                    IsValid = true,
                    ClientId = storedKey.ClientId,
                    ApiKeyId = storedKey.Id,
                    Permissions = permissions ?? new List<string>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating API key");
                return new ApiKeyValidationResult
                {
                    IsValid = false,
                    Error = "Error validating API key"
                };
            }
        }

        public async Task UpdateApiKeyUsageAsync(Guid? apiKeyId)
        {
            if (apiKeyId == null) return;

            try
            {
                var apiKey = await _apiKeyRepository.GetByIdAsync(apiKeyId.Value);
                if (apiKey != null)
                {
                    apiKey.LastUsedAt = DateTime.UtcNow;
                    _apiKeyRepository.Update(apiKey);
                    await _apiKeyRepository.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to update API key usage for key {apiKeyId}");
            }
        }

        public async Task<string> CreateApiKeyAsync(Guid clientId, string name, DateTime? expiresAt = null, List<string>? permissions = null)
        {
            // Verify the client exists
            var client = await _clientRepository.GetByIdAsync(clientId);
            if (client == null)
            {
                throw new ArgumentException($"Client with ID {clientId} not found");
            }

            // Generate a new API key
            string apiKey = GenerateApiKey();
            string hashedKey = HashApiKey(apiKey);

            // Create API key entity
            var apiKeyEntity = new ApiKey
            {
                ClientId = clientId,
                KeyName = name,
                KeyHash = hashedKey,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                IsActive = true
            };

            // Set permissions if provided
            if (permissions != null)
            {
                apiKeyEntity.Permissions = permissions;
            }

            // Save to database
            await _apiKeyRepository.AddAsync(apiKeyEntity);

            // Return the unhashed API key (this is the only time it will be available)
            return apiKey;
        }

        public async Task<bool> RevokeApiKeyAsync(Guid apiKeyId)
        {
            try
            {
                var apiKey = await _apiKeyRepository.GetByIdAsync(apiKeyId);
                if (apiKey == null)
                {
                    return false;
                }

                apiKey.IsActive = false;
                _apiKeyRepository.Update(apiKey);
                await _apiKeyRepository.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to revoke API key {apiKeyId}");
                return false;
            }
        }

        public async Task<bool> HasPermissionAsync(Guid clientId, string permission)
        {
            try
            {
                // Get all active API keys for the client
                var apiKeys = await _apiKeyRepository.FindAsync(k => 
                    k.ClientId == clientId && 
                    k.IsActive && 
                    (!k.ExpiresAt.HasValue || k.ExpiresAt > DateTime.UtcNow));

                foreach (var key in apiKeys)
                {
                    if (key.Permissions == null) continue;

                    // Check if permission exists in the list
                    if (key.Permissions.Contains(permission))
                    {
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking permission {permission} for client {clientId}");
                return false;
            }
        }

        #region Helper Methods

        private static string GenerateApiKey()
        {
            // Generate a random API key (32 bytes = 256 bits)
            byte[] keyBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(keyBytes);
            }

            // Convert to Base64 for easier handling and storage
            return Convert.ToBase64String(keyBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "");
        }

        private static string HashApiKey(string apiKey)
        {
            // Hash the API key using SHA256
            using var sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(apiKey));
            
            // Convert to lowercase hex string
            return BitConverter
                .ToString(hashBytes)
                .Replace("-", "")
                .ToLowerInvariant();
        }

        #endregion
    }
}
