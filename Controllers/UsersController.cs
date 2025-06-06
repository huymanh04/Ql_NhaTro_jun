using Microsoft.AspNetCore.Mvc;

namespace Ql_NhaTro_jun.Controllers
{
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Login()
        {
            if (User.Identity.IsAuthenticated)
            {

                return RedirectToAction("index", "Home");
            }
            return View();
        }
        public async Task<IActionResult> Register()
        {
            if (User.Identity.IsAuthenticated)
            {

                return RedirectToAction("index", "Home");
            }
            return View();
        }

    }
}
