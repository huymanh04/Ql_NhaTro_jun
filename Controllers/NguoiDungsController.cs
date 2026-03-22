using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ql_NhaTro_jun.Models;
using static Ql_NhaTro_jun.Controllers.AdminController;

namespace Ql_NhaTro_jun.Controllers
{
    public class NguoiDungsController : Controller
    {
        private readonly QlNhatroContext _context;

        public NguoiDungsController(QlNhatroContext context)
        {
            _context = context;
        }


        // GET: NguoiDungs - Personal Profile
        public async Task<IActionResult> Index()
        {
            if (!User!.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Login", "Users");
            }

            var parsedUserId = GetCurrentUserId();
            if (!parsedUserId.HasValue)
            {
                return RedirectToAction("Login", "Users");
            }

            var userSummary = await _context.NguoiDungs
                .AsNoTracking()
                .Where(u => u.MaNguoiDung == parsedUserId.Value)
                .Select(u => new { u.MaNguoiDung, u.VaiTro })
                .FirstOrDefaultAsync();

            if (userSummary == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var nguoiDung = await _context.NguoiDungs
                .AsNoTracking()
                .Where(u => u.MaNguoiDung == parsedUserId.Value)
                .Select(u => new NguoiDung
                {
                    MaNguoiDung = u.MaNguoiDung,
                    HoTen = u.HoTen,
                    SoDienThoai = u.SoDienThoai,
                    Email = u.Email,
                    MatKhau = null,
                    VaiTro = u.VaiTro,
                    so_cccd = u.so_cccd ?? string.Empty
                })
                .FirstOrDefaultAsync();

            if (nguoiDung == null)
            {
                return NotFound();
            }

            return View(nguoiDung);
        }
        public async Task<IActionResult> Denbu()
        {
            return View();
        }
        public async Task<IActionResult> Banner()
        {
            return View("~/Views/Admin/Banner.cshtml");
        }
        // POST: NguoiDungs/UpdateProfile - Update personal profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile([Bind("MaNguoiDung,HoTen,MatKhau")] NguoiDung nguoiDung)
        {
            if (!User!.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Login", "Users");
            }

            // Get current user from UserInfo middleware
            var currentUserId = HttpContext.Items["id"];
            if (currentUserId == null || (int)currentUserId != nguoiDung.MaNguoiDung)
            {
                return Forbid(); // User can only edit their own profile
            }

            // Get the existing user from database
            var existingUser = await _context.NguoiDungs.FindAsync(nguoiDung.MaNguoiDung);
            if (existingUser == null)
            {
                return NotFound();
            }

            // Only update allowed fields (HoTen and MatKhau)
            if (!string.IsNullOrEmpty(nguoiDung.HoTen))
            {
                existingUser.HoTen = nguoiDung.HoTen;
            }

            if (!string.IsNullOrEmpty(nguoiDung.MatKhau))
            {
                existingUser.MatKhau = nguoiDung.MatKhau;
            }

            try
            {
                _context.Update(existingUser);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật thông tin cá nhân thành công!";
            }
            catch (DbUpdateConcurrencyException)
            {

                throw;

            }

            return RedirectToAction(nameof(Index));
        }

        // API: Update personal profile


        public async Task<IActionResult> Dashborad()
        {
            if (!User!.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Login", "Users");
            }

            var parsedUserId = GetCurrentUserId();
            if (!parsedUserId.HasValue)
            {
                return RedirectToAction("Login", "Users");
            }

            var user = await _context.NguoiDungs
                .AsNoTracking()
                .Where(u => u.MaNguoiDung == parsedUserId.Value)
                .Select(u => new { u.MaNguoiDung, u.VaiTro })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return RedirectToAction("Login", "Users");
            }

            if (user.VaiTro == "0") // Kiểm tra quyền người dùng
            {
                return View("~/Views/Users/Dashborad.cshtml");
            }

            return View();
        }

        // GET: Account Management
        public async Task<IActionResult> QuanlyUser()
        {
            if (!User!.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Login", "Users");
            }

            var currentUserId = GetCurrentUserId();
            if (!currentUserId.HasValue)
            {
                return RedirectToAction("Login", "Users");
            }

            // Get current user to check role (project only needed fields).
            var currentUser = await _context.NguoiDungs
                .AsNoTracking()
                .Where(u => u.MaNguoiDung == currentUserId.Value)
                .Select(u => new { u.MaNguoiDung, u.VaiTro })
                .FirstOrDefaultAsync();

            if (currentUser == null)
            {
                return RedirectToAction("Login", "Users");
            }

            // Check if user has admin role
            if (currentUser.VaiTro == "0")
            {
                return Forbid();
            }

            // Get all users
            var users = await _context.NguoiDungs
                .AsNoTracking()
                .Select(u => new NguoiDung
                {
                    MaNguoiDung = u.MaNguoiDung,
                    HoTen = u.HoTen,
                    SoDienThoai = u.SoDienThoai,
                    Email = u.Email,
                    VaiTro = u.VaiTro,
                    so_cccd = u.so_cccd ?? string.Empty
                })
                .ToListAsync();
            return View(users);
        }

        private int? GetCurrentUserId()
        {
            if (HttpContext.Items["id"] is int idFromItems)
            {
                return idFromItems;
            }

            var claimValue = User.FindFirst("MaNguoiDung")?.Value;
            return int.TryParse(claimValue, out var idFromClaim) ? idFromClaim : null;
        }

        // GET: NguoiDungs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var nguoiDung = await _context.NguoiDungs
                .FirstOrDefaultAsync(m => m.MaNguoiDung == id);
            if (nguoiDung == null)
            {
                return NotFound();
            }

            return View(nguoiDung);
        }

       
    }
}
