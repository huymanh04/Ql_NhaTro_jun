using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ql_NhaTro_jun.Models;
using System.Text.Json;

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
                        StartDate = c.NgayBatDau.HasValue
                            ? c.NgayBatDau.Value.ToDateTime(TimeOnly.MinValue)
                            : default(DateTime),
                        EndDate = c.NgayKetThuc.HasValue
                            ? c.NgayKetThuc.Value.ToDateTime(TimeOnly.MinValue)
                            : default(DateTime),
                        NumberOfTenants = c.SoNguoiO ?? 0,
                        TenantIds = c.HopDongNguoiThues.Select(nt => nt.MaKhachThue).ToList(),
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
        [HttpGet("get-contract-by-room-id/{id}")]
        public async Task<IActionResult> GetContractById1(int id)
        {
            try
            {
                var contract = await _context.HopDongs
                    .Where(c => c.MaPhong == id)
                    .Select(c => new ContractDto
                    {
                        ContractId = c.MaHopDong,
                        RoomId = c.MaPhong ?? 0,
                        StartDate = c.NgayBatDau.HasValue
                            ? c.NgayBatDau.Value.ToDateTime(TimeOnly.MinValue)
                            : default(DateTime),
                        EndDate = c.NgayKetThuc.HasValue
                            ? c.NgayKetThuc.Value.ToDateTime(TimeOnly.MinValue)
                            : default(DateTime),
                        NumberOfTenants = c.SoNguoiO ?? 0,
                        Soxe = c.SoXe,
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
                bool daCoHopDong = await _context.HopDongs
        .AnyAsync(h => h.MaPhong == model.RoomId && h.DaKetThuc == false);
                if (daCoHopDong)
                    return BadRequest(ApiResponse<object>.CreateError("Phòng đã có hợp đồng chưa kết thúc"));

                var contract = new HopDong
                {
                    MaPhong = model.RoomId,
                    NgayBatDau = DateOnly.FromDateTime(model.StartDate),
                    NgayKetThuc = DateOnly.FromDateTime(model.EndDate),
                    SoNguoiO = model.NumberOfTenants,
                    SoXe = model.Soxe,
                    TienDatCoc = model.DepositAmount,
                    DaKetThuc = false // Mặc định là chưa kết thúc
                };

                _context.HopDongs.Add(contract);
                var m = await _context.PhongTros.FindAsync(model.RoomId);
                m.ConTrong = false;
                _context.PhongTros.Update(m);
                await _context.SaveChangesAsync();
                // Lấy cấu hình hệ thống (giá điện/nước)
                var caiDat = await _context.CaiDatHeThongs.FirstOrDefaultAsync();
                decimal tienDien = caiDat?.TienDien ?? 0;
                decimal tienNuoc = caiDat?.TienNuoc ?? 0;

                // Lấy phòng và giá thuê
                var phong = await _context.PhongTros.FindAsync(model.RoomId);
                decimal tienPhong = phong?.Gia ?? 0;

                // Tính tổng tiền hóa đơn tiện ích

                decimal tongTien = tienPhong;

                // Tạo hóa đơn tiện ích
                var hoaDonTienIch = new HoaDonTienIch
                {
                    MaPhong = model.RoomId,
                    Thang = DateTime.Now.Month,
                    Nam = DateTime.Now.Year,
                    SoDien = 0,
                    SoNuoc = 0,
                    DonGiaDien = tienDien,
                    DonGiaNuoc = tienNuoc,
                    TongTien = 0,
                    DaThanhToan = true
                };
                _context.HoaDonTienIches.Add(hoaDonTienIch);
                await _context.SaveChangesAsync();

                // Tạo hóa đơn tổng
                var hoaDonTong = new HoaDonTong
                {
                    MaHopDong = contract.MaHopDong,  // FK đến hợp đồng vừa tạo
                    NgayXuat = DateOnly.FromDateTime(DateTime.Today),
                    TongTien = tongTien,
                    GhiChu = "Đóng tiền cọc tháng đầu tiên. Chưa có hóa đơn tiện ích cụ thể. " +
                             "Hóa đơn tiện ích sẽ được tạo sau khi có số liệu điện nước.",
                };
                _context.HoaDonTongs.Add(hoaDonTong);

                foreach (var id in model.TenantIds)
                {
                    _context.HopDongNguoiThues.Add(new HopDongNguoiThue
                    {
                        MaHopDong = contract.MaHopDong,
                        MaKhachThue = id
                    });
                }
                //Console.WriteLine(JsonSerializer.Serialize(contract)); // sẽ lỗi nếu có vòng lặp

                await _context.SaveChangesAsync();
                var dto = new
                {
                    MaHopDong = contract.MaHopDong,
                    MaPhong = contract.MaPhong,
                    NgayBatDau = contract.NgayBatDau?.ToDateTime(TimeOnly.MinValue),
                    NgayKetThuc = contract.NgayKetThuc?.ToDateTime(TimeOnly.MinValue),
                    SoNguoiO = contract.SoNguoiO,
                    Soxe = contract.SoXe,
                    TienDatCoc = contract.TienDatCoc,
                    DaKetThuc = contract.DaKetThuc,

                    Tenants = contract.HopDongNguoiThues.Select(x => new HopDongNguoiThueDto
                    {
                        MaKhachThue = x.MaKhachThue
                    })
                };
                return Ok(ApiResponse<object>.CreateSuccess(
                    "Tạo hợp đồng thành công",
                    dto
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
        public async Task<IActionResult> UpdateContract(int id, [FromBody] ContractUpdateDto model, bool xoangdung = false)
        {

            try
            {
                var contract = await _context.HopDongs.FindAsync(id);
                if (contract == null)
                    return NotFound(ApiResponse<object>.CreateError("Hợp đồng không tồn tại"));
                if (model != null && !xoangdung)
                {
                    contract.NgayKetThuc = DateOnly.FromDateTime(model.EndDate);
                    contract.SoNguoiO = model.NumberOfTenants;
                    contract.SoXe = model.Soxe;
                    contract.DaKetThuc = model.DaKetThuc; // Cập nhật trạng thái kết thúc
                }
                // Mặc định là chưa kết thúc
                // Cập nhật danh sách người thuê
                var manh = await _context.HopDongNguoiThues
                    .Where(h => h.MaHopDong == contract.MaHopDong)
                    .ToListAsync();
                if (!xoangdung)
                {

                    #region add thêm người thuê
                    foreach (var tenantId in model.TenantIds)
                    {
                        _context.HopDongNguoiThues.Add(new HopDongNguoiThue
                        {
                            MaHopDong = contract.MaHopDong,
                            MaKhachThue = tenantId
                        });
                    }
                    #endregion
                }
                else
                {
                    #region Xóa người dùng
                    _context.HopDongNguoiThues.RemoveRange(
                  manh.Where(h => model.TenantIds.Contains(h.MaKhachThue)));
                    #endregion
                }
                _context.HopDongs.Update(contract);
                await _context.SaveChangesAsync();
                var dto = new
                {
                    MaHopDong = contract.MaHopDong,
                    MaPhong = contract.MaPhong,
                    NgayBatDau = contract.NgayBatDau?.ToDateTime(TimeOnly.MinValue),
                    NgayKetThuc = contract.NgayKetThuc?.ToDateTime(TimeOnly.MinValue),
                    SoNguoiO = contract.SoNguoiO,
                    Soxe = contract.SoXe,
                    TienDatCoc = contract.TienDatCoc,
                    DaKetThuc = contract.DaKetThuc,

                    Tenants = contract.HopDongNguoiThues.Select(x => new HopDongNguoiThueDto
                    {
                        MaKhachThue = x.MaKhachThue
                    })
                };
                return Ok(ApiResponse<object>.CreateSuccess(
                    "Cập nhật hợp đồng thành công",
                    dto
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
                var hoadontong = await _context.HoaDonTongs
                    .FirstOrDefaultAsync(h => h.MaHopDong == contract.MaHopDong);
                if (hoadontong != null)
                {
                    var hoadowntienich = await _context.HoaDonTienIches
                        .Where(h => h.MaPhong == contract.MaPhong)
                        .ToListAsync();
                    _context.HoaDonTienIches.RemoveRange(hoadowntienich);
                    await _context.SaveChangesAsync();
                    _context.HoaDonTongs.Remove(hoadontong);
                    await _context.SaveChangesAsync();
                }

                // Xóa các bản ghi liên quan đến hợp đồng
                var hodongnguoithue = await _context.HopDongNguoiThues
                    .Where(h => h.MaHopDong == contract.MaHopDong)
                    .ToListAsync();
                _context.HopDongNguoiThues.RemoveRange(hodongnguoithue);
                await _context.SaveChangesAsync();
                // Xóa hợp đồng

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
        public class HopDongNguoiThueDto
        {
            public int MaKhachThue { get; set; }
        }

        public class ContractDto
        {
            public int ContractId { get; set; }              // MaHopDong
            public int RoomId { get; set; }                  // MaPhong
                                                             // MaKhachThue
            public DateTime StartDate { get; set; }          // NgayBatDau
            public DateTime EndDate { get; set; }            // NgayKetThuc
            public int NumberOfTenants { get; set; }         // SoNguoiO
            public int Soxe { get; set; }         // SoNguoiO
            public decimal DepositAmount { get; set; }       // TienDatCoc
            public bool IsCompleted { get; set; }            // DaKetThuc
            public List<int>? TenantIds { get; set; }
        }

        // DTO cho tạo mới hợp đồng
        public class ContractCreateDto
        {
            public int RoomId { get; set; }                  // MaPhong
            public DateTime StartDate { get; set; }          // NgayBatDau
            public DateTime EndDate { get; set; }            // NgayKetThuc
            public int NumberOfTenants { get; set; }         // SoNguoiO
            public int Soxe { get; set; }         // SoNguoiO
            public decimal DepositAmount { get; set; }       // TienDatCoc
            public List<int>? TenantIds { get; set; }
        }

        public class ContractUpdateDto
        {

            public DateTime EndDate { get; set; }            // NgayKetThuc
            public int NumberOfTenants { get; set; }         // SoNguoiO
            public int Soxe { get; set; }         // SoNguoiO
            public List<int>? TenantIds { get; set; }
            public bool DaKetThuc { get; set; } = false; // Mặc định là chưa kết thúc
        }
    }
}