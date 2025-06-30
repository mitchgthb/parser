using Dualite.Models.Requests;
using Dualite.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Dualite.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;

        public AuthController(ILogger<AuthController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Register a new client
        /// </summary>
        /// <param name="request">Client registration request</param>
        /// <returns>Client information and initial API key</returns>
        [HttpPost("register")]
        [ProducesResponseType(typeof(ClientRegistrationResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterClient([FromBody] ClientRegistrationRequest request)
        {
            try
            {
                // Validate request
                if (string.IsNullOrWhiteSpace(request.Name) || 
                    string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest("Name and email are required");
                }

                // For now, return a mock response
                // In a real implementation, we would create a client and initial API key
                var clientId = Guid.NewGuid();
                var apiKeyId = Guid.NewGuid();
                var apiKey = $"dk_{Guid.NewGuid().ToString("N")}";

                var response = new ClientRegistrationResponse
                {
                    ClientId = clientId,
                    Name = request.Name,
                    Email = request.Email,
                    CreatedAt = DateTime.UtcNow,
                    ApiKey = new ApiKeyResponse
                    {
                        ApiKey = apiKey,
                        Name = "Initial API Key",
                        CreatedAt = DateTime.UtcNow,
                        ExpiresAt = null,
                        Permissions = new System.Collections.Generic.List<string>
                        {
                            "invoice:read",
                            "invoice:write",
                            "email:read",
                            "email:write"
                        }
                    },
                    SubscriptionLevel = "free",
                    QuotaLimit = 100,
                    QuotaUsed = 0
                };

                return CreatedAtAction(nameof(GetClient), new { clientId = clientId }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering client");
                return StatusCode(500, "An error occurred while processing the request");
            }
        }

        /// <summary>
        /// Get client information
        /// </summary>
        /// <returns>Client information</returns>
        [HttpGet("client")]
        [ProducesResponseType(typeof(ClientInfoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetClient()
        {
            try
            {
                // Get client information from the context (set by auth middleware)
                var clientId = HttpContext.Items["ClientId"] as Guid?;
                if (clientId == null)
                {
                    return Unauthorized("Valid API key required");
                }

                // For now, return a mock response
                // In a real implementation, we would fetch the client info from database
                var response = new ClientInfoResponse
                {
                    ClientId = clientId.Value,
                    Name = "Example Client",
                    Email = "client@example.com",
                    CreatedAt = DateTime.UtcNow.AddDays(-30),
                    IsActive = true,
                    SubscriptionLevel = "professional",
                    QuotaLimit = 10000,
                    QuotaUsed = 1234,
                    ApiKeyCount = 3
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting client info");
                return StatusCode(500, "An error occurred while processing the request");
            }
        }

        /// <summary>
        /// Get usage statistics for the client
        /// </summary>
        /// <param name="startDate">Start date for the usage period</param>
        /// <param name="endDate">End date for the usage period</param>
        /// <returns>Usage statistics</returns>
        [HttpGet("usage")]
        [ProducesResponseType(typeof(UsageResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUsage(
            [FromQuery] DateTime? startDate = null, 
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                // Get client information from the context (set by auth middleware)
                var clientId = HttpContext.Items["ClientId"] as Guid?;
                if (clientId == null)
                {
                    return Unauthorized("Valid API key required");
                }

                // Default date range to the current month if not specified
                startDate ??= new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
                endDate ??= startDate.Value.AddMonths(1).AddDays(-1);

                // For now, return a mock response
                // In a real implementation, we would fetch usage data from database
                var response = new UsageResponse
                {
                    StartDate = startDate.Value,
                    EndDate = endDate.Value,
                    TotalRequests = 1234,
                    RequestsByEndpoint = new System.Collections.Generic.Dictionary<string, int>
                    {
                        { "/api/v1/invoice/parse", 567 },
                        { "/api/v1/email/process", 456 },
                        { "/api/v1/invoice/jobs", 123 },
                        { "/api/v1/email/jobs", 88 }
                    },
                    QuotaLimit = 10000,
                    QuotaRemaining = 8766,
                    BillingAmount = 49.99m
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting usage stats");
                return StatusCode(500, "An error occurred while processing the request");
            }
        }
    }
}
