using Api_Ql_nhatro.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Ql_NhaTro_jun.Models;
using System.Globalization;
using System.Text;
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

        [HttpGet("RecentActivities")]
        public async Task<IActionResult> GetRecentActivities()
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

                var activities = new List<ActivityItem>();

                // Lấy hợp đồng mới (7 ngày gần đây)
                var contractsData = await _context.HopDongs
                    .Include(h => h.MaPhongNavigation)
                    .Where(h => h.NgayBatDau >= DateOnly.FromDateTime(DateTime.Now.AddDays(-7)))
                    .OrderByDescending(h => h.NgayBatDau)
                    .Take(5)
                    .ToListAsync();

                foreach (var contract in contractsData)
                {
                    activities.Add(new ActivityItem
                    {
                        id = contract.MaHopDong,
                        type = "contract",
                        icon = "fas fa-handshake",
                        iconClass = "success",
                        title = "Hợp đồng mới được ký",
                        description = $"Hợp đồng thuê phòng {contract.MaPhongNavigation?.TenPhong ?? "N/A"} đã được ký",
                        time = GetTimeAgo(contract.NgayBatDau?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now),
                        timestamp = contract.NgayBatDau?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now
                    });
                }

                // Lấy hóa đơn được thanh toán gần đây
                var paymentsData = await _context.HoaDonTongs
                    .Include(h => h.MaHopDongNavigation)
                    .ThenInclude(hd => hd.MaPhongNavigation)
                    .Where(h => h.NgayXuat >= DateOnly.FromDateTime(DateTime.Now.AddDays(-7)))
                    .OrderByDescending(h => h.NgayXuat)
                    .Take(5)
                    .ToListAsync();

                foreach (var payment in paymentsData)
                {
                    activities.Add(new ActivityItem
                    {
                        id = payment.MaHoaDon,
                        type = "payment",
                        icon = "fas fa-credit-card",
                        iconClass = "info",
                        title = "Thanh toán hóa đơn",
                        description = $"Hóa đơn phòng {payment.MaHopDongNavigation?.MaPhongNavigation?.TenPhong ?? "N/A"} đã được thanh toán {payment.TongTien:N0} VNĐ",
                        time = GetTimeAgo(payment.NgayXuat?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now),
                        timestamp = payment.NgayXuat?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now
                    });
                }

                // Lấy đền bù gần đây
                var maintenanceData = await _context.DenBus
                    .Include(d => d.MaHopDongNavigation)
                    .ThenInclude(hd => hd.MaPhongNavigation)
                    .Where(d => d.NgayTao >= DateTime.Now.AddDays(-7))
                    .OrderByDescending(d => d.NgayTao)
                    .Take(3)
                    .ToListAsync();

                foreach (var maintenance in maintenanceData)
                {
                    activities.Add(new ActivityItem
                    {
                        id = maintenance.MaDenBu,
                        type = "maintenance",
                        icon = "fas fa-tools",
                        iconClass = "warning",
                        title = "Yêu cầu đền bù",
                        description = $"Phòng {maintenance.MaHopDongNavigation?.MaPhongNavigation?.TenPhong ?? "N/A"} có yêu cầu đền bù: {maintenance.NoiDung}",
                        time = GetTimeAgo(maintenance.NgayTao ?? DateTime.Now),
                        timestamp = maintenance.NgayTao ?? DateTime.Now
                    });
                }

                // Lấy hợp đồng sắp hết hạn
                var expiringData = await _context.HopDongs
                    .Include(h => h.MaPhongNavigation)
                    .Where(h => h.NgayKetThuc <= DateOnly.FromDateTime(DateTime.Now.AddDays(7)) && h.NgayKetThuc >= DateOnly.FromDateTime(DateTime.Now))
                    .OrderBy(h => h.NgayKetThuc)
                    .Take(3)
                    .ToListAsync();

                foreach (var expiring in expiringData)
                {
                    var daysLeft = (expiring.NgayKetThuc?.ToDateTime(TimeOnly.MinValue) - DateTime.Now)?.Days ?? 0;
                    activities.Add(new ActivityItem
                    {
                        id = expiring.MaHopDong,
                        type = "contract",
                        icon = "fas fa-file-contract",
                        iconClass = "danger",
                        title = "Hợp đồng sắp hết hạn",
                        description = $"Hợp đồng phòng {expiring.MaPhongNavigation?.TenPhong ?? "N/A"} sẽ hết hạn vào {expiring.NgayKetThuc:dd/MM/yyyy}",
                        time = $"Còn {daysLeft} ngày",
                        timestamp = expiring.NgayKetThuc?.ToDateTime(TimeOnly.MinValue) ?? DateTime.Now
                    });
                }

                // Lấy khách hàng mới đăng ký
                var customersData = await _context.NguoiDungs
                    .Where(n => n.VaiTro == "0")
                    .OrderByDescending(n => n.MaNguoiDung)
                    .Take(3)
                    .ToListAsync();

                foreach (var customer in customersData)
                {
                    activities.Add(new ActivityItem
                    {
                        id = customer.MaNguoiDung,
                        type = "customer",
                        icon = "fas fa-user-plus",
                        iconClass = "success",
                        title = "Khách hàng mới đăng ký",
                        description = $"{customer.HoTen} đã đăng ký tài khoản với email {customer.Email}",
                        time = "Vừa đăng ký",
                        timestamp = DateTime.Now.AddDays(-1)
                    });
                }

                // Sắp xếp theo thời gian và lấy 10 hoạt động gần nhất
                var sortedActivities = activities
                    .OrderByDescending(a => a.timestamp)
                    .Take(10)
                    .ToList();

                return Ok(ApiResponse<object>.CreateSuccess("Lấy hoạt động gần đây thành công", sortedActivities));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching recent activities");
                return StatusCode(500, "Internal server error");
            }
        }

        public class ActivityItem
        {
            public int id { get; set; }
            public string type { get; set; }
            public string icon { get; set; }
            public string iconClass { get; set; }
            public string title { get; set; }
            public string description { get; set; }
            public string time { get; set; }
            public DateTime timestamp { get; set; }
        }

        private string GetTimeAgo(DateTime dateTime)
        {
            var timeSpan = DateTime.Now - dateTime;
            
            if (timeSpan.TotalMinutes < 1)
                return "Vừa xong";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} phút trước";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} giờ trước";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} ngày trước";
            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)} tuần trước";
            
            return dateTime.ToString("dd/MM/yyyy");
        }

        [HttpGet("ExportReport")]
        public async Task<IActionResult> ExportReport()
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
                if (user.VaiTro == "0")
                {
                    return BadRequest(ApiResponse<object>.CreateError("Bạn không có quyền thực hiện hành động này"));
                }
                #endregion

                // Lấy dữ liệu dashboard
                var dashboardResponse = await doarboard();
                var dashboardResult = dashboardResponse as OkObjectResult;
                var dashboardData = ((ApiResponse<object>)dashboardResult.Value).Data as Doarboard;

                // Tạo HTML cho PDF
                var html = GenerateReportHtml(dashboardData);

                // Chuyển đổi HTML thành PDF (sử dụng thư viện như iTextSharp hoặc DinkToPdf)
                var pdfBytes = ConvertHtmlToPdf(html);

                var fileName = $"BaoCao_Dashboard_{DateTime.Now:yyyyMMdd_HHmmss}.html";
                
                return File(pdfBytes, "text/html", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting report");
                return StatusCode(500, "Internal server error");
            }
        }

        private string GenerateReportHtml(Doarboard data)
        {
            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Báo Cáo Dashboard</title>
    <style>
        body {{ 
            font-family: 'Times New Roman', serif; 
            margin: 20px; 
            color: #333;
            line-height: 1.6;
        }}
        .header {{ 
            text-align: center; 
            margin-bottom: 30px; 
            border-bottom: 2px solid #4a90a4;
            padding-bottom: 20px;
        }}
        .header h1 {{ 
            color: #4a90a4; 
            font-size: 28px; 
            margin: 0;
        }}
        .header p {{ 
            color: #666; 
            font-size: 14px; 
            margin: 5px 0;
        }}
        .section {{ 
            margin-bottom: 25px; 
            page-break-inside: avoid;
        }}
        .section h2 {{ 
            color: #2c5f6f; 
            border-bottom: 1px solid #ddd; 
            padding-bottom: 5px;
            font-size: 18px;
        }}
        .stats-grid {{ 
            display: grid; 
            grid-template-columns: repeat(4, 1fr); 
            gap: 15px; 
            margin-bottom: 20px;
        }}
        .stat-item {{ 
            border: 1px solid #ddd; 
            padding: 15px; 
            text-align: center; 
            border-radius: 8px;
            background: #f9f9f9;
        }}
        .stat-number {{ 
            font-size: 24px; 
            font-weight: bold; 
            color: #4a90a4; 
            display: block;
        }}
        .stat-label {{ 
            font-size: 12px; 
            color: #666; 
            margin-top: 5px;
        }}
        .revenue-table {{ 
            width: 100%; 
            border-collapse: collapse; 
            margin-top: 15px;
        }}
        .revenue-table th, .revenue-table td {{ 
            border: 1px solid #ddd; 
            padding: 10px; 
            text-align: right;
        }}
        .revenue-table th {{ 
            background-color: #4a90a4; 
            color: white; 
            text-align: center;
        }}
        .positive {{ color: #10b981; }}
        .negative {{ color: #ef4444; }}
        .footer {{ 
            margin-top: 40px; 
            text-align: center; 
            font-size: 12px; 
            color: #666;
            border-top: 1px solid #ddd;
            padding-top: 20px;
        }}
        .utility-section {{
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 20px;
            margin-top: 15px;
        }}
        .utility-item {{
            border: 1px solid #ddd;
            padding: 15px;
            border-radius: 8px;
            background: #f9f9f9;
        }}
        .utility-item h3 {{
            margin: 0 0 10px 0;
            color: #2c5f6f;
            font-size: 16px;
        }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>BÁO CÁO DASHBOARD HỆ THỐNG QUẢN LÝ NHÀ TRỌ</h1>
        <p>Ngày xuất báo cáo: {DateTime.Now:dd/MM/yyyy HH:mm:ss}</p>
        <p>Người xuất: Quản trị viên</p>
    </div>

    <div class='section'>
        <h2>📊 THỐNG KÊ TỔNG QUAN</h2>
        <div class='stats-grid'>
            <div class='stat-item'>
                <span class='stat-number'>{data.SoHopDong:N0}</span>
                <div class='stat-label'>Hợp Đồng</div>
            </div>
            <div class='stat-item'>
                <span class='stat-number'>{data.SoPhong:N0}</span>
                <div class='stat-label'>Tổng Phòng</div>
            </div>
            <div class='stat-item'>
                <span class='stat-number'>{data.SoKhachHang:N0}</span>
                <div class='stat-label'>Khách Hàng</div>
            </div>
            <div class='stat-item'>
                <span class='stat-number'>{data.SoNhaTro:N0}</span>
                <div class='stat-label'>Nhà Trọ</div>
            </div>
            <div class='stat-item'>
                <span class='stat-number'>{data.SophongDaThue:N0}</span>
                <div class='stat-label'>Phòng Đã Thuê</div>
            </div>
            <div class='stat-item'>
                <span class='stat-number'>{data.SophongTrong:N0}</span>
                <div class='stat-label'>Phòng Trống</div>
            </div>
            <div class='stat-item'>
                <span class='stat-number'>{(data.SoPhong > 0 ? (data.SophongDaThue * 100.0 / data.SoPhong) : 0):F1}%</span>
                <div class='stat-label'>Tỷ Lệ Lấp Đầy</div>
            </div>
            <div class='stat-item'>
                <span class='stat-number'>{data.SoDenBu:N0}</span>
                <div class='stat-label'>Đền Bù</div>
            </div>
        </div>
    </div>

    <div class='section'>
        <h2>💰 PHÂN TÍCH DOANH THU</h2>
        <table class='revenue-table'>
            <thead>
                <tr>
                    <th>Thời Gian</th>
                    <th>Doanh Thu (VNĐ)</th>
                    <th>Tỷ Lệ Thay Đổi (%)</th>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td>Hôm nay</td>
                    <td>{data.HomNay:N0}</td>
                    <td class='{(data.TyLeThayDoiHomNay >= 0 ? "positive" : "negative")}'>{data.TyLeThayDoiHomNay:+0.0;-0.0;0.0}%</td>
                </tr>
                <tr>
                    <td>Hôm qua</td>
                    <td>{data.HomQua:N0}</td>
                    <td>-</td>
                </tr>
                <tr>
                    <td>Tuần này</td>
                    <td>{data.TuanNay:N0}</td>
                    <td class='{(data.TyLeThayDoiTuanNay >= 0 ? "positive" : "negative")}'>{data.TyLeThayDoiTuanNay:+0.0;-0.0;0.0}%</td>
                </tr>
                <tr>
                    <td>Tuần trước</td>
                    <td>{data.TuanTruoc:N0}</td>
                    <td>-</td>
                </tr>
                <tr>
                    <td>Tháng này</td>
                    <td>{data.ThangNay:N0}</td>
                    <td class='{(data.TyLeThayDoiThangNay >= 0 ? "positive" : "negative")}'>{data.TyLeThayDoiThangNay:+0.0;-0.0;0.0}%</td>
                </tr>
                <tr>
                    <td>Tháng trước</td>
                    <td>{data.ThangTruoc:N0}</td>
                    <td>-</td>
                </tr>
                <tr>
                    <td>Năm này</td>
                    <td>{data.NamNay:N0}</td>
                    <td class='{(data.TyLeThayDoiNamNay >= 0 ? "positive" : "negative")}'>{data.TyLeThayDoiNamNay:+0.0;-0.0;0.0}%</td>
                </tr>
                <tr>
                    <td>Năm trước</td>
                    <td>{data.NamTruoc:N0}</td>
                    <td>-</td>
                </tr>
            </tbody>
        </table>
    </div>

    <div class='section'>
        <h2>⚡ TIỀN ĐIỆN & NƯỚC</h2>
        <div class='utility-section'>
            <div class='utility-item'>
                <h3>💡 Tiền Điện</h3>
                <p><strong>Tháng này:</strong> {data.Dienthangnay:N0} VNĐ</p>
                <p><strong>Tháng trước:</strong> {data.Dienthangtruoc:N0} VNĐ</p>
                <p><strong>Thay đổi:</strong> <span class='{(data.TyLeThayDoidien >= 0 ? "positive" : "negative")}'>{data.TyLeThayDoidien:+0.0;-0.0;0.0}%</span></p>
            </div>
            <div class='utility-item'>
                <h3>💧 Tiền Nước</h3>
                <p><strong>Tháng này:</strong> {data.Nuocthangnay:N0} VNĐ</p>
                <p><strong>Tháng trước:</strong> {data.Nuocthangtruoc:N0} VNĐ</p>
                <p><strong>Thay đổi:</strong> <span class='{(data.TyLeThayDoinuoc >= 0 ? "positive" : "negative")}'>{data.TyLeThayDoinuoc:+0.0;-0.0;0.0}%</span></p>
            </div>
        </div>
    </div>

    <div class='section'>
        <h2>🏢 THỐNG KÊ HỆ THỐNG</h2>
        <div class='stats-grid'>
            <div class='stat-item'>
                <span class='stat-number'>{data.SoTinh:N0}</span>
                <div class='stat-label'>Tỉnh/Thành Phố</div>
            </div>
            <div class='stat-item'>
                <span class='stat-number'>{data.SoKhuVuc:N0}</span>
                <div class='stat-label'>Khu Vực</div>
            </div>
            <div class='stat-item'>
                <span class='stat-number'>{data.SoLoaiPhong:N0}</span>
                <div class='stat-label'>Loại Phòng</div>
            </div>
            <div class='stat-item'>
                <span class='stat-number'>{data.SoBank:N0}</span>
                <div class='stat-label'>Ngân Hàng</div>
            </div>
            <div class='stat-item'>
                <span class='stat-number'>{data.SoTinNhan:N0}</span>
                <div class='stat-label'>Tin Nhắn</div>
            </div>
            <div class='stat-item'>
                <span class='stat-number'>{data.SoBanner:N0}</span>
                <div class='stat-label'>Banner</div>
            </div>
        </div>
    </div>

    <div class='footer'>
        <p>© 2024 Hệ Thống Quản Lý Nhà Trọ - Báo cáo được tạo tự động</p>
        <p>Liên hệ hỗ trợ: manhcansa04@gmail.com | Hotline: 0349278153</p>
    </div>
</body>
</html>";
            return html;
        }

        private byte[] ConvertHtmlToPdf(string html)
        {
            try
            {
                var tempPath = Path.GetTempFileName() + ".html";
                System.IO.File.WriteAllText(tempPath, html, Encoding.UTF8);
                
                // Đọc lại và trả về dưới dạng bytes
                var htmlBytes = System.IO.File.ReadAllBytes(tempPath);
                
                // Xóa file tạm
                System.IO.File.Delete(tempPath);
                
                return htmlBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting HTML to PDF");
                // Fallback: trả về HTML dưới dạng bytes
                return Encoding.UTF8.GetBytes(html);
            }
        }

        // API quản lý tài khoản
        [HttpGet("Users")]
        public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string search = "", [FromQuery] string roleFilter = "")
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
                if (user.VaiTro == "0")
                {
                    return BadRequest(ApiResponse<object>.CreateError("Bạn không có quyền thực hiện hành động này"));
                }
                #endregion

                var query = _context.NguoiDungs.AsQueryable();

                // Tìm kiếm
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(u => u.HoTen.Contains(search) || 
                                           u.Email.Contains(search) || 
                                           u.SoDienThoai.Contains(search));
                }

                // Lọc theo vai trò
                if (!string.IsNullOrEmpty(roleFilter))
                {
                    query = query.Where(u => u.VaiTro == roleFilter);
                }

                var totalUsers = await query.CountAsync();
                var users = await query
                    .OrderByDescending(u => u.MaNguoiDung)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new
                    {
                        u.MaNguoiDung,
                        u.HoTen,
                        u.Email,
                        u.SoDienThoai,
                        u.VaiTro,
                        VaiTroText = u.VaiTro == "2" ? "Admin" : u.VaiTro == "1" ? "Quản lý" : "Khách hàng",
                    })
                    .ToListAsync();

                var result = new
                {
                    users = users,
                    totalUsers = totalUsers,
                    totalPages = (int)Math.Ceiling((double)totalUsers / pageSize),
                    currentPage = page,
                    pageSize = pageSize
                };

                return Ok(ApiResponse<object>.CreateSuccess("Lấy danh sách người dùng thành công", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching users");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("CreateUser")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
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
                if (user.VaiTro == "0")
                {
                    return BadRequest(ApiResponse<object>.CreateError("Bạn không có quyền thực hiện hành động này"));
                }
                #endregion

                // Kiểm tra email đã tồn tại
                var existingEmail = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == request.Email);
                if (existingEmail != null)
                {
                    return BadRequest(ApiResponse<object>.CreateError("Email đã tồn tại"));
                }

                // Kiểm tra số điện thoại đã tồn tại
                var existingPhone = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.SoDienThoai == request.SoDienThoai);
                if (existingPhone != null)
                {
                    return BadRequest(ApiResponse<object>.CreateError("Số điện thoại đã tồn tại"));
                }

                var newUser = new NguoiDung
                {
                    HoTen = request.HoTen,
                    Email = request.Email,
                    SoDienThoai = request.SoDienThoai,
                    MatKhau = request.MatKhau, // Trong thực tế nên hash password
                    VaiTro = request.VaiTro
                };

                _context.NguoiDungs.Add(newUser);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.CreateSuccess("Tạo tài khoản thành công", new { 
                    MaNguoiDung = newUser.MaNguoiDung,
                    HoTen = newUser.HoTen,
                    Email = newUser.Email,
                    SoDienThoai = newUser.SoDienThoai,
                    VaiTro = newUser.VaiTro
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("UpdateUser/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
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
                if (user.VaiTro == "0")
                {
                    return BadRequest(ApiResponse<object>.CreateError("Bạn không có quyền thực hiện hành động này"));
                }
                #endregion

                var existingUser = await _context.NguoiDungs.FindAsync(id);
                if (existingUser == null)
                {
                    return NotFound(ApiResponse<object>.CreateError("Không tìm thấy người dùng"));
                }

                // Kiểm tra email trùng (trừ chính user đang update)
                var emailExists = await _context.NguoiDungs
                    .FirstOrDefaultAsync(u => u.Email == request.Email && u.MaNguoiDung != id);
                if (emailExists != null)
                {
                    return BadRequest(ApiResponse<object>.CreateError("Email đã tồn tại"));
                }

                // Kiểm tra số điện thoại trùng (trừ chính user đang update)
                var phoneExists = await _context.NguoiDungs
                    .FirstOrDefaultAsync(u => u.SoDienThoai == request.SoDienThoai && u.MaNguoiDung != id);
                if (phoneExists != null)
                {
                    return BadRequest(ApiResponse<object>.CreateError("Số điện thoại đã tồn tại"));
                }

                existingUser.HoTen = request.HoTen;
                existingUser.Email = request.Email;
                existingUser.SoDienThoai = request.SoDienThoai;
                existingUser.VaiTro = request.VaiTro;

                // Chỉ update mật khẩu nếu có mật khẩu mới
                if (!string.IsNullOrEmpty(request.MatKhau))
                {
                    existingUser.MatKhau = request.MatKhau; // Trong thực tế nên hash password
                }

                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.CreateSuccess("Cập nhật thông tin thành công", new {
                    MaNguoiDung = existingUser.MaNguoiDung,
                    HoTen = existingUser.HoTen,
                    Email = existingUser.Email,
                    SoDienThoai = existingUser.SoDienThoai,
                    VaiTro = existingUser.VaiTro
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("SetUserRole/{id}")]
        public async Task<IActionResult> SetUserRole(int id, [FromBody] SetRoleRequest request)
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
                if (user.VaiTro == "0")
                {
                    return BadRequest(ApiResponse<object>.CreateError("Bạn không có quyền thực hiện hành động này"));
                }
                #endregion

                var existingUser = await _context.NguoiDungs.FindAsync(id);
                if (existingUser == null)
                {
                    return NotFound(ApiResponse<object>.CreateError("Không tìm thấy người dùng"));
                }

                existingUser.VaiTro = request.VaiTro;
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.CreateSuccess("Cập nhật quyền thành công", new {
                    MaNguoiDung = existingUser.MaNguoiDung,
                    HoTen = existingUser.HoTen,
                    VaiTro = existingUser.VaiTro,
                    VaiTroText = existingUser.VaiTro == "1" ? "Admin" : "Khách hàng"
                }));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting user role");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("DeleteUser/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
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
                if (user.VaiTro == "0")
                {
                    return BadRequest(ApiResponse<object>.CreateError("Bạn không có quyền thực hiện hành động này"));
                }
                #endregion

                var existingUser = await _context.NguoiDungs.FindAsync(id);
                if (existingUser == null)
                {
                    return NotFound(ApiResponse<object>.CreateError("Không tìm thấy người dùng"));
                }

                // Không cho phép xóa chính mình
                if (existingUser.SoDienThoai == userName || existingUser.Email == userName)
                {
                    return BadRequest(ApiResponse<object>.CreateError("Không thể xóa tài khoản của chính mình"));
                }

                _context.NguoiDungs.Remove(existingUser);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.CreateSuccess("Xóa tài khoản thành công", null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user");
                return StatusCode(500, "Internal server error");
            }
        }

        // Request models
        public class CreateUserRequest
        {
            public string HoTen { get; set; }
            public string Email { get; set; }
            public string SoDienThoai { get; set; }
            public string MatKhau { get; set; }
            public string VaiTro { get; set; }
        }

        public class UpdateUserRequest
        {
            public string HoTen { get; set; }
            public string Email { get; set; }
            public string SoDienThoai { get; set; }
            public string MatKhau { get; set; } // Optional
            public string VaiTro { get; set; }
        }

        public class SetRoleRequest
        {
            public string VaiTro { get; set; }
        }


    }
}
