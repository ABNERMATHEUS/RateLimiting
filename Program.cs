using AspNetCoreRateLimit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<IpRateLimitOptions>(options =>
{
    // Enables Rate Limiting for specific endpoints.
    options.EnableEndpointRateLimiting = true;

    // Determines if blocked requests will be stacked for later processing.
    options.StackBlockedRequests = false;

    // HTTP status code returned when rate limit is reached.
    options.HttpStatusCode = 429;

    // Header containing the real client IP address (useful when behind a proxy).
    options.RealIpHeader = "X-Real-IP";

    // Header used to identify different clients (for independent limits).
    options.ClientIdHeader = "X-ClientId";

    // Specific rate-limiting rules.
    options.GeneralRules = new List<RateLimitRule>
    {
        // Rule 1: Allows only 1 request every 10 seconds for the GET:/WeatherForecast endpoint.
        new ()
        {
            Endpoint = "GET:/WeatherForecast",
            Limit = 1,
            Period = "10s"
        },
    };
});

// Configures the IP policy to store information about IP policies in the memory cache.
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();

// Configures the counter storage for Rate Limiting, using the memory cache.
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();

// Configures the object representing general Rate Limiting settings.
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// Configures the processing strategy to handle rate limits, using asynchronous key locking.
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

// Adds the Rate Limiting configuration to memory, using the configured instances above.
builder.Services.AddInMemoryRateLimiting();

// Adds memory cache.
builder.Services.AddMemoryCache();

var app = builder.Build();

// Enables Rate Limiting.
app.UseIpRateLimiting();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//ATIVAR
app.UseIpRateLimiting();


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
