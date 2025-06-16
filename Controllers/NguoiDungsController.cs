using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Ql_NhaTro_jun.Models;

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
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Users");
            }

            // Get current user from UserInfo middleware
            var currentUserId = HttpContext.Items["id"];
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            var nguoiDung = await _context.NguoiDungs.FindAsync((int)currentUserId);
            if (nguoiDung == null)
            {
                return NotFound();
            }

            return View(nguoiDung);
        }

        // POST: NguoiDungs/UpdateProfile - Update personal profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile([Bind("MaNguoiDung,HoTen,MatKhau")] NguoiDung nguoiDung)
        {
            if (!User.Identity.IsAuthenticated)
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
                if (!NguoiDungExists(nguoiDung.MaNguoiDung))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // API: Update personal profile
       
     
        public async Task<IActionResult> Dashborad()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Users");
            }
            return View();
        }

        // GET: Account Management
        public async Task<IActionResult> QuanlyUser()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Users");
            }
            return View();
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

      

        private bool NguoiDungExists(int id)
        {
            return _context.NguoiDungs.Any(e => e.MaNguoiDung == id);
        }
    }
}
