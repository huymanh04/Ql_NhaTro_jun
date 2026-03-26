using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Ql_NhaTro_jun.Controllers;
using Ql_NhaTro_jun.Models;
using System;
using System.Threading.Tasks;
using static Ql_NhaTro_jun.Controllers.MotelController;

namespace SonarCloud_QL_NhaTro
{
    [TestFixture]
    public class MotelControllerTests
    {
        private QlNhatroContext _context;
        private Mock<ILogger<MotelController>> _mockLogger;
        private MotelController _controller;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<QlNhatroContext>()
                .UseInMemoryDatabase($"MotelDb_{Guid.NewGuid()}")
                .Options;
            _context = new QlNhatroContext(options);
            _mockLogger = new Mock<ILogger<MotelController>>();
            _controller = new MotelController(_mockLogger.Object, _context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        // ─── getMotel ───────────────────────────────────────────────

        [Test]
        public async Task GetMotel_ReturnsOk_WhenEmpty()
        {
            var result = await _controller.getMotel();
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetMotel_ReturnsOk_WithData()
        {
            _context.NguoiDungs.Add(new NguoiDung { MaNguoiDung = 1, HoTen = "Chu Tro", VaiTro = "1" });
            _context.NhaTros.Add(new NhaTro { MaNhaTro = 1, TenNhaTro = "Nha Tro A", DiaChi = "123", MaChuTro = 1 });
            await _context.SaveChangesAsync();

            var result = await _controller.getMotel();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        // ─── GetMotelById ───────────────────────────────────────────

        [Test]
        public async Task GetMotelById_ReturnsOk_WhenFound()
        {
            _context.NhaTros.Add(new NhaTro { MaNhaTro = 1, TenNhaTro = "Nha Tro A", DiaChi = "123" });
            await _context.SaveChangesAsync();

            var result = await _controller.GetMotelById(1);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetMotelById_ReturnsNotFound_WhenNotExist()
        {
            var result = await _controller.GetMotelById(999);

            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        // ─── AddMotel ───────────────────────────────────────────────

        [Test]
        public async Task AddMotel_ReturnsOk_WhenValidDto()
        {
            var dto = new NhaTroDto
            {
                TenNhaTro = "Nha Tro Moi",
                DiaChi = "456 Duong B",
                MaTinh = 1,
                MaKhuVuc = 1,
                MaChuTro = 1
            };

            var result = await _controller.AddTinh(dto);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task AddMotel_ReturnsBadRequest_WhenModelInvalid()
        {
            _controller.ModelState.AddModelError("TenNhaTro", "Bắt buộc");
            var dto = new NhaTroDto { TenNhaTro = "", DiaChi = "123", MaTinh = 1, MaKhuVuc = 1 };

            var result = await _controller.AddTinh(dto);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        // ─── EditNhaTro ─────────────────────────────────────────────

        [Test]
        public async Task EditNhaTro_ReturnsOk_WhenFound()
        {
            _context.NhaTros.Add(new NhaTro { MaNhaTro = 1, TenNhaTro = "Old Name", DiaChi = "123", MaTinh = 1, MaKhuVuc = 1 });
            await _context.SaveChangesAsync();

            var dto = new NhaTroDto
            {
                TenNhaTro = "New Name",
                DiaChi = "789 Duong C",
                MaTinh = 1,
                MaKhuVuc = 1,
                MaChuTro = 1
            };

            var result = await _controller.EditNhaTro(1, dto);

            Assert.That(result, Is.InstanceOf<ObjectResult>());
        }

        [Test]
        public async Task EditNhaTro_ReturnsNotFound_WhenNotExist()
        {
            var dto = new NhaTroDto { TenNhaTro = "X", DiaChi = "123", MaTinh = 1, MaKhuVuc = 1, MaChuTro = 1 };

            var result = await _controller.EditNhaTro(999, dto);

            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task EditNhaTro_ReturnsBadRequest_WhenModelInvalid()
        {
            _controller.ModelState.AddModelError("TenNhaTro", "Bắt buộc");
            var dto = new NhaTroDto { TenNhaTro = "", DiaChi = "123", MaTinh = 1, MaKhuVuc = 1 };

            var result = await _controller.EditNhaTro(1, dto);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        // ─── DeleteNhaTro ───────────────────────────────────────────

        [Test]
        public async Task DeleteNhaTro_ReturnsOk_WhenFound()
        {
            _context.NhaTros.Add(new NhaTro { MaNhaTro = 1, TenNhaTro = "To Delete", DiaChi = "123" });
            await _context.SaveChangesAsync();

            var result = await _controller.DeleteNhaTro(1);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task DeleteNhaTro_ReturnsNotFound_WhenNotExist()
        {
            var result = await _controller.DeleteNhaTro(999);

            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        // ─── GetOwners ──────────────────────────────────────────────

        [Test]
        public async Task GetOwners_ReturnsOk_WithOwners()
        {
            _context.NguoiDungs.AddRange(
                new NguoiDung { MaNguoiDung = 1, HoTen = "Chu Tro A", VaiTro = "1" },
                new NguoiDung { MaNguoiDung = 2, HoTen = "Admin B",   VaiTro = "2" },
                new NguoiDung { MaNguoiDung = 3, HoTen = "Khach C",   VaiTro = "0" }
            );
            await _context.SaveChangesAsync();

            var result = await _controller.GetOwners();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetOwners_ReturnsOk_WhenNoOwners()
        {
            var result = await _controller.GetOwners();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        // ─── TestData ───────────────────────────────────────────────

        [Test]
        public async Task TestData_ReturnsOk_WithSeedData()
        {
            _context.TinhThanhs.Add(new TinhThanh { MaTinh = 1, TenTinh = "Ha Noi" });
            _context.KhuVucs.Add(new KhuVuc { MaKhuVuc = 1, TenKhuVuc = "Hoan Kiem", MaTinh = 1 });
            await _context.SaveChangesAsync();

            var result = await _controller.TestData();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task TestData_ReturnsOk_WhenEmpty()
        {
            var result = await _controller.TestData();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }
    }
}
