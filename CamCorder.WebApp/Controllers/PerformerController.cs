using CamCorder.Business.Services;
using CamCorder.Business.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;


namespace CamCorder.WebApp.Controllers
{
    public class PerformerController : Controller
    {
        private readonly IPerformerService _performerService;

        public PerformerController(IPerformerService performerService)
        {
            _performerService = performerService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Details(int id)
        {
            var performer = await _performerService.GetPerformerByIdAsync(id);
            if (performer == null) return NotFound();
            return View(performer);
        }

        public IActionResult Create()
        {
            return View(new PerformerDTO { Name = string.Empty, Url = string.Empty });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PerformerDTO model)
        {
            if (!ModelState.IsValid) return View(model);
            await _performerService.CreatePerformerAsync(model);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var performer = await _performerService.GetPerformerByIdAsync(id);
            if (performer == null) return NotFound();
            return View(performer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PerformerDTO model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);
            var ok = await _performerService.UpdatePerformerAsync(model);
            if (!ok) return NotFound();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var performer = await _performerService.GetPerformerByIdAsync(id);
            if (performer == null) return NotFound();
            return View(performer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ok = await _performerService.DeletePerformerAsync(id);
            if (!ok) return NotFound();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var performers = await _performerService.GetPerformersAsync();
            return Json(performers);
        }
    }
}
