using Ql_NhaTro_jun.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Ql_NhaTro_jun.Controllers.AdminController;
using MailKit.Net.Smtp;
using MimeKit;
using Ql_NhaTro_jun.Controllers;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;
using Microsoft.AspNetCore.Http;
using Org.BouncyCastle.Ocsp;
using Microsoft.AspNetCore.Authentication.Google;

namespace Api_Ql_nhatro.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {

        private readonly ILogger<AuthController> _logger;
        QlNhatroContext _context;
        private readonly IEmailService _emailService;
        public AuthController(ILogger<AuthController> logger, QlNhatroContext cc, IEmailService emailService)
        {
            _logger = logger; _context = cc;
            _emailService = emailService;
        }
        #region login and Register
        // POST: api/auth/get-aes-key
        [HttpPost("juntech")]
        public IActionResult GetAesKey([FromBody] EmailRequest request)
        {
            var email = request.Email;
            var manh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)); // 256-bit key
            var juntech = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));  // 128-bit IV

            HttpContext.Session.SetString($"AES_{email}_Key", manh);
            HttpContext.Session.SetString($"AES_{email}_IV", juntech);

            return Ok(new { manh, juntech });
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            try
            {
                var key = HttpContext.Session.GetString($"AES_{model.SoDienThoai}_Key");
                var iv = HttpContext.Session.GetString($"AES_{model.SoDienThoai}_IV");

                if (key == null || iv == null)
                    return BadRequest(new { message = "Phiên đã hết hạn hoặc không tồn tại AES Key" });
                var decryptedPassword = AesHelper.Decrypt(model.Password, key, iv);
                var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.SoDienThoai == model.SoDienThoai);

                if (user == null || user.MatKhau != decryptedPassword)
                {
                    if (model.SoDienThoai.Contains("@"))
                    {
                        user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == model.SoDienThoai&& u.MatKhau== decryptedPassword);
                        if (user == null)
                        {
                            return Unauthorized(new { message = "Email hoặc mật khẩu không đúng" });
                        }
                    }
                    else
                    {
                        return Unauthorized(new { message = "Số điện thoại hoặc mật khẩu không đúng" });
                    }

                }

                HttpContext.Session.Remove($"AES_{model.SoDienThoai}_Key");
                HttpContext.Session.Remove($"AES_{model.SoDienThoai}_IV");
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.SoDienThoai),
                new Claim("MaNguoiDung", user.MaNguoiDung.ToString())
            };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);


                var authProps = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe,
                    ExpiresUtc = model.RememberMe ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddHours(1)
                };
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);

                await _context.SaveChangesAsync();
            }
            catch (Exception eee) { return BadRequest(new { message = "Phiên đã hết hạn hoặc không tồn tại AES Key" }); }
            return Ok(new { message = "Đăng nhập thành công!" });
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] NguoiDung model)
        {
            var key = HttpContext.Session.GetString($"AES_{model.Email}_Key");
            var iv = HttpContext.Session.GetString($"AES_{model.Email}_IV");
            if (key == null || iv == null)
                return BadRequest(new { message = "Phiên đã hết hạn hoặc không tồn tại AES Key" });
            var decryptedPassword = AesHelper.Decrypt(model.MatKhau, key, iv);
            model.MatKhau = decryptedPassword;
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.SoDienThoai == model.SoDienThoai);
            if (user != null)
            {
                return Unauthorized(new { message = "Số điện thoại đã tồn tại" });
            }
            var emaill = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (emaill != null)
            {
                return Unauthorized(new { message = "Email đã tồn tại" });
            }
            var recaptchaResponse = model.RecaptchaResponse;
            string secretKey = "6LcQdhUrAAAAALA0Kf-pPNX8yTyHbpdZpVC3bsuG";
            var client = new WebClient();
            var result = client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", secretKey, recaptchaResponse));
            var response = JsonSerializer.Deserialize<RecaptchaResponse>(result);
            if (!response.Success)
            {
                return BadRequest(new { message = "Xác minh reCAPTCHA thất bại!" });
            }
            model.VaiTro = "0";
            var confirmationCode = new Random().Next(100000, 999999).ToString();
            HttpContext.Session.SetString("code", confirmationCode);
            HttpContext.Session.SetString("Email", model.Email);
            // Lưu model vào session (serialize)
            HttpContext.Session.SetString("RegisterModel", JsonSerializer.Serialize(model));
            var body = $@"<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9;'><div style='background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'><div style='text-align: center; margin-bottom: 30px;'><h2 style='color: #2c3e50; margin: 0; font-size: 24px;'>🏠 Xác thực tài khoản Nhà trọ</h2></div><div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px; border-left: 4px solid #007bff; margin: 20px 0;'><p style='color: #495057; font-size: 16px; line-height: 1.6; margin: 0 0 15px 0;'>Chào mừng bạn đến với hệ thống phòng trọ sô 1 Việt nam!</p><p style='color: #495057; font-size: 16px; line-height: 1.6; margin: 0;'>Để hoàn tất quá trình đăng ký, vui lòng sử dụng mã xác nhận bên dưới:</p></div><div style='text-align: center; margin: 30px 0;'><div style='background: linear-gradient(135deg, #3182ce 0%, #63b3ed 100%); color: white; padding: 20px; border-radius: 10px; display: inline-block; min-width: 200px;'><p style='margin: 0 0 10px 0; font-size: 14px; opacity: 0.9;'>MÃ XÁC NHẬN</p><p style='margin: 0; font-size: 32px; font-weight: bold; letter-spacing: 3px; font-family: monospace;'>{confirmationCode}</p></div></div><div style='background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 8px; margin: 20px 0;'><p style='color: #856404; font-size: 14px; margin: 0; text-align: center;'>⚠️ Mã này có hiệu lực trong 15 phút. Vui lòng không chia sẻ mã với bất kỳ ai.</p></div><div style='text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #dee2e6;'><p style='color: #6c757d; font-size: 14px; margin: 0;'>Nếu bạn không yêu cầu xác thực này, vui lòng bỏ qua email này.</p><p style='color: #6c757d; font-size: 14px; margin: 10px 0 0 0;'>© 2025 Hệ thống quản lý Nhà trọ</p></div></div></div>";
            await _emailService.SendEmailAsync(model.Email, "Mã xác thực tài khoản", body);
            return Ok(new { Success = true, message = "Đăng Ký thành công vui lòng xác thực gmail !" });
        }

        [HttpPost("resend-email-code")]
        public async Task<IActionResult> ResendEmailCode([FromBody] EmailRequest req)
        {
            var email = req.Email;
            var code = new Random().Next(100000, 999999).ToString();
            HttpContext.Session.SetString("code", code);
            HttpContext.Session.SetString("Email", email);
            var body = $@"<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9;'><div style='background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'><div style='text-align: center; margin-bottom: 30px;'><h2 style='color: #2c3e50; margin: 0; font-size: 24px;'>🏠 Xác thực tài khoản Nhà trọ</h2></div><div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px; border-left: 4px solid #007bff; margin: 20px 0;'><p style='color: #495057; font-size: 16px; line-height: 1.6; margin: 0 0 15px 0;'>Chào mừng bạn đến với hệ thống phòng trọ sô 1 Việt nam!</p><p style='color: #495057; font-size: 16px; line-height: 1.6; margin: 0;'>Để hoàn tất quá trình đăng ký, vui lòng sử dụng mã xác nhận bên dưới:</p></div><div style='text-align: center; margin: 30px 0;'><div style='background: linear-gradient(135deg, #3182ce 0%, #63b3ed 100%); color: white; padding: 20px; border-radius: 10px; display: inline-block; min-width: 200px;'><p style='margin: 0 0 10px 0; font-size: 14px; opacity: 0.9;'>MÃ XÁC NHẬN</p><p style='margin: 0; font-size: 32px; font-weight: bold; letter-spacing: 3px; font-family: monospace;'>{code}</p></div></div><div style='background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 8px; margin: 20px 0;'><p style='color: #856404; font-size: 14px; margin: 0; text-align: center;'>⚠️ Mã này có hiệu lực trong 15 phút. Vui lòng không chia sẻ mã với bất kỳ ai.</p></div><div style='text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #dee2e6;'><p style='color: #6c757d; font-size: 14px; margin: 0;'>Nếu bạn không yêu cầu xác thực này, vui lòng bỏ qua email này.</p><p style='color: #6c757d; font-size: 14px; margin: 10px 0 0 0;'>© 2025 Hệ thống quản lý Nhà trọ</p></div></div></div>";
            await _emailService.SendEmailAsync(email, "Mã xác thực tài khoản", body);
            return Ok(new { message = "Mã xác thực mới đã được gửi đến email của bạn!" });
        }

        [HttpPost("verify-email-code")]
        public async Task<IActionResult> VerifyEmailCode([FromBody] VerifyEmailCodeRequest req)
        {
            var email = HttpContext.Session.GetString("Email");
            var code = HttpContext.Session.GetString("code");
            if (string.IsNullOrEmpty(email))
                return NotFound(new { message = "Không tìm thấy tài khoản" });
            if (code != req.Code)
                return BadRequest(new { message = "Mã xác thực không đúng" });
            var modelStr = HttpContext.Session.GetString("RegisterModel");
            if (string.IsNullOrEmpty(modelStr))
                return BadRequest(new { message = "Thông tin đăng ký không hợp lệ" });
            var model = JsonSerializer.Deserialize<NguoiDung>(modelStr);
            _context.Add(model);
            HttpContext.Session.Remove($"AES_{model.Email}_Key");
            HttpContext.Session.Remove($"AES_{model.Email}_IV");
            HttpContext.Session.Remove("code");
            HttpContext.Session.Remove("Email");
            HttpContext.Session.Remove("RegisterModel");
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception EE) { }
            return Ok(new { message = "Xác thực email thành công!" });
        }
        public class VerifyEmailCodeRequest
        {
            public string Email { get; set; }
            public string Code { get; set; }
        }
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "Đăng xuất thành công!" });
        }
        public class RecaptchaResponse
        {
            [JsonPropertyName("success")]
            public bool Success { get; set; }

            [JsonPropertyName("error-codes")]
            public List<string> ErrorCodes { get; set; }
        }
        public class LoginRequest
        {
            [JsonPropertyName("soDienThoai")]
            public string SoDienThoai { get; set; }

            [JsonPropertyName("password")]
            public string Password { get; set; }

            [JsonPropertyName("rememberMe")]
            public bool RememberMe { get; set; }
        }
        public class RegisterRequest
        {

            public string SoDienThoai { get; set; }

            public string HoTen { get; set; }
            public string Email { get; set; }
            public string MatKhau { get; set; }
            public string RecaptchaResponse { get; set; }

        }
        public static class AesHelper
        {
            public static string Decrypt(string cipherTextBase64, string keyBase64, string ivBase64)
            {
                try
                {
                    var cipherText = Convert.FromBase64String(cipherTextBase64);
                    var key = Convert.FromBase64String(keyBase64);
                    var iv = Convert.FromBase64String(ivBase64);

                    if (key.Length != 16 && key.Length != 24 && key.Length != 32)
                        throw new ArgumentException("Invalid key length.");
                    if (iv.Length != 16)
                        throw new ArgumentException("Invalid IV length.");

                    using var aes = Aes.Create();
                    aes.Key = key;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using var decryptor = aes.CreateDecryptor();
                    using var ms = new MemoryStream(cipherText);
                    using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                    using var sr = new StreamReader(cs);
                    return sr.ReadToEnd();
                }
                catch (Exception ex)
                {
                    // Ghi log hoặc throw để biết lỗi
                    throw new Exception("Giải mã thất bại", ex);
                }
            }

        }
        #endregion
        #region get info user
        [HttpGet("get-user-info")]
        public async Task<IActionResult> GetUserInfo()
        {
            var userName = User.Identity.Name;
            if (userName == null)
            {
             
                return Unauthorized(new { message = "Bạn chưa đăng nhập" });
            }

            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.SoDienThoai == userName);
            if (user == null)
            {
                user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == userName);
                if (user == null)
                {
                    return NotFound(new { message = "Người dùng không tồn tại" });
                }
            }
           
            return Ok(ApiResponse<object>.CreateSuccess("Lấy thông tin thành công", new
            {
                HoTen = user.HoTen,
                phone = user.SoDienThoai,
                email = user.Email,
                MaNguoidung = user.MaNguoiDung,
               
            }));


        }
        [HttpGet("info-with-motels/{userId}")]
        public async Task<IActionResult> GetUserWithMotels(int userId)
        {
            var user = await _context.NguoiDungs
                .Where(u => u.MaNguoiDung == userId && u.VaiTro == "0")
                .Select(u => new
                {
                    u.MaNguoiDung,
                    u.HoTen,
                    u.Email,
                    u.SoDienThoai,
                    NhaTros = u.NhaTros.Select(n => new
                    {
                        n.MaNhaTro,
                        n.TenNhaTro,
                        n.DiaChi,
                        PhongTros = n.PhongTros.Select(p => new
                        {
                            p.MaPhong,
                            p.TenPhong,
                            p.Gia,
                            p.DienTich,
                            p.ConTrong,
                            p.MoTa,
                            TheLoai = p.MaTheLoaiNavigation.TenTheLoai
                        }).ToList()
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return NotFound(ApiResponse<object>.CreateError("Không tìm thấy người dùng hoặc không phải chủ trọ"));

            return Ok(ApiResponse<object>.CreateSuccess("Lấy thông tin thành công", user));
        }

        #endregion
        #region get info
        [HttpGet("get-phong/{id}")]
        public async Task<IActionResult> GetPhong(int id)
        {
            var phong = await _context.PhongTros
                .Where(p => p.MaPhong == id)
                .Select(p => new
                {
                    p.MaPhong,
                    p.TenPhong,
                    p.Gia,
                    p.DienTich,
                    p.ConTrong,
                    p.MoTa,
                    TheLoai = p.MaTheLoaiNavigation.TenTheLoai
                })
                .FirstOrDefaultAsync();

            if (phong == null)
            {
                return NotFound(ApiResponse<object>.CreateError("Không tìm thấy phòng trọ"));
            }

            return Ok(ApiResponse<object>.CreateSuccess("Lấy thông tin phòng thành công", phong));
        }
        [HttpGet("get-hop-dong/{id}")]
        public async Task<IActionResult> Gethopdong(int id)
        {
            #region check quyền và login
            var userName = User.Identity.Name;
            if (userName == null)
            {
                return Unauthorized(new { message = "Bạn chưa đăng nhập" });
            }
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.SoDienThoai == userName);
            if (user == null)
            {
                user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == userName);
            }
            if (user == null)
            {
                return Unauthorized(new { message = "Người dùng không tồn tại" });
            }
            
            #endregion
            try
            {
                var hopDong = await _context.HopDongs
                     .Where(h => h.MaPhong == id)
                     .Select(h => new
                     {
                         h.MaHopDong,
                         h.MaPhong, 
                         NgayBatDau = h.NgayBatDau.HasValue ? h.NgayBatDau.Value.ToString("yyyy-MM-dd") : null,
                         NgayKetThuc = h.NgayKetThuc.HasValue ? h.NgayKetThuc.Value.ToString("yyyy-MM-dd") : null,
                         h.SoNguoiO,
                         h.TienDatCoc,
                         h.DaKetThuc
                     })
                     .FirstOrDefaultAsync();

                if (hopDong == null)
                {
                    return NotFound(ApiResponse<object>.CreateError("Không tìm thấy hợp đồng"));
                }

                return Ok(ApiResponse<object>.CreateSuccess("Lấy thông tin hợp đồng thành công", hopDong));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.CreateError("Lỗi khi lấy thông tin hợp đồng: " + ex.Message));
            }
            #endregion

        }
        // API: Get all users for Admin management
        [HttpGet("api/Admin/Users")]
        public async Task<IActionResult> GetAllUsersForAdmin(int page = 1, int pageSize = 10, string search = "", string roleFilter = "")
        {
            try
            {
                var query = _context.NguoiDungs.AsQueryable();

                // Apply search filter
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(n =>
                        n.HoTen.ToLower().Contains(search.ToLower()) ||
                        n.Email.ToLower().Contains(search.ToLower()) ||
                        n.SoDienThoai.Contains(search)
                    );
                }

                // Apply role filter
                if (!string.IsNullOrEmpty(roleFilter))
                {
                    query = query.Where(n => n.VaiTro == roleFilter);
                }

                // Get total count for pagination
                var totalUsers = await query.CountAsync();
                var totalPages = (int)Math.Ceiling((double)totalUsers / pageSize);

                // Apply pagination
                var users = await query
                    .OrderBy(n => n.MaNguoiDung)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(n => new
                    {
                        maNguoiDung = n.MaNguoiDung,
                        hoTen = n.HoTen,
                        email = n.Email,
                        soDienThoai = n.SoDienThoai,
                        vaiTro = n.VaiTro,
                        vaiTroText = n.VaiTro == "2" ? "Admin" : n.VaiTro == "1" ? "Quản lý" : "Khách hàng",
                        ngayTao = DateTime.Now.ToString("yyyy-MM-dd")
                    })
                    .ToListAsync();

                var result = new
                {
                    users = users,
                    totalUsers = totalUsers,
                    totalPages = totalPages,
                    currentPage = page,
                    pageSize = pageSize
                };

                return Ok(ApiResponse<object>.CreateSuccess(
                    "Lấy danh sách người dùng thành công",
                    result
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi lấy danh sách người dùng"
                ));
            }
        }

        // API: Get all users for management (legacy endpoint)
        [HttpGet("get-all-users")]
        public async Task<IActionResult> GetAllUsers(int page = 1, int pageSize = 10, string search = "", string roleFilter = "")
        {
            return await GetAllUsersForAdmin(page, pageSize, search, roleFilter);
        }

        // API: Get all customers
        [HttpGet("get-all-customer")]
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
        [HttpGet("get-available-tenants")]
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
                so_cccd=user.so_cccd,
                vaiTro = user.VaiTro
            });
        }

        // API: Create new user for Admin
        [HttpPost("api/Admin/CreateUser")]
        public async Task<IActionResult> CreateUserForAdmin([FromBody] CreateUserRequesta request)
        {
            try
            {
                // Check if email exists
                var existingEmail = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == request.email);
                if (existingEmail != null)
                {
                    return BadRequest(ApiResponse<object>.CreateError("Email đã tồn tại"));
                }

                // Check if phone exists
                var existingPhone = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.SoDienThoai == request.soDienThoai);
                if (existingPhone != null)
                {
                    return BadRequest(ApiResponse<object>.CreateError("Số điện thoại đã tồn tại"));
                }

                var user = new NguoiDung
                {
                    HoTen = request.hoTen,
                    Email = request.email,
                    SoDienThoai = request.soDienThoai,
                    MatKhau = request.matKhau,
                    VaiTro = request.vaiTro
                };

                _context.NguoiDungs.Add(user);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.CreateSuccess("Thêm người dùng thành công", null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.CreateError("Lỗi khi thêm người dùng: " + ex.Message));
            }
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

        // API: Update user for Admin
        [HttpPut("api/Admin/UpdateUser/{id}")]
        public async Task<IActionResult> UpdateUserForAdmin(int id, [FromBody] CreateUserRequesta request)
        {
            try
            {
                var existingUser = await _context.NguoiDungs.FindAsync(id);
                if (existingUser == null)
                {
                    return NotFound(ApiResponse<object>.CreateError("Không tìm thấy người dùng"));
                }

                // Check if email exists (except current user)
                var existingEmail = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == request.email && u.MaNguoiDung != id);
                if (existingEmail != null)
                {
                    return BadRequest(ApiResponse<object>.CreateError("Email đã tồn tại"));
                }

                // Check if phone exists (except current user)
                var existingPhone = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.SoDienThoai == request.soDienThoai && u.MaNguoiDung != id);
                if (existingPhone != null)
                {
                    return BadRequest(ApiResponse<object>.CreateError("Số điện thoại đã tồn tại"));
                }

                // Update user info
                existingUser.HoTen = request.hoTen;
                existingUser.Email = request.email;
                existingUser.SoDienThoai = request.soDienThoai;
                existingUser.VaiTro = request.vaiTro;

                // Only update password if provided
                if (!string.IsNullOrEmpty(request.matKhau))
                {
                    existingUser.MatKhau = request.matKhau;
                }

                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.CreateSuccess("Cập nhật người dùng thành công", null));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.CreateError("Lỗi khi cập nhật người dùng: " + ex.Message));
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

        // API: Delete user for Admin
        [HttpDelete("api/Admin/DeleteUser/{id}")]
        public async Task<IActionResult> DeleteUserForAdmin(int id)
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

        // DTO for create user request
        public class CreateUserRequesta
        {
            public string hoTen { get; set; }
            public string email { get; set; }
            public string soDienThoai { get; set; }
            public string matKhau { get; set; }
            public string vaiTro { get; set; }
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

        [HttpPost("profile/update")]
        public async Task<IActionResult> UpdateProfileApi([FromBody] UpdateProfileRequest request)
        {
            try
            {
                _logger.LogInformation("Profile update request received");
                
                if (!User.Identity.IsAuthenticated)
                {
                    _logger.LogWarning("User not authenticated");
                    return Unauthorized(new { message = "Bạn chưa đăng nhập" });
                }

                // Get current user using same method as other APIs
                var userName = User.Identity.Name;
                _logger.LogInformation($"Current user name: {userName}");
                
                if (userName == null)
                {
                    _logger.LogWarning("User name is null");
                    return Unauthorized(new { message = "Bạn chưa đăng nhập" });
                }

                var existingUser = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.SoDienThoai == userName);
                _logger.LogInformation($"User found by phone: {existingUser != null}");
                
                if (existingUser == null)
                {
                    // Try to find by email if not found by phone
                    existingUser = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == userName);
                    _logger.LogInformation($"User found by email: {existingUser != null}");
                }

                if (existingUser == null)
                {
                    _logger.LogWarning($"User not found with identifier: {userName}");
                    return NotFound(new { message = "Không tìm thấy người dùng" });
                }

                // Validate input
                if (string.IsNullOrWhiteSpace(request.HoTen))
                {
                    return BadRequest(new { message = "Họ tên không được để trống" });
                }

                // Update allowed fields only
                existingUser.HoTen = request.HoTen.Trim();

                // Only update password if provided
                if (!string.IsNullOrWhiteSpace(request.MatKhau))
                {
                    existingUser.MatKhau = request.MatKhau;
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Cập nhật thông tin cá nhân thành công!",
                    data = new
                    {
                        MaNguoiDung = existingUser.MaNguoiDung,
                        HoTen = existingUser.HoTen,
                        Email = existingUser.Email,
                        SoDienThoai = existingUser.SoDienThoai
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Có lỗi xảy ra khi cập nhật thông tin", error = ex.Message });
            }
        }

        // API: Get current user profile
        [HttpGet("Dashborad")]
        public async Task<IActionResult> doarboard()
        {
            try
            {
                #region check quyền và login
                var userName = User.Identity.Name;
                if (userName == null)
                {
                    return Unauthorized(new { message = "Bạn chưa đăng nhập" });
                }
                var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.SoDienThoai == userName);
                if (user == null)
                {
                    user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == userName);
                }
                if (user == null)
                {
                    return Unauthorized(new { message = "Người dùng không tồn tại" });
                }

                // Kiểm tra vai trò - chỉ cho phép khách thuê (vai trò "0")
                if (user.VaiTro != "0")
                {
                    return Forbid("Chỉ khách thuê mới được xem thống kê này!");
                }

                #endregion
                var hdnguoithue = await _context.HopDongNguoiThues.FirstOrDefaultAsync(t => t.MaKhachThue == user.MaNguoiDung);
                if (hdnguoithue == null)
                {
                    return BadRequest(ApiResponse<object>.CreateError("Bạn chưa có hợp đồng thuê trọ nào!"));
                }
                
                var HopDong = await _context.HopDongs.Include(t=>t.DenBus).FirstOrDefaultAsync(t => t.MaHopDong == hdnguoithue.MaHopDong);
                if (HopDong == null)
                {
                    return BadRequest(ApiResponse<object>.CreateError("Không tìm thấy thông tin hợp đồng!"));
                }
                
                var phongtro = await _context.PhongTros.FirstOrDefaultAsync(t => t.MaPhong == HopDong.MaPhong);
                var lisuthanhtoan = await _context.LichSuThanhToans.FirstOrDefaultAsync(t => t.MaHopDong == HopDong.MaHopDong);
                

                var homNay = DateTime.Today;
                var homQua = homNay.AddDays(-1);
                #region Thống kê Doanh thu
                // Tính mốc thời gian
                int lechThu = (7 + (int)homNay.DayOfWeek - (int)DayOfWeek.Monday) % 7;
                var dauTuan = homNay.AddDays(-1 * lechThu).Date;
                var dauTuanTruoc = dauTuan.AddDays(-7);
                var cuoiTuanTruoc = dauTuan.AddDays(-1);

                var dauThang = new DateTime(homNay.Year, homNay.Month, 1);
                var dauThangTruoc = dauThang.AddMonths(-1);
                var cuoiThangTruoc = dauThang.AddDays(-1);

                var dauNam = new DateTime(homNay.Year, 1, 1);
                var dauNamTruoc = dauNam.AddYears(-1);
                var cuoiNamTruoc = dauNam.AddDays(-1);

                var truyVan = _context.HoaDonTongs
                 .Where(t => t.MaHopDong == HopDong.MaHopDong)
                 .AsQueryable(); 
                var homNayDateOnly = DateOnly.FromDateTime(homNay);
                var homQuaDateOnly = DateOnly.FromDateTime(homQua);

                var dauTuanDateOnly = DateOnly.FromDateTime(dauTuan);
                var dauTuanTruocDateOnly = dauTuanDateOnly.AddDays(-7);
                var cuoiTuanTruocDateOnly = dauTuanDateOnly.AddDays(-1);

                var dauThangDateOnly = DateOnly.FromDateTime(dauThang);
                var dauThangTruocDateOnly = dauThangDateOnly.AddMonths(-1);
                var cuoiThangTruocDateOnly = dauThangDateOnly.AddDays(-1);

                var dauNamDateOnly = DateOnly.FromDateTime(dauNam);
                var dauNamTruocDateOnly = dauNamDateOnly.AddYears(-1);
                var cuoiNamTruocDateOnly = dauNamDateOnly.AddDays(-1);
                //var homNay = DateTime.Today;
                var thangNay = homNay.Month;
                var namNay = homNay.Year;

                int thangTruoc = thangNay == 1 ? 12 : thangNay - 1;
                int namTruoc = thangNay == 1 ? namNay - 1 : namNay;

                // Tiền điện tháng này
                var tongTienDienThangNay = await _context.HoaDonTienIches
          .Where(h => h.MaPhong == HopDong.MaPhong && h.Thang == thangNay && h.Nam == namNay)
          .SumAsync(h => (decimal?)((decimal)h.SoDien * h.DonGiaDien)) ?? 0;


                // Tiền điện tháng trước
                var tongTienDienThangTruoc = await _context.HoaDonTienIches
                    .Where(h => h.MaPhong == HopDong.MaPhong&&h.Thang == thangTruoc && h.Nam == namTruoc)
                    .SumAsync(h => (decimal?)((decimal)h.SoDien * h.DonGiaDien)) ?? 0;

                // Tỷ lệ thay đổi (%)
                decimal tyLeThayDoi = tongTienDienThangTruoc == 0
                    ? 0
                    : ((tongTienDienThangNay - tongTienDienThangTruoc) / tongTienDienThangTruoc) * 100;
                #endregion
                //lmlaij phần tính darb ni nì
                var ketQua = new Doarboard
                {
                    
                   
                    ThangNay = await truyVan.Where(x => x.NgayXuat >= dauThangDateOnly && x.NgayXuat <= homNayDateOnly).SumAsync(x => (decimal?)x.TongTien) ?? 0,
                    ThangTruoc = await truyVan.Where(x => x.NgayXuat >= dauThangTruocDateOnly && x.NgayXuat <= cuoiThangTruocDateOnly).SumAsync(x => (decimal?)x.TongTien) ?? 0,
                    NamNay = await truyVan.Where(x => x.NgayXuat >= dauNamDateOnly && x.NgayXuat <= homNayDateOnly).SumAsync(x => (decimal?)x.TongTien) ?? 0,
                    NamTruoc = await truyVan.Where(x => x.NgayXuat >= dauNamTruocDateOnly && x.NgayXuat <= cuoiNamTruocDateOnly).SumAsync(x => (decimal?)x.TongTien) ?? 0,
                    Dienthangnay = await _context.HoaDonTienIches
                 .Where(h => h.Thang == thangNay && h.Nam == namNay)
                 .SumAsync(h => (decimal?)((decimal)h.SoDien * h.DonGiaDien)) ?? 0,
                    Dienthangtruoc = await _context.HoaDonTienIches
                 .Where(h => h.Thang == thangTruoc && h.Nam == namTruoc)
                 .SumAsync(h => (decimal?)((decimal)h.SoDien * h.DonGiaDien)) ?? 0,
                    Nuocthangnay = await _context.HoaDonTienIches
                     .Where(h => h.Thang == thangNay && h.Nam == namNay)
                     .SumAsync(h => (decimal?)((decimal)h.SoNuoc * h.DonGiaNuoc)) ?? 0,
                    Nuocthangtruoc = await _context.HoaDonTienIches.Where(h => h.Thang == thangTruoc && h.Nam == namTruoc)
                     .SumAsync(h => (decimal?)((decimal)h.SoNuoc * h.DonGiaNuoc)) ?? 0
                };
                var mddsf =  truyVan.Where(x => x.NgayXuat >= dauThangTruocDateOnly && x.NgayXuat <= cuoiThangTruocDateOnly).ToList();

                var fromDate = dauThangTruocDateOnly.ToDateTime(TimeOnly.MinValue);
                var toDate = cuoiThangTruocDateOnly.ToDateTime(TimeOnly.MaxValue);


                var tatCa = await truyVan.ToListAsync();
                Console.WriteLine("Tổng số bản ghi trong truyVan: " + tatCa.Count);

                var caidat=await _context.CaiDatHeThongs.FirstOrDefaultAsync();


                ketQua.TyLeThayDoiThangNay = ketQua.ThangTruoc == 0 ? 0 : ((ketQua.ThangNay - ketQua.ThangTruoc) / ketQua.ThangTruoc) * 100;
                ketQua.TyLeThayDoiNamNay = ketQua.NamTruoc == 0 ? 0 : ((ketQua.NamNay - ketQua.NamTruoc) / ketQua.NamTruoc) * 100;
                ketQua.TyLeThayDoidien = ketQua.Dienthangtruoc == 0 ? 0 : ((ketQua.Dienthangnay - ketQua.Dienthangtruoc) / ketQua.Dienthangtruoc) * 100;
                ketQua.TyLeThayDoinuoc = ketQua.Nuocthangtruoc == 0 ? 0 : ((ketQua.Nuocthangnay - ketQua.Nuocthangtruoc) / ketQua.Nuocthangtruoc) * 100;

                // Thêm thông tin phòng đang thuê
                var roomInfo = phongtro != null ? $"{phongtro.TenPhong} - {phongtro.MaNhaTroNavigation?.TenNhaTro}" : "Chưa có thông tin";
                var soDien = await _context.HoaDonTienIches
     .Where(h => h.MaPhong == HopDong.MaPhong && h.Thang == thangNay && h.Nam == namNay)
     .Select(h => h.SoDien)
     .FirstOrDefaultAsync();
                var soNuoc = await _context.HoaDonTienIches
                    .Where(h => h.MaPhong == HopDong.MaPhong && h.Thang == thangNay && h.Nam == namNay)
                    .Select(h => h.SoNuoc)
                    .FirstOrDefaultAsync();

                var result = new
                {
                    // Thống kê chi phí
                    ThangNay = ketQua.ThangNay,
                    ThangTruoc = ketQua.ThangTruoc,
                    TyLeThayDoiThangNay = ketQua.TyLeThayDoiThangNay,
                    NamNay = ketQua.NamNay,
                    NamTruoc = ketQua.NamTruoc,
                    TyLeThayDoiNamNay = ketQua.TyLeThayDoiNamNay,
                    Dienthangnay = ketQua.Dienthangnay,
                    Dienthangtruoc = ketQua.Dienthangtruoc,
                    Nuocthangnay = ketQua.Nuocthangnay,
                    Nuocthangtruoc = ketQua.Nuocthangtruoc,
                    TyLeThayDoidien = ketQua.TyLeThayDoidien,
                    TyLeThayDoinuoc = ketQua.TyLeThayDoinuoc,
                    Soxe=HopDong.SoXe,
                    Songuoi=HopDong.SoNguoiO,
                    Giaguixe=HopDong.SoXe*caidat.PhiGiuXe,
                    PhiDV=caidat.Phidv,
                    chisodien= soDien,
                    chisonuoc = soNuoc,
                    
                    // Thông tin phòng
                    RoomInfo = roomInfo,
                    RoomName = phongtro?.TenPhong,
                    MotelName = phongtro?.MaNhaTroNavigation?.TenNhaTro,
                    RoomPrice = phongtro?.Gia,
                    RoomArea = phongtro?.DienTich,
                    RoomDescription = phongtro?.MoTa
                };

                return Ok(ApiResponse<object>.CreateSuccess("Lấy Kết quả thành công", result));
            }
            catch (Exception ex)
            {

                return StatusCode(500, "Internal server error");
            }
        }
        public class Doarboard
        {
           
          
            public decimal ThangNay { get; set; }
            public decimal ThangTruoc { get; set; }
            public decimal TyLeThayDoiThangNay { get; set; }
            public decimal NamNay { get; set; }
            public decimal NamTruoc { get; set; }
            public decimal TyLeThayDoiNamNay { get; set; }
            public decimal Dienthangtruoc { get; set; }
            public decimal Dienthangnay { get; set; }
            public decimal Nuocthangtruoc { get; set; }
            public decimal Nuocthangnay { get; set; }
            public decimal TyLeThayDoinuoc { get; set; }
            public decimal TyLeThayDoidien { get; set; }
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] EmailRequest req)
        {
            var email = req.Email;
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return NotFound(new { message = "Không tìm thấy tài khoản với email này." });
            var code = new Random().Next(100000, 999999).ToString();
            HttpContext.Session.SetString($"reset_code_{email}", code);
            HttpContext.Session.SetString($"reset_code_time_{email}", DateTime.UtcNow.ToString("o"));
            var body = $@"<div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f9f9f9;'><div style='background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'><div style='text-align: center; margin-bottom: 30px;'><h2 style='color: #2c3e50; margin: 0; font-size: 24px;'>🔑 Đặt lại mật khẩu Nhà trọ</h2></div><div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px; border-left: 4px solid #007bff; margin: 20px 0;'><p style='color: #495057; font-size: 16px; line-height: 1.6; margin: 0 0 15px 0;'>Bạn vừa yêu cầu đặt lại mật khẩu cho tài khoản hệ thống phòng trọ.</p><p style='color: #495057; font-size: 16px; line-height: 1.6; margin: 0;'>Mã xác nhận đặt lại mật khẩu của bạn là:</p></div><div style='text-align: center; margin: 30px 0;'><div style='background: linear-gradient(135deg, #3182ce 0%, #63b3ed 100%); color: white; padding: 20px; border-radius: 10px; display: inline-block; min-width: 200px;'><p style='margin: 0 0 10px 0; font-size: 14px; opacity: 0.9;'>MÃ XÁC NHẬN</p><p style='margin: 0; font-size: 32px; font-weight: bold; letter-spacing: 3px; font-family: monospace;'>{code}</p></div></div><div style='background-color: #fff3cd; border: 1px solid #ffeaa7; padding: 15px; border-radius: 8px; margin: 20px 0;'><p style='color: #856404; font-size: 14px; margin: 0; text-align: center;'>⚠️ Mã này có hiệu lực trong 3 phút. Vui lòng không chia sẻ mã với bất kỳ ai.</p></div><div style='text-align: center; margin-top: 30px; padding-top: 20px; border-top: 1px solid #dee2e6;'><p style='color: #6c757d; font-size: 14px; margin: 0;'>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p><p style='color: #6c757d; font-size: 14px; margin: 10px 0 0 0;'>© 2025 Hệ thống quản lý Nhà trọ</p></div></div></div>";
            await _emailService.SendEmailAsync(email, "Mã xác nhận đặt lại mật khẩu", body);
            return Ok(new { message = "Mã xác nhận đã được gửi đến email của bạn!" });
        }

        [HttpPost("verify-reset-code")]
        public async Task<IActionResult> VerifyResetCode([FromBody] VerifyResetCodeRequest req)
        {
            var email = req.Email;
            var code = HttpContext.Session.GetString($"reset_code_{email}");
            var timeStr = HttpContext.Session.GetString($"reset_code_time_{email}");
            if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(timeStr))
                return NotFound(new { message = "Không tìm thấy mã xác nhận hoặc email." });
            if (code != req.Code)
                return BadRequest(new { message = "Mã xác nhận không đúng." });
            if (DateTime.UtcNow - DateTime.Parse(timeStr) > TimeSpan.FromMinutes(3))
                return BadRequest(new { message = "Mã xác nhận đã hết hạn. Vui lòng gửi lại mã mới." });
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return NotFound(new { message = "Không tìm thấy tài khoản." });
            if(req.NewPassword== "___dummy___")
            {
                return Ok(new { message = "Xác thực code thành công!" });
            }
            user.MatKhau = req.NewPassword;
            HttpContext.Session.Remove($"reset_code_{email}");
            HttpContext.Session.Remove($"reset_code_time_{email}");
            await _context.SaveChangesAsync();
            return Ok(new { message = "Đổi mật khẩu thành công!" });
        }
        public class VerifyResetCodeRequest
        {
            public string Email { get; set; }
            public string Code { get; set; }
            public string NewPassword { get; set; }
        }
        #region Google Login
        [HttpGet("login-google")]
        public IActionResult LoginWithGoogle()
        {
            var redirectUrl = Url.Action("GoogleResponse", "Auth");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [HttpGet("google-response")]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                return Unauthorized(new { message = "Google authentication failed." });
            }
            var principal = result.Principal;
            var email = principal?.FindFirst(ClaimTypes.Email)?.Value;
            var name = principal?.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new { message = "Không lấy được email từ Google." });
            }
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                var newUser = new NguoiDung
                {
                    HoTen = name,
                    Email = email,
                    VaiTro = "0"
                };
                _context.NguoiDungs.Add(newUser);
                await _context.SaveChangesAsync();
                // Đăng nhập user mới
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, email ?? ""),
                    new Claim(ClaimTypes.Email, email ?? ""),
                      new Claim("MaNguoiDung", newUser.MaNguoiDung.ToString())
                };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principalNew = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principalNew);
                return Redirect("/");
            }
            else
            {
                // Đăng nhập user đã tồn tại
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Email ?? ""),
                    new Claim(ClaimTypes.Email, user.Email ?? ""),
                      new Claim("MaNguoiDung", user.MaNguoiDung.ToString())
                };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principalExist = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principalExist);
                return Redirect("/");
            }
        }
        #endregion
    } 
}

public class EmailRequest { public string Email { get; set; } }
