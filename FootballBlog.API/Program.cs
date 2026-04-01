using FootballBlog.Infrastructure.Data;
using FootballBlog.Infrastructure.Repositories;
using FootballBlog.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
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

    // Repositories
    builder.Services.AddScoped<IPostRepository, PostRepository>();
    builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
    builder.Services.AddScoped<ITagRepository, TagRepository>();
    builder.Services.AddScoped<ILiveMatchRepository, LiveMatchRepository>();

    // Health checks
    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!);

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
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
