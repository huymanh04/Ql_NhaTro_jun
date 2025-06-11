using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ql_NhaTro_jun.Models;

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
