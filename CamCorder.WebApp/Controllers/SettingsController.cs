using System.Text.Json;
using System.Text.Json.Nodes;
using CamCorder.WebApp.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CamCorder.WebApp.Controllers
{
    public class SettingsController(IWebHostEnvironment environment, IOptionsMonitor<CamCorderOptions> options) : Controller
    {
        private readonly IWebHostEnvironment _environment = environment;
        private readonly IOptionsMonitor<CamCorderOptions> _options = options;

        [HttpGet]
        public IActionResult Index()
        {
            return View(_options.CurrentValue);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CamCorderOptions model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            model.ChaturbateSettings ??= new ChaturbateSettings();

            var appSettingsPath = Path.Combine(_environment.ContentRootPath, "appsettings.json");
            JsonObject root;

            if (System.IO.File.Exists(appSettingsPath))
            {
                var json = await System.IO.File.ReadAllTextAsync(appSettingsPath);
                root = JsonNode.Parse(json)?.AsObject() ?? [];
            }
            else
            {
                root = [];
            }

            root[CamCorderOptions.SectionName] = new JsonObject
            {
                [nameof(CamCorderOptions.RecordingsPath)] = model.RecordingsPath,
                [nameof(CamCorderOptions.PollingIntervalSeconds)] = model.PollingIntervalSeconds,
                [nameof(CamCorderOptions.DownloadEnabled)] = model.DownloadEnabled,
                [nameof(CamCorderOptions.MaxConcurrentRecordings)] = model.MaxConcurrentRecordings,
                [nameof(CamCorderOptions.ChaturbateSettings)] = new JsonObject
                {
                    [nameof(ChaturbateSettings.MaxConcurrentRequests)] = model.ChaturbateSettings.MaxConcurrentRequests
                }
            };

            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            await System.IO.File.WriteAllTextAsync(appSettingsPath, root.ToJsonString(jsonOptions));

            TempData["StatusMessage"] = "Settings saved.";

            return RedirectToAction(nameof(Index));
        }
    }
}
