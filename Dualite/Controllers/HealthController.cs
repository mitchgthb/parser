using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Dualite.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly HealthCheckService _healthCheckService;
        private readonly ILogger<HealthController> _logger;

        public HealthController(
            HealthCheckService healthCheckService,
            ILogger<HealthController> logger)
        {
            _healthCheckService = healthCheckService;
            _logger = logger;
        }

        /// <summary>
        /// Get the health status of the system and its dependencies
        /// </summary>
        /// <returns>Health status report</returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var report = await _healthCheckService.CheckHealthAsync();
            
            var response = new 
            {
                Status = report.Status.ToString(),
                Components = report.Entries.Select(e => new 
                {
                    Key = e.Key,
                    Status = e.Value.Status.ToString(),
                    Description = e.Value.Description,
                    Duration = e.Value.Duration
                }).ToArray()
            };

            return report.Status == HealthStatus.Healthy 
                ? Ok(response) 
                : StatusCode((int)HttpStatusCode.ServiceUnavailable, response);
        }

        /// <summary>
        /// Simple health endpoint for load balancers
        /// </summary>
        /// <returns>200 OK if service is running</returns>
        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("pong");
        }
    }
}
