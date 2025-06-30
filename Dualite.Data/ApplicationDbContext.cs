using Microsoft.EntityFrameworkCore;
using Dualite.Models;

namespace Dualite.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
            : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<ApiKey> ApiKeys { get; set; }
        public DbSet<ProcessingJob> ProcessingJobs { get; set; }
        public DbSet<EmailExtraction> EmailExtractions { get; set; }
        public DbSet<InvoiceExtraction> InvoiceExtractions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure relationships
            modelBuilder.Entity<ApiKey>()
                .HasOne(a => a.Client)
                .WithMany(c => c.ApiKeys)
                .HasForeignKey(a => a.ClientId);

            modelBuilder.Entity<ProcessingJob>()
                .HasOne(p => p.Client)
                .WithMany(c => c.ProcessingJobs)
                .HasForeignKey(p => p.ClientId);

            modelBuilder.Entity<ProcessingJob>()
                .HasOne(p => p.ApiKey)
                .WithMany(a => a.ProcessingJobs)
                .HasForeignKey(p => p.ApiKeyId);

            modelBuilder.Entity<EmailExtraction>()
                .HasOne(e => e.ProcessingJob)
                .WithOne(p => p.EmailExtraction)
                .HasForeignKey<EmailExtraction>(e => e.JobId);

            modelBuilder.Entity<InvoiceExtraction>()
                .HasOne(i => i.ProcessingJob)
                .WithOne(p => p.InvoiceExtraction)
                .HasForeignKey<InvoiceExtraction>(i => i.JobId);

            // JSON column configurations
            modelBuilder.Entity<ProcessingJob>()
                .Property(p => p.InputMetadata)
                .HasColumnType("jsonb");

            modelBuilder.Entity<ProcessingJob>()
                .Property(p => p.OutputData)
                .HasColumnType("jsonb");

            modelBuilder.Entity<EmailExtraction>()
                .Property(e => e.ExtractedEntities)
                .HasColumnType("jsonb");

            modelBuilder.Entity<EmailExtraction>()
                .Property(e => e.ConfidenceScores)
                .HasColumnType("jsonb");

            modelBuilder.Entity<InvoiceExtraction>()
                .Property(i => i.LineItems)
                .HasColumnType("jsonb");

            modelBuilder.Entity<InvoiceExtraction>()
                .Property(i => i.ExtractedFields)
                .HasColumnType("jsonb");

            modelBuilder.Entity<ApiKey>()
                .Property(a => a.Permissions)
                .HasColumnType("text[]");
        }
    }
}
