using Microsoft.AspNetCore.Mvc;

namespace Ql_NhaTro_jun.Controllers
{
    public class BankingController : Controller
    {
        public IActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Users");
            }
            return View();
        }
    }
} 