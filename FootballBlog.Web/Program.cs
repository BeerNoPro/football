using FootballBlog.Web.Components;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((context, services, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {SourceContext} | {Message:lj}{NewLine}{Exception}")
            .WriteTo.File("logs/app/app-.log", rollingInterval: RollingInterval.Day,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {SourceContext} | {Message:lj}{NewLine}{Exception}")
            .WriteTo.File("logs/error/error-.log", rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {SourceContext} | {Message:lj}{NewLine}{Exception}"));

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
