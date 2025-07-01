using System;
using System.Collections.Generic;

namespace Dualite.Models.Responses
{
    public class UsageResponse
    {
        /// <summary>
        /// Start date of the usage period
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End date of the usage period
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Total requests made during the period
        /// </summary>
        public int TotalRequests { get; set; }

        /// <summary>
        /// Dictionary of requests by endpoint
        /// </summary>
        public Dictionary<string, int>? RequestsByEndpoint { get; set; }

        /// <summary>
        /// Total number of API calls allowed in the current billing period
        /// </summary>
        public int QuotaLimit { get; set; }

        /// <summary>
        /// Remaining quota for this period
        /// </summary>
        public int QuotaRemaining { get; set; }

        /// <summary>
        /// Amount billed for this period
        /// </summary>
        public decimal BillingAmount { get; set; }

        /// <summary>
        /// Average response time in milliseconds
        /// </summary>
        public double? AverageResponseTimeMs { get; set; }
    }
}
