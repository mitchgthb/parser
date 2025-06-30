using Dualite.Business.Services;
using Dualite.Data;
using Dualite.Data.Repositories;
using Dualite.Middleware;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.StackExchangeRedis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Add API versioning
builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
    npgsqlOptions => npgsqlOptions.EnableRetryOnFailure()));

// Add repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Add business services
builder.Services.AddScoped<IApiKeyService, ApiKeyService>();

// Add HttpClient for microservices
builder.Services.AddHttpClient("EmailNlpService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["EmailNlpService:BaseUrl"] ?? "http://localhost:8000");
});
builder.Services.AddHttpClient("InvoiceParserService", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["InvoiceParserService:BaseUrl"] ?? "http://localhost:8001");
});

// Add health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection") ?? string.Empty)
    .AddUrlGroup(new Uri(builder.Configuration["EmailNlpService:BaseUrl"] + "/health" ?? "http://localhost:8000/health"), "Email NLP Service")
    .AddUrlGroup(new Uri(builder.Configuration["InvoiceParserService:BaseUrl"] + "/health" ?? "http://localhost:8001/health"), "Invoice Parser Service");

// Add caching
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379";
});

// Add rate limiting
builder.Services.AddMemoryCache();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Use API key authentication
app.UseMiddleware<ApiKeyAuthenticationMiddleware>();

app.UseAuthorization();

// Add health check endpoint
app.MapHealthChecks("/health");

app.MapControllers();

// Initialize database if needed
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database initialization.");
    }
}

app.Run();
