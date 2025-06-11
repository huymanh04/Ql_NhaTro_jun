using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ql_NhaTro_jun.Models;

namespace Ql_NhaTro_jun.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HoaDonTongController : ControllerBase
    {

        private readonly ILogger<HoaDonTongController> _logger;
        private readonly QlNhatroContext _context;

        public HoaDonTongController(ILogger<HoaDonTongController> logger, QlNhatroContext context)
        {
            _logger = logger;
            _context = context;
        }
        [HttpGet("get-hoadon-tong")]
        public async Task<IActionResult> GetHoaDonTong()
        {
            try
            {
                var hoaDonTongs = await _context.HoaDonTongs
     .Select(h => new HoaDonTongDTO
     {
         MaHoaDon = h.MaHoaDon,
         MaHopDong = h.MaHopDong ?? 0,
         NgayXuat = h.NgayXuat.HasValue
             ? h.NgayXuat.Value.ToDateTime(TimeOnly.MinValue)
             : default(DateTime),
         TongTien = h.TongTien ?? 0,
         GhiChu = h.GhiChu
     })
     .ToListAsync();


                return Ok(ApiResponse<List<HoaDonTongDTO>>.CreateSuccess(
                    "Lấy danh sách hóa đơn tổng thành công",
                    hoaDonTongs
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách hóa đơn tổng");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi lấy danh sách hóa đơn tổng"
                ));
            }
        }
        [HttpPost("add-hoadon-tong")]
        public async Task<IActionResult> CreateHoaDonTong([FromBody] HoaDonTongDTO model)
        {
            if (model == null)
                return BadRequest(ApiResponse<object>.CreateError("Dữ liệu không hợp lệ"));

            try
            {
                var hoaDonTong = new HoaDonTong
                {
                    MaHopDong = model.MaHopDong,
                    NgayXuat = DateOnly.FromDateTime(model.NgayXuat),  // Chuyển DateTime sang DateOnly nếu entity đang dùng DateOnly
                    TongTien = model.TongTien,
                    GhiChu = model.GhiChu
                };

                _context.HoaDonTongs.Add(hoaDonTong);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<HoaDonTongDTO>.CreateSuccess(
                    "Thêm hóa đơn tổng thành công",
                    new HoaDonTongDTO
                    {
                        MaHoaDon = hoaDonTong.MaHoaDon,
                        MaHopDong = hoaDonTong.MaHopDong??0,
                        NgayXuat = hoaDonTong.NgayXuat.HasValue
                            ? hoaDonTong.NgayXuat.Value.ToDateTime(TimeOnly.MinValue)
                            : default(DateTime),
                        TongTien = hoaDonTong.TongTien ?? 0,
                        GhiChu = hoaDonTong.GhiChu
                    }
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm hóa đơn tổng");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi thêm hóa đơn tổng"
                ));
            }
        }
        [HttpPut("edit-hoadon-tong/{id}")]
        public async Task<IActionResult> EditHoaDonTong(int id, [FromBody] HoaDonTongDTO model)
        {
            if (model == null || id <= 0)
                return BadRequest(ApiResponse<object>.CreateError("Dữ liệu không hợp lệ"));

            try
            {
                var hoaDonTong = await _context.HoaDonTongs.FindAsync(id);
                if (hoaDonTong == null)
                    return NotFound(ApiResponse<object>.CreateError("Hóa đơn tổng không tồn tại"));

                hoaDonTong.MaHopDong = model.MaHopDong;
                hoaDonTong.NgayXuat = DateOnly.FromDateTime(model.NgayXuat);
                hoaDonTong.TongTien = model.TongTien;
                hoaDonTong.GhiChu = model.GhiChu;

                _context.HoaDonTongs.Update(hoaDonTong);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<HoaDonTongDTO>.CreateSuccess(
                    "Cập nhật hóa đơn tổng thành công",
                    new HoaDonTongDTO
                    {
                        MaHoaDon = hoaDonTong.MaHoaDon,
                        MaHopDong = hoaDonTong.MaHopDong ?? 0,
                        NgayXuat = hoaDonTong.NgayXuat.HasValue
                            ? hoaDonTong.NgayXuat.Value.ToDateTime(TimeOnly.MinValue)
                            : default(DateTime),
                        TongTien = hoaDonTong.TongTien ?? 0,
                        GhiChu = hoaDonTong.GhiChu
                    }
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật hóa đơn tổng");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi cập nhật hóa đơn tổng"
                ));
            }
        }
        [HttpDelete("delete-hoadon-tong/{id}")]
        public async Task<IActionResult> DeleteHoaDonTong(int id)
        {
            if (id <= 0)
                return BadRequest(ApiResponse<object>.CreateError("ID không hợp lệ"));

            try
            {
                var hoaDonTong = await _context.HoaDonTongs.FindAsync(id);
                if (hoaDonTong == null)
                    return NotFound(ApiResponse<object>.CreateError("Hóa đơn tổng không tồn tại"));

                _context.HoaDonTongs.Remove(hoaDonTong);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.CreateSuccess(
                    "Xóa hóa đơn tổng thành công",
                    null
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa hóa đơn tổng");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi xóa hóa đơn tổng"
                ));
            }
        }
        public class HoaDonTongDTO
        {
            public int MaHoaDon { get; set; }
            public int MaHopDong { get; set; }
            public DateTime NgayXuat { get; set; }
            public decimal TongTien { get; set; }
            public string GhiChu { get; set; }
        }
    }
}
