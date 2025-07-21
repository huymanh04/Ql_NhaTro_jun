using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Ql_NhaTro_jun.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Ql_NhaTro_jun.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankController : ControllerBase
    {
      private readonly ILogger<BankController> _logger;
        private readonly QlNhatroContext _context;
        public BankController(ILogger<BankController> logger, QlNhatroContext context)
        {
            _logger = logger;
            _context = context;
        }
        [HttpPost]
        [Route("/check-status")]
        public async Task<JsonResult> CheckPaymentStatusAsync(string noidung)
        {

            int userId = (int)JunTech.id;
            NguoiDung user = _context.NguoiDungs.Find(userId);

            var url = "https://thueapi.dailysieure.com/api-mbbank";
            var data = new Dictionary<string, string>
    {
        { "taikhoan", "DAMHUYMANH" },
        { "matkhau", "Manh@2005" },
        { "sotaikhoan", "3456686868678" }
    };

            try
            {
                using (var client = new HttpClient())
                using (var content = new FormUrlEncodedContent(data))
                {
                    client.Timeout = TimeSpan.FromSeconds(20);
                    var response = await client.PostAsync(url, content);
                    string responseString = await response.Content.ReadAsStringAsync();

                    JObject obj = JObject.Parse(responseString);
                    var transactions = obj["transactions"]
                        .Where(tx => tx["addDescription"]?.ToString().Contains(noidung) == true);

                    // Nếu không tìm thấy, thử loại bỏ @ hoặc .
                    if (!transactions.Any())
                    {
                        string altNoiDung = noidung.Replace("@", "").Replace(".", "");
                        transactions = obj["transactions"]
                            .Where(tx => tx["addDescription"]?.ToString().Contains(altNoiDung) == true);
                    }

                    foreach (var tx in transactions)
                    {
                        int amount = int.Parse(tx["amount"].ToString());
                        if (amount < 10000) continue;

                        string description = tx["addDescription"]?.ToString()?.Replace(" ", "") ?? "";
                        if (description.Length > 250)
                            description = description.Substring(0, 250);

                        var existed = _context.BankHistories.FirstOrDefault(c => c.TransactionCode == description);
                        if (existed != null) continue;

                        var bank = new BankHistory
                        {
                            Amount = amount,
                            CreatedAt = DateTime.Parse(tx["transactionDate"].ToString()),
                            TransactionCode = description,
                            Note = noidung,
                            BankName = "MB BANK"
                        };

                        var usera = _context.NguoiDungs.FirstOrDefault(u => u.MaNguoiDung == JunTech.id);
                        {
                            bank.MaNguoiDung = usera.MaNguoiDung;
                            var hoadon = _context.HoaDonTienIches.FirstOrDefault(t => t.MaHoaDon == int.Parse(noidung.Replace("#", "")));
                            hoadon.DaThanhToan = true;
                            _context.Update(hoadon);
                            _context.Entry(usera).State = EntityState.Modified;
                            _context.BankHistories.Add(bank);
                            _context.SaveChanges();

                            return new JsonResult(new
                            {
                                success = true,
                                isPaid = true,
                                message = $"Bạn đã thanh toán thành công {amount:N0} VNĐ cho hóa đơn {noidung}"
                            });
                        }
                    }
                }

                return new JsonResult(new { success = false, isPaid = false, message = "Không tìm thấy giao dịch phù hợp" });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, isPaid = false, message = "Lỗi xử lý: " + ex.Message });
            }
        }

        [HttpGet("get-banks")]
        public async Task<IActionResult> GetBanks()
        {
            try
            {
                var banks = await _context.Banks
                    .Select(b => new BankDTO
                    {
                        Id = b.Id,
                        TenNganHang = b.TenNganHang ?? "",
                        SoTaiKhoan = b.SoTaiKhoan ??"",
                        Ten = b.Ten ?? ""
                    })
                    .ToListAsync();

                return Ok(ApiResponse<List<BankDTO>>.CreateSuccess(
                    "Lấy danh sách ngân hàng thành công",
                    banks
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách ngân hàng");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi lấy danh sách ngân hàng"
                ));
            }
        }
        [HttpPost("add-bank")]
        public async Task<IActionResult> CreateBank([FromBody] BankDTO model)
        {
            if (model == null)
                return BadRequest(ApiResponse<object>.CreateError("Dữ liệu không hợp lệ"));
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
            try
            {
                var bank = new Bank
                {
                    TenNganHang = model.TenNganHang,
                    SoTaiKhoan = model.SoTaiKhoan,
                    Ten = model.Ten
                };

                _context.Banks.Add(bank);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<BankDTO>.CreateSuccess(
                    "Thêm ngân hàng thành công",
                    new BankDTO
                    {
                        Id = bank.Id,
                        TenNganHang = bank.TenNganHang,
                        SoTaiKhoan = bank.SoTaiKhoan,
                        Ten = bank.Ten
                    }
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm ngân hàng");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi thêm ngân hàng"
                ));
            }
        }
        [HttpPut("update-bank/{id}")]
        public async Task<IActionResult> UpdateBank(int id, [FromBody] BankDTO model)
        {
            if (model == null || id <= 0)
                return BadRequest(ApiResponse<object>.CreateError("Dữ liệu không hợp lệ"));

            try
            {
                var bank = await _context.Banks.FindAsync(id);
                if (bank == null)
                    return NotFound(ApiResponse<object>.CreateError("Ngân hàng không tồn tại"));

                bank.TenNganHang = model.TenNganHang;
                bank.SoTaiKhoan = model.SoTaiKhoan;
                bank.Ten = model.Ten;

                _context.Banks.Update(bank);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<BankDTO>.CreateSuccess(
                    "Cập nhật ngân hàng thành công",
                    new BankDTO
                    {
                        Id = bank.Id,
                        TenNganHang = bank.TenNganHang,
                        SoTaiKhoan = bank.SoTaiKhoan,
                        Ten = bank.Ten
                    }
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật ngân hàng");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra l���i khi cập nhật ngân hàng"
                ));
            }
        }
        [HttpDelete("delete-bank/{id}")]
        public async Task<IActionResult> DeleteBank(int id)
        {
            try
            {
                var bank = await _context.Banks.FindAsync(id);
                if (bank == null)
                    return NotFound(ApiResponse<object>.CreateError("Ngân hàng không tồn tại"));

                _context.Banks.Remove(bank);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.CreateSuccess(
                    "Xóa ngân hàng thành công",
                    null
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa ngân hàng");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi xóa ngân hàng"
                ));
            }
        }
        public class BankDTO
        {
            public int Id { get; set; }
            public string Ten { get; set; }
            public string SoTaiKhoan { get; set; }
            public string TenNganHang { get; set; }
        }
    }
}
