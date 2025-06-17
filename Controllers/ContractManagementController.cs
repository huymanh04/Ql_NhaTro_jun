using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Ql_NhaTro_jun.Controllers
{
    [Authorize]
    public class ContractManagementController : Controller
    {
        public IActionResult Index()
        {
            // Kiểm tra quyền truy cập
            var userRole = HttpContext.Items["role"]?.ToString();
            
            if (userRole == "0") // Khách hàng không được phép truy cập
            {
                return RedirectToAction("Index", "Home");
            }
            
            return View();
        }
    }
} 