using Microsoft.AspNetCore.Mvc;
using portfolio.Data;

namespace portfolio.Controllers
{
    public class PortfolioController : Controller
    {
        private readonly ICourseRepository _courses;
        private readonly IProjectRepository _projects;
        private readonly IProjectImageRepository _images;

        public PortfolioController(
            ICourseRepository courses,
            IProjectRepository projects,
            IProjectImageRepository images)
        {
            _courses = courses;
            _projects = projects;
            _images = images;
        }

        // GET: /Portfolio
        public async Task<IActionResult> Index(string? search)
        {
            var allCourses = await _courses.GetAllAsync();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                allCourses = allCourses
                    .Where(c => c.CourseCode.Contains(term, StringComparison.OrdinalIgnoreCase)
                             || c.CourseTitle.Contains(term, StringComparison.OrdinalIgnoreCase)
                             || c.TermTaken.Contains(term, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            ViewBag.Search = search;
            return View(allCourses);
        }

        // GET: /Portfolio/Course/5
        public async Task<IActionResult> Course(int id)
        {
            var course = await _courses.GetByIdAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            ViewBag.Course = course;
            var projects = await _projects.GetByCourseIdAsync(id);
            return View(projects);
        }

        // GET: /Portfolio/Project/5
        public async Task<IActionResult> Project(int id)
        {
            var project = await _projects.GetByIdAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            project.Images = await _images.GetByProjectIdAsync(id);
            return View(project);
        }
    }
}