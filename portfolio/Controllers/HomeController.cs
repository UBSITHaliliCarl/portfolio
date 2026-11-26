using Microsoft.AspNetCore.Mvc;

namespace portfolio.Controllers
{
    public class HomeController : Controller
    {
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }

        public IActionResult StatusCode(int code)
        {
            if (code == 404)
            {
                return View("NotFound");
            }
            return View("Error");
        }
    }
}