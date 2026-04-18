using System.Text;
using System.Threading.RateLimiting;
using FootballBlog.API.ApiClients.FootballApi;
using FootballBlog.API.Hubs;
using FootballBlog.API.Jobs;
using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Interfaces.Services;
using FootballBlog.Core.Models;
using FootballBlog.Core.Options;
using FootballBlog.Core.Services;
using FootballBlog.Infrastructure.Data;
using FootballBlog.Infrastructure.Repositories;
using FootballBlog.Infrastructure.Services;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;

const string OutputTemplate = "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {SourceContext} | {Message:lj}{NewLine}{Exception}";

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Đường dẫn log tập trung tại solution root /logs/
    var logBasePath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "logs"));

    builder.Host.UseSerilog((context, services, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.When(
                e => e.Properties.TryGetValue("SourceContext", out var sc) &&
                    (sc.ToString().Contains("\"FootballBlog.API.Jobs") || sc.ToString().Contains("\"Hangfire")),
                e => e.WithProperty("IsJobLog", true))
            // Console
            .WriteTo.Console(outputTemplate: OutputTemplate)
            // app/ — log chung toàn app (Information+)
            .WriteTo.File(Path.Combine(logBasePath, "app", "app-.log"),
                rollingInterval: RollingInterval.Day,
                outputTemplate: OutputTemplate)
            // error/ — chỉ Error + Fatal
            .WriteTo.File(Path.Combine(logBasePath, "error", "error-.log"),
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: LogEventLevel.Error,
                outputTemplate: OutputTemplate)
            // api/ — HTTP request/response log (từ UseSerilogRequestLogging middleware)
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(e => e.Properties.ContainsKey("RequestPath"))
                .WriteTo.File(Path.Combine(logBasePath, "api", "api-.log"),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: OutputTemplate))
            // jobs/ — Hangfire background jobs + job classes
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(e =>
                    e.Properties.TryGetValue("IsJobLog", out var isJob) && isJob.ToString() == "True")
                .WriteTo.File(Path.Combine(logBasePath, "jobs", "jobs-.log"),
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: OutputTemplate)));

    builder.Services.AddControllers();

    // EF Core + PostgreSQL
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    // ASP.NET Core Identity
    builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

    // JWT Bearer — override default scheme (AddIdentity dùng IdentityConstants.ApplicationScheme)
    // Sau khi override, [Authorize] trên API controllers dùng JWT Bearer
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var key = builder.Configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("Jwt:Key chưa được cấu hình. Chạy: dotnet user-secrets set \"Jwt:Key\" \"<32+ ký tự>\"");
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });

    // Unit of Work (bao gồm tất cả repositories)
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    // Services
    builder.Services.AddScoped<IPostService, PostService>();
    builder.Services.AddScoped<ICategoryService, CategoryService>();
    builder.Services.AddScoped<ILiveScoreService, LiveScoreService>();

    // AI Prediction providers — Claude (primary) + Gemini (fallback)
    builder.Services.AddScoped<IAIPredictionProvider, ClaudeAIPredictionProvider>();
    builder.Services.AddScoped<IAIPredictionProvider, GeminiAIPredictionProvider>();

    // API Key rotation
    builder.Services.AddScoped<IApiKeyRotator, ApiKeyRotator>();
    builder.Services.AddScoped<ApiKeySeeder>();

    // Telegram
    builder.Services.AddScoped<ITelegramService, TelegramService>();

    // Output Cache — cache GET blog endpoints 5 phút, invalidate khi có write
    builder.Services.AddOutputCache(options =>
    {
        options.AddPolicy("BlogPages", p => p.Expire(TimeSpan.FromMinutes(5)).Tag("posts"));
    });

    // ── Phase 4: Football API Integration ────────────────────────────────────

    // 1. Options
    builder.Services.Configure<FootballApiOptions>(
        builder.Configuration.GetSection(FootballApiOptions.SectionName));

    // 2. Redis connection — singleton, thread-safe
    builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
        ConnectionMultiplexer.Connect(
            builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379"));

    // 3. Rate limiter — singleton (chia sẻ Redis singleton)
    builder.Services.AddSingleton<IFootballApiRateLimiter, RedisFootballApiRateLimiter>();

    // 4. Football API typed HttpClient + Polly retry (3 lần, exponential backoff)
    builder.Services.AddHttpClient<IFootballApiClient, FootballApiClient>(client =>
    {
        client.BaseAddress = new Uri(
            builder.Configuration["FootballApi:BaseUrl"] ?? "https://v3.football.api-sports.io");
    })
    .AddPolicyHandler(HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))));

    // 5. SignalR + Redis backplane
    builder.Services.AddSignalR()
        .AddStackExchangeRedis(
            builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379");

    // 6. Hangfire — PostgreSQL storage
    builder.Services.AddHangfire(cfg => cfg
        .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
        .UseSimpleAssemblyNameTypeSerializer()
        .UseRecommendedSerializerSettings()
        .UsePostgreSqlStorage(
            o => o.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")),
            new PostgreSqlStorageOptions { SchemaName = "hangfire" }));

    builder.Services.AddHangfireServer(opt => { opt.WorkerCount = 2; });

    // ── End Phase 4 ───────────────────────────────────────────────────────────

    // Health checks
    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!)
        .AddRedis(builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379");

    // Rate Limiting — login 5 req/phút/IP (theo security.md)
    builder.Services.AddRateLimiter(options =>
    {
        options.AddFixedWindowLimiter("login", o =>
        {
            o.PermitLimit = 5;
            o.Window = TimeSpan.FromMinutes(1);
            o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            o.QueueLimit = 0;
        });
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });

    // CORS — chỉ allow Web project gọi vào API
    builder.Services.AddCors(options =>
        options.AddPolicy("BlazorWeb", policy =>
            policy.WithOrigins(builder.Configuration["WebBaseUrl"] ?? "https://localhost:7000")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials()));

    // Swagger — chỉ dùng trong Development
    if (builder.Environment.IsDevelopment())
    {
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "FootballBlog API", Version = "v1" }));
    }

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FootballBlog API v1"));
        app.UseHangfireDashboard("/hangfire");
    }

    // Recurring jobs — toggle từng job qua appsettings "Jobs" section
    IConfigurationSection jobs = app.Configuration.GetSection("Jobs");

    if (jobs.GetValue<bool>("FetchUpcomingMatches"))
    {
        RecurringJob.AddOrUpdate<FetchUpcomingMatchesJob>(
            "fetch-upcoming-matches",
            j => j.ExecuteAsync(),
            "0 */6 * * *",
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
    }
    else
    {
        RecurringJob.RemoveIfExists("fetch-upcoming-matches");
    }

    if (jobs.GetValue<bool>("LiveScorePolling"))
    {
        RecurringJob.AddOrUpdate<LiveScorePollingJob>(
            "live-score-polling",
            j => j.ExecuteAsync(),
            Cron.Minutely(),
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
    }
    else
    {
        RecurringJob.RemoveIfExists("live-score-polling");
    }

    if (jobs.GetValue<bool>("GeneratePrediction"))
    {
        RecurringJob.AddOrUpdate<GeneratePredictionJob>(
            "generate-predictions",
            j => j.ExecuteAsync(),
            Cron.Hourly(),
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
    }
    else
    {
        RecurringJob.RemoveIfExists("generate-predictions");
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseCors("BlazorWeb");
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseOutputCache();
    app.MapControllers();
    app.MapHub<LiveScoreHub>("/hubs/livescore");
    app.MapHealthChecks("/health");

    // Seed API keys từ appsettings vào DB (chỉ chạy nếu bảng còn trống)
    using (var scope = app.Services.CreateScope())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<ApiKeySeeder>();
        await seeder.SeedAsync();
    }

    // Seed default admin user nếu chưa tồn tại
    using (var scope = app.Services.CreateScope())
    {
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
        var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole<int>("Admin"));
        }

        var email = cfg["DefaultAdmin:Email"] ?? "admin@footballblog.dev";
        if (await userManager.FindByEmailAsync(email) == null)
        {
            var admin = new ApplicationUser { Email = email, UserName = email, EmailConfirmed = true };
            var result = await userManager.CreateAsync(admin, cfg["DefaultAdmin:Password"] ?? "Admin123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
            else
            {
                Log.Warning("Failed to seed admin user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    Log.Information("FootballBlog API starting up");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
