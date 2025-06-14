using Api_Ql_nhatro.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Ql_NhaTro_jun.Models;
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Ql_NhaTro_jun.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ILogger<AdminController> _logger;
        QlNhatroContext _context;
        public AdminController(ILogger<AdminController> logger, QlNhatroContext context)
        {
            _logger = logger;
            _context = context;
        }
        [HttpGet("doarboard")]
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
                if (user.VaiTro == "0") // Kiểm tra quyền người dùng
                {
                    return BadRequest(ApiResponse<object>.CreateError("Bạn không có quyền thực hiện hành động này"));
                }
                #endregion
                var soHopDong = await _context.HopDongs.CountAsync();
                var soPhong = await _context.PhongTros.CountAsync();
                var soKhachHang = await _context.NguoiDungs.Where(t => t.VaiTro == "0").CountAsync();
                var soNhaTro = await _context.NhaTros.CountAsync();
                var soTinh = await _context.TinhThanhs.CountAsync();
                var soKhuVuc = await _context.KhuVucs.CountAsync();
                var soLoaiPhong = await _context.TheLoaiPhongTros.CountAsync();
                var soBanner = await _context.Banners.CountAsync();
                var soBank = await _context.Banks.CountAsync();
                var soTinNhan = await _context.TinNhans.CountAsync();
                var sophongTrong = await _context.PhongTros.CountAsync(t => t.ConTrong == true);
                var sophongDaThue = await _context.PhongTros.CountAsync(t => t.ConTrong == false);
                var soDenBu=await _context.DenBus.CountAsync();
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

                var truyVan = _context.HoaDonTongs.AsQueryable();

                // Chuyển sang DateOnly
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
                    .Where(h => h.Thang == thangNay && h.Nam == namNay)
                    .SumAsync(h => (decimal?)((decimal)h.SoDien * h.DonGiaDien)) ?? 0;

                // Tiền điện tháng trước
                var tongTienDienThangTruoc = await _context.HoaDonTienIches
                    .Where(h => h.Thang == thangTruoc && h.Nam == namTruoc)
                    .SumAsync(h => (decimal?)((decimal)h.SoDien * h.DonGiaDien)) ?? 0;

                // Tỷ lệ thay đổi (%)
                decimal tyLeThayDoi = tongTienDienThangTruoc == 0
                    ? 0
                    : ((tongTienDienThangNay - tongTienDienThangTruoc) / tongTienDienThangTruoc) * 100;
                #endregion
                var ketQua = new Doarboard
                {
                    SoHopDong = soHopDong ,
                    SoDenBu = soDenBu,
                    SoPhong = soPhong,
                    SoKhachHang = soKhachHang,
                    SoNhaTro = soNhaTro,
                    SoTinh = soTinh,
                    SoKhuVuc = soKhuVuc,
                    SoLoaiPhong = soLoaiPhong,
                    SoBanner = soBanner,
                    SoBank = soBank,
                    SoTinNhan = soTinNhan,
                    SophongTrong = sophongTrong,
                    SophongDaThue = sophongDaThue,
                    HomNay = await truyVan.Where(x => x.NgayXuat == homNayDateOnly).SumAsync(x => (decimal?)x.TongTien) ?? 0,
                    HomQua = await truyVan.Where(x => x.NgayXuat == homQuaDateOnly).SumAsync(x => (decimal?)x.TongTien) ?? 0,
                    TuanNay = await truyVan.Where(x => x.NgayXuat >= dauTuanDateOnly && x.NgayXuat <= homNayDateOnly).SumAsync(x => (decimal?)x.TongTien) ?? 0,
                    TuanTruoc = await truyVan.Where(x => x.NgayXuat >= dauTuanTruocDateOnly && x.NgayXuat <= cuoiTuanTruocDateOnly).SumAsync(x => (decimal?)x.TongTien) ?? 0,
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
                ketQua.TyLeThayDoiHomNay = ketQua.HomQua == 0 ? 0 : ((ketQua.HomNay - ketQua.HomQua) / ketQua.HomQua) * 100;
                ketQua.TyLeThayDoiTuanNay = ketQua.TuanTruoc == 0 ? 0 : ((ketQua.TuanNay - ketQua.TuanTruoc) / ketQua.TuanTruoc) * 100;
                ketQua.TyLeThayDoiThangNay = ketQua.ThangTruoc == 0 ? 0 : ((ketQua.ThangNay - ketQua.ThangTruoc) / ketQua.ThangTruoc) * 100;
                ketQua.TyLeThayDoiNamNay = ketQua.NamTruoc == 0 ? 0 : ((ketQua.NamNay - ketQua.NamTruoc) / ketQua.NamTruoc) * 100;
                ketQua.TyLeThayDoidien = ketQua.Dienthangtruoc == 0 ? 0 : ((ketQua.Dienthangnay - ketQua.Dienthangtruoc) / ketQua.Dienthangtruoc) * 100;
                ketQua.TyLeThayDoinuoc = ketQua.Nuocthangtruoc == 0 ? 0 : ((ketQua.Nuocthangnay - ketQua.Nuocthangtruoc) / ketQua.Nuocthangtruoc) * 100;

                return Ok(ApiResponse<object>.CreateSuccess( "Lấy Kết quả thành công", ketQua));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching number of contracts");
                return StatusCode(500, "Internal server error");
            }
        }


        public class Doarboard
        {
            public int SoHopDong { get; set; }
            public int SoDenBu { get; set; }
            public int SoPhong { get; set; }
            public int SoKhachHang { get; set; }
            public int SoNhaTro { get; set; }
            public int SoTinh { get; set; }
            public int SoKhuVuc { get; set; }
            public int SoLoaiPhong { get; set; }
            public int SoBanner { get; set; }
            public int SoBank { get; set; }
            public int SoTinNhan { get; set; }
            public int SophongTrong { get; set; }
            public int SophongDaThue { get; set; }




            public decimal HomNay { get; set; }
            public decimal HomQua { get; set; }
            public decimal TyLeThayDoiHomNay { get; set; }

            public decimal TuanNay { get; set; }
            public decimal TuanTruoc { get; set; }
            public decimal TyLeThayDoiTuanNay { get; set; }

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


    }
}
