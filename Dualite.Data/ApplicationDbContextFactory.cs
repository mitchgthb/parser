using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Dualite.Data
{
    /// <summary>
    /// Design-time factory so that the `dotnet ef` CLI can construct an ApplicationDbContext
    /// when no dependency-injection container is running. It reads the connection string
    /// from the same configuration sources used at runtime (appsettings.* + env-vars).
    /// </summary>
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            // Allow CLI invocation from either solution root or project directory
            var basePath = Directory.GetCurrentDirectory();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{env}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // Build connection string: prefer ConnectionStrings:DefaultConnection; otherwise assemble from POSTGRES_* env vars
            var conn = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(conn))
            {
                var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
                var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5432";
                var user = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "postgres";
                var pwd  = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "postgres";
                var db   = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "postgres";
                conn = $"Host={host};Port={port};Database={db};Username={user};Password={pwd}";
            }

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(conn);

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
