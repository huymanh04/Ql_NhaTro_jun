using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Ql_NhaTro_jun.Models;
using System.ComponentModel.DataAnnotations;
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
                                var tinhThanhs = await (
                   from p in _context.PhongTros
                   join nt in _context.NhaTros on p.MaNhaTro equals nt.MaNhaTro
                   select new PhongTroDTO
                   {
                       MaPhong = p.MaPhong,
                       MaNhaTro = (int)p.MaNhaTro,
                       MaTheLoai = p.MaTheLoai,
                       TenPhong = p.TenPhong,
                       Gia = (decimal)p.Gia,
                       DienTich = (float)p.DienTich,
                       ConTrong = (bool)p.ConTrong,
                       MoTa = p.MoTa,
                       Sdt_chu = nt.MaChuTroNavigation.SoDienThoai,
                       gg_map = nt.gg_map
                   }
                ).ToListAsync();


                var img = await _context.HinhAnhPhongTros

                        .Select(i => new HinhAnhPhongDto
                        {
                            MaHinhAnh = i.MaHinhAnh,
                            MaPhong = i.MaPhong,
                            DuongDanHinhBase64 = Convert.ToBase64String(i.DuongDanHinh),
                            IsMain = (bool)i.IsMain
                        })
                        .ToListAsync();
                    var result = new PhongVaHinhDtoo
                    {
                        Phong = tinhThanhs,
                        HinhAnh = img
                    };
                    await _context.SaveChangesAsync();
                    return Ok(ApiResponse<object>.CreateSuccess("Lấy thành công", result));
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

                var img = await _context.HinhAnhPhongTros
          .Where(i => i.MaPhong == id)
          .Select(i => new HinhAnhPhongDto
          {
              MaHinhAnh = i.MaHinhAnh,
              MaPhong = i.MaPhong,
              DuongDanHinhBase64 = Convert.ToBase64String(i.DuongDanHinh),
              IsMain = (bool)i.IsMain
          })
          .ToListAsync();
                var result = new PhongVaHinhDtoo
                {
                    Phong = phongTros,
                    HinhAnh = img
                };

                return Ok(ApiResponse<object>.CreateSuccess("Lấy thành công", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách phòng theo nhà trọ");
                return StatusCode(500, ApiResponse<object>.CreateError("Đã xảy ra lỗi khi lấy danh sách phòng theo nhà trọ"));
            }
        }
        [HttpGet("get-room-trong-by-nha-tro/{id}")]
        public async Task<IActionResult> GetRoomByNhaTroa(int id)
        {
            try
            {
                var phongTros = await _context.PhongTros
                    .Where(p => p.MaNhaTro == id&&p.ConTrong==false)
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

                var img = await _context.HinhAnhPhongTros
          .Where(i => i.MaPhong == id)
          .Select(i => new HinhAnhPhongDto
          {
              MaHinhAnh = i.MaHinhAnh,
              MaPhong = i.MaPhong,
              DuongDanHinhBase64 = Convert.ToBase64String(i.DuongDanHinh),
              IsMain = (bool)i.IsMain
          })
          .ToListAsync();
                var result = new PhongVaHinhDtoo
                {
                    Phong = phongTros,
                    HinhAnh = img
                };

                return Ok(ApiResponse<object>.CreateSuccess("Lấy thành công", result));
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

                var img = await _context.HinhAnhPhongTros
              .Where(i => i.MaPhong == id)
              .Select(i => new HinhAnhPhongDto
              {
                  MaHinhAnh = i.MaHinhAnh,
                  MaPhong = i.MaPhong,
                  DuongDanHinhBase64 = Convert.ToBase64String(i.DuongDanHinh),
                  IsMain = (bool)i.IsMain
              })
              .ToListAsync();
                var result = new PhongVaHinhDtoo
                {
                    Phong = phongTros,
                    HinhAnh = img
                };

                return Ok(ApiResponse<object>.CreateSuccess("Lấy thành công", result));
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
                    .Select(p => new {
                        p.MaPhong,
                        p.MaNhaTro,
                        p.MaTheLoai,
                        p.TenPhong,
                        p.Gia,
                        p.DienTich,
                        p.ConTrong,
                        p.MoTa,
                        TenTheLoai = p.MaTheLoaiNavigation.TenTheLoai,
                        TenNhaTro = p.MaNhaTroNavigation.TenNhaTro,
                        DiaChi = p.MaNhaTroNavigation.DiaChi,
                        TenKhuVuc = p.MaNhaTroNavigation.MaKhuVucNavigation.TenKhuVuc,
                        gg_map=p.MaNhaTroNavigation.gg_map,
                        Sdt_chu=p.MaNhaTroNavigation.MaChuTroNavigation.SoDienThoai
                    })
                    .FirstOrDefaultAsync();

                if (phongTro == null)
                {
                    return NotFound(ApiResponse<object>.CreateError("Phòng không tồn tại"));
                }
                var img = await _context.HinhAnhPhongTros
                    .Where(i => i.MaPhong == id)
                    .Select(i => new HinhAnhPhongDto
                    {
                        MaHinhAnh = i.MaHinhAnh,
                        MaPhong = i.MaPhong,
                        DuongDanHinhBase64 = Convert.ToBase64String(i.DuongDanHinh),
                        IsMain = (bool)i.IsMain
                    })
                    .ToListAsync();
                var result = new {
                    Phong = phongTro,
                    HinhAnh = img
                };

                return Ok(ApiResponse<object>.CreateSuccess("Lấy thành công", result));
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
                var images = await _context.HinhAnhPhongTros
           .Where(i => i.MaPhong == id)
           .Select(i => new HinhAnhPhongDto
           {
               MaHinhAnh = i.MaHinhAnh,
               MaPhong = i.MaPhong,
               DuongDanHinhBase64 = Convert.ToBase64String(i.DuongDanHinh),
               IsMain = (bool)i.IsMain
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
        [HttpGet("related-rooms")]
        public async Task<IActionResult> GetRelatedRooms([FromQuery] int loaiPhongId, [FromQuery] int excludeId)
        {
            try
            {
                var phongTros = await _context.PhongTros
                    .Where(p => p.MaTheLoai == loaiPhongId && p.MaPhong != excludeId)
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

                // Lấy ảnh đại diện cho từng phòng (IsMain=true hoặc ảnh đầu tiên nếu không có)
                var phongIds = phongTros.Select(x => x.MaPhong).ToList();
                var anhMap = await _context.HinhAnhPhongTros
                    .Where(h => phongIds.Contains(h.MaPhong) && h.IsMain == true)
                    .GroupBy(h => h.MaPhong)
                    .Select(g => new { MaPhong = g.Key, DuongDanHinhBase64 = Convert.ToBase64String(g.First().DuongDanHinh) })
                    .ToListAsync();
                var anhDict = anhMap.ToDictionary(x => x.MaPhong, x => x.DuongDanHinhBase64);

                // Bổ sung trường AnhDaiDienBase64 vào từng phòng (dùng dynamic cho đúng style)
                var result = phongTros.Select(p => new {
                    p.MaPhong,
                    p.MaNhaTro,
                    p.MaTheLoai,
                    p.TenPhong,
                    p.Gia,
                    p.DienTich,
                    p.ConTrong,
                    p.MoTa,
                    AnhDaiDienBase64 = anhDict.ContainsKey(p.MaPhong) ? anhDict[p.MaPhong] : null
                }).ToList();

                return Ok(ApiResponse<object>.CreateSuccess("Lấy phòng liên quan thành công", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy phòng liên quan");
                return StatusCode(500, ApiResponse<object>.CreateError("Đã xảy ra lỗi khi lấy phòng liên quan"));
            }
        }
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] int? theLoai, [FromQuery] int? khuVuc, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice)
        {
            try
            {
                var query = _context.PhongTros.AsQueryable();
                if (theLoai.HasValue)
                    query = query.Where(p => p.MaTheLoai == theLoai.Value);
                if (khuVuc.HasValue)
                    query = query.Where(p => p.MaNhaTroNavigation.MaKhuVuc == khuVuc.Value);
                if (minPrice.HasValue)
                    query = query.Where(p => p.Gia >= minPrice.Value);
                if (maxPrice.HasValue)
                    query = query.Where(p => p.Gia <= maxPrice.Value);

                var phongTros = await query
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

                var phongIds = phongTros.Select(x => x.MaPhong).ToList();
                var img = await _context.HinhAnhPhongTros
                    .Where(i => phongIds.Contains(i.MaPhong) && i.IsMain == true)
                    .Select(i => new HinhAnhPhongDto
                    {
                        MaHinhAnh = i.MaHinhAnh,
                        MaPhong = i.MaPhong,
                        DuongDanHinhBase64 = Convert.ToBase64String(i.DuongDanHinh),
                        IsMain = (bool)i.IsMain
                    })
                    .ToListAsync();
                var result = new PhongVaHinhDtoo
                {
                    Phong = phongTros,
                    HinhAnh = img
                };
                return Ok(ApiResponse<object>.CreateSuccess("Lấy thành công", result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tìm kiếm phòng");
                return StatusCode(500, ApiResponse<object>.CreateError("Đã xảy ra lỗi khi tìm kiếm phòng"));
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
                                var image = new HinhAnhPhongTro
                                {
                                    MaPhong = m.MaPhong,
                                    DuongDanHinh = imageData,
                                    IsMain = (i == 0) // ảnh đầu tiên là ảnh chính
                                };

                                _context.HinhAnhPhongTros.Add(image);
                            }
                        }
                    }
                    await _context.SaveChangesAsync();


                }

                var dto = new 
                {
                    MaPhong = phongTro.MaPhong,
                    TenPhong = phongTro.TenPhong,
                    Gia = phongTro.Gia,
                    DienTich = phongTro.DienTich,
                    ConTrong = phongTro.ConTrong,
                    MoTa = phongTro.MoTa,
                    AnhChinhBase64 = await _context.HinhAnhPhongTros
        .Where(x => x.MaPhong == phongTro.MaPhong && (bool)x.IsMain)
        .Select(x => Convert.ToBase64String(x.DuongDanHinh))
        .ToListAsync()
                };

                return Ok(ApiResponse<object>.CreateSuccess("Thêm loại phòng thành công", dto));

               
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm loại phòng: {TenTheLoai}", createDto.TenPhong);
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi thêm loại phòng"));
            }
        }
        // PUT api/<ValuesControlle r>/5
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

                // Tìm phòng cần cập nhật
                var phongTro = await _context.PhongTros.FindAsync(id);
                if (phongTro == null)
                {
                    return NotFound(ApiResponse<object>.CreateError("Phòng không tồn tại"));
                }

                // Kiểm tra trùng tên phòng (trừ phòng hiện tại)
                if (await _context.PhongTros.AnyAsync(t =>
                    t.TenPhong.ToLower() == updateDto.TenPhong.ToLower().Trim() && t.MaPhong != id))
                {
                    return BadRequest(ApiResponse<object>.CreateError("Tên phòng đã tồn tại"));
                }

                // Cập nhật thông tin phòng
                phongTro.TenPhong = updateDto.TenPhong;
                phongTro.Gia = updateDto.Gia;
                phongTro.DienTich = updateDto.DienTich;
                phongTro.ConTrong = updateDto.ConTrong;
                phongTro.MoTa = updateDto.MoTa;

                _context.PhongTros.Update(phongTro);
                await _context.SaveChangesAsync();

                // Xử lý hình ảnh mới nếu có
                if (updateDto.Images != null && updateDto.Images.Count > 0)
                {
                    // Xóa tất cả hình ảnh cũ trước khi thêm hình ảnh mới
                    var oldImages = await _context.HinhAnhPhongTros
                        .Where(i => i.MaPhong == id)
                        .ToListAsync();
                    
                    if (oldImages.Any())
                    {
                        _context.HinhAnhPhongTros.RemoveRange(oldImages);
                        await _context.SaveChangesAsync();
                    }

                    // Thêm hình ảnh mới
                    for (int i = 0; i < updateDto.Images.Count; i++)
                    {
                        var file = updateDto.Images[i];
                        if (file != null && file.Length > 0)
                        {
                            using (var binaryReader = new BinaryReader(file.OpenReadStream()))
                            {
                                var imageData = binaryReader.ReadBytes((int)file.Length);
                                var image = new HinhAnhPhongTro
                                {
                                    MaPhong = id,
                                    DuongDanHinh = imageData,
                                    IsMain = (i == 0) // ảnh đầu tiên là ảnh chính
                                };

                                _context.HinhAnhPhongTros.Add(image);
                            }
                        }
                    }
                    await _context.SaveChangesAsync();
                }

                var dto = new
                {
                    MaPhong = phongTro.MaPhong,
                    TenPhong = phongTro.TenPhong,
                    Gia = phongTro.Gia,
                    DienTich = phongTro.DienTich,
                    ConTrong = phongTro.ConTrong,
                    MoTa = phongTro.MoTa,
                    AnhChinhBase64 = await _context.HinhAnhPhongTros
            .Where(x => x.MaPhong == phongTro.MaPhong && (bool)x.IsMain)
            .Select(x => Convert.ToBase64String(x.DuongDanHinh))
            .ToListAsync()
                };

                return Ok(ApiResponse<object>.CreateSuccess("Update phòng thành công", dto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật phòng: {TenPhong}", updateDto.TenPhong);
                return StatusCode(500, ApiResponse<object>.CreateError(
                    "Đã xảy ra lỗi khi cập nhật phòng"));
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
                var images = await _context.HinhAnhPhongTros
                 .Where(i => i.MaPhong == id)
                 .ToListAsync();
                _context.HinhAnhPhongTros.RemoveRange(images);
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
        public string Sdt_chu {  get; set; }
        public string gg_map {  get; set; }
    }
    public class PhongVaHinhDto
    {
        public PhongTroDTO Phong { get; set; }
        public List<HinhAnhPhongDto> HinhAnh { get; set; }
    }   public class PhongVaHinhDtoo
    {
        public List<PhongTroDTO> Phong { get; set; }
        public List<HinhAnhPhongDto> HinhAnh { get; set; }
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
        public List<IFormFile>? Images { get; set; }
    }

    public class UpdatePhongTroDTO
    {
        [Required(ErrorMessage = "Tên phòng không được để trống")]
        public string TenPhong { get; set; }
        
        [Required(ErrorMessage = "Giá thuê không được để trống")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá thuê phải lớn hơn 0")]
        public decimal Gia { get; set; }
        
        [Required(ErrorMessage = "Diện tích không được để trống")]
        [Range(0.1, float.MaxValue, ErrorMessage = "Diện tích phải lớn hơn 0")]
        public float DienTich { get; set; }
        
        [Required(ErrorMessage = "Trạng thái không được để trống")]
        public bool ConTrong { get; set; }
        
        public string MoTa { get; set; }
        
        // Images is optional for updates - only used when user wants to change images
        public List<IFormFile>? Images { get; set; }
    }
    #endregion
}
