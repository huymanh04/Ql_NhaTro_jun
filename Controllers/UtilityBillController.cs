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
    public class UtilityBillController : ControllerBase
    {

        private readonly ILogger<UtilityBillController> _logger;
        QlNhatroContext _context;
        public UtilityBillController(ILogger<UtilityBillController> logger, QlNhatroContext cc)
        {
            _logger = logger; _context = cc;
        }

        [HttpGet("get-hoa-don-chi-tiet/{id}")]
        public async Task<IActionResult> GetHoaDonTienIchById(int id)
        {
            try
            {
                var hoaDon = await _context.HoaDonTienIches
                    .Where(h => h.MaHoaDon == id)
                    .Select(h => new HoaDonTienIch
                    {
                        MaHoaDon = h.MaHoaDon,
                        MaPhong = h.MaPhong ?? 0,
                        Thang = h.Thang ?? 0,
                        Nam = h.Nam ?? 0,
                        SoDien = h.SoDien ?? 0,
                        SoNuoc = h.SoNuoc ?? 0,
                        Phidv = h.Phidv,
                        Soxe = h.Soxe,
                        DonGiaDien = h.DonGiaDien ?? 0,
                        DonGiaNuoc = h.DonGiaNuoc ?? 0,
                        TongTien = h.TongTien ?? 0,
                        DaThanhToan = h.DaThanhToan ?? false
                    })
                    .FirstOrDefaultAsync();

                if (hoaDon == null)
                    return NotFound(ApiResponse<object>.CreateError("Hóa đơn tiện ích không tồn tại"));

                return Ok(ApiResponse<object>.CreateSuccess(
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
            //add hóa đơn chi tiết kèm hóa đơn tong cho khách hàng
            if (model == null)
                return BadRequest(ApiResponse<object>.CreateError("Dữ liệu không hợp lệ"));

            try
            {
                DateTime date = DateTime.Now;
                var m = await _context.CaiDatHeThongs.FirstOrDefaultAsync();
                var phong = await _context.PhongTros.FirstOrDefaultAsync(p => p.MaPhong == model.MaPhong);
                var hopdong = await _context.HopDongs.FirstOrDefaultAsync(h => h.MaPhong == model.MaPhong);
                var hoaDon = new HoaDonTienIch
                {
                    MaPhong = model.MaPhong,
                    Thang = date.Month,
                    Nam = date.Year,
                    SoDien = model.SoDien,
                    SoNuoc = model.SoNuoc,
                    DonGiaDien = m.TienDien ?? 0,
                    DonGiaNuoc = m.TienNuoc ?? 0,
                    Soxe = hopdong.SoXe,
                    Phidv = m.Phidv ?? 0,
                    TongTien = phong.Gia + ((decimal)model.SoDien * m.TienDien) + ((decimal)model.SoNuoc * m.TienNuoc) + ((decimal)hopdong.SoXe * m.PhiGiuXe) + m.Phidv,
                    DaThanhToan = false,
                };

                _context.HoaDonTienIches.Add(hoaDon);
                await _context.SaveChangesAsync();
                var hdct = await _context.HoaDonTienIches.FirstOrDefaultAsync(h => h.MaPhong == model.MaPhong && h.Thang == date.Month && h.Nam == date.Year);

                var hoaDonTong = new HoaDonTong
                {
                    MaHopDong = hopdong.MaHopDong,
                    NgayXuat = DateOnly.FromDateTime(date),
                    TongTien = hdct.TongTien,
                    GhiChu = model.note
                };

                _context.HoaDonTongs.Add(hoaDonTong);
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
        public async Task<IActionResult> UpdateHoaDonTienIch(int id, [FromBody] HoaDonTienIchDTO model, bool DaThanhToan = false)
        {
            if (model == null)
                return BadRequest(ApiResponse<object>.CreateError("Dữ liệu không hợp lệ"));

            try
            {
                var hoaDon = await _context.HoaDonTienIches.FindAsync(id);
                if (hoaDon == null)
                    return NotFound(ApiResponse<object>.CreateError("Hóa đơn tiện ích không tồn tại"));
                var hoadowntong = await _context.HoaDonTongs.FirstOrDefaultAsync(h => h.MaHopDong == hoaDon.MaHoaDon);
                hoaDon.SoDien = model.SoDien;
                hoaDon.SoNuoc = model.SoNuoc;
                if (DaThanhToan)
                {
                    hoaDon.DaThanhToan = true;
                }
                decimal tong = 0;
                if (model.SoDien > 0 || model.SoNuoc > 0)
                {
                    if (model.SoNuoc != hoaDon.SoNuoc && model.SoDien != 0)
                    {
                        hoaDon.SoNuoc = model.SoNuoc;

                    }
                    if (model.SoDien != hoaDon.SoDien && model.SoDien != 0)
                    {
                        hoaDon.SoDien = model.SoDien;
                    }
                    decimal tongcu = hoaDon.TongTien - ((decimal)hoaDon.SoDien * hoaDon.DonGiaDien) + ((decimal)hoaDon.SoNuoc * hoaDon.DonGiaNuoc) ?? 0;
                    hoaDon.TongTien = tongcu + ((decimal)model.SoDien * hoaDon.DonGiaDien) + ((decimal)model.SoNuoc * hoaDon.DonGiaNuoc);
                    tong = hoaDon.TongTien ?? 0;
                    hoadowntong.TongTien = tong;
                }



                if (model.note != string.Empty && model.note != null)
                {
                    hoadowntong.GhiChu = model.note;
                }
                _context.HoaDonTienIches.Update(hoaDon);
                _context.HoaDonTongs.Update(hoadowntong);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.CreateSuccess(
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
            try
            {
                var hoaDon = await _context.HoaDonTienIches.FindAsync(id);
                if (hoaDon == null)
                    return NotFound(ApiResponse<object>.CreateError("Hóa đơn tiện ích không tồn tại"));

                // Tìm và xóa hóa đơn tổng liên quan
                var hoaDonTong = await _context.HoaDonTongs
                    .FirstOrDefaultAsync(h => h.MaHopDong == hoaDon.MaHoaDon);
                
                if (hoaDonTong != null)
                {
                    _context.HoaDonTongs.Remove(hoaDonTong);
                }

                _context.HoaDonTienIches.Remove(hoaDon);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.CreateSuccess(
                    "Xóa hóa đơn thành công",
                    null
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa hóa đơn");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi xóa hóa đơn"
                ));
            }
        }
        [HttpGet("get-all-phong")]
        public async Task<IActionResult> GetAllPhong()
        {
            try
            {
                var phongs = await _context.PhongTros
                    .Include(p => p.HopDongs)
                    .Where(p => p.HopDongs.Any(h => h.DaKetThuc == false))
                    .Select(p => new
                    {
                        p.MaPhong,
                        p.TenPhong,
                        p.Gia,
                        HopDong = p.HopDongs.FirstOrDefault(h => h.DaKetThuc == false)
                    })
                    .ToListAsync();

                return Ok(ApiResponse<object>.CreateSuccess(
                    "Lấy danh sách phòng thành công",
                    phongs
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phòng");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi lấy danh sách phòng"
                ));
            }
        }
        [HttpGet("get-all-hoa-don")]
        public async Task<IActionResult> GetAllHoaDon()
        {
            try
            {
                var hoaDons = await _context.HoaDonTienIches
                    .Include(h => h.MaPhongNavigation)
                    .Select(h => new
                    {
                        h.MaHoaDon,
                        h.MaPhong,
                        TenPhong = h.MaPhongNavigation.TenPhong,
                        h.Thang,
                        h.Nam,
                        h.SoDien,
                        h.SoNuoc,
                        h.DonGiaDien,
                        h.DonGiaNuoc,
                        h.TongTien,
                        h.DaThanhToan,
                        h.Phidv,
                        h.Soxe
                    })
                    .ToListAsync();

                return Ok(ApiResponse<object>.CreateSuccess(
                    "Lấy danh sách hóa đơn thành công",
                    hoaDons
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách hóa đơn");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi lấy danh sách hóa đơn"
                ));
            }
        }
        [HttpGet("get-hoa-don-by-phong/{maPhong}")]
        public async Task<IActionResult> GetHoaDonByPhong(int maPhong)
        {
            try
            {
                var hoaDons = await _context.HoaDonTienIches
                    .Include(h => h.MaPhongNavigation)
                    .Where(h => h.MaPhong == maPhong)
                    .Select(h => new
                    {
                        h.MaHoaDon,
                        h.MaPhong,
                        TenPhong = h.MaPhongNavigation.TenPhong,
                        h.Thang,
                        h.Nam,
                        h.SoDien,
                        h.SoNuoc,
                        h.DonGiaDien,
                        h.DonGiaNuoc,
                        h.TongTien,
                        h.DaThanhToan,
                        h.Phidv,
                        h.Soxe
                    })
                    .ToListAsync();

                return Ok(ApiResponse<object>.CreateSuccess(
                    "Lấy danh sách hóa đơn theo phòng thành công",
                    hoaDons
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách hóa đơn theo phòng");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi lấy danh sách hóa đơn theo phòng"
                ));
            }
        }
        [HttpGet("get-hoa-don-by-khach-hang/{maKhachHang}")]
        public async Task<IActionResult> GetHoaDonByKhachHang(int maKhachHang)
        {
            try
            {
                // Lấy danh sách phòng mà khách hàng đang thuê
                var phongs = await _context.HopDongNguoiThues
                    .Include(h => h.MaHopDongNavigation)
                    .Where(h => h.MaKhachThue == maKhachHang && h.MaHopDongNavigation.DaKetThuc == false)
                    .Select(h => h.MaHopDongNavigation.MaPhong)
                    .ToListAsync();

                // Lấy danh sách hóa đơn của các phòng đó
                var hoaDons = await _context.HoaDonTienIches
                    .Include(h => h.MaPhongNavigation)
                    .Where(h => phongs.Contains(h.MaPhong))
                    .Select(h => new
                    {
                        h.MaHoaDon,
                        h.MaPhong,
                        TenPhong = h.MaPhongNavigation.TenPhong,
                        h.Thang,
                        h.Nam,
                        h.SoDien,
                        h.SoNuoc,
                        h.DonGiaDien,
                        h.DonGiaNuoc,
                        h.TongTien,
                        h.DaThanhToan,
                        h.Phidv,
                        h.Soxe
                    })
                    .ToListAsync();

                return Ok(ApiResponse<object>.CreateSuccess(
                    "Lấy danh sách hóa đơn của khách hàng thành công",
                    hoaDons
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách hóa đơn của khách hàng");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi lấy danh sách hóa đơn của khách hàng"
                ));
            }
        }
        [HttpGet("print-hoa-don/{id}")]
        public async Task<IActionResult> PrintHoaDon(int id)
        {
            try
            {
                var hoaDon = await _context.HoaDonTienIches
                    .Include(h => h.MaPhongNavigation)
                    .ThenInclude(p => p.HopDongs)
                    .ThenInclude(h => h.HopDongNguoiThues)
                    .ThenInclude(h => h.MaKhachThueNavigation)
                    .FirstOrDefaultAsync(h => h.MaHoaDon == id);

                if (hoaDon == null)
                    return NotFound(ApiResponse<object>.CreateError("Hóa đơn không tồn tại"));

                // Tạo dữ liệu cho hóa đơn
                var data = new
                {
                    MaHoaDon = hoaDon.MaHoaDon,
                    TenPhong = hoaDon.MaPhongNavigation.TenPhong,
                    ThangNam = $"{hoaDon.Thang}/{hoaDon.Nam}",
                    NgayXuat = DateTime.Now.ToString("dd/MM/yyyy"),
                    KhachHang = hoaDon.MaPhongNavigation.HopDongs
                        .FirstOrDefault()?.HopDongNguoiThues
                        .Select(h => h.MaKhachThueNavigation.HoTen)
                        .FirstOrDefault(),
                    SoDien = hoaDon.SoDien ?? 0,
                    DonGiaDien = hoaDon.DonGiaDien ?? 0,
                    ThanhTienDien = (decimal)(hoaDon.SoDien ?? 0) * (hoaDon.DonGiaDien ?? 0),
                    SoNuoc = hoaDon.SoNuoc ?? 0,
                    DonGiaNuoc = hoaDon.DonGiaNuoc ?? 0,
                    ThanhTienNuoc = (decimal)(hoaDon.SoNuoc ?? 0) * (hoaDon.DonGiaNuoc ?? 0),
                    TienPhong = hoaDon.MaPhongNavigation.Gia ?? 0,
                    Phidv = hoaDon.Phidv,
                    Soxe = hoaDon.Soxe,
                    TongTien = hoaDon.TongTien ?? 0,
                    DaThanhToan = (hoaDon.DaThanhToan ?? false) ? "Đã thanh toán" : "Chưa thanh toán"
                };

                return Ok(ApiResponse<object>.CreateSuccess(
                    "Lấy thông tin hóa đơn thành công",
                    data
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin hóa đơn");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi lấy thông tin hóa đơn"
                ));
            }
        }
        public class HoaDonTienIchDTO
        {
            public int MaPhong { get; set; }
            public double SoDien { get; set; }
            public double SoNuoc { get; set; }
            public string note { get; set; }
        }
    }
}
