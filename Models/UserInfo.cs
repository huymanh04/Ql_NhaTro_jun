using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
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
            var request = context.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";

            // Chỉ xử lý các trang chính, bỏ qua static file, login, v.v.
            if (!path.StartsWithSegments("/api") && !path.StartsWithSegments("/css") && !path.StartsWithSegments("/js"))
            {
                var handler = new HttpClientHandler
                {
                    UseCookies = true,
                    CookieContainer = new CookieContainer()
                };
               
                foreach (var cookie in context.Request.Cookies)
                {
                    handler.CookieContainer.Add(new Uri(baseUrl), new System.Net.Cookie(cookie.Key, cookie.Value));
                }

                using var client = new HttpClient(handler);
                client.BaseAddress = new Uri(baseUrl);

                try
                {
                    var response = await client.GetAsync("/api/Auth/get-user-info");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var user = Regex.Match(json, @"""hoTen"":""(.*?)""").Groups[1].Value;
                        var Email = Regex.Match(json, @"""email"":""(.*?)""").Groups[1].Value;
                        context.Items["CurrentUser"] = user;
                        context.Items["Email"] = Email;

                        var manh = await _context.NguoiDungs.FirstOrDefaultAsync(t => t.Email == Email);
                        if (manh != null)
                        {
                            context.Items["role"] = manh.VaiTro;
                            context.Items["id"] = manh.MaNguoiDung;
                            JunTech.id = manh.MaNguoiDung;
                            JunTech.nguoiDung = manh;
                        }
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
                hd.DaKetThuc = hd.NgayKetThuc < homNay;
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
