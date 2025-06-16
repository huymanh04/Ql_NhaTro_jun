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

namespace Api_Ql_nhatro.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {

        private readonly ILogger<AuthController> _logger;
        QlNhatroContext _context;
        public AuthController(ILogger<AuthController> logger, QlNhatroContext cc)
        {
            _logger = logger; _context = cc;
        }
        #region login and Register
        public class EmailRequest
        {
            public string Email { get; set; }
        }
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
                        user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == model.SoDienThoai);
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

            if (user != null)
            {
                return Unauthorized(new { message = "Email đã tồn tại" });
            }
            var recaptchaResponse = model.RecaptchaResponse;


            string secretKey = "6LcQdhUrAAAAALA0Kf-pPNX8yTyHbpdZpVC3bsuG";

            var client = new WebClient();
            var result = client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}",
                                    secretKey, recaptchaResponse));

            var response = JsonSerializer.Deserialize<RecaptchaResponse>(result);
            if (!response.Success)
            {
                return BadRequest(new { message = "Xác minh reCAPTCHA thất bại!" });
            }

            model.VaiTro = "0";
            _context.Add(model);

            HttpContext.Session.Remove($"AES_{model.Email}_Key");
            HttpContext.Session.Remove($"AES_{model.Email}_IV");
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception EE) { }
            return Ok(new { Success = true, message = "Đăng Ký thành công !" });
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
                return NotFound(new { message = "Người dùng không tồn tại" });
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
       
    } 
}
