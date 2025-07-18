using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dualite.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CompanyName = table.Column<string>(type: "text", nullable: true),
                    ContactEmail = table.Column<string>(type: "text", nullable: true),
                    ServiceTier = table.Column<string>(type: "text", nullable: false),
                    MonthlyQuota = table.Column<int>(type: "integer", nullable: false),
                    QuotaUsed = table.Column<int>(type: "integer", nullable: false),
                    QuotaResetDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApiKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    KeyHash = table.Column<string>(type: "text", nullable: false),
                    KeyName = table.Column<string>(type: "text", nullable: true),
                    Permissions = table.Column<List<string>>(type: "text[]", nullable: false),
                    RateLimitPerMinute = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApiKeys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApiKeys_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProcessingJobs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApiKeyId = table.Column<Guid>(type: "uuid", nullable: false),
                    JobType = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    InputHash = table.Column<string>(type: "text", nullable: true),
                    InputMetadata = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    OutputData = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    ProcessingTimeMs = table.Column<int>(type: "integer", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessingJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessingJobs_ApiKeys_ApiKeyId",
                        column: x => x.ApiKeyId,
                        principalTable: "ApiKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProcessingJobs_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmailExtractions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderName = table.Column<string>(type: "text", nullable: true),
                    SenderEmail = table.Column<string>(type: "text", nullable: true),
                    SenderCompany = table.Column<string>(type: "text", nullable: true),
                    SubjectLine = table.Column<string>(type: "text", nullable: true),
                    DetectedIntent = table.Column<string>(type: "text", nullable: true),
                    EstimatedEffortMinutes = table.Column<int>(type: "integer", nullable: true),
                    UrgencyScore = table.Column<decimal>(type: "numeric", nullable: true),
                    ExtractedEntities = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    ConfidenceScores = table.Column<Dictionary<string, decimal>>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailExtractions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmailExtractions_ProcessingJobs_JobId",
                        column: x => x.JobId,
                        principalTable: "ProcessingJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceExtractions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JobId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "text", nullable: true),
                    InvoiceDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    VatAmount = table.Column<decimal>(type: "numeric", nullable: true),
                    VatRate = table.Column<decimal>(type: "numeric", nullable: true),
                    Currency = table.Column<string>(type: "text", nullable: false),
                    SellerName = table.Column<string>(type: "text", nullable: true),
                    SellerKvk = table.Column<string>(type: "text", nullable: true),
                    SellerIban = table.Column<string>(type: "text", nullable: true),
                    BuyerName = table.Column<string>(type: "text", nullable: true),
                    BuyerKvk = table.Column<string>(type: "text", nullable: true),
                    LineItems = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    ExtractedFields = table.Column<Dictionary<string, object>>(type: "jsonb", nullable: true),
                    ValidationStatus = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceExtractions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceExtractions_ProcessingJobs_JobId",
                        column: x => x.JobId,
                        principalTable: "ProcessingJobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApiKeys_ClientId",
                table: "ApiKeys",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_EmailExtractions_JobId",
                table: "EmailExtractions",
                column: "JobId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceExtractions_JobId",
                table: "InvoiceExtractions",
                column: "JobId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProcessingJobs_ApiKeyId",
                table: "ProcessingJobs",
                column: "ApiKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessingJobs_ClientId",
                table: "ProcessingJobs",
                column: "ClientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailExtractions");

            migrationBuilder.DropTable(
                name: "InvoiceExtractions");

            migrationBuilder.DropTable(
                name: "ProcessingJobs");

            migrationBuilder.DropTable(
                name: "ApiKeys");

            migrationBuilder.DropTable(
                name: "Clients");
        }
    }
}
