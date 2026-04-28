using FootballBlog.Web.ApiClients;
using FootballBlog.Web.Components;
using FootballBlog.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using MudBlazor.Services;
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
            // app/ — log chung (Information+)
            .WriteTo.File(Path.Combine(logBasePath, "app", "web-.log"),
                rollingInterval: RollingInterval.Day,
                outputTemplate: OutputTemplate)
            // error/ — chỉ Error + Fatal
            .WriteTo.File(Path.Combine(logBasePath, "error", "web-error-.log"),
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: LogEventLevel.Error,
                outputTemplate: OutputTemplate));

    var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
        ?? throw new InvalidOperationException("ApiBaseUrl không được cấu hình");

    // Typed HttpClients để gọi FootballBlog.API
    builder.Services.AddHttpContextAccessor();
    builder.Services.AddHttpClient<IPostApiClient, PostApiClient>(client =>
        client.BaseAddress = new Uri(apiBaseUrl));

    builder.Services.AddHttpClient<ICategoryApiClient, CategoryApiClient>(client =>
        client.BaseAddress = new Uri(apiBaseUrl));

    builder.Services.AddHttpClient<ITagApiClient, TagApiClient>(client =>
        client.BaseAddress = new Uri(apiBaseUrl));

    builder.Services.AddHttpClient<ILiveScoreApiClient, LiveScoreApiClient>(client =>
        client.BaseAddress = new Uri(apiBaseUrl));

    builder.Services.AddHttpClient<IFixtureApiClient, FixtureApiClient>(client =>
        client.BaseAddress = new Uri(apiBaseUrl));

    // Admin API client — handlers chain:
    // 1. JwtAuthHandler — thêm Bearer token vào request
    // 2. ApiUnauthorizedHandler — catch 401 → redirect tới login
    builder.Services.AddTransient<JwtAuthHandler>();
    builder.Services.AddTransient<ApiUnauthorizedHandler>();
    builder.Services.AddHttpClient<IAdminApiClient, AdminApiClient>(client =>
        client.BaseAddress = new Uri(apiBaseUrl))
        .AddHttpMessageHandler<JwtAuthHandler>()
        .AddHttpMessageHandler<ApiUnauthorizedHandler>();

    // Cookie Auth — bảo vệ admin pages trên Blazor Web
    builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/admin/login";
            options.LogoutPath = "/admin/logout";
            options.ExpireTimeSpan = TimeSpan.FromDays(7);
            options.SlidingExpiration = true;
        });

    builder.Services.AddAuthorization();
    builder.Services.AddCascadingAuthenticationState();

    // MudBlazor — chỉ dùng cho Admin pages (InteractiveServer)
    builder.Services.AddMudServices();

    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    var app = builder.Build();

    app.UseSerilogRequestLogging();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseAuthentication();
    app.UseAuthorization();
    app.UseAntiforgery();

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    Log.Information("FootballBlog Web starting up");
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
