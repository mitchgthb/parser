using Dualite.Business.Services;
using Dualite.Models;
using Dualite.Models.Requests;
using Dualite.Models.Responses;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Dualite.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<InvoiceController> _logger;

        public InvoiceController(
            IHttpClientFactory httpClientFactory,
            ILogger<InvoiceController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// Parse an invoice file and extract structured data
        /// </summary>
        /// <param name="request">Invoice parsing request</param>
        /// <returns>Job ID for tracking the invoice parsing progress</returns>
        [HttpPost("parse")]
        [ProducesResponseType(typeof(JobResponse), StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> ParseInvoice([FromForm] InvoiceParseRequest request)
        {
            try
            {
                // Get client information from the context (set by auth middleware)
                var clientId = HttpContext.Items["ClientId"] as Guid?;
                if (clientId == null)
                {
                    return Unauthorized("Valid API key required");
                }

                // Validate the file
                if (request.File == null || request.File.Length == 0)
                {
                    return BadRequest("No file was uploaded");
                }

                // Check file type
                string extension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
                if (extension != ".pdf" && extension != ".png" && extension != ".jpg" && extension != ".jpeg")
                {
                    return BadRequest("Unsupported file format. Please upload a PDF or image file.");
                }

                // Forward the request to the Invoice Parser microservice
                var client = _httpClientFactory.CreateClient("InvoiceParserService");

                // Create multipart form content
                using var formContent = new MultipartFormDataContent();

                // Add client ID
                formContent.Add(new StringContent(clientId.ToString()), "client_id");

                // Convert IFormFile to byte array for the HTTP client
                using var memoryStream = new MemoryStream();
                await request.File.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();

                // Add file content
                var fileContent = new ByteArrayContent(fileBytes);
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(request.File.ContentType);
                formContent.Add(fileContent, "file", request.File.FileName);

                // Add optional parameters
                if (!string.IsNullOrEmpty(request.Language))
                {
                    formContent.Add(new StringContent(request.Language), "language");
                }

                if (request.UseOcr.HasValue)
                {
                    formContent.Add(new StringContent(request.UseOcr.Value.ToString()), "use_ocr");
                }

                // Send to microservice
                var response = await client.PostAsync("/api/v1/invoices/parse", formContent);

                // Process response from microservice
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var jobResponse = JsonSerializer.Deserialize<JobResponse>(content, new JsonSerializerOptions
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
                    _logger.LogError("Invoice microservice error: {ErrorContent}", errorContent);
                    return StatusCode((int)response.StatusCode, "Error processing invoice");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing invoice");
                return StatusCode(500, "An error occurred while processing the request");
            }
        }

        /// <summary>
        /// Get the status and results of an invoice parsing job
        /// </summary>
        /// <param name="jobId">ID of the parsing job</param>
        /// <returns>Current status and any available results</returns>
        [HttpGet("jobs/{jobId}")]
        [ProducesResponseType(typeof(InvoiceJobStatusResponse), StatusCodes.Status200OK)]
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
                var client = _httpClientFactory.CreateClient("InvoiceParserService");
                var response = await client.GetAsync($"/api/v1/invoices/jobs/{jobId}?client_id={clientId}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var jobStatus = JsonSerializer.Deserialize<InvoiceJobStatusResponse>(content, new JsonSerializerOptions
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
                    _logger.LogError("Invoice microservice error: {ErrorContent}", errorContent);
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
        /// List invoice parsing jobs for the client
        /// </summary>
        /// <param name="page">Page number</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <param name="status">Optional filter by job status</param>
        /// <returns>List of invoice parsing jobs</returns>
        [HttpGet("jobs")]
        [ProducesResponseType(typeof(PagedResponse<InvoiceJobStatusResponse>), StatusCodes.Status200OK)]
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
                var client = _httpClientFactory.CreateClient("InvoiceParserService");
                string url = $"/api/v1/invoices/jobs?client_id={clientId}&page={page}&page_size={pageSize}";
                if (!string.IsNullOrEmpty(status))
                {
                    url += $"&status={status}";
                }
                
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var jobList = JsonSerializer.Deserialize<PagedResponse<InvoiceJobStatusResponse>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return Ok(jobList);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Invoice microservice error: {ErrorContent}", errorContent);
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
