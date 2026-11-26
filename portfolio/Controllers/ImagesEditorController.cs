using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Npgsql;
using portfolio.Data;
using portfolio.Models;
using portfolio.ViewModels;

namespace portfolio.Controllers
{
    [Authorize]
    public class ImagesEditorController : Controller
    {
        private readonly IProjectImageRepository _images;
        private readonly IProjectRepository _projects;

        public ImagesEditorController(IProjectImageRepository images, IProjectRepository projects)
        {
            _images = images;
            _projects = projects;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _images.GetAllAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new ProjectImageFormViewModel();
            await PopulateProjectsAsync(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectImageFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await PopulateProjectsAsync(vm);
                return View(vm);
            }

            try
            {
                await _images.CreateAsync(new ProjectImage
                {
                    ProjectId = vm.ProjectId,
                    ImageUrl = vm.ImageUrl,
                    Caption = vm.Caption,
                    SortOrder = vm.SortOrder
                });

                TempData["Success"] = "Image added.";
                return RedirectToAction(nameof(Index));
            }
            catch (PostgresException ex)
            {
                ModelState.AddModelError(string.Empty, PostgresExceptionHelper.ToFriendlyMessage(ex));
                await PopulateProjectsAsync(vm);
                return View(vm);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var img = await _images.GetByIdAsync(id);
            if (img == null) return NotFound();

            var vm = new ProjectImageFormViewModel
            {
                ImageId = img.ImageId,
                ProjectId = img.ProjectId,
                ImageUrl = img.ImageUrl,
                Caption = img.Caption,
                SortOrder = img.SortOrder
            };
            await PopulateProjectsAsync(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProjectImageFormViewModel vm)
        {
            if (id != vm.ImageId) return NotFound();
            if (!ModelState.IsValid)
            {
                await PopulateProjectsAsync(vm);
                return View(vm);
            }

            try
            {
                var updated = await _images.UpdateAsync(new ProjectImage
                {
                    ImageId = vm.ImageId,
                    ProjectId = vm.ProjectId,
                    ImageUrl = vm.ImageUrl,
                    Caption = vm.Caption,
                    SortOrder = vm.SortOrder
                });

                if (!updated) return NotFound();

                TempData["Success"] = "Image updated.";
                return RedirectToAction(nameof(Index));
            }
            catch (PostgresException ex)
            {
                ModelState.AddModelError(string.Empty, PostgresExceptionHelper.ToFriendlyMessage(ex));
                await PopulateProjectsAsync(vm);
                return View(vm);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var img = await _images.GetByIdAsync(id);
            if (img == null) return NotFound();
            return View(img);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var img = await _images.GetByIdAsync(id);
            if (img == null) return NotFound();

            try
            {
                await _images.DeleteAsync(id);
                TempData["Success"] = "Image deleted.";
                return RedirectToAction(nameof(Index));
            }
            catch (PostgresException ex)
            {
                ModelState.AddModelError(string.Empty, PostgresExceptionHelper.ToFriendlyMessage(ex));
                return View("Delete", img);
            }
        }

        private async Task PopulateProjectsAsync(ProjectImageFormViewModel vm)
        {
            var projects = await _projects.GetAllAsync();
            vm.Projects = projects.Select(p => new SelectListItem
            {
                Value = p.ProjectId.ToString(),
                Text = $"{p.CourseCode} — {p.ProjectTitle}"
            });
        }
    }
}