using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Ql_NhaTro_jun.Controllers;
using Ql_NhaTro_jun.Models;
using System;
using System.Threading.Tasks;
using static Ql_NhaTro_jun.Controllers.LocationController;

namespace Ql_NhaTro_jun.Tests
{
    [TestFixture]
    public class LocationControllerTests
    {
        private QlNhatroContext _context;
        private Mock<ILogger<LocationController>> _mockLogger;
        private LocationController _controller;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<QlNhatroContext>()
                .UseInMemoryDatabase($"LocationDb_{Guid.NewGuid()}")
                .Options;
            _context = new QlNhatroContext(options);
            _mockLogger = new Mock<ILogger<LocationController>>();
            _controller = new LocationController(_mockLogger.Object, _context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        // ─── GetTinh ────────────────────────────────────────────────

        [Test]
        public async Task GetTinh_ReturnsOk_WhenEmpty()
        {
            var result = await _controller.Gettinh();
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetTinh_ReturnsOk_WithData()
        {
            _context.TinhThanhs.Add(new TinhThanh { MaTinh = 1, TenTinh = "Ha Noi" });
            _context.TinhThanhs.Add(new TinhThanh { MaTinh = 2, TenTinh = "Ho Chi Minh" });
            await _context.SaveChangesAsync();

            var result = await _controller.Gettinh();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        // ─── AddTinh ────────────────────────────────────────────────

        [Test]
        public async Task AddTinh_ReturnsOk_WhenValidDto()
        {
            var dto = new TinhThanhDTO { TenTinh = "Da Nang" };

            var result = await _controller.AddTinh(dto);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task AddTinh_ReturnsBadRequest_WhenModelInvalid()
        {
            _controller.ModelState.AddModelError("TenTinh", "Bắt buộc");
            var dto = new TinhThanhDTO { TenTinh = "" };

            var result = await _controller.AddTinh(dto);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        // ─── EditTinh ───────────────────────────────────────────────

        [Test]
        public async Task EditTinh_ReturnsOk_WhenFound()
        {
            _context.TinhThanhs.Add(new TinhThanh { MaTinh = 1, TenTinh = "Da Nang" });
            await _context.SaveChangesAsync();

            var dto = new TinhThanhDTO { TenTinh = "Da Nang Updated" };

            var result = await _controller.Edittinh(1, dto);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task EditTinh_ReturnsBadRequest_WhenNotFound()
        {
            var dto = new TinhThanhDTO { TenTinh = "Not Exist" };

            var result = await _controller.Edittinh(999, dto);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task EditTinh_ReturnsBadRequest_WhenModelInvalid()
        {
            _controller.ModelState.AddModelError("TenTinh", "Bắt buộc");
            var dto = new TinhThanhDTO { TenTinh = "" };

            var result = await _controller.Edittinh(1, dto);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        // ─── DeleteTinh ─────────────────────────────────────────────

        [Test]
        public async Task DeleteTinh_ReturnsOk_WhenFound()
        {
            _context.TinhThanhs.Add(new TinhThanh { MaTinh = 1, TenTinh = "Can Xoa" });
            await _context.SaveChangesAsync();

            var result = await _controller.Deletetinh(1);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task DeleteTinh_ReturnsNotFound_WhenNotExist()
        {
            var result = await _controller.Deletetinh(999);
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        // ─── GetKhuVuc (paged) ──────────────────────────────────────

        [Test]
        public async Task GetKhuVuc_ReturnsOk_WhenEmpty()
        {
            var result = await _controller.GetKhuVucPaged();
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetKhuVuc_ReturnsOk_WithData()
        {
            _context.TinhThanhs.Add(new TinhThanh { MaTinh = 1, TenTinh = "Ha Noi" });
            _context.KhuVucs.AddRange(
                new KhuVuc { MaKhuVuc = 1, TenKhuVuc = "Hoan Kiem", MaTinh = 1 },
                new KhuVuc { MaKhuVuc = 2, TenKhuVuc = "Dong Da",   MaTinh = 1 },
                new KhuVuc { MaKhuVuc = 3, TenKhuVuc = "Ba Dinh",   MaTinh = 1 }
            );
            await _context.SaveChangesAsync();

            var result = await _controller.GetKhuVucPaged(pageNumber: 1, pageSize: 2);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetKhuVuc_FilterByMaTinh_ReturnsOk()
        {
            _context.KhuVucs.Add(new KhuVuc { MaKhuVuc = 1, TenKhuVuc = "Q1", MaTinh = 2 });
            _context.KhuVucs.Add(new KhuVuc { MaKhuVuc = 2, TenKhuVuc = "Q2", MaTinh = 3 });
            await _context.SaveChangesAsync();

            var result = await _controller.GetKhuVucPaged(maTinh: 2);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        // ─── AddKhuVuc ──────────────────────────────────────────────

        [Test]
        public async Task AddKhuVuc_ReturnsOk_WhenValidDto()
        {
            var dto = new KhuVucDTO { TenKhuVuc = "Quan 1", MaTinh = 1 };

            var result = await _controller.AddKhuVuc(dto);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task AddKhuVuc_ReturnsBadRequest_WhenModelInvalid()
        {
            _controller.ModelState.AddModelError("TenKhuVuc", "Bắt buộc");
            var dto = new KhuVucDTO { TenKhuVuc = "", MaTinh = 1 };

            var result = await _controller.AddKhuVuc(dto);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        // ─── EditKhuVuc ─────────────────────────────────────────────

        [Test]
        public async Task EditKhuVuc_ReturnsOk_WhenFound()
        {
            _context.KhuVucs.Add(new KhuVuc { MaKhuVuc = 1, TenKhuVuc = "Old Name", MaTinh = 1 });
            await _context.SaveChangesAsync();

            var dto = new KhuVucDTO { TenKhuVuc = "New Name", MaTinh = 1 };

            var result = await _controller.EditKhuVuc(1, dto);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task EditKhuVuc_ReturnsBadRequest_WhenNotFound()
        {
            var dto = new KhuVucDTO { TenKhuVuc = "X", MaTinh = 1 };

            var result = await _controller.EditKhuVuc(999, dto);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task EditKhuVuc_ReturnsBadRequest_WhenModelInvalid()
        {
            _controller.ModelState.AddModelError("TenKhuVuc", "Bắt buộc");
            var dto = new KhuVucDTO { TenKhuVuc = "", MaTinh = 1 };

            var result = await _controller.EditKhuVuc(1, dto);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        // ─── DeleteKhuVuc ───────────────────────────────────────────

        [Test]
        public async Task DeleteKhuVuc_ReturnsOk_WhenFound()
        {
            _context.KhuVucs.Add(new KhuVuc { MaKhuVuc = 1, TenKhuVuc = "Can Xoa", MaTinh = 1 });
            await _context.SaveChangesAsync();

            var result = await _controller.DeleteKhuVuc(1);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task DeleteKhuVuc_ReturnsNotFound_WhenNotExist()
        {
            var result = await _controller.DeleteKhuVuc(999);
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }
    }
}
