using Dualite.Business.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Dualite.Middleware
{
    public class ApiKeyAuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiKeyAuthenticationMiddleware> _logger;

        public ApiKeyAuthenticationMiddleware(RequestDelegate next, ILogger<ApiKeyAuthenticationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IApiKeyService apiKeyService)
        {
            // Skip authentication for endpoints that don't require it
            if (ShouldSkipAuthentication(context))
            {
                await _next(context);
                return;
            }

            // Try to extract API key from headers, query, or request body
            string apiKey = string.Empty;
            
            if (context.Request.Headers.TryGetValue("X-API-Key", out var apiKeyHeaderValues))
            {
                apiKey = apiKeyHeaderValues.ToString();
            }
            else if (context.Request.Query.TryGetValue("api_key", out var apiKeyQueryValues))
            {
                apiKey = apiKeyQueryValues.ToString();
            }
            else
            {
                _logger.LogWarning("API key missing in request");
                context.Response.StatusCode = 401; // Unauthorized
                await context.Response.WriteAsync("API key is missing");
                return;
            }

            // Check if apiKey is empty after extraction

            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("API key is empty");
                context.Response.StatusCode = 401; // Unauthorized
                await context.Response.WriteAsync("API key is required");
                return;
            }

            // Validate the API key
            var validationResult = await apiKeyService.ValidateApiKeyAsync(apiKey);

            if (!validationResult.IsValid)
            {
                _logger.LogWarning($"Invalid API key attempted: {apiKey}");
                context.Response.StatusCode = 401; // Unauthorized
                await context.Response.WriteAsync(validationResult.Error ?? "Invalid API key");
                return;
            }

            // Add client information to the context for downstream use
            context.Items["ClientId"] = validationResult.ClientId;
            context.Items["ApiKeyId"] = validationResult.ApiKeyId;
            context.Items["Permissions"] = validationResult.Permissions;

            // Log successful API key usage
            _logger.LogInformation($"API key validated for client {validationResult.ClientId}");

            // Update last used timestamp (fire and forget)
            _ = apiKeyService.UpdateApiKeyUsageAsync(validationResult.ApiKeyId);

            await _next(context);
        }

        private static bool ShouldSkipAuthentication(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant();

            // Skip authentication for health check, docs, and login endpoints
            return path != null && (
                path.StartsWith("/health") ||
                path.StartsWith("/swagger") ||
                path.StartsWith("/docs") ||
                path.StartsWith("/auth") ||
                path.Equals("/"));
        }
    }
}
