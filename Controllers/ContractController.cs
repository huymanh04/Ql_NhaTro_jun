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
                    .Include(c => c.MaPhongNavigation)
                        .ThenInclude(p => p.MaNhaTroNavigation)
                    .Include(c => c.MaPhongNavigation)
                        .ThenInclude(p => p.MaTheLoaiNavigation)
                    .Include(c => c.HopDongNguoiThues)
                        .ThenInclude(hn => hn.MaKhachThueNavigation)
                    .Select(c => new ContractDetailDto
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
                        IsCompleted = c.DaKetThuc ?? false,

                        // Room Information
                        Room = c.MaPhongNavigation != null ? new RoomInfoDto
                        {
                            MaPhong = c.MaPhongNavigation.MaPhong,
                            TenPhong = c.MaPhongNavigation.TenPhong,
                            Gia = c.MaPhongNavigation.Gia ?? 0,
                            DienTich = c.MaPhongNavigation.DienTich ?? 0,
                            ConTrong = c.MaPhongNavigation.ConTrong ?? false,
                            MoTa = c.MaPhongNavigation.MoTa,

                            // Motel Information
                            NhaTro = c.MaPhongNavigation.MaNhaTroNavigation != null ? new MotelInfoDto
                            {
                                MaNhaTro = c.MaPhongNavigation.MaNhaTroNavigation.MaNhaTro,
                                TenNhaTro = c.MaPhongNavigation.MaNhaTroNavigation.TenNhaTro,
                                DiaChi = c.MaPhongNavigation.MaNhaTroNavigation.DiaChi
                            } : null,

                            // Room Type Information
                            TheLoaiPhong = c.MaPhongNavigation.MaTheLoaiNavigation != null ? new RoomTypeInfoDto
                            {
                                MaTheLoai = c.MaPhongNavigation.MaTheLoaiNavigation.MaTheLoai,
                                TenTheLoai = c.MaPhongNavigation.MaTheLoaiNavigation.TenTheLoai,
                                MoTa = c.MaPhongNavigation.MaTheLoaiNavigation.MoTa
                            } : null
                        } : null,

                        // Tenant Information
                        Tenants = c.HopDongNguoiThues.Select(hn => new TenantInfoDto
                        {
                            MaKhachThue = hn.MaKhachThue,
                            HoTen = hn.MaKhachThueNavigation.HoTen,
                            SoDienThoai = hn.MaKhachThueNavigation.SoDienThoai,
                            Email = hn.MaKhachThueNavigation.Email
                        }).ToList(),

                        TenantIds = c.HopDongNguoiThues.Select(hn => hn.MaKhachThue).ToList()
                    })
     .ToListAsync();

                return Ok(ApiResponse<List<ContractDetailDto>>.CreateSuccess(
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
                    .Include(c => c.MaPhongNavigation)
                        .ThenInclude(p => p.MaNhaTroNavigation)
                    .Include(c => c.MaPhongNavigation)
                        .ThenInclude(p => p.MaTheLoaiNavigation)
                    .Include(c => c.HopDongNguoiThues)
                        .ThenInclude(hn => hn.MaKhachThueNavigation)
                    .Include(c => c.HoaDonTongs)
                    .Where(c => c.MaHopDong == id)
                    .Select(c => new ContractDetailDto
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
                        IsCompleted = c.DaKetThuc ?? false,

                        // Room Information
                        Room = c.MaPhongNavigation != null ? new RoomInfoDto
                        {
                            MaPhong = c.MaPhongNavigation.MaPhong,
                            TenPhong = c.MaPhongNavigation.TenPhong,
                            Gia = c.MaPhongNavigation.Gia ?? 0,
                            DienTich = c.MaPhongNavigation.DienTich ?? 0,
                            ConTrong = c.MaPhongNavigation.ConTrong ?? false,
                            MoTa = c.MaPhongNavigation.MoTa,

                            // Motel Information
                            NhaTro = c.MaPhongNavigation.MaNhaTroNavigation != null ? new MotelInfoDto
                            {
                                MaNhaTro = c.MaPhongNavigation.MaNhaTroNavigation.MaNhaTro,
                                TenNhaTro = c.MaPhongNavigation.MaNhaTroNavigation.TenNhaTro,
                                DiaChi = c.MaPhongNavigation.MaNhaTroNavigation.DiaChi
                            } : null,

                            // Room Type Information
                            TheLoaiPhong = c.MaPhongNavigation.MaTheLoaiNavigation != null ? new RoomTypeInfoDto
                            {
                                MaTheLoai = c.MaPhongNavigation.MaTheLoaiNavigation.MaTheLoai,
                                TenTheLoai = c.MaPhongNavigation.MaTheLoaiNavigation.TenTheLoai,
                                MoTa = c.MaPhongNavigation.MaTheLoaiNavigation.MoTa
                            } : null
                        } : null,

                        // Tenant Information
                        Tenants = c.HopDongNguoiThues.Select(hn => new TenantInfoDto
                        {
                            MaKhachThue = hn.MaKhachThue,
                            HoTen = hn.MaKhachThueNavigation.HoTen,
                            SoDienThoai = hn.MaKhachThueNavigation.SoDienThoai,
                            Email = hn.MaKhachThueNavigation.Email
                        }).ToList(),

                        // Bills Information
                        HoaDonTongs = c.HoaDonTongs.Select(hd => new BillInfoDto
                        {
                            MaHoaDon = hd.MaHoaDon,
                            NgayXuat = hd.NgayXuat.HasValue ? hd.NgayXuat.Value.ToDateTime(TimeOnly.MinValue) : default(DateTime),
                            TongTien = hd.TongTien ?? 0,
                            GhiChu = hd.GhiChu
                        }).ToList(),

                        TenantIds = c.HopDongNguoiThues.Select(hn => hn.MaKhachThue).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (contract == null)
                    return NotFound(ApiResponse<object>.CreateError("Hợp đồng không tồn tại"));

                return Ok(ApiResponse<ContractDetailDto>.CreateSuccess(
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
        public async Task<IActionResult> GetContractByRoomId(int id)
        {
            try
            {
                if (id == -1)
                {
                    var m = await _context.HopDongNguoiThues.FirstOrDefaultAsync(m => m.MaKhachThue == JunTech.id);
                    id = m.MaHopDong;
                    var ma = await _context.HopDongs.FirstOrDefaultAsync(t => t.MaHopDong == id);
                    id = (int)ma.MaPhong;
                }
                var contract = await _context.HopDongs
                    .Include(c => c.MaPhongNavigation)
                        .ThenInclude(p => p.MaNhaTroNavigation)
                    .Include(c => c.MaPhongNavigation)
                        .ThenInclude(p => p.MaTheLoaiNavigation)
                    .Include(c => c.HopDongNguoiThues)
                        .ThenInclude(hn => hn.MaKhachThueNavigation)
                    .Where(c => c.MaPhong == id)
                    .Select(c => new ContractDetailDto
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
                        IsCompleted = c.DaKetThuc ?? false,

                        // Room Information
                        Room = c.MaPhongNavigation != null ? new RoomInfoDto
                        {
                            MaPhong = c.MaPhongNavigation.MaPhong,
                            TenPhong = c.MaPhongNavigation.TenPhong,
                            Gia = c.MaPhongNavigation.Gia ?? 0,
                            DienTich = c.MaPhongNavigation.DienTich ?? 0,
                            ConTrong = c.MaPhongNavigation.ConTrong ?? false,
                            MoTa = c.MaPhongNavigation.MoTa,

                            // Motel Information
                            NhaTro = c.MaPhongNavigation.MaNhaTroNavigation != null ? new MotelInfoDto
                            {
                                MaNhaTro = c.MaPhongNavigation.MaNhaTroNavigation.MaNhaTro,
                                TenNhaTro = c.MaPhongNavigation.MaNhaTroNavigation.TenNhaTro,
                                DiaChi = c.MaPhongNavigation.MaNhaTroNavigation.DiaChi
                            } : null,

                            // Room Type Information
                            TheLoaiPhong = c.MaPhongNavigation.MaTheLoaiNavigation != null ? new RoomTypeInfoDto
                            {
                                MaTheLoai = c.MaPhongNavigation.MaTheLoaiNavigation.MaTheLoai,
                                TenTheLoai = c.MaPhongNavigation.MaTheLoaiNavigation.TenTheLoai,
                                MoTa = c.MaPhongNavigation.MaTheLoaiNavigation.MoTa
                            } : null
                        } : null,

                        // Tenant Information
                        Tenants = c.HopDongNguoiThues.Select(hn => new TenantInfoDto
                        {
                            MaKhachThue = hn.MaKhachThue,
                            HoTen = hn.MaKhachThueNavigation.HoTen,
                            SoDienThoai = hn.MaKhachThueNavigation.SoDienThoai,
                            Email = hn.MaKhachThueNavigation.Email
                        }).ToList(),

                        TenantIds = c.HopDongNguoiThues.Select(hn => hn.MaKhachThue).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (contract == null)
                    return NotFound(ApiResponse<object>.CreateError("Hợp đồng không tồn tại"));

                return Ok(ApiResponse<ContractDetailDto>.CreateSuccess(
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
                var room = await _context.PhongTros.FindAsync(model.RoomId);
                if (room != null)
                {
                    room.ConTrong = false;
                    _context.PhongTros.Update(room);
                }
                await _context.SaveChangesAsync();

                // Lấy cấu hình hệ thống (giá điện/nước)
                var caiDat = await _context.CaiDatHeThongs.FirstOrDefaultAsync();
                decimal tienDien = caiDat?.TienDien ?? 0;
                decimal tienNuoc = caiDat?.TienNuoc ?? 0;

                // Lấy phòng và giá thuê
                var phong = await _context.PhongTros.FindAsync(model.RoomId);
                decimal tienPhong = phong?.Gia ?? 0;

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
                    TongTien = model.DepositAmount,
                    DaThanhToan = true
                };
                _context.HoaDonTienIches.Add(hoaDonTienIch);
                await _context.SaveChangesAsync();
      
                // Tạo hóa đơn tổng
                var hoaDonTong = new HoaDonTong
                {
                    MaHopDong = contract.MaHopDong,  // FK đến hợp đồng vừa tạo
                    NgayXuat = DateOnly.FromDateTime(DateTime.Today),
                    TongTien = model.DepositAmount,
                    GhiChu = "Đóng tiền cọc tháng đầu tiên. Chưa có hóa đơn tiện ích cụ thể. " +
                             "Hóa đơn tiện ích sẽ được tạo sau khi có số liệu điện nước.",
                };
                
     
                _context.HoaDonTongs.Add(hoaDonTong);
        

                foreach (var tenantId in model.TenantIds)
                {
                    _context.HopDongNguoiThues.Add(new HopDongNguoiThue
                    {
                        MaHopDong = contract.MaHopDong,
                        MaKhachThue = tenantId
                    });
                    
                }
                var bank = new BankHistory
                {
                    Amount = model.DepositAmount,
                    CreatedAt = DateTime.Now,
                    TransactionCode = "Cọc tiền phòng",
                    Note = "Mã hóa đơn HD" + hoaDonTienIch.MaHoaDon,
                    BankName = "MB BANK",
                    Phuong_thuc = "Thanh toán Tiền mặt",
                    MaPhong = (int)phong.MaPhong
                };
                _context.BankHistories.Add(bank);
                await _context.SaveChangesAsync();

                // Return detailed contract information
                    var createdContract = await GetContractDetailById(contract.MaHopDong);

                return Ok(ApiResponse<ContractDetailDto>.CreateSuccess(
                    "Tạo hợp đồng thành công",
                    createdContract
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
                    contract.DaKetThuc = model.DaKetThuc;

                    // Update room availability when contract is completed
                    if (model.DaKetThuc && contract.MaPhong.HasValue)
                    {
                        var room = await _context.PhongTros.FindAsync(contract.MaPhong.Value);
                        if (room != null)
                        {
                            room.ConTrong = true;
                            _context.PhongTros.Update(room);
                        }
                    }
                }

                // Update tenant assignments
                var existingTenants = await _context.HopDongNguoiThues
                    .Where(h => h.MaHopDong == contract.MaHopDong)
                    .ToListAsync();

                if (!xoangdung)
                {
                    // Remove existing tenant assignments
                    _context.HopDongNguoiThues.RemoveRange(existingTenants);

                    // Add new tenant assignments
                    foreach (var tenantId in model.TenantIds)
                    {
                        _context.HopDongNguoiThues.Add(new HopDongNguoiThue
                        {
                            MaHopDong = contract.MaHopDong,
                            MaKhachThue = tenantId
                        }); 
                     
                    }
                }
                else
                {
                    // Remove specified tenants
                    _context.HopDongNguoiThues.RemoveRange(
                        existingTenants.Where(h => model.TenantIds.Contains(h.MaKhachThue)));
                }

                _context.HopDongs.Update(contract);
                await _context.SaveChangesAsync();

                // Return updated contract details
                var updatedContract = await GetContractDetailById(contract.MaHopDong);

                return Ok(ApiResponse<ContractDetailDto>.CreateSuccess(
                    "Cập nhật hợp đồng thành công",
                    updatedContract
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

                // Update room availability
                if (contract.MaPhong.HasValue)
                {
                    var room = await _context.PhongTros.FindAsync(contract.MaPhong.Value);
                    if (room != null)
                    {
                        room.ConTrong = true;
                        _context.PhongTros.Update(room);
                    }
                }

                // Delete related bills
                var hoaDonTongs = await _context.HoaDonTongs
                    .Where(h => h.MaHopDong == contract.MaHopDong)
                    .ToListAsync();
                if (hoaDonTongs.Any())
                {
                    _context.HoaDonTongs.RemoveRange(hoaDonTongs);
                }

                // Delete utility bills
                var hoaDonTienIches = await _context.HoaDonTienIches
                        .Where(h => h.MaPhong == contract.MaPhong)
                        .ToListAsync();
                if (hoaDonTienIches.Any())
                {
                    _context.HoaDonTienIches.RemoveRange(hoaDonTienIches);
                }

                // Delete tenant assignments
                var hopDongNguoiThues = await _context.HopDongNguoiThues
                    .Where(h => h.MaHopDong == contract.MaHopDong)
                    .ToListAsync();
                _context.HopDongNguoiThues.RemoveRange(hopDongNguoiThues);

                // Delete compensation records
                var denBus = await _context.DenBus
                    .Where(d => d.MaHopDong == contract.MaHopDong)
                    .ToListAsync();
                if (denBus.Any())
                {
                    _context.DenBus.RemoveRange(denBus);
                }

                // Delete payment history
                var lichSuThanhToans = await _context.LichSuThanhToans
                    .Where(l => l.MaHopDong == contract.MaHopDong)
                    .ToListAsync();
                if (lichSuThanhToans.Any())
                {
                    _context.LichSuThanhToans.RemoveRange(lichSuThanhToans);
                }

                // Delete contract
                _context.HopDongs.Remove(contract);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.CreateSuccess(
                    "Xóa hợp đồng thành công",
                    new { MaHopDong = contract.MaHopDong }
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

        // Helper method to get contract details
        private async Task<ContractDetailDto> GetContractDetailById(int contractId)
        {
            return await _context.HopDongs
                .Include(c => c.MaPhongNavigation)
                    .ThenInclude(p => p.MaNhaTroNavigation)
                .Include(c => c.MaPhongNavigation)
                    .ThenInclude(p => p.MaTheLoaiNavigation)
                .Include(c => c.HopDongNguoiThues)
                    .ThenInclude(hn => hn.MaKhachThueNavigation)
                .Include(c => c.HoaDonTongs)
                .Where(c => c.MaHopDong == contractId)
                .Select(c => new ContractDetailDto
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
                    IsCompleted = c.DaKetThuc ?? false,

                    Room = c.MaPhongNavigation != null ? new RoomInfoDto
                    {
                        MaPhong = c.MaPhongNavigation.MaPhong,
                        TenPhong = c.MaPhongNavigation.TenPhong,
                        Gia = c.MaPhongNavigation.Gia ?? 0,
                        DienTich = c.MaPhongNavigation.DienTich ?? 0,
                        ConTrong = c.MaPhongNavigation.ConTrong ?? false,
                        MoTa = c.MaPhongNavigation.MoTa,

                        NhaTro = c.MaPhongNavigation.MaNhaTroNavigation != null ? new MotelInfoDto
                        {
                            MaNhaTro = c.MaPhongNavigation.MaNhaTroNavigation.MaNhaTro,
                            TenNhaTro = c.MaPhongNavigation.MaNhaTroNavigation.TenNhaTro,
                            DiaChi = c.MaPhongNavigation.MaNhaTroNavigation.DiaChi
                        } : null,

                        TheLoaiPhong = c.MaPhongNavigation.MaTheLoaiNavigation != null ? new RoomTypeInfoDto
                        {
                            MaTheLoai = c.MaPhongNavigation.MaTheLoaiNavigation.MaTheLoai,
                            TenTheLoai = c.MaPhongNavigation.MaTheLoaiNavigation.TenTheLoai,
                            MoTa = c.MaPhongNavigation.MaTheLoaiNavigation.MoTa
                        } : null
                    } : null,

                    Tenants = c.HopDongNguoiThues.Select(hn => new TenantInfoDto
                    {
                        MaKhachThue = hn.MaKhachThue,
                        HoTen = hn.MaKhachThueNavigation.HoTen,
                        SoDienThoai = hn.MaKhachThueNavigation.SoDienThoai,
                        Email = hn.MaKhachThueNavigation.Email
                    }).ToList(),

                    HoaDonTongs = c.HoaDonTongs.Select(hd => new BillInfoDto
                    {
                        MaHoaDon = hd.MaHoaDon,
                        NgayXuat = hd.NgayXuat.HasValue ? hd.NgayXuat.Value.ToDateTime(TimeOnly.MinValue) : default(DateTime),
                        TongTien = hd.TongTien ?? 0,
                        GhiChu = hd.GhiChu
                    }).ToList(),

                    TenantIds = c.HopDongNguoiThues.Select(hn => hn.MaKhachThue).ToList()
                })
                .FirstOrDefaultAsync();
        }

        // DTOs
        public class ContractDetailDto
        {
            public int ContractId { get; set; }              // MaHopDong
            public int RoomId { get; set; }                  // MaPhong
            public DateTime StartDate { get; set; }          // NgayBatDau
            public DateTime EndDate { get; set; }            // NgayKetThuc
            public int NumberOfTenants { get; set; }         // SoNguoiO
            public int Soxe { get; set; }                    // SoXe
            public decimal DepositAmount { get; set; }       // TienDatCoc
            public bool IsCompleted { get; set; }            // DaKetThuc
            public List<int> TenantIds { get; set; } = new List<int>();

            // Related Information
            public RoomInfoDto Room { get; set; }
            public List<TenantInfoDto> Tenants { get; set; } = new List<TenantInfoDto>();
            public List<BillInfoDto> HoaDonTongs { get; set; } = new List<BillInfoDto>();
        }

        public class RoomInfoDto
        {
            public int MaPhong { get; set; }
            public string TenPhong { get; set; }
            public decimal Gia { get; set; }
            public double DienTich { get; set; }
            public bool ConTrong { get; set; }
            public string MoTa { get; set; }
            public MotelInfoDto NhaTro { get; set; }
            public RoomTypeInfoDto TheLoaiPhong { get; set; }
        }

        public class MotelInfoDto
        {
            public int MaNhaTro { get; set; }
            public string TenNhaTro { get; set; }
            public string DiaChi { get; set; }
        }

        public class RoomTypeInfoDto
        {
            public int MaTheLoai { get; set; }
            public string TenTheLoai { get; set; }
            public string MoTa { get; set; }
        }

        public class TenantInfoDto
        {
            public int MaKhachThue { get; set; }
            public string HoTen { get; set; }
            public string SoDienThoai { get; set; }
            public string Email { get; set; }
        }

        public class BillInfoDto
        {
            public int MaHoaDon { get; set; }
            public DateTime NgayXuat { get; set; }
            public decimal TongTien { get; set; }
            public string GhiChu { get; set; }
        }

        // DTO cho tạo mới hợp đồng
        public class ContractCreateDto
        {
            public int RoomId { get; set; }                  // MaPhong
            public DateTime StartDate { get; set; }          // NgayBatDau
            public DateTime EndDate { get; set; }            // NgayKetThuc
            public int NumberOfTenants { get; set; }         // SoNguoiO
            public int Soxe { get; set; }                    // SoXe
            public decimal DepositAmount { get; set; }       // TienDatCoc
            public List<int> TenantIds { get; set; } = new List<int>();
        }

        public class ContractUpdateDto
        {
            public DateTime EndDate { get; set; }            // NgayKetThuc
            public int NumberOfTenants { get; set; }         // SoNguoiO
            public int Soxe { get; set; }                    // SoXe
            public List<int> TenantIds { get; set; } = new List<int>();
            public bool DaKetThuc { get; set; } = false;     // Mặc định là chưa kết thúc
        }
    }
}