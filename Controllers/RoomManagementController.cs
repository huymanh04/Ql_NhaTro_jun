using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ql_NhaTro_jun.Models;

namespace Ql_NhaTro_jun.Controllers
{
    [Authorize]
    public class RoomManagementController : Controller
    {
        private readonly QlNhatroContext _context;

        public RoomManagementController(QlNhatroContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Kiểm tra quyền truy cập
            var userName = User.Identity.Name;
            if (userName == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.SoDienThoai == userName);
            if (user == null)
            {
                user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == userName);
            }

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Chỉ cho phép admin và manager truy cập
            if (user.VaiTro == "0") // User thường
            {
                return RedirectToAction("AccessDenied", "Account");
            }

            return View();
        }
    }
} 