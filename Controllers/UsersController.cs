using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ql_NhaTro_jun.Models;

namespace Ql_NhaTro_jun.Controllers
{
    public class UsersController : Controller
    {
        private readonly QlNhatroContext _context;

        public UsersController(QlNhatroContext context)
        {
            _context = context;
        }

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
        public IActionResult Verycode()
        {
            return View();
        }
        public IActionResult ForgotPassword()
        {
            return View();
        }
        public async Task<IActionResult> Contract()
        {
           
            return View();
        }
        public IActionResult HoaDonTong()
        {
            // Truyền id người dùng hiện tại vào ViewBag
            ViewBag.UserId = HttpContext.Items["id"];
            return View();
        }
        public async Task<IActionResult> Dashborad()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login", "Users");
            }

            // Kiểm tra vai trò - chỉ cho phép khách thuê (vai trò "0")
            var userName = User.Identity.Name;
            if (userName == null)
            {
                return RedirectToAction("Login", "Users");
            }

            // Lấy thông tin user từ database để kiểm tra vai trò
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.SoDienThoai == userName);
            if (user == null)
            {
                user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == userName);
            }

            if (user == null)
            {
                return RedirectToAction("Login", "Users");
            }

            // Kiểm tra vai trò - chỉ cho phép khách thuê
            if (user.VaiTro != "0")
            {
                TempData["ErrorMessage"] = "Chỉ khách thuê mới được truy cập trang này!";
                return RedirectToAction("Index", "Home");
            }

            return View();
        }
        public IActionResult Denbu()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login", "Users");
            }
            return View();
        }
        
        public IActionResult Chatbot()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login", "Users");
            }
            return View();
        }
    }
}
