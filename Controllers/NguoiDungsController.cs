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
        public async Task<IActionResult> Denbu()
        {
            return View();
        }
        public async Task<IActionResult> Banner()
        {
            return View();
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

            // Get current user from UserInfo middleware
            var currentUserId = HttpContext.Items["id"];
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Users");
            }

            // Get current user to check role
            var currentUser = await _context.NguoiDungs.FindAsync((int)currentUserId);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Users");
            }

            // Check if user has admin role
            if (currentUser.VaiTro != "2")
            {
                return Forbid();
            }

            // Get all users
            var users = await _context.NguoiDungs.ToListAsync();
            return View(users);
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

        // API: Get all customers
        [HttpGet("api/NguoiDung/get-all-customer")]
        public async Task<IActionResult> GetAllCustomers()
        {
            try
            {
                var customers = await _context.NguoiDungs
                    .Where(n => n.VaiTro == "0") // Only customers (role = "0")
                    .Select(n => new
                    {
                        MaKhachThue = n.MaNguoiDung,
                        HoTen = n.HoTen,
                        SoDienThoai = n.SoDienThoai,
                        Email = n.Email
                    })
                    .ToListAsync();

                return Ok(ApiResponse<object>.CreateSuccess(
                    "Lấy danh sách khách hàng thành công",
                    customers
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi lấy danh sách khách hàng"
                ));
            }
        }

        // API: Get available tenants (customers without active contracts)
        [HttpGet("api/NguoiDung/get-available-tenants")]
        public async Task<IActionResult> GetAvailableTenants()
        {
            try
            {
                // Get IDs of customers who have active contracts
                var tenantsWithActiveContracts = await _context.HopDongNguoiThues
                    .Include(hn => hn.MaHopDongNavigation)
                    .Where(hn => hn.MaHopDongNavigation.DaKetThuc == false)
                    .Select(hn => hn.MaKhachThue)
                    .Distinct()
                    .ToListAsync();

                // Get customers who don't have active contracts
                var availableTenants = await _context.NguoiDungs
                    .Where(n => n.VaiTro == "0" && !tenantsWithActiveContracts.Contains(n.MaNguoiDung))
                    .Select(n => new
                    {
                        MaKhachThue = n.MaNguoiDung,
                        HoTen = n.HoTen,
                        SoDienThoai = n.SoDienThoai,
                        Email = n.Email
                    })
                    .ToListAsync();

                return Ok(ApiResponse<object>.CreateSuccess(
                    "Lấy danh sách khách thuê khả dụng thành công",
                    availableTenants
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi lấy danh sách khách thuê khả dụng"
                ));
            }
        }

        // API: Get user by id
        [HttpGet("GetUser/{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.NguoiDungs.FindAsync(id);
            if (user == null)
            {
                return NotFound(ApiResponse<object>.CreateError("Không tìm thấy người dùng"));
            }

            return Ok(new
            {
                maNguoiDung = user.MaNguoiDung,
                hoTen = user.HoTen,
                email = user.Email,
                soDienThoai = user.SoDienThoai,
                vaiTro = user.VaiTro
            });
        }

        // API: Create new user
        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] NguoiDung user)
        {
            try
            {
                // Check if email exists
                var existingEmail = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == user.Email);
                if (existingEmail != null)
                {
                    return BadRequest(ApiResponse<object>.CreateError("Email đã tồn tại"));
                }

                // Check if phone exists
                var existingPhone = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.SoDienThoai == user.SoDienThoai);
                if (existingPhone != null)
                {
                    return BadRequest(ApiResponse<object>.CreateError("Số điện thoại đã tồn tại"));
                }

                _context.NguoiDungs.Add(user);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.CreateSuccess("Thêm người dùng thành công", null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.CreateError("Lỗi khi thêm người dùng: " + ex.Message));
            }
        }

        // API: Update user
        [HttpPost("Update")]
        public async Task<IActionResult> Update([FromBody] NguoiDung user)
        {
            try
            {
                var existingUser = await _context.NguoiDungs.FindAsync(user.MaNguoiDung);
                if (existingUser == null)
                {
                    return NotFound(ApiResponse<object>.CreateError("Không tìm thấy người dùng"));
                }

                // Check if email exists (except current user)
                var existingEmail = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == user.Email && u.MaNguoiDung != user.MaNguoiDung);
                if (existingEmail != null)
                {
                    return BadRequest(ApiResponse<object>.CreateError("Email đã tồn tại"));
                }

                // Check if phone exists (except current user)
                var existingPhone = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.SoDienThoai == user.SoDienThoai && u.MaNguoiDung != user.MaNguoiDung);
                if (existingPhone != null)
                {
                    return BadRequest(ApiResponse<object>.CreateError("Số điện thoại đã tồn tại"));
                }

                // Update user info
                existingUser.HoTen = user.HoTen;
                existingUser.Email = user.Email;
                existingUser.SoDienThoai = user.SoDienThoai;
                existingUser.VaiTro = user.VaiTro;

                // Only update password if provided
                if (!string.IsNullOrEmpty(user.MatKhau))
                {
                    existingUser.MatKhau = user.MatKhau;
                }

                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.CreateSuccess("Cập nhật người dùng thành công", null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.CreateError("Lỗi khi cập nhật người dùng: " + ex.Message));
            }
        }

        // API: Delete user
        [HttpPost("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var user = await _context.NguoiDungs.FindAsync(id);
                if (user == null)
                {
                    return NotFound(ApiResponse<object>.CreateError("Không tìm thấy người dùng"));
                }

                _context.NguoiDungs.Remove(user);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.CreateSuccess("Xóa người dùng thành công", null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.CreateError("Lỗi khi xóa người dùng: " + ex.Message));
            }
        }

        private bool NguoiDungExists(int id)
        {
            return _context.NguoiDungs.Any(e => e.MaNguoiDung == id);
        }

        // DTO for API responses
        public class ApiResponse<T>
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public T Data { get; set; }

            public static ApiResponse<T> CreateSuccess(string message, T data)
            {
                return new ApiResponse<T>
                {
                    Success = true,
                    Message = message,
                    Data = data
                };
            }

            public static ApiResponse<T> CreateError(string message)
            {
                return new ApiResponse<T>
                {
                    Success = false,
                    Message = message,
                    Data = default(T)
                };
            }
        }
    }
}
