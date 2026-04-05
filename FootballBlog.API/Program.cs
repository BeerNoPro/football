using FootballBlog.Core.Interfaces;
using FootballBlog.Core.Interfaces.Services;
using FootballBlog.Core.Models;
using FootballBlog.Core.Services;
using FootballBlog.Infrastructure.Data;
using FootballBlog.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

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
            // jobs/ — Hangfire background jobs (Phase 4)
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(e =>
                    e.Properties.TryGetValue("SourceContext", out var sc) &&
                    sc.ToString().Contains("Hangfire"))
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

    // Unit of Work (bao gồm tất cả repositories)
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    // Services
    builder.Services.AddScoped<IPostService, PostService>();
    builder.Services.AddScoped<ICategoryService, CategoryService>();

    // Output Cache — cache GET blog endpoints 5 phút, invalidate khi có write
    builder.Services.AddOutputCache(options =>
    {
        options.AddPolicy("BlogPages", p => p.Expire(TimeSpan.FromMinutes(5)).Tag("posts"));
    });

    // Health checks
    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

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
    }

    app.UseHttpsRedirection();
    app.UseCors("BlazorWeb");
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseOutputCache();
    app.MapControllers();
    app.MapHealthChecks("/health");

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
