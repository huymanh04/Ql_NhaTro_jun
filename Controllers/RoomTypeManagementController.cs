using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Ql_NhaTro_jun.Controllers
{
    [Authorize]
    public class RoomTypeManagementController : Controller
    {
        public IActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Users");
            }
            
            // Check if user has admin or manager role
            var role = HttpContext.Items["role"]?.ToString();
            if (role == "0") // Customer role
            {
                return RedirectToAction("AccessDenied", "Users");
            }
            
            return View();
        }
    }
} 