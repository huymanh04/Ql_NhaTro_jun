using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ql_NhaTro_jun.Models;

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
        } public async Task<IActionResult> Contract()
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
            if (!User.Identity.IsAuthenticated)
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
            using (var context = new Models.QlNhatroContext())
            {
                var user = await context.NguoiDungs.FirstOrDefaultAsync(u => u.SoDienThoai == userName);
                if (user == null)
                {
                    user = await context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == userName);
                }

                if (user == null)
                {
                    return RedirectToAction("Login", "Users");
                }

                // Kiểm tra vai trò - chỉ cho phép khách thuê
                if (user.VaiTro != "0")
                {
                    // Redirect về trang chủ hoặc hiển thị thông báo lỗi
                    TempData["ErrorMessage"] = "Chỉ khách thuê mới được truy cập trang này!";
                    return RedirectToAction("Index", "Home");
                }
            }

            return View();
        }
        public IActionResult Denbu()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Users");
            }
            return View();
        }
    }
}
