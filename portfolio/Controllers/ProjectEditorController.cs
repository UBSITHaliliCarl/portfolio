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
    public class ProjectsEditorController : Controller
    {
        private readonly IProjectRepository _projects;
        private readonly ICourseRepository _courses;
        private readonly IProjectImageRepository _images;

        public ProjectsEditorController(
            IProjectRepository projects,
            ICourseRepository courses,
            IProjectImageRepository images)
        {
            _projects = projects;
            _courses = courses;
            _images = images;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _projects.GetAllAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new ProjectFormViewModel();
            await PopulateCoursesAsync(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectFormViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCoursesAsync(vm);
                return View(vm);
            }

            try
            {
                var project = new Project
                {
                    CourseId = vm.CourseId,
                    ProjectTitle = vm.ProjectTitle,
                    Summary = vm.Summary,
                    RepoUrl = vm.RepoUrl,
                    VideoUrl = vm.VideoUrl
                };

                await _projects.CreateWithImagesAsync(project, vm.ParseImageUrls());
                TempData["Success"] = "Project and images saved in one transaction.";
                return RedirectToAction(nameof(Index));
            }
            catch (PostgresException ex)
            {
                // If an image URL repeats, Postgres throws 23505 (uq_project_image_url),
                // transaction rolls back, and nothing is saved.
                ModelState.AddModelError(string.Empty, PostgresExceptionHelper.ToFriendlyMessage(ex));
                await PopulateCoursesAsync(vm);
                return View(vm);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var project = await _projects.GetByIdAsync(id);
            if (project == null) return NotFound();

            var imgs = await _images.GetByProjectIdAsync(id);
            var vm = new ProjectFormViewModel
            {
                ProjectId = project.ProjectId,
                CourseId = project.CourseId,
                ProjectTitle = project.ProjectTitle,
                Summary = project.Summary,
                RepoUrl = project.RepoUrl,
                VideoUrl = project.VideoUrl,
                ImageUrlsText = string.Join(Environment.NewLine, imgs.Select(i => i.ImageUrl))
            };

            await PopulateCoursesAsync(vm);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProjectFormViewModel vm)
        {
            if (id != vm.ProjectId) return NotFound();
            if (!ModelState.IsValid)
            {
                await PopulateCoursesAsync(vm);
                return View(vm);
            }

            try
            {
                var updated = await _projects.UpdateWithImagesAsync(new Project
                {
                    ProjectId = vm.ProjectId,
                    CourseId = vm.CourseId,
                    ProjectTitle = vm.ProjectTitle,
                    Summary = vm.Summary,
                    RepoUrl = vm.RepoUrl,
                    VideoUrl = vm.VideoUrl
                }, vm.ParseImageUrls());

                if (!updated) return NotFound();

                TempData["Success"] = "Project and images updated in one transaction.";
                return RedirectToAction(nameof(Index));
            }
            catch (PostgresException ex)
            {
                ModelState.AddModelError(string.Empty, PostgresExceptionHelper.ToFriendlyMessage(ex));
                await PopulateCoursesAsync(vm);
                return View(vm);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var project = await _projects.GetByIdAsync(id);
            if (project == null) return NotFound();
            return View(project);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var project = await _projects.GetByIdAsync(id);
            if (project == null) return NotFound();

            try
            {
                await _projects.DeleteAsync(id);
                TempData["Success"] = "Project deleted.";
                return RedirectToAction(nameof(Index));
            }
            catch (PostgresException ex)
            {
                ModelState.AddModelError(string.Empty, PostgresExceptionHelper.ToFriendlyMessage(ex));
                return View("Delete", project);
            }
        }

        private async Task PopulateCoursesAsync(ProjectFormViewModel vm)
        {
            var courses = await _courses.GetAllAsync();
            vm.Courses = courses.Select(c => new SelectListItem
            {
                Value = c.CourseId.ToString(),
                Text = $"{c.CourseCode} — {c.CourseTitle}"
            });
        }
    }
}