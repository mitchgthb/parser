using Dualite.Models;
using Dualite.Models.Requests;
using Dualite.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dualite.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<EmailController> _logger;

        public EmailController(
            IHttpClientFactory httpClientFactory,
            ILogger<EmailController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// Process an email and extract insights using NLP
        /// </summary>
        /// <param name="request">Email processing request</param>
        /// <returns>Job ID for tracking the email processing progress</returns>
        [HttpPost("process")]
        [ProducesResponseType(typeof(JobResponse), StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> ProcessEmail([FromBody] EmailProcessRequest request)
        {
            try
            {
                // Get client information from the context (set by auth middleware)
                var clientId = HttpContext.Items["ClientId"] as Guid?;
                if (clientId == null)
                {
                    return Unauthorized("Valid API key required");
                }

                // Validate the request
                if (string.IsNullOrWhiteSpace(request.EmailContent) && 
                    string.IsNullOrWhiteSpace(request.Subject))
                {
                    return BadRequest("Email content or subject must be provided");
                }

                // Forward the request to the Email NLP microservice
                var client = _httpClientFactory.CreateClient("EmailNlpService");
                
                // Add client ID to the request
                var microserviceRequest = new 
                {
                    client_id = clientId.ToString(),
                    subject = request.Subject,
                    sender_email = request.SenderEmail,
                    sender_name = request.SenderName,
                    recipient_emails = request.RecipientEmails,
                    cc_emails = request.CcEmails,
                    email_content = request.EmailContent,
                    options = new
                    {
                        extract_entities = request.ExtractEntities ?? true,
                        classify_intent = request.ClassifyIntent ?? true,
                        calculate_urgency = request.CalculateUrgency ?? true,
                        estimate_effort = request.EstimateEffort ?? true,
                        language = request.Language ?? "en"
                    }
                };

                var jsonContent = JsonSerializer.Serialize(microserviceRequest);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Send to microservice
                var response = await client.PostAsync("/api/v1/emails/process", content);

                // Process response from microservice
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var jobResponse = JsonSerializer.Deserialize<JobResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return Accepted(jobResponse);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    return StatusCode(429, "Rate limit exceeded");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Email NLP microservice error: {ErrorContent}", errorContent);
                    return StatusCode((int)response.StatusCode, "Error processing email");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing email");
                return StatusCode(500, "An error occurred while processing the request");
            }
        }

        /// <summary>
        /// Get the status and results of an email processing job
        /// </summary>
        /// <param name="jobId">ID of the processing job</param>
        /// <returns>Current status and any available results</returns>
        [HttpGet("jobs/{jobId}")]
        [ProducesResponseType(typeof(EmailJobStatusResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetJobStatus(Guid jobId)
        {
            try
            {
                // Get client information from the context (set by auth middleware)
                var clientId = HttpContext.Items["ClientId"] as Guid?;
                if (clientId == null)
                {
                    return Unauthorized("Valid API key required");
                }

                // Forward request to microservice
                var client = _httpClientFactory.CreateClient("EmailNlpService");
                var response = await client.GetAsync($"/api/v1/emails/jobs/{jobId}?client_id={clientId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var jobStatus = JsonSerializer.Deserialize<EmailJobStatusResponse>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return Ok(jobStatus);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return NotFound("Job not found");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return Forbid("You do not have permission to access this job");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Email NLP microservice error: {ErrorContent}", errorContent);
                    return StatusCode((int)response.StatusCode, "Error retrieving job status");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting job status");
                return StatusCode(500, "An error occurred while processing the request");
            }
        }

        /// <summary>
        /// List email processing jobs for the client
        /// </summary>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="status">Optional filter by job status</param>
        /// <returns>List of email processing jobs</returns>
        [HttpGet("jobs")]
        [ProducesResponseType(typeof(PagedResponse<EmailJobStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ListJobs([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null)
        {
            try
            {
                // Get client information from the context (set by auth middleware)
                var clientId = HttpContext.Items["ClientId"] as Guid?;
                if (clientId == null)
                {
                    return Unauthorized("Valid API key required");
                }

                // Forward request to microservice
                var client = _httpClientFactory.CreateClient("EmailNlpService");
                string url = $"/api/v1/emails/jobs?client_id={clientId}&page={page}&page_size={pageSize}";
                if (!string.IsNullOrEmpty(status))
                {
                    url += $"&status={status}";
                }
                
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var jobList = JsonSerializer.Deserialize<PagedResponse<EmailJobStatusResponse>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return Ok(jobList);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Email NLP microservice error: {ErrorContent}", errorContent);
                    return StatusCode((int)response.StatusCode, "Error retrieving jobs");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing jobs");
                return StatusCode(500, "An error occurred while processing the request");
            }
        }
    }
}
