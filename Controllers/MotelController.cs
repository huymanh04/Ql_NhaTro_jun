using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ql_NhaTro_jun.Models;
using static Ql_NhaTro_jun.Controllers.LocationController;
using System.Globalization;
using System.ComponentModel.DataAnnotations;

namespace Ql_NhaTro_jun.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MotelController : ControllerBase
    {
        private readonly ILogger<MotelController> _logger;
        QlNhatroContext _context { get; set; }
        public MotelController(ILogger<MotelController> logger, QlNhatroContext context)
        {
            _logger = logger;
            _context = context;
        }
        #region Nhà trọ
        [HttpGet("get-motel")]
        public async Task<IActionResult> getMotel()
        {
            var banners = await _context.NhaTros.ToListAsync();
            return Ok(banners);
        }
        [HttpPost("add-motel")]
        public async Task<IActionResult> AddTinh([FromBody] NhaTroDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    return BadRequest(ApiResponse<NhaTro>.CreateError("Dữ liệu không hợp lệ", errors));
                }

                // Chuẩn hóa tên tỉnh (loại bỏ khoảng trắng thừa và chuyển về proper case)
                var normalizedName = CultureInfo.CurrentCulture.TextInfo
                    .ToTitleCase(dto.TenNhaTro.Trim().ToLower());

                // Kiểm tra trùng lặp (không phân biệt hoa thường)
                var existingTinhThanh = await _context.NhaTros
                    .FirstOrDefaultAsync(t => EF.Functions.Collate(t.TenNhaTro, "SQL_Latin1_General_CP1_CI_AI") == normalizedName);

                if (existingTinhThanh != null)
                {
                    return BadRequest(ApiResponse<NhaTro>.CreateError("Tên nhà trọ đã tồn tại"));
                }
               var nhatro=new NhaTro { TenNhaTro = normalizedName, DiaChi = dto.DiaChi?.Trim(), MaTinh = dto.MaTinh, MaKhuVuc = dto.MaKhuVuc, MoTa = dto.MoTa, MaChuTro = dto.MaChuTro,NgayTao=DateTime.Now };
                await _context.NhaTros.AddAsync(nhatro);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<NhaTro>.CreateSuccess("Thêm nhà trọ thành công", nhatro));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêmnhà trọ: {TenTinh}", dto.TenNhaTro);
                return StatusCode(500, ApiResponse<NhaTro>.CreateError(
                    "Đã xảy ra lỗi khi thêm nhà trọ",
                    "Vui lòng thử lại sau hoặc liên hệ admin"));
            }
        }
        [HttpPut("edit-motel/{id}")]
        public async Task<IActionResult> EditNhaTro(int id, [FromBody] NhaTroDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    return BadRequest(ApiResponse<NhaTro>.CreateError("Dữ liệu không hợp lệ", errors));
                }

                // Chuẩn hóa tên nhà trọ
                var normalizedName = CultureInfo.CurrentCulture.TextInfo
                    .ToTitleCase(dto.TenNhaTro.Trim().ToLower());

                // Kiểm tra trùng tên (khác ID hiện tại)
                var isDuplicate = await _context.NhaTros
                    .AnyAsync(t => EF.Functions.Collate(t.TenNhaTro, "SQL_Latin1_General_CP1_CI_AI") == normalizedName
                                   && t.MaNhaTro != id);
                if (isDuplicate)
                {
                    return BadRequest(ApiResponse<NhaTro>.CreateError("Tên nhà trọ đã tồn tại"));
                }

                // Tìm nhà trọ để cập nhật
                var nhaTro = await _context.NhaTros.FindAsync(id);
                if (nhaTro == null)
                {
                    return NotFound(ApiResponse<NhaTro>.CreateError("Không tìm thấy nhà trọ để cập nhật"));
                }

                // Cập nhật thông tin
                nhaTro.TenNhaTro = normalizedName;
                nhaTro.DiaChi = dto.DiaChi?.Trim();
                nhaTro.MaTinh = dto.MaTinh;
                nhaTro.MaKhuVuc = dto.MaKhuVuc;
                nhaTro.MoTa = dto.MoTa;
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<NhaTro>.CreateSuccess("Sửa nhà trọ thành công", nhaTro));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi sửa nhà trọ: {TenNhaTro}", dto.TenNhaTro);
                return StatusCode(500, ApiResponse<NhaTro>.CreateError(
                    "Đã xảy ra lỗi khi sửa nhà trọ",
                    "Vui lòng thử lại sau hoặc liên hệ admin"));
            }
        }
        [HttpDelete("delete-motel/{id}")]
        public async Task<IActionResult> DeleteNhaTro(int id)
        {
            try
            {
                var nhaTro = await _context.NhaTros.FindAsync(id);
                if (nhaTro == null)
                {
                    return NotFound(ApiResponse<NhaTro>.CreateError("Không tìm thấy nhà trọ để xóa"));
                }

                _context.NhaTros.Remove(nhaTro);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<NhaTro>.CreateSuccess("Xóa nhà trọ thành công", nhaTro));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa nhà trọ: {Id}", id);
                return StatusCode(500, ApiResponse<NhaTro>.CreateError(
                    "Đã xảy ra lỗi khi xóa nhà trọ",
                    "Vui lòng thử lại sau hoặc liên hệ admin"));
            }

        }
        #endregion
        public class NhaTroDto
        {       
            [Required(ErrorMessage = "Tên nhà trọ là bắt buộc")]
            [StringLength(100, ErrorMessage = "Tên nhà trọ tối đa 100 ký tự")]
            public string TenNhaTro { get; set; }
            [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
            [StringLength(255, ErrorMessage = "Địa chỉ tối đa 255 ký tự")]
            public string DiaChi { get; set; }

            [Required(ErrorMessage = "Mã tỉnh là bắt buộc")]
            public int MaTinh { get; set; }
       
            public int MaChuTro { get; set; }

            [Required(ErrorMessage = "Mã khu vực là bắt buộc")]
            public int MaKhuVuc { get; set; }

            public string MoTa { get; set; }
        }
    }
}