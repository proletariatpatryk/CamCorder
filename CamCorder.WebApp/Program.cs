using CamCorder.WebApp;
using Hangfire;
using Hangfire.Storage.SQLite;
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
