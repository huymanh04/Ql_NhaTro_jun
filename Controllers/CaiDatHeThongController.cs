using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ql_NhaTro_jun.Models;
using static RoomTypeController;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Ql_NhaTro_jun.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CaiDatHeThongController : ControllerBase
    {
        private readonly ILogger<CaiDatHeThongController> _logger;
        QlNhatroContext _context;
        private const int MaxImageSize = 5 * 1024 * 1024; // 5MB
        public CaiDatHeThongController(ILogger<CaiDatHeThongController> logger, QlNhatroContext context)
        {
            _logger = logger;
            _context = context;
        }
        [HttpGet("get-cai-dat-he-thong")]
        public async Task<IActionResult> GetCaiDatHeThong()
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
            if (user.VaiTro == "0") // Kiểm tra quyền người dùng
            {
                return BadRequest(ApiResponse<object>.CreateError("Bạn không có quyền thực hiện hành động này"));
            }
            #endregion
            try
            {
                var caiDat = await _context.CaiDatHeThongs.FirstOrDefaultAsync();

                if (caiDat == null)
                {   
                    return NotFound(ApiResponse<object>.CreateError("Cài đặt hệ thống không tồn tại"));
                }
                return Ok(ApiResponse<object>.CreateSuccess("Lấy cài đặt hệ thống thành công", caiDat));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy cài đặt hệ thống");
                return StatusCode(500, ApiResponse<object>.CreateError("Đã xảy ra lỗi khi lấy cài đặt hệ thống"));
            }
        }
        [HttpPost("add-cai-dat-he-thong")]
        public async Task<IActionResult> addcaidat([FromForm] CaiDatHeThongDTO modle)
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
            if (user.VaiTro == "0") // Kiểm tra quyền người dùng
            {
                return BadRequest(ApiResponse<object>.CreateError("Bạn không có quyền thực hiện hành động này"));
            }
            if(await _context.CaiDatHeThongs.CountAsync() > 0)
            {
                return BadRequest(ApiResponse<object>.CreateError("Cài đặt hệ thống đã tồn tại, vui lòng cập nhật thay vì thêm mới"));
            }
            #endregion
            try
            {
               
                if (modle == null)
                    return BadRequest(ApiResponse<object>.CreateError("Thông tin cài đặt không hợp lệ"));

                byte[] imageBytes = null;
                if (modle.ImageFile != null)
                {
                    if (modle.ImageFile.Length > MaxImageSize)
                    {
                        return BadRequest(ApiResponse<object>.CreateError(
                            "Kích thước ảnh không được vượt quá 5MB"));
                    }

                    if (!IsValidImageFile(modle.ImageFile))
                    {
                        return BadRequest(ApiResponse<object>.CreateError(
                            "Chỉ chấp nhận file ảnh định dạng JPG, PNG, GIF"));
                    }

                    using var ms = new MemoryStream();
                    await modle.ImageFile.CopyToAsync(ms);
                    imageBytes = ms.ToArray();
                }
                var caiDatHeThong = new CaiDatHeThong
                {
                    CheDoGiaoDien = modle.CheDoGiaoDien,
                    LogoUrl = imageBytes,
                    TieuDeWeb = modle.TieuDeWeb,
                    TienDien = modle.TienDien,
                    TienNuoc = modle.TienNuoc,
                    DiaChi = modle.DiaChi,
                    SoDienThoai = modle.SoDienThoai,
                    GoogleMapEmbed = modle.GoogleMapEmbed,
                    AiApikey = modle.AiApikey,
                    MoTaThem = modle.MoTaThem
                };
                _context.CaiDatHeThongs.Add(caiDatHeThong);
                await _context.SaveChangesAsync();
                return Ok(ApiResponse<object>.CreateSuccess("Cài đặt hệ thống đã được cập nhật thành công", modle));
            }
            catch { }
            return Ok(ApiResponse<object>.CreateError("Cài đặt hệ thống đã được cập nhật thất bại"));
        }
        [HttpPut("update-cai-dat-he-thong")]
        public async Task<IActionResult> UpdateCaiDatHeThong([FromBody] CaiDatHeThongDTO model)
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
            if (user.VaiTro == "0") // Kiểm tra quyền người dùng
            {
                return BadRequest(ApiResponse<object>.CreateError("Bạn không có quyền thực hiện hành động này"));
            }
            #endregion
            try
            {
                if (model == null)
                    return BadRequest(ApiResponse<object>.CreateError("Thông tin cài đặt không hợp lệ"));

                var existingCaiDat = await _context.CaiDatHeThongs.FirstOrDefaultAsync();
                if (existingCaiDat == null)
                    return NotFound(ApiResponse<object>.CreateError("Cài đặt hệ thống không tồn tại"));

                // Chỉ cập nhật nếu giá trị thay đổi
                if (model.CheDoGiaoDien != null && model.CheDoGiaoDien != existingCaiDat.CheDoGiaoDien)
                {
                    existingCaiDat.CheDoGiaoDien = model.CheDoGiaoDien;
                }
                //if (model.LogoUrl != null && !model.LogoUrl.SequenceEqual(existingCaiDat.LogoUrl ?? Array.Empty<byte>()))
                //{
                //    existingCaiDat.LogoUrl = model.LogoUrl;
                //}
                if (model.TieuDeWeb != null && model.TieuDeWeb != existingCaiDat.TieuDeWeb)
                {
                    existingCaiDat.TieuDeWeb = model.TieuDeWeb;
                }
                if (model.TienDien != null && model.TienDien != existingCaiDat.TienDien)
                {
                    existingCaiDat.TienDien = model.TienDien;
                }
                if (model.TienNuoc != null && model.TienNuoc != existingCaiDat.TienNuoc)
                {
                    existingCaiDat.TienNuoc = model.TienNuoc;
                }
                if (model.DiaChi != null && model.DiaChi != existingCaiDat.DiaChi)
                {
                    existingCaiDat.DiaChi = model.DiaChi;
                }
                if (model.SoDienThoai != null && model.SoDienThoai != existingCaiDat.SoDienThoai)
                {
                    existingCaiDat.SoDienThoai = model.SoDienThoai;
                }
                if (model.GoogleMapEmbed != null && model.GoogleMapEmbed != existingCaiDat.GoogleMapEmbed)
                {
                    existingCaiDat.GoogleMapEmbed = model.GoogleMapEmbed;
                }
                if (model.AiApikey != null && model.AiApikey != existingCaiDat.AiApikey)
                {
                    existingCaiDat.AiApikey = model.AiApikey;
                }
                if (model.MoTaThem != null && model.MoTaThem != existingCaiDat.MoTaThem)
                {
                    existingCaiDat.MoTaThem = model.MoTaThem;
                }
                _context.CaiDatHeThongs.Update(existingCaiDat);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.CreateSuccess("Cài đặt hệ thống đã được cập nhật thành công", existingCaiDat));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật cài đặt hệ thống");
                return StatusCode(500, ApiResponse<object>.CreateError("Đã xảy ra lỗi khi cập nhật cài đặt hệ thống"));
            }
        }

        public class CaiDatHeThongDTO
        {
            public string? CheDoGiaoDien { get; set; }

            public IFormFile ImageFile { get; set; }

            public string? TieuDeWeb { get; set; }
            public decimal? TienDien { get; set; }

            public decimal? TienNuoc { get; set; }

            public string? DiaChi { get; set; }

            public string? SoDienThoai { get; set; }

            public string? GoogleMapEmbed { get; set; }

            public string? AiApikey { get; set; }

            public string? MoTaThem { get; set; }
        }
        #region Helper Methods
        private bool IsValidImageFile(IFormFile file)
        {
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
            return allowedTypes.Contains(file.ContentType.ToLower());
        }

        private string GetModelStateErrors()
        {
            return string.Join(", ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
        }

        private RoomTypeResponseDto MapToResponseDto(TheLoaiPhongTro model)
        {
            return new RoomTypeResponseDto
            {
                MaTheLoai = model.MaTheLoai,
                TenTheLoai = model.TenTheLoai,
                MoTa = model.MoTa,
                ImageBase64 = model.ImageUrl != null ? Convert.ToBase64String(model.ImageUrl) : null,
                RedirectUrl = model.RedirectUrl
            };
        }
        #endregion
    }
}
