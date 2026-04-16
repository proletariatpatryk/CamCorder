using CamCorder.Data;
using CamCorder.WebApp;
using Hangfire;
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

    builder.Services.AddCamCorderDependencies(builder.Configuration, builder.Environment);

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
