using System;
using System.Collections.Generic;

namespace Dualite.Models.Responses
{
    public class InvoiceJobStatusResponse
    {
        /// <summary>
        /// Unique identifier for the job
        /// </summary>
        public Guid JobId { get; set; }

        /// <summary>
        /// Current status of the job
        /// </summary>
        public string Status { get; set; } = "pending";

        /// <summary>
        /// Timestamp when the job was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Timestamp when the job was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Error message if the job failed
        /// </summary>
        public string? Error { get; set; }

        /// <summary>
        /// Progress percentage (0-100)
        /// </summary>
        public int Progress { get; set; } = 0;

        /// <summary>
        /// Extracted invoice data
        /// </summary>
        public InvoiceExtractionData? Data { get; set; }
    }

    public class InvoiceExtractionData
    {
        /// <summary>
        /// The type of invoice document
        /// </summary>
        public string? DocumentType { get; set; }

        /// <summary>
        /// Invoice number
        /// </summary>
        public string? InvoiceNumber { get; set; }

        /// <summary>
        /// Invoice date
        /// </summary>
        public DateTime? InvoiceDate { get; set; }

        /// <summary>
        /// Due date
        /// </summary>
        public DateTime? DueDate { get; set; }

        /// <summary>
        /// Total amount
        /// </summary>
        public decimal? TotalAmount { get; set; }

        /// <summary>
        /// Tax/VAT amount
        /// </summary>
        public decimal? TaxAmount { get; set; }

        /// <summary>
        /// Net amount
        /// </summary>
        public decimal? NetAmount { get; set; }

        /// <summary>
        /// Currency code (e.g., USD, EUR, GBP)
        /// </summary>
        public string? CurrencyCode { get; set; }

        /// <summary>
        /// Vendor/seller information
        /// </summary>
        public VendorInfo? Vendor { get; set; }

        /// <summary>
        /// Customer/buyer information
        /// </summary>
        public CustomerInfo? Customer { get; set; }

        /// <summary>
        /// Payment information
        /// </summary>
        public PaymentInfo? Payment { get; set; }

        /// <summary>
        /// Line items on the invoice
        /// </summary>
        public List<LineItem>? LineItems { get; set; }

        /// <summary>
        /// Any tax breakdown (e.g., different VAT rates)
        /// </summary>
        public List<TaxItem>? TaxBreakdown { get; set; }

        /// <summary>
        /// Confidence score for the extraction (0-100)
        /// </summary>
        public int ConfidenceScore { get; set; }

        /// <summary>
        /// Any validation warnings or errors
        /// </summary>
        public List<ValidationMessage>? ValidationMessages { get; set; }
    }

    public class VendorInfo
    {
        /// <summary>
        /// Vendor name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Vendor address
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Vendor tax/VAT ID
        /// </summary>
        public string? TaxId { get; set; }

        /// <summary>
        /// Vendor registration number (e.g., company number)
        /// </summary>
        public string? RegistrationNumber { get; set; }

        /// <summary>
        /// Vendor contact information (e.g., phone, email)
        /// </summary>
        public Dictionary<string, string>? ContactInfo { get; set; }

        /// <summary>
        /// Vendor bank account details
        /// </summary>
        public BankDetails? BankDetails { get; set; }
    }

    public class CustomerInfo
    {
        /// <summary>
        /// Customer name
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Customer address
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// Customer tax/VAT ID
        /// </summary>
        public string? TaxId { get; set; }

        /// <summary>
        /// Customer reference number
        /// </summary>
        public string? CustomerNumber { get; set; }

        /// <summary>
        /// Purchase order number
        /// </summary>
        public string? PurchaseOrderNumber { get; set; }
    }

    public class PaymentInfo
    {
        /// <summary>
        /// Payment terms
        /// </summary>
        public string? Terms { get; set; }

        /// <summary>
        /// Payment method
        /// </summary>
        public string? Method { get; set; }

        /// <summary>
        /// Payment status
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Payment due date
        /// </summary>
        public DateTime? DueDate { get; set; }
    }

    public class BankDetails
    {
        /// <summary>
        /// Account holder name
        /// </summary>
        public string? AccountHolder { get; set; }

        /// <summary>
        /// Bank name
        /// </summary>
        public string? BankName { get; set; }

        /// <summary>
        /// IBAN (International Bank Account Number)
        /// </summary>
        public string? Iban { get; set; }

        /// <summary>
        /// BIC/SWIFT code
        /// </summary>
        public string? Bic { get; set; }

        /// <summary>
        /// Account number
        /// </summary>
        public string? AccountNumber { get; set; }

        /// <summary>
        /// Sort code / routing number
        /// </summary>
        public string? SortCode { get; set; }
    }

    public class LineItem
    {
        /// <summary>
        /// Item description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Quantity
        /// </summary>
        public decimal? Quantity { get; set; }

        /// <summary>
        /// Unit of measure
        /// </summary>
        public string? Unit { get; set; }

        /// <summary>
        /// Unit price
        /// </summary>
        public decimal? UnitPrice { get; set; }

        /// <summary>
        /// Net amount (before tax)
        /// </summary>
        public decimal? NetAmount { get; set; }

        /// <summary>
        /// Tax amount
        /// </summary>
        public decimal? TaxAmount { get; set; }

        /// <summary>
        /// Tax rate percentage
        /// </summary>
        public decimal? TaxRate { get; set; }

        /// <summary>
        /// Total amount (after tax)
        /// </summary>
        public decimal? TotalAmount { get; set; }

        /// <summary>
        /// Item code or SKU
        /// </summary>
        public string? ItemCode { get; set; }
    }

    public class TaxItem
    {
        /// <summary>
        /// Tax/VAT rate percentage
        /// </summary>
        public decimal? Rate { get; set; }

        /// <summary>
        /// Tax category or name
        /// </summary>
        public string? Category { get; set; }

        /// <summary>
        /// Net amount subject to this tax rate
        /// </summary>
        public decimal? NetAmount { get; set; }

        /// <summary>
        /// Tax amount
        /// </summary>
        public decimal? TaxAmount { get; set; }
    }

    public class ValidationMessage
    {
        /// <summary>
        /// Message type ("warning" or "error")
        /// </summary>
        public string Type { get; set; } = "warning";

        /// <summary>
        /// Message code
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// Message text
        /// </summary>
        public string Message { get; set; } = "";

        /// <summary>
        /// Field that the message relates to
        /// </summary>
        public string? Field { get; set; }
    }
}
