using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Security.Claims;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Ql_NhaTro_jun.Models
{
    public class UserInfo
    {
        private readonly RequestDelegate _next;


        public UserInfo(RequestDelegate next)
        {
            _next = next;

        }

        public async Task Invoke(HttpContext context)
        {
            var _context = context.RequestServices.GetRequiredService<QlNhatroContext>();
            var path = context.Request.Path;

            // Chỉ xử lý các trang chính, bỏ qua static file, login, v.v.
            if (!path.StartsWithSegments("/api") && !path.StartsWithSegments("/css") && !path.StartsWithSegments("/js"))
            {
                try
                {
                    NguoiDung? currentUser = null;

                    var userIdClaim = context.User.FindFirst("MaNguoiDung")?.Value;
                    if (int.TryParse(userIdClaim, out var userId))
                    {
                        currentUser = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.MaNguoiDung == userId);
                    }

                    if (currentUser == null && context.User.Identity?.IsAuthenticated == true)
                    {
                        var loginName = context.User.Identity?.Name;
                        if (!string.IsNullOrWhiteSpace(loginName))
                        {
                            currentUser = await _context.NguoiDungs
                                .FirstOrDefaultAsync(u => u.SoDienThoai == loginName || u.Email == loginName);
                        }
                    }

                    if (currentUser != null)
                    {
                        context.Items["CurrentUser"] = currentUser.HoTen;
                        context.Items["Email"] = currentUser.Email;
                        context.Items["role"] = currentUser.VaiTro;
                        context.Items["id"] = currentUser.MaNguoiDung;
                        JunTech.id = currentUser.MaNguoiDung;
                        JunTech.nguoiDung = currentUser;
                    }
                }
                catch
                {
                }

          
            }
            var homNay = DateOnly.FromDateTime(DateTime.Today);

            // Lấy toàn bộ hợp đồng 1 lần
            var hopDongs = await _context.HopDongs.ToListAsync();

            // Cập nhật trạng thái kết thúc cho từng hợp đồng
            foreach (var hd in hopDongs)
            {
                // Nếu hợp đồng đã được đánh dấu là đã kết thúc thì giữ nguyên, còn nếu chưa thì kiểm tra theo ngày kết thúc
                bool isContractManuallyCompleted = hd.DaKetThuc ?? false;
                bool isContractExpiredByDate = hd.NgayKetThuc < homNay;
                hd.DaKetThuc = isContractManuallyCompleted || isContractExpiredByDate;
            }

            // Cập nhật danh sách phòng có hợp đồng còn hiệu lực
            var danhSachPhongCoHopDongConHieuLuc = hopDongs
                .Where(h => (bool)!h.DaKetThuc) // Còn hiệu lực
                .Select(h => h.MaPhong)
                .Distinct()
                .ToHashSet();

            // Lấy toàn bộ phòng
            var tatCaPhong = await _context.PhongTros.ToListAsync();

            foreach (var phong in tatCaPhong)
            {
                phong.ConTrong = !danhSachPhongCoHopDongConHieuLuc.Contains(phong.MaPhong);
            }

            // Cập nhật và lưu thay đổi
            _context.HopDongs.UpdateRange(hopDongs);
            _context.PhongTros.UpdateRange(tatCaPhong);

            await _context.SaveChangesAsync();

            JunTech.caidat = await _context.CaiDatHeThongs.FirstOrDefaultAsync();
            await _next(context);
        }
    }
    
}
