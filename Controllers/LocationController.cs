using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Ql_NhaTro_jun.Models;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Ql_NhaTro_jun.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationController : ControllerBase
    {
        private readonly ILogger<LocationController> _logger;
        QlNhatroContext _context;
        public LocationController(ILogger<LocationController> logger, QlNhatroContext cc)
        {
            _logger = logger; _context = cc;
        }
        #region Tỉnh Thành
        [HttpGet("Tinh-thanh")]
        public async Task<IActionResult> Gettinh()
        {
            try
            {
                var tinhThanhs = await _context.TinhThanhs
                    .Select(t => new TinhThanhResponseDto
                    {
                        MaTinh = t.MaTinh,
                        TenTinh = t.TenTinh,
                        SoLuongKhuVuc = t.KhuVucs.Count
                    })
                    .ToListAsync();

                return Ok(ApiResponse<List<TinhThanhResponseDto>>.CreateSuccess(
                    "Lấy danh sách tỉnh thành thành công",
                    tinhThanhs));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách tỉnh thành");
                return StatusCode(500, ApiResponse<object>.CreateError("Đã xảy ra lỗi khi lấy danh sách tỉnh thành"));
            }
        }
        [HttpPost("add-tinh-thanh")]
        public async Task<IActionResult> AddTinh([FromBody] TinhThanhDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    return BadRequest(ApiResponse<TinhThanh>.CreateError("Dữ liệu không hợp lệ", errors));
                }

                // Chuẩn hóa tên tỉnh (loại bỏ khoảng trắng thừa và chuyển về proper case)
                var normalizedName = CultureInfo.CurrentCulture.TextInfo
                    .ToTitleCase(dto.TenTinh.Trim().ToLower());

                // Kiểm tra trùng lặp (không phân biệt hoa thường)
                var existingTinhThanh = await _context.TinhThanhs
                    .FirstOrDefaultAsync(t => EF.Functions.Collate(t.TenTinh, "SQL_Latin1_General_CP1_CI_AI") == normalizedName);

                if (existingTinhThanh != null)
                {
                    return BadRequest(ApiResponse<TinhThanh>.CreateError("Tỉnh thành đã tồn tại"));
                }

                var newTinhThanh = new TinhThanh
                {
                    TenTinh = normalizedName
                };

                _context.TinhThanhs.Add(newTinhThanh);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<TinhThanh>.CreateSuccess("Thêm tỉnh thành thành công", newTinhThanh));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm tỉnh thành: {TenTinh}", dto.TenTinh);
                return StatusCode(500, ApiResponse<TinhThanh>.CreateError(
                    "Đã xảy ra lỗi khi thêm tỉnh thành",
                    "Vui lòng thử lại sau hoặc liên hệ admin"));
            }
        }
        [HttpPut("edit-tinh-thanh/{id}")]
        public async Task<IActionResult> Edittinh(int id,[FromBody] TinhThanhDTO dto)
        {
            try
                {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    return BadRequest(ApiResponse<TinhThanh>.CreateError("Dữ liệu không hợp lệ", errors));
                }

                // Chuẩn hóa tên tỉnh (loại bỏ khoảng trắng thừa và chuyển về proper case)
                var normalizedName = CultureInfo.CurrentCulture.TextInfo
                    .ToTitleCase(dto.TenTinh.Trim().ToLower());

                // Kiểm tra trùng lặp (không phân biệt hoa thường)
                var existingTinhThanh = await _context.TinhThanhs
                    .FirstOrDefaultAsync(t => t.MaTinh==id);

                if (existingTinhThanh == null)
                {
                    return BadRequest(ApiResponse<TinhThanh>.CreateError("Tỉnh thành không tồn tại"));
                }


                existingTinhThanh.TenTinh = dto.TenTinh;
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<TinhThanh>.CreateSuccess("Cập nhật thành thành công", existingTinhThanh));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi edut tỉnh thành: {TenTinh}", dto.TenTinh);
                return StatusCode(500, ApiResponse<TinhThanh>.CreateError(
                    "Đã xảy ra lỗi khi cập nhật",
                    "Vui lòng thử lại sau hoặc liên hệ admin"));
            }
        }
        [HttpDelete("delete-tinh-thanh/{id}")]
        public async Task<IActionResult> Deletetinh(int id)
        {
            try
            {
                var tinhThanh = await _context.TinhThanhs.FindAsync(id);
                if (tinhThanh == null)
                {
                    return NotFound(ApiResponse<TinhThanh>.CreateError("Tỉnh thành không tồn tại"));
                }

                _context.TinhThanhs.Remove(tinhThanh);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<TinhThanh>.CreateSuccess("Xóa tỉnh thành thành công", tinhThanh));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa tỉnh thành: {Id}", id);
                return StatusCode(500, ApiResponse<TinhThanh>.CreateError(
                    "Đã xảy ra lỗi khi xóa tỉnh thành",
                    "Vui lòng thử lại sau hoặc liên hệ admin"));
            }
        }
        #endregion
        #region Khu vực
        [HttpGet("get-khu-vuc")]
        public async Task<IActionResult> GetKhuVucPaged( [FromQuery] int pageNumber = 1,[FromQuery] int pageSize = 4,[FromQuery] int? maTinh = null)
        {
            var query = _context.KhuVucs.AsQueryable();

            if (maTinh.HasValue)
            {
                query = query.Where(k => k.MaTinh == maTinh.Value);
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var data = await query
                .OrderBy(k => k.MaKhuVuc)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new
            {
                success = true,
                message = "Lấy danh sách khu vực theo trang thành công",
                currentPage = pageNumber,
                totalPages = totalPages,
                totalItems = totalItems,
                pageSize = pageSize,
                data = data
            });
        }


        [HttpPost("add-khu-vuc")]
        public async Task<IActionResult> AddKhuVuc([FromBody] KhuVucDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    return BadRequest(ApiResponse<KhuVuc>.CreateError("Dữ liệu không hợp lệ", errors));
                }

                // Chuẩn hóa tên khu vực (loại bỏ khoảng trắng thừa và chuyển về proper case)
                var normalizedName = CultureInfo.CurrentCulture.TextInfo
                    .ToTitleCase(dto.TenKhuVuc.Trim().ToLower());

                // Kiểm tra trùng lặp (không phân biệt hoa thường)
                var existingKhuVuc = await _context.KhuVucs
                    .FirstOrDefaultAsync(t => EF.Functions.Collate(t.TenKhuVuc, "SQL_Latin1_General_CP1_CI_AI") == normalizedName);

                if (existingKhuVuc != null)
                {
                    return BadRequest(ApiResponse<KhuVuc>.CreateError("Khu vực đã tồn tại"));
                }

                var newKhuVuc = new KhuVuc
                {
                    TenKhuVuc = normalizedName,
                    MaTinh = dto.MaTinh
                };

                _context.KhuVucs.Add(newKhuVuc);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<KhuVuc>.CreateSuccess("Thêm khu vực thành công", newKhuVuc));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm khu vực: {TenKhuVuc}", dto.TenKhuVuc);
                return StatusCode(500, ApiResponse<KhuVuc>.CreateError(
                    "Đã xảy ra lỗi khi thêm khu vực",
                    "Vui lòng thử lại sau hoặc liên hệ admin"));
            }
        }
        [HttpPut("Edit-khu-vuc/{id}")]
        public async Task<IActionResult> EditKhuVuc(int id,[FromBody] KhuVucDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = string.Join(", ", ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage));
                    return BadRequest(ApiResponse<KhuVuc>.CreateError("Dữ liệu không hợp lệ", errors));
                }

                // Chuẩn hóa tên khu vực (loại bỏ khoảng trắng thừa và chuyển về proper case)
                var normalizedName = CultureInfo.CurrentCulture.TextInfo
                    .ToTitleCase(dto.TenKhuVuc.Trim().ToLower());

                // Kiểm tra trùng lặp (không phân biệt hoa thường)
                var existingKhuVuc = await _context.KhuVucs
                    .FirstOrDefaultAsync(t => t.MaKhuVuc == id);

                if (existingKhuVuc == null)
                {
                    return BadRequest(ApiResponse<KhuVuc>.CreateError("Khu vực không tồn tại"));
                }

                existingKhuVuc.TenKhuVuc = dto.TenKhuVuc;
                existingKhuVuc.MaTinh = dto.MaTinh;

                _context.KhuVucs.Update(existingKhuVuc);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<KhuVuc>.CreateSuccess("Cập nhật khu vực thành công", existingKhuVuc));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật khu vực: {TenKhuVuc}", dto.TenKhuVuc);
                return StatusCode(500, ApiResponse<KhuVuc>.CreateError(
                    "Đã xảy ra lỗi khi cập nhật khu vực",
                    "Vui lòng thử lại sau hoặc liên hệ admin"));
            }
        }
        [HttpDelete("delete-khu-vuc/{id}")]
        public async Task<IActionResult> DeleteKhuVuc(int id)
        {
            try
            {
                var khuVuc = await _context.KhuVucs.FindAsync(id);
                if (khuVuc == null)
                {
                    return NotFound(ApiResponse<KhuVuc>.CreateError("Khu vực không tồn tại"));
                }

                _context.KhuVucs.Remove(khuVuc);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<KhuVuc>.CreateSuccess("Xóa khu vực thành công", khuVuc));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa khu vực: {Id}", id);
                return StatusCode(500, ApiResponse<KhuVuc>.CreateError(
                    "Đã xảy ra lỗi khi xóa khu vực",
                    "Vui lòng thử lại sau hoặc liên hệ admin"));
            }
        }
        #endregion
        #region Các Data Transfer Object
        public class KhuVucDTO
        {
            [Required(ErrorMessage = "Tên khu vực không được để trống")]
            [StringLength(100, ErrorMessage = "Tên khu vực không được vượt quá 100 ký tự")]

            public string TenKhuVuc { get; set; }

            [Required(ErrorMessage = "Mã tỉnh thành không được để trống")]
            public int MaTinh { get; set; }
        }
        public class TinhThanhDTO
        {
            [Required(ErrorMessage = "Tên tỉnh thành không được để trống")]
            [StringLength(100, ErrorMessage = "Tên tỉnh thành không được vượt quá 100 ký tự")]
            [RegularExpression(@"^[a-zA-ZÀ-ỹ\s]+$", ErrorMessage = "Tên tỉnh thành chỉ được chứa chữ cái và khoảng trắng")]
            public string TenTinh { get; set; }
        }
        public class TinhThanhResponseDto
        {
            public int MaTinh { get; set; }
            public string TenTinh { get; set; }
            public int SoLuongKhuVuc { get; set; }
        }
        #endregion

    }
}
