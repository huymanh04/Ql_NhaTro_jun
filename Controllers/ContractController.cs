using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ql_NhaTro_jun.Models;

namespace Ql_NhaTro_jun.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContractController : ControllerBase
    {
        private readonly ILogger<ContractController> _logger;
        private readonly QlNhatroContext _context;

        public ContractController(ILogger<ContractController> logger, QlNhatroContext context)
        {
            _logger = logger;
            _context = context;
        }
        [HttpGet("get-contracts")]
        public async Task<IActionResult> GetContracts()
        {
            try
            {
                var contracts = await _context.HopDongs
     .Select(c => new ContractDto
     {
         ContractId = c.MaHopDong,
         RoomId = c.MaPhong ?? 0,
         TenantId = c.MaKhachThue ?? 0,
         StartDate = c.NgayBatDau.HasValue
             ? c.NgayBatDau.Value.ToDateTime(TimeOnly.MinValue)
             : default(DateTime),
         EndDate = c.NgayKetThuc.HasValue
             ? c.NgayKetThuc.Value.ToDateTime(TimeOnly.MinValue)
             : default(DateTime),
         NumberOfTenants = c.SoNguoiO ?? 0,
         DepositAmount = c.TienDatCoc ?? 0,
         IsCompleted = c.DaKetThuc ?? false
     })
     .ToListAsync();


                return Ok(ApiResponse<List<ContractDto>>.CreateSuccess(
                    "Lấy danh sách hợp đồng thành công",
                    contracts
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách hợp đồng");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi lấy danh sách hợp đồng"
                ));
            }
        }
        [HttpGet("get-contract/{id}")]
        public async Task<IActionResult> GetContractById(int id)
        {
            try
            {
                var contract = await _context.HopDongs
                    .Where(c => c.MaHopDong == id)
                    .Select(c => new ContractDto
                    {
                        ContractId = c.MaHopDong,
                        RoomId = c.MaPhong ?? 0,
                        TenantId = c.MaKhachThue ?? 0,
                        StartDate = c.NgayBatDau.HasValue
                            ? c.NgayBatDau.Value.ToDateTime(TimeOnly.MinValue)
                            : default(DateTime),
                        EndDate = c.NgayKetThuc.HasValue
                            ? c.NgayKetThuc.Value.ToDateTime(TimeOnly.MinValue)
                            : default(DateTime),
                        NumberOfTenants = c.SoNguoiO ?? 0,
                        DepositAmount = c.TienDatCoc ?? 0,
                        IsCompleted = c.DaKetThuc ?? false
                    })
                    .FirstOrDefaultAsync();

                if (contract == null)
                    return NotFound(ApiResponse<object>.CreateError("Hợp đồng không tồn tại"));

                return Ok(ApiResponse<ContractDto>.CreateSuccess(
                    "Lấy hợp đồng thành công",
                    contract
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy hợp đồng");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi lấy hợp đồng"
                ));
            }
        }[HttpGet("get-contract-by-id/{id}")]
        public async Task<IActionResult> GetContractById1(int id)
        {
            try
            {
                var contract = await _context.HopDongs
                    .Where(c => c.MaKhachThue == id)
                    .Select(c => new ContractDto
                    {
                        ContractId = c.MaHopDong,
                        RoomId = c.MaPhong ?? 0,
                        TenantId = c.MaKhachThue ?? 0,
                        StartDate = c.NgayBatDau.HasValue
                            ? c.NgayBatDau.Value.ToDateTime(TimeOnly.MinValue)
                            : default(DateTime),
                        EndDate = c.NgayKetThuc.HasValue
                            ? c.NgayKetThuc.Value.ToDateTime(TimeOnly.MinValue)
                            : default(DateTime),
                        NumberOfTenants = c.SoNguoiO ?? 0,
                        DepositAmount = c.TienDatCoc ?? 0,
                        IsCompleted = c.DaKetThuc ?? false
                    })
                    .FirstOrDefaultAsync();

                if (contract == null)
                    return NotFound(ApiResponse<object>.CreateError("Hợp đồng không tồn tại"));

                return Ok(ApiResponse<ContractDto>.CreateSuccess(
                    "Lấy hợp đồng thành công",
                    contract
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy hợp đồng");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi lấy hợp đồng"
                ));
            }
        }
        [HttpPost("add-contract")]
        public async Task<IActionResult> CreateContract([FromBody] ContractCreateDto model)
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
                var contract = new HopDong
                {
                    MaPhong = model.RoomId,
                    MaKhachThue = model.TenantId,
                    NgayBatDau = DateOnly.FromDateTime(  model.StartDate),
                    NgayKetThuc = DateOnly.FromDateTime(model.EndDate),
                    SoNguoiO = model.NumberOfTenants,
                    TienDatCoc = model.DepositAmount,
                    DaKetThuc = false // Mặc định là chưa kết thúc
                };

                _context.HopDongs.Add(contract);
                var m = await _context.PhongTros.FindAsync(model.RoomId);
                m.ConTrong = false;
                _context.PhongTros.Update(m);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.CreateSuccess(
                    "Tạo hợp đồng thành công",
                    contract
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo hợp đồng");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi tạo hợp đồng"
                ));
            }
        }
        [HttpPut("edit-contract/{id}")]
        public async Task<IActionResult> UpdateContract(int id, [FromBody] ContractCreateDto model)
        {
            if (model == null)
                return BadRequest(ApiResponse<object>.CreateError("Dữ liệu không hợp lệ"));

            try
            {
                var contract = await _context.HopDongs.FindAsync(id);
                if (contract == null)
                    return NotFound(ApiResponse<object>.CreateError("Hợp đồng không tồn tại"));

                contract.MaPhong = model.RoomId;
                contract.MaKhachThue = model.TenantId;
                contract.NgayBatDau = DateOnly.FromDateTime(model.StartDate);
                contract.NgayKetThuc = DateOnly.FromDateTime(model.EndDate);
                contract.SoNguoiO = model.NumberOfTenants;
                contract.TienDatCoc = model.DepositAmount;

                _context.HopDongs.Update(contract);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.CreateSuccess(
                    "Cập nhật hợp đồng thành công",
                    contract
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật hợp đồng");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi cập nhật hợp đồng"
                ));
            }
        }
        [HttpDelete("delete-contract/{id}")]
        public async Task<IActionResult> DeleteContract(int id)
        {
            try
            {
                var contract = await _context.HopDongs.FindAsync(id);
                if (contract == null)
                    return NotFound(ApiResponse<object>.CreateError("Hợp đồng không tồn tại"));

                _context.HopDongs.Remove(contract);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.CreateSuccess(
                    "Xóa hợp đồng thành công",
                    contract
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa hợp đồng");
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi xóa hợp đồng"
                ));
            }
        }
        public class ContractDto
        {
            public int ContractId { get; set; }              // MaHopDong
            public int RoomId { get; set; }                  // MaPhong
            public int TenantId { get; set; }                // MaKhachThue
            public DateTime StartDate { get; set; }          // NgayBatDau
            public DateTime EndDate { get; set; }            // NgayKetThuc
            public int NumberOfTenants { get; set; }         // SoNguoiO
            public decimal DepositAmount { get; set; }       // TienDatCoc
            public bool IsCompleted { get; set; }            // DaKetThuc
        }

        // DTO cho tạo mới hợp đồng
        public class ContractCreateDto
        {
            public int RoomId { get; set; }                  // MaPhong
            public int TenantId { get; set; }                // MaKhachThue
            public DateTime StartDate { get; set; }          // NgayBatDau
            public DateTime EndDate { get; set; }            // NgayKetThuc
            public int NumberOfTenants { get; set; }         // SoNguoiO
            public decimal DepositAmount { get; set; }       // TienDatCoc
        }


    }
}