using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using portfolio.Data;
using portfolio.Models;
using portfolio.ViewModels;

namespace portfolio.Controllers
{
    [Authorize]
    public class CoursesEditorController : Controller
    {
        private readonly ICourseRepository _courses;

        public CoursesEditorController(ICourseRepository courses)
        {
            _courses = courses;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _courses.GetAllAsync());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CourseFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseFormViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            try
            {
                await _courses.CreateAsync(new Course
                {
                    CourseCode = vm.CourseCode,
                    CourseTitle = vm.CourseTitle,
                    TermTaken = vm.TermTaken,
                    Units = vm.Units,
                    Description = vm.Description
                });

                TempData["Success"] = "Course added successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (PostgresException ex)
            {
                ModelState.AddModelError(string.Empty, PostgresExceptionHelper.ToFriendlyMessage(ex));
                return View(vm);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var course = await _courses.GetByIdAsync(id);
            if (course == null) return NotFound();

            return View(new CourseFormViewModel
            {
                CourseId = course.CourseId,
                CourseCode = course.CourseCode,
                CourseTitle = course.CourseTitle,
                TermTaken = course.TermTaken,
                Units = course.Units,
                Description = course.Description
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CourseFormViewModel vm)
        {
            if (id != vm.CourseId) return NotFound();
            if (!ModelState.IsValid) return View(vm);

            try
            {
                var updated = await _courses.UpdateAsync(new Course
                {
                    CourseId = vm.CourseId,
                    CourseCode = vm.CourseCode,
                    CourseTitle = vm.CourseTitle,
                    TermTaken = vm.TermTaken,
                    Units = vm.Units,
                    Description = vm.Description
                });

                if (!updated) return NotFound();

                TempData["Success"] = "Course updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (PostgresException ex)
            {
                ModelState.AddModelError(string.Empty, PostgresExceptionHelper.ToFriendlyMessage(ex));
                return View(vm);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var course = await _courses.GetByIdAsync(id);
            if (course == null) return NotFound();
            return View(course);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _courses.GetByIdAsync(id);
            if (course == null) return NotFound();

            try
            {
                await _courses.DeleteAsync(id);
                TempData["Success"] = "Course deleted.";
                return RedirectToAction(nameof(Index));
            }
            catch (PostgresException ex)
            {
                // Catches 23503 when course still has projects (ON DELETE RESTRICT)
                ModelState.AddModelError(string.Empty, PostgresExceptionHelper.ToFriendlyMessage(ex));
                return View("Delete", course);
            }
        }
    }
}