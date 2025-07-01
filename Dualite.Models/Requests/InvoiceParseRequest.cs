using Microsoft.AspNetCore.Http;

namespace Dualite.Models.Requests
{
    public class InvoiceParseRequest
    {
        /// <summary>
        /// The invoice file to parse
        /// </summary>
        public IFormFile? File { get; set; }

        /// <summary>
        /// Optional language code for OCR and extraction (e.g., en, fr, de)
        /// </summary>
        public string? Language { get; set; }

        /// <summary>
        /// Whether to use OCR for text extraction. If null, service will determine automatically.
        /// </summary>
        public bool? UseOcr { get; set; }
    }
}
