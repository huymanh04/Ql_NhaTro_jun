using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Ql_NhaTro_jun.Models;
using static Ql_NhaTro_jun.Controllers.LocationController;
using static RoomTypeController;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Ql_NhaTro_jun.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        // GET: api/<ValuesController>

        private readonly ILogger<RoomController> _logger;
        QlNhatroContext _context { get; set; }
        public RoomController(ILogger<RoomController> logger, QlNhatroContext context)
        {
            _logger = logger;
            _context = context;
        }
        // GET api/<ValuesController>/5
        [HttpGet("get-all-room")]
        public async Task<IActionResult> Getall()
        {
            try
            {
                var tinhThanhs= await _context.PhongTros
                 .Select(p => new PhongTroDTO
                 {
                     MaPhong = p.MaPhong,
                     MaNhaTro = (int)p.MaNhaTro,
                     MaTheLoai = p.MaTheLoai,
                     TenPhong = p.TenPhong,
                     Gia = (decimal)p.Gia,
                     DienTich = (float)p.DienTich,
                     ConTrong = (bool)p.ConTrong,
                     MoTa = p.MoTa
                 })
                 .ToListAsync();

                return Ok(ApiResponse<List<PhongTroDTO>>.CreateSuccess(
                    "Lấy danh sách tỉnh thành thành công",
                    tinhThanhs));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách tỉnh thành");
                return StatusCode(500, ApiResponse<object>.CreateError("Đã xảy ra lỗi khi lấy danh sách tỉnh thành"));
            }
        }
        [HttpGet("get-room-by-nha-tro/{id}")]
        public async Task<IActionResult> GetRoomByNhaTro(int id)
        {
            try
            {
                var phongTros = await _context.PhongTros
                    .Where(p => p.MaNhaTro == id)
                    .Select(p => new PhongTroDTO
                    {
                        MaPhong = p.MaPhong,
                        MaNhaTro = (int)p.MaNhaTro,
                        MaTheLoai = p.MaTheLoai,
                        TenPhong = p.TenPhong,
                        Gia = (decimal)p.Gia,
                        DienTich = (float)p.DienTich,
                        ConTrong = (bool)p.ConTrong,
                        MoTa = p.MoTa
                    })
                    .ToListAsync();

                return Ok(ApiResponse<List<PhongTroDTO>>.CreateSuccess(
                    "Lấy danh sách phòng theo nhà trọ thành công",
                    phongTros));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phòng theo nhà trọ");
                return StatusCode(500, ApiResponse<object>.CreateError("Đã xảy ra lỗi khi lấy danh sách phòng theo nhà trọ"));
            }
        }
        [HttpGet("get-room-by-the-loai/{id}")]
        public async Task<IActionResult> GetRoomByTheLoai(int id)
        {
            try
            {
                var phongTros = await _context.PhongTros
                    .Where(p => p.MaTheLoai == id)
                    .Select(p => new PhongTroDTO
                    {
                        MaPhong = p.MaPhong,
                        MaNhaTro = (int)p.MaNhaTro,
                        MaTheLoai = p.MaTheLoai,
                        TenPhong = p.TenPhong,
                        Gia = (decimal)p.Gia,
                        DienTich = (float)p.DienTich,
                        ConTrong = (bool)p.ConTrong,
                        MoTa = p.MoTa
                    })
                    .ToListAsync();

                return Ok(ApiResponse<List<PhongTroDTO>>.CreateSuccess(
                    "Lấy danh sách phòng theo thể loại thành công",
                    phongTros));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phòng theo thể loại");
                return StatusCode(500, ApiResponse<object>.CreateError("Đã xảy ra lỗi khi lấy danh sách phòng theo thể loại"));
            }
        }
        [HttpGet("get-room-by-id/{id}")]
        public async Task<IActionResult> GetRoomById(int id)
        {
            try
            {
                var phongTro = await _context.PhongTros
                    .Where(p => p.MaPhong == id)
                    .Select(p => new PhongTroDTO
                    {
                        MaPhong = p.MaPhong,
                        MaNhaTro = (int)p.MaNhaTro,
                        MaTheLoai = p.MaTheLoai,
                        TenPhong = p.TenPhong,
                        Gia = (decimal)p.Gia,
                        DienTich = (float)p.DienTich,
                        ConTrong = (bool)p.ConTrong,
                        MoTa = p.MoTa
                    })
                    .FirstOrDefaultAsync();

                if (phongTro == null)
                {
                    return NotFound(ApiResponse<object>.CreateError("Phòng không tồn tại"));
                }

                return Ok(ApiResponse<PhongTroDTO>.CreateSuccess(
                    "Lấy thông tin phòng thành công",
                    phongTro));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy thông tin phòng");
                return StatusCode(500, ApiResponse<object>.CreateError("Đã xảy ra lỗi khi lấy thông tin phòng"));
            }
        }
        [HttpGet("get-room-images/{id}")]
        public async Task<IActionResult> GetRoomImages(int id)
        {
            try
            {
                var images = await _context.HinhAnhPhongs
           .Where(i => i.MaPhong == id)
           .Select(i => new HinhAnhPhongDto
           {
               MaHinhAnh = i.MaHinhAnh,
               MaPhong = i.MaPhong,
               DuongDanHinhBase64 = Convert.ToBase64String(i.DuongDanHinh),
               IsMain = i.IsMain
           })
           .ToListAsync();

                if (images == null || images.Count == 0)
                {
                    return NotFound(ApiResponse<HinhAnhPhongDto>.CreateError("Không tìm thấy hình ảnh cho phòng này"));
                }

                return Ok(ApiResponse<List<HinhAnhPhongDto>>.CreateSuccess(
                    "Lấy danh sách hình ảnh phòng thành công",
                    images));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách hình ảnh phòng");
                return StatusCode(500, ApiResponse<object>.CreateError("Đã xảy ra lỗi khi lấy danh sách hình ảnh phòng"));
            }
        }
        [HttpGet("get-room-by-nha-tro-and-the-loai/{nhaTroId}/{theLoaiId}")]
        public async Task<IActionResult> GetRoomByNhaTroAndTheLoai(int nhaTroId, int theLoaiId)
        {
            try
            {
                var phongTros = await _context.PhongTros
                    .Where(p => p.MaNhaTro == nhaTroId && p.MaTheLoai == theLoaiId)
                    .Select(p => new PhongTroDTO
                    {
                        MaPhong = p.MaPhong,
                        MaNhaTro = (int)p.MaNhaTro,
                        MaTheLoai = p.MaTheLoai,
                        TenPhong = p.TenPhong,
                        Gia = (decimal)p.Gia,
                        DienTich = (float)p.DienTich,
                        ConTrong = (bool)p.ConTrong,
                        MoTa = p.MoTa
                    })
                    .ToListAsync();

                return Ok(ApiResponse<List<PhongTroDTO>>.CreateSuccess(
                    "Lấy danh sách phòng theo nhà trọ và thể loại thành công",
                    phongTros));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phòng theo nhà trọ và thể loại");
                return StatusCode(500, ApiResponse<object>.CreateError("Đã xảy ra lỗi khi lấy danh sách phòng theo nhà trọ và thể loại"));
            }
        }

        [HttpGet("get-room-trong")]
        public async Task<IActionResult> GetRoomTrong()
        {
            try
            {
                var phongTros = await _context.PhongTros
                    .Where(p => p.ConTrong == true)
                    .Select(p => new PhongTroDTO
                    {
                        MaPhong = p.MaPhong,
                        MaNhaTro = (int)p.MaNhaTro,
                        MaTheLoai = p.MaTheLoai,
                        TenPhong = p.TenPhong,
                        Gia = (decimal)p.Gia,
                        DienTich = (float)p.DienTich,
                        ConTrong = (bool)p.ConTrong,
                        MoTa = p.MoTa
                    })
                    .ToListAsync();

                return Ok(ApiResponse<List<PhongTroDTO>>.CreateSuccess(
                    "Lấy danh sách phòng trống thành công",
                    phongTros));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phòng trống");
                return StatusCode(500, ApiResponse<object>.CreateError("Đã xảy ra lỗi khi lấy danh sách phòng trống"));
            }
        }
        // POST api/<ValuesController>
        [HttpPost("add-room")]
        public async Task<IActionResult> AddRoomType([FromForm] CreatePhongTroDTO createDto)
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
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.CreateError(
                        "Dữ liệu không hợp lệ"));
                }

                // Kiểm tra trùng tên loại phòng
                if (await _context.PhongTros.AnyAsync(t =>
                    t.TenPhong.ToLower() == createDto.TenPhong.ToLower().Trim()))
                {
                    return BadRequest(ApiResponse<object>.CreateError("Loại phòng đã tồn tại"));
                }

                // Xử lý upload ảnh


                var phongTro = new PhongTro
                {
                    MaNhaTro = createDto.MaNhaTro,
                    MaTheLoai = createDto.MaTheLoai,
                    TenPhong = createDto.TenPhong,
                    Gia = createDto.Gia,
                    DienTich = createDto.DienTich,
                    ConTrong = createDto.ConTrong,
                    MoTa = createDto.MoTa
                };
                _context.PhongTros.Add(phongTro);
                await _context.SaveChangesAsync();
                if (createDto.Images != null && createDto.Images.Count > 0)
                {
                    for (int i = 0; i < createDto.Images.Count; i++)
                    {
                        var file = createDto.Images[i];
                        if (file != null && file.Length > 0)
                        {

                            using (var binaryReader = new BinaryReader(file.OpenReadStream()))
                            {
                                var imageData = binaryReader.ReadBytes((int)file.Length);
                                var m = await _context.PhongTros.FirstOrDefaultAsync(t => t.TenPhong == createDto.TenPhong);
                                var image = new HinhAnhPhong
                                {
                                    MaPhong = m.MaPhong,
                                    DuongDanHinh = imageData,
                                    IsMain = (i == 0) // ảnh đầu tiên là ảnh chính
                                };

                                _context.HinhAnhPhongs.Add(image);
                            }
                        }
                    }
                    await _context.SaveChangesAsync();


                }
                

                return Ok(ApiResponse<PhongTro>.CreateSuccess(
                    "Thêm loại phòng thành công",
                    phongTro));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm loại phòng: {TenTheLoai}", createDto.TenPhong);
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi thêm loại phòng"));
            }
        }
        // PUT api/<ValuesController>/5
       [HttpPut("edit-room/{id}")]
       public async Task<IActionResult> EditRoomType(int id, [FromForm] UpdatePhongTroDTO updateDto)
        {
            try {

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
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.CreateError(
                        "Dữ liệu không hợp lệ"));
                }

                // Kiểm tra trùng tên loại phòng
                if (await _context.PhongTros.AnyAsync(t =>
                    t.TenPhong.ToLower() == updateDto.TenPhong.ToLower().Trim()))
                {
                    return BadRequest(ApiResponse<object>.CreateError("Phòng không đã tồn tại"));
                }
                var phongTro = new PhongTro
                {
                    TenPhong = updateDto.TenPhong,
                    MaPhong=id,
                    MaNhaTro = _context.PhongTros.Where(p => p.MaPhong == id).Select(p => p.MaNhaTro).FirstOrDefault(),
                    Gia = updateDto.Gia,
                    DienTich = updateDto.DienTich,
                    ConTrong = updateDto.ConTrong,
                    MoTa = updateDto.MoTa
                };
           
                await _context.SaveChangesAsync();
                if (updateDto.Images != null && updateDto.Images.Count > 0)
                {
                    for (int i = 0; i < updateDto.Images.Count; i++)
                    {
                        var file = updateDto.Images[i];
                        if (file != null && file.Length > 0)
                        {

                            using (var binaryReader = new BinaryReader(file.OpenReadStream()))
                            {
                                var imageData = binaryReader.ReadBytes((int)file.Length);
                                var m = await _context.PhongTros.FirstOrDefaultAsync(t => t.TenPhong == updateDto.TenPhong);
                                var image = new HinhAnhPhong
                                {
                                    MaPhong = m.MaPhong,
                                    DuongDanHinh = imageData,
                                    IsMain = (i == 0) // ảnh đầu tiên là ảnh chính
                                };

                                _context.HinhAnhPhongs.Add(image);
                            }
                        }
                    }
                    await _context.SaveChangesAsync();


                }


                return Ok(ApiResponse<PhongTro>.CreateSuccess(
                    "Thêm loại phòng thành công",
                    phongTro));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm loại phòng: {TenTheLoai}", updateDto.TenPhong);
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi thêm loại phòng"));
            }
        }
        // DELETE api/<ValuesController>/5
        [HttpDelete("delete-room/{id}")]
        public async Task<IActionResult> DeleteRoomType(int id)
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
                // Xóa tất cả hình ảnh liên quan đến phòng
                var images = await _context.HinhAnhPhongs
                 .Where(i => i.MaPhong == id)
                 .ToListAsync();
                _context.HinhAnhPhongs.RemoveRange(images);
                var phongTro = await _context.PhongTros.FindAsync(id);
         
                if (phongTro == null)
                {
                    return NotFound(ApiResponse<object>.CreateError("Phòng không tồn tại"));
                }

                _context.PhongTros.Remove(phongTro);
     
             
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.CreateSuccess(
                    "Xóa phòng thành công",phongTro));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xóa phòng: {Id}", id);
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi xóa phòng"));
            }
        }

    }
    #region DTO
    public class PhongTroDTO
    {
        public int MaPhong { get; set; }
        public int MaNhaTro { get; set; }
        public int MaTheLoai { get; set; }
        public string TenPhong { get; set; }
        public decimal Gia { get; set; }
        public float DienTich { get; set; }
        public bool ConTrong { get; set; }
        public string MoTa { get; set; }
    }
    public class HinhAnhPhongDto
    {
        public int MaHinhAnh { get; set; }
        public int MaPhong { get; set; }
        public string DuongDanHinhBase64 { get; set; } = "";
        public bool IsMain { get; set; }
    }

    public class CreatePhongTroDTO
    {
        public int MaNhaTro { get; set; }
        public int MaTheLoai { get; set; }
        public string TenPhong { get; set; }
        public decimal Gia { get; set; }
        public float DienTich { get; set; }
        public bool ConTrong { get; set; }
        public string MoTa { get; set; }
        public List<IFormFile> Images { get; set; }
    }

    public class UpdatePhongTroDTO
    {
        public string TenPhong { get; set; }
        public decimal Gia { get; set; }
        public float DienTich { get; set; }
        public bool ConTrong { get; set; }
        public string MoTa { get; set; }
        public List<IFormFile> Images { get; set; }
    }
    #endregion
}
