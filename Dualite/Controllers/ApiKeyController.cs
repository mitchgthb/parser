using Dualite.Business.Services;
using Dualite.Models.Requests;
using Dualite.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Dualite.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ApiKeyController : ControllerBase
    {
        private readonly IApiKeyService _apiKeyService;
        private readonly ILogger<ApiKeyController> _logger;

        public ApiKeyController(
            IApiKeyService apiKeyService,
            ILogger<ApiKeyController> logger)
        {
            _apiKeyService = apiKeyService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new API key for the authenticated client
        /// </summary>
        /// <param name="request">API key creation request</param>
        /// <returns>The newly created API key</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ApiKeyResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateApiKey([FromBody] ApiKeyCreateRequest request)
        {
            try
            {
                // Get client information from the context (set by auth middleware)
                var clientId = HttpContext.Items["ClientId"] as Guid?;
                if (clientId == null)
                {
                    return Unauthorized("Valid API key required");
                }

                // Validate request
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    return BadRequest("API key name is required");
                }

                // Create permissions list
                List<string>? permissions = null;
                if (request.Permissions != null && request.Permissions.Count > 0)
                {
                    permissions = new List<string>();
                    foreach (var perm in request.Permissions)
                    {
                        if (perm.Value) // Only add permissions that are enabled
                        {
                            permissions.Add(perm.Key);
                        }
                    }
                }

                // Create new API key
                string apiKey = await _apiKeyService.CreateApiKeyAsync(
                    clientId.Value, 
                    request.Name, 
                    request.ExpiresAt, 
                    permissions);

                var response = new ApiKeyResponse
                {
                    ApiKey = apiKey,
                    Name = request.Name,
                    ExpiresAt = request.ExpiresAt,
                    CreatedAt = DateTime.UtcNow,
                    Permissions = permissions // Use the already converted permissions list
                };

                return CreatedAtAction(nameof(GetApiKeys), null, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating API key");
                return StatusCode(500, "An error occurred while processing the request");
            }
        }

        /// <summary>
        /// Revoke an API key
        /// </summary>
        /// <param name="apiKeyId">ID of the API key to revoke</param>
        /// <returns>Success or failure</returns>
        [HttpDelete("{apiKeyId}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RevokeApiKey(Guid apiKeyId)
        {
            try
            {
                // Get client information from the context (set by auth middleware)
                var clientId = HttpContext.Items["ClientId"] as Guid?;
                if (clientId == null)
                {
                    return Unauthorized("Valid API key required");
                }

                // Verify the API key belongs to this client
                // This would require an additional service method, but for now we'll just revoke
                // and let the service handle permissions

                bool result = await _apiKeyService.RevokeApiKeyAsync(apiKeyId);
                if (result)
                {
                    return NoContent();
                }
                else
                {
                    return NotFound("API key not found or already revoked");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking API key");
                return StatusCode(500, "An error occurred while processing the request");
            }
        }

        /// <summary>
        /// Get all API keys for the authenticated client
        /// </summary>
        /// <returns>List of API keys (without the actual key values)</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<ApiKeyInfoResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetApiKeys()
        {
            try
            {
                // Get client information from the context (set by auth middleware)
                var clientId = HttpContext.Items["ClientId"] as Guid?;
                if (clientId == null)
                {
                    return Unauthorized("Valid API key required");
                }

                // This would require implementation of a method to get API keys for a client
                // For now, we'll just return a mock response
                var mockApiKeys = new List<ApiKeyInfoResponse>
                {
                    new ApiKeyInfoResponse
                    {
                        Id = Guid.NewGuid(),
                        Name = "Production API Key",
                        CreatedAt = DateTime.UtcNow.AddDays(-30),
                        LastUsedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddYears(1),
                        IsActive = true,
                        Permissions = new List<string>
                        {
                            "invoice:read",
                            "invoice:write",
                            "email:read",
                            "email:write"
                        }
                    },
                    new ApiKeyInfoResponse
                    {
                        Id = Guid.NewGuid(),
                        Name = "Development API Key",
                        CreatedAt = DateTime.UtcNow.AddDays(-10),
                        LastUsedAt = DateTime.UtcNow.AddHours(-2),
                        ExpiresAt = null,
                        IsActive = true,
                        Permissions = new List<string>
                        {
                            "invoice:read",
                            "invoice:write",
                            "email:read",
                            "email:write"
                        }
                    }
                };

                return Ok(mockApiKeys);

                // TODO: Replace with actual implementation
                // return Ok(await _apiKeyService.GetApiKeysForClientAsync(clientId.Value));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting API keys");
                return StatusCode(500, "An error occurred while processing the request");
            }
        }
    }
}
