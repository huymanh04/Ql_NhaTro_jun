using Api_Ql_nhatro.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ql_NhaTro_jun.Models;
using System.Diagnostics.Contracts;

namespace Ql_NhaTro_jun.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilityBill : ControllerBase
    {

        private readonly ILogger<UtilityBill> _logger;
        QlNhatroContext _context;
        public UtilityBill(ILogger<UtilityBill> logger, QlNhatroContext cc)
        {
            _logger = logger; _context = cc;
        }
        [HttpGet("get-hoadon-tien-ich")]
        public async Task<IActionResult> GetHoaDonTienIch()
        {
            try
            {
                var hoaDonTienIch = await _context.HoaDonTienIches
                    .Select(h => new HoaDonTienIchDTO
                    {
                        MaHoaDon = h.MaHoaDon,
                        MaPhong = h.MaPhong ?? 0,
                        Thang = h.Thang ?? 0,
                        Nam = h.Nam ?? 0,
                        SoDien = h.SoDien ?? 0,
                        SoNuoc = h.SoNuoc ?? 0,
                        DonGiaDien = h.DonGiaDien ?? 0,
                        DonGiaNuoc = h.DonGiaNuoc ?? 0,
                        TongTien = h.TongTien ?? 0,
                        DaThanhToan = h.DaThanhToan ?? false
                    })
                    .ToListAsync();

                return Ok(ApiResponse<List<HoaDonTienIchDTO>>.CreateSuccess(
                    "Lấy danh sách hóa đơn tiện ích thành công",
                    hoaDonTienIch
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách hóa đơn tiện ích");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi lấy danh sách hóa đơn tiện ích"
                ));
            }
        }
        [HttpGet("get-hoadon-tien-ich/{id}")]
        public async Task<IActionResult> GetHoaDonTienIchById(int id)
        {
            try
            {
                var hoaDon = await _context.HoaDonTienIches
                    .Where(h => h.MaHoaDon == id)
                    .Select(h => new HoaDonTienIchDTO
                    {
                        MaHoaDon = h.MaHoaDon,
                        MaPhong = h.MaPhong ?? 0,
                        Thang = h.Thang ?? 0,
                        Nam = h.Nam ?? 0,
                        SoDien = h.SoDien ?? 0,
                        SoNuoc = h.SoNuoc ?? 0,
                        DonGiaDien = h.DonGiaDien ?? 0,
                        DonGiaNuoc = h.DonGiaNuoc ?? 0,
                        TongTien = h.TongTien ?? 0,
                        DaThanhToan = h.DaThanhToan ?? false
                    })
                    .FirstOrDefaultAsync();

                if (hoaDon == null)
                    return NotFound(ApiResponse<object>.CreateError("Hóa đơn tiện ích không tồn tại"));

                return Ok(ApiResponse<HoaDonTienIchDTO>.CreateSuccess(
                    "Lấy hóa đơn tiện ích thành công",
                    hoaDon
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy hóa đơn tiện ích");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi lấy hóa đơn tiện ích"
                ));
            }
        }
        [HttpPost("add-hoadon-tien-ich")]
        public async Task<IActionResult> CreateHoaDonTienIch([FromBody] HoaDonTienIchDTO model)
        {
            if (model == null)
                return BadRequest(ApiResponse<object>.CreateError("Dữ liệu không hợp lệ"));

            try
            {
                var hoaDon = new HoaDonTienIch
                {
                    MaPhong = model.MaPhong,
                    Thang = model.Thang,
                    Nam = model.Nam,
                    SoDien = model.SoDien,
                    SoNuoc = model.SoNuoc,
                    DonGiaDien = model.DonGiaDien,
                    DonGiaNuoc = model.DonGiaNuoc,
                    TongTien = model.TongTien,
                    DaThanhToan = model.DaThanhToan
                };

                _context.HoaDonTienIches.Add(hoaDon);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<HoaDonTienIchDTO>.CreateSuccess(
                    "Thêm mới hóa đơn tiện ích thành công",
                    model
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm mới hóa đơn tiện ích");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi thêm mới hóa đơn tiện ích"
                ));
            }
        }
        [HttpPut("edit-hoadon-tien-ich/{id}")]
        public async Task<IActionResult> UpdateHoaDonTienIch(int id, [FromBody] HoaDonTienIchDTO model)
        {
            if (model == null)
                return BadRequest(ApiResponse<object>.CreateError("Dữ liệu không hợp lệ"));

            try
            {
                var hoaDon = await _context.HoaDonTienIches.FindAsync(id);
                if (hoaDon == null)
                    return NotFound(ApiResponse<object>.CreateError("Hóa đơn tiện ích không tồn tại"));

                hoaDon.MaPhong = model.MaPhong;
                hoaDon.Thang = model.Thang;
                hoaDon.Nam = model.Nam;
                hoaDon.SoDien = model.SoDien;
                hoaDon.SoNuoc = model.SoNuoc;
                hoaDon.DonGiaDien = model.DonGiaDien;
                hoaDon.DonGiaNuoc = model.DonGiaNuoc;
                hoaDon.TongTien = model.TongTien;
                hoaDon.DaThanhToan = model.DaThanhToan;

                _context.HoaDonTienIches.Update(hoaDon);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<HoaDonTienIchDTO>.CreateSuccess(
                    "Cập nhật hóa đơn tiện ích thành công",
                    model
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật hóa đơn tiện ích");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi cập nhật hóa đơn tiện ích"
                ));
            }
        }
        [HttpDelete("delete-hoadon-tienich/{id}")]
        public async Task<IActionResult> delete(int id)
        {
            var manh = await _context.HoaDonTienIches.FindAsync(id);
            if (manh == null)
                return NotFound(ApiResponse<object>.CreateError("Hóa đơn tiện ích không tồn tại"));
            try {
                _context.HoaDonTienIches.Remove(manh);
                await _context.SaveChangesAsync();
                return Ok(ApiResponse<object>.CreateSuccess(
                    "Xóa hó đơn thành công",
                    null
                ));
            } catch { }
            return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi xóa hợp đồng"
                ));
        }
        public class HoaDonTienIchDTO
        {
            public int MaHoaDon { get; set; }
            public int MaPhong { get; set; }
            public int Thang { get; set; }
            public int Nam { get; set; }
            public double SoDien { get; set; }
            public double SoNuoc { get; set; }
            public decimal DonGiaDien { get; set; }
            public decimal DonGiaNuoc { get; set; }
            public decimal TongTien { get; set; }
            public bool DaThanhToan { get; set; }
        }
    }
}
