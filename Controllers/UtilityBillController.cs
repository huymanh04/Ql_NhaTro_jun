using Api_Ql_nhatro.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ql_NhaTro_jun.Models;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;

namespace Ql_NhaTro_jun.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilityBillController : ControllerBase
    {
        private readonly QlNhatroContext _context;
        private readonly ILogger<UtilityBillController> _logger;

        public UtilityBillController(QlNhatroContext context, ILogger<UtilityBillController> logger)
        {
            _context = context;
            _logger = logger;
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
                var hopDong = await _context.HopDongs.FirstOrDefaultAsync(h => h.MaPhong == hoaDon.MaPhong && h.DaKetThuc == false);

                var hoadowntong = await _context.HoaDonTongs
           .Where(h => h.MaHopDong == hopDong.MaHopDong)
           .Where(h => h.NgayXuat.Value.Month == hoaDon.Thang)
           .Where(h => h.NgayXuat.Value.Year == hoaDon.Nam)
           .FirstOrDefaultAsync();
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

        [HttpPut("update-payment-status/{id}")]
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
        [HttpDelete("delete-hoadon-tienich/{id}")]
        public async Task<IActionResult> DeleteHoaDonTienIch(int id)
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
                    .Select(p => new PhongWithHopDongDto
                    {
                        MaPhong = p.MaPhong,
                        TenPhong = p.TenPhong,
                        Gia = (decimal)p.Gia,
                        HopDong = p.HopDongs
                            .Where(h => (bool)!h.DaKetThuc)
                            .Select(h => new HopDongDto
                            {
                                Id = h.MaHopDong,
                                NgayBatDau = (DateTime)(h.NgayBatDau.HasValue
                                    ? h.NgayBatDau.Value.ToDateTime(TimeOnly.MinValue)
                                    : (DateTime?)null),
                                NgayKetThuc = h.NgayKetThuc.HasValue
                                    ? h.NgayKetThuc.Value.ToDateTime(TimeOnly.MinValue)
                                    : (DateTime?)null,
                                DaKetThuc = (bool)h.DaKetThuc
                            })
                            .FirstOrDefault()
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
                // Lấy cài đặt hệ thống
                var caiDat = await _context.CaiDatHeThongs.FirstOrDefaultAsync();

                var hoaDons = await _context.HoaDonTienIches
                    .Include(h => h.MaPhongNavigation)
                    .ThenInclude(p => p.HopDongs)
                    .Select(h => new
                    {
                        h.MaHoaDon,
                        h.MaPhong,
                        tenPhong = h.MaPhongNavigation.TenPhong,
                        giaphong=h.MaPhongNavigation.Gia,
                        h.Thang,
                        h.Nam,
                        h.SoDien,
                        h.SoNuoc,
                        h.TongTien,
                        h.DaThanhToan,
                        phidv = caiDat.Phidv ?? 0,
                        soxe = h.MaPhongNavigation.HopDongs
                            .Where(hd => hd.DaKetThuc == false)
                            .Select(hd => hd.SoXe)
                            .FirstOrDefault(),
                        phiGiuXe = caiDat.PhiGiuXe ?? 0,
                        donGiaDien = caiDat.TienDien ?? 0,
                        donGiaNuoc = caiDat.TienNuoc ?? 0,
                        dongiaxe=caiDat.PhiGiuXe ?? 0
                    })
                    .ToListAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Lấy danh sách hóa đơn thành công",
                    Data = hoaDons
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách hóa đơn");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Lỗi khi lấy danh sách hóa đơn",
                    Data = null
                });
            }
        }
        [HttpGet("get-hoa-don-by-hoa-don-tong/{maPhong}")]
        public async Task<IActionResult> GetHoaDonByPhong(int maPhong)
        {
            try
            {
                var mkt=await _context.HopDongNguoiThues.FirstOrDefaultAsync(m=>m.MaKhachThue==JunTech.id);
                if (mkt == null)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Khách hàng không có hợp đồng thuê phòng",
                        Data = null
                    });
                }
                   var hopdong = await _context.HopDongs
                    .Where(h => h.MaHopDong == mkt.MaHopDong)
                    .FirstOrDefaultAsync();

                var hoaDons = await _context.HoaDonTienIches
                    .Include(h => h.MaPhongNavigation)
                    .Where(h => h.MaPhong == hopdong.MaPhong)
                    .Select(h => new
                    {
                        h.MaHoaDon,
                        h.MaPhong,
                        tenPhong = h.MaPhongNavigation.TenPhong,
                        h.Thang,
                        h.Nam,
                        h.SoDien,
                        h.SoNuoc,
                        h.TongTien,
                        h.DaThanhToan,
                        Phidv = JunTech.caidat.Phidv ?? 0,
                        Soxe = hopdong.SoXe,

                    })
                    .ToListAsync();

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Lấy danh sách hóa đơn theo phòng thành công",
                    Data = hoaDons
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách hóa đơn theo phòng");
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = "Lỗi khi lấy danh sách hóa đơn theo phòng",
                    Data = null
                });
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
                        Phidv = JunTech.caidat.Phidv ?? 0,
                        Soxe = h.MaPhongNavigation.HopDongs.FirstOrDefault(g=>g.MaPhong==h.MaPhong).SoXe,
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

        [HttpGet("print-hoa-don-beautiful/{id}")]
        public async Task<IActionResult> PrintHoaDonBeautiful(int id)
        {
            try
            {
                var hoaDon = await _context.HoaDonTienIches
                    .Include(h => h.MaPhongNavigation)
                   .Include(d=>d.MaPhongNavigation)
                    .ThenInclude(p => p.HopDongs)
                    .ThenInclude(h => h.HopDongNguoiThues)
                
                    .ThenInclude(h => h.MaKhachThueNavigation)
                    .FirstOrDefaultAsync(h => h.MaHoaDon == id);

                if (hoaDon == null)
                    return NotFound(ApiResponse<object>.CreateError("Hóa đơn không tồn tại"));

                // Lấy thông tin khách hàng
                var khachHang = hoaDon.MaPhongNavigation.HopDongs
                    .FirstOrDefault()?.HopDongNguoiThues
                    .Select(h => h.MaKhachThueNavigation)
                    .FirstOrDefault();

                // Lấy cài đặt hệ thống
                var caiDat = await _context.CaiDatHeThongs.FirstOrDefaultAsync();
                var nhatro = await _context.NhaTros.Include(t => t.PhongTros).Include(a => a.MaChuTroNavigation).FirstOrDefaultAsync(id=>id.MaNhaTro==hoaDon.MaPhongNavigation.MaNhaTro);
                var request = HttpContext.Request;
                var currentUrl = $"{request.Scheme}://{request.Host}";
               
                var use=await _context.NguoiDungs.Where(m=>m.MaNguoiDung== JunTech.id).FirstOrDefaultAsync();
                // Tạo dữ liệu cho hóa đơn đẹp

                var hopdong = hoaDon.MaPhongNavigation.HopDongs.FirstOrDefault(r => r.MaPhong == hoaDon.MaPhongNavigation.MaPhong);
                var data = new
                {
                    // Thông tin hóa đơn
                    MaHoaDon = hoaDon.MaHoaDon,
                    NgayXuat = DateTime.Now.ToString("dd/MM/yyyy"),
                    ThangNam = $"{hoaDon.Thang}/{hoaDon.Nam}",
                    
                    // Thông tin phòng
                    TenPhong = hoaDon.MaPhongNavigation.TenPhong,
                    MaPhong = hoaDon.MaPhong,
                    GiaPhong = hoaDon.MaPhongNavigation.Gia ?? 0,
                    
                    // Thông tin khách hàng
                    TenKhachHang = khachHang?.HoTen ?? "N/A",
                    SoDienThoai = khachHang?.SoDienThoai ?? "N/A",
                    
                    // Chi tiết tiện ích
                    SoDien = hoaDon.SoDien ?? 0,
                    DonGiaDien = hoaDon.DonGiaDien ?? 0,
                    ThanhTienDien = (decimal)(hoaDon.SoDien ?? 0) * (hoaDon.DonGiaDien ?? 0),
                    
                    SoNuoc = hoaDon.SoNuoc ?? 0,
                    DonGiaNuoc = hoaDon.DonGiaNuoc ?? 0,
                    ThanhTienNuoc = (decimal)(hoaDon.SoNuoc ?? 0) * (hoaDon.DonGiaNuoc ?? 0),
                    
                    // Chi phí khác
                    Phidv = caiDat.Phidv,
                    Soxe = hopdong.SoXe,
                    PhiGiuXe = (decimal)(hopdong.SoXe) * (caiDat != null && caiDat.PhiGiuXe.HasValue ? caiDat.PhiGiuXe.Value : 50000),
                    
                    // Tổng tiền
                    TongTien = hoaDon.TongTien ?? 0,
                    DaThanhToan = (hoaDon.DaThanhToan ?? false) ? "Đã thanh toán" : "Chưa thanh toán",
                    
                    // Thông tin công ty
                    TenCongTy = nhatro.TenNhaTro,
                    DiaChiCongTy = nhatro.DiaChi,
                    SoDienThoaiCongTy = nhatro.MaChuTroNavigation.SoDienThoai,
                    EmailCongTy = nhatro.MaChuTroNavigation.Email,
                    Website = currentUrl,
                    nguoilaphoadon= ChuyenKhongDau(nhatro.MaChuTroNavigation.HoTen) +"\r\n"+nhatro.MaChuTroNavigation.HoTen,
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
        public static string ChuyenKhongDau(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Bỏ dấu
            string normalized = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }

            string khongDau = sb.ToString().Normalize(NormalizationForm.FormC);

            // Bỏ khoảng trắng và ký tự đặc biệt
            khongDau = Regex.Replace(khongDau, @"\s+", ""); // bỏ khoảng trắng
            khongDau = Regex.Replace(khongDau, @"[^a-zA-Z0-9]", ""); // bỏ ký tự đặc biệt nếu cần

            // Chuyển về chữ thường
            return khongDau.ToLower();
        }

        public class HoaDonTienIchDTO
        {
            public int MaPhong { get; set; }
            public double SoDien { get; set; }
            public double SoNuoc { get; set; }
            public string note { get; set; }
        }
        public class PhongWithHopDongDto
        {
            public int MaPhong { get; set; }
            public string TenPhong { get; set; }
            public decimal Gia { get; set; }
            public HopDongDto HopDong { get; set; }
        }

        public class HopDongDto
        {
            public int Id { get; set; }
            public DateTime NgayBatDau { get; set; }
            public DateTime? NgayKetThuc { get; set; }
            public bool DaKetThuc { get; set; }
        }

    }
}
