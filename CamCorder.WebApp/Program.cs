using CamCorder.Data;
using CamCorder.Business.Services;
using CamCorder.WebApp.Options;
using CamCorder.WebApp;
using CamCorder.WebApp.Hubs;
using Hangfire;
using Hangfire.Storage.SQLite;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;

var logger = LogManager.Setup()
    .LoadConfigurationFromAppSettings()
    .GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add NLog to ASP.NET Core
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // Add services to the container.
    builder.Services.AddControllersWithViews();
    // SignalR for real-time performer updates
    builder.Services.AddSignalR();
    builder.Services.AddOptions<CamCorderOptions>()
        .Bind(builder.Configuration.GetSection(CamCorderOptions.SectionName))
        .ValidateDataAnnotations();

    var sqliteDbPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "hangfire.db");
    var sqliteDbDirectory = Path.GetDirectoryName(sqliteDbPath);

    if (!string.IsNullOrWhiteSpace(sqliteDbDirectory))
    {
        Directory.CreateDirectory(sqliteDbDirectory);
    }

    if (!File.Exists(sqliteDbPath))
    {
        using var _ = File.Create(sqliteDbPath);
    }

    var appDataDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
    Directory.CreateDirectory(appDataDirectory);

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? $"Data Source={Path.Combine(appDataDirectory, "camcorder.db")}";

    builder.Services.AddDbContext<CamCorderContext>(options =>
        options.UseSqlite(connectionString));

    // Application services
    builder.Services.AddScoped<IPerformerService, PerformerService>();
    // Notifier that uses SignalR to broadcast performer updates
    builder.Services.AddSingleton<IPerformerNotifier, CamCorder.WebApp.Services.PerformerNotifier>();

    builder.Services.AddHangfire(configuration =>
    {
        configuration
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSQLiteStorage(sqliteDbPath, new SQLiteStorageOptions());
    });

    builder.Services.AddHangfireServer(options =>
    {
        options.SchedulePollingInterval = TimeSpan.FromSeconds(1);
    });

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<CamCorderContext>();
        dbContext.Database.Migrate();
    }

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseRouting();

    app.UseAuthorization();

    app.MapStaticAssets();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
        .WithStaticAssets();
    // SignalR hub endpoint for performer updates
    app.MapHub<PerformerHub>("/hubs/performer");
    app.UseHangfireDashboard("/hangfire", new DashboardOptions
    {
        Authorization = [new DashboardNoAuthorizationFilter()]
    });
    app.Run();
}
catch (Exception exception)
{
    logger.Error(exception, "Application stopped because of an exception.");
    throw;
}
finally
{
    LogManager.Shutdown();
}
