using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using portfolio.ViewModels;

namespace portfolio.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _config;

        public AccountController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var expectedUser = _config["EditorAuth:Username"] ?? "admin";
            var expectedPass = _config["EditorAuth:Password"];

            if (string.IsNullOrWhiteSpace(expectedPass) ||
                !string.Equals(model.Username, expectedUser, StringComparison.Ordinal) ||
                !string.Equals(model.Password, expectedPass, StringComparison.Ordinal))
            {
                ModelState.AddModelError(string.Empty, "Invalid editor credentials.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, expectedUser),
                new(ClaimTypes.Role, "Editor")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "CoursesEditor");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Portfolio");
        }
    }
}