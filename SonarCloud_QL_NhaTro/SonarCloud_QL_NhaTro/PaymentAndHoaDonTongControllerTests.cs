using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Ql_NhaTro_jun.Controllers;
using Ql_NhaTro_jun.Models;
using System;
using System.Threading.Tasks;
using static Ql_NhaTro_jun.Controllers.PaymentHistoryController;
using static Ql_NhaTro_jun.Controllers.HoaDonTongController;

namespace SonarCloud_QL_NhaTro
{
    // ════════════════════════════════════════════════════════════════
    //  PaymentHistoryController Tests
    // ════════════════════════════════════════════════════════════════
    [TestFixture]
    public class PaymentHistoryControllerTests
    {
        private QlNhatroContext _context;
        private Mock<ILogger<PaymentHistoryController>> _mockLogger;
        private PaymentHistoryController _controller;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<QlNhatroContext>()
                .UseInMemoryDatabase($"PaymentDb_{Guid.NewGuid()}")
                .Options;
            _context = new QlNhatroContext(options);
            _mockLogger = new Mock<ILogger<PaymentHistoryController>>();
            _controller = new PaymentHistoryController(_mockLogger.Object, _context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        // ─── GetPaymentHistory ──────────────────────────────────────

        [Test]
        public async Task GetPaymentHistory_ReturnsOk_WhenEmpty()
        {
            var result = await _controller.GetPaymentHistory(1);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetPaymentHistory_ReturnsOk_WithData()
        {
            _context.LichSuThanhToans.Add(new LichSuThanhToan
            {
                MaThanhToan = 1,
                MaHopDong = 5,
                NgayThanhToan = DateTime.Now,
                SoTien = 1500000m,
                PhuongThuc = "Chuyển khoản",
                GhiChu = "Tháng 3"
            });
            await _context.SaveChangesAsync();

            var result = await _controller.GetPaymentHistory(5);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetPaymentHistory_ReturnsOk_WhenContractIdNotMatch()
        {
            _context.LichSuThanhToans.Add(new LichSuThanhToan
            {
                MaThanhToan = 1, MaHopDong = 5, NgayThanhToan = DateTime.Now, SoTien = 500000m
            });
            await _context.SaveChangesAsync();

            // Query different contract ID
            var result = await _controller.GetPaymentHistory(999);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        // ─── CreatePaymentHistory ───────────────────────────────────

        [Test]
        public async Task CreatePaymentHistory_ReturnsBadRequest_WhenModelIsNull()
        {
            var result = await _controller.CreatePaymentHistory(null);
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task CreatePaymentHistory_ReturnsOk_WhenValidModel()
        {
            var model = new LichSuThanhToanDTO
            {
                MaHopDong = 1,
                NgayThanhToan = DateTime.Now,
                SoTien = 2000000m,
                PhuongThuc = "Tiền mặt",
                GhiChu = "Thanh toán đủ"
            };

            var result = await _controller.CreatePaymentHistory(model);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task CreatePaymentHistory_SavesCorrectData()
        {
            var model = new LichSuThanhToanDTO
            {
                MaHopDong = 2,
                NgayThanhToan = new DateTime(2025, 3, 1),
                SoTien = 3500000m,
                PhuongThuc = "Chuyển khoản",
                GhiChu = "Tháng 3/2025"
            };

            await _controller.CreatePaymentHistory(model);

            var saved = await _context.LichSuThanhToans.FirstOrDefaultAsync(p => p.MaHopDong == 2);
            Assert.That(saved, Is.Not.Null);
            Assert.That(saved.SoTien, Is.EqualTo(3500000m));
        }
    }

    // ════════════════════════════════════════════════════════════════
    //  HoaDonTongController Tests
    // ════════════════════════════════════════════════════════════════
    [TestFixture]
    public class HoaDonTongControllerTests
    {
        private QlNhatroContext _context;
        private Mock<ILogger<HoaDonTongController>> _mockLogger;
        private HoaDonTongController _controller;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<QlNhatroContext>()
                .UseInMemoryDatabase($"HoaDonTongDb_{Guid.NewGuid()}")
                .Options;
            _context = new QlNhatroContext(options);
            _mockLogger = new Mock<ILogger<HoaDonTongController>>();
            _controller = new HoaDonTongController(_mockLogger.Object, _context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        // ─── GetHoaDonTong ──────────────────────────────────────────

        [Test]
        public async Task GetHoaDonTong_ReturnsOk_WhenEmpty()
        {
            var result = await _controller.GetHoaDonTong();
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetHoaDonTong_ReturnsOk_WithData()
        {
            _context.HoaDonTongs.Add(new HoaDonTong
            {
                MaHoaDon = 1,
                MaHopDong = 1,
                NgayXuat = new DateOnly(2025, 3, 1),
                TongTien = 5000000m,
                GhiChu = "Hóa đơn tháng 3"
            });
            await _context.SaveChangesAsync();

            var result = await _controller.GetHoaDonTong();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        // ─── CreateHoaDonTong ───────────────────────────────────────

        [Test]
        public async Task CreateHoaDonTong_ReturnsBadRequest_WhenModelIsNull()
        {
            var result = await _controller.CreateHoaDonTong(null);
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task CreateHoaDonTong_ReturnsOk_WhenValidModel()
        {
            var model = new HoaDonTongDTO
            {
                MaHopDong = 1,
                NgayXuat = new DateTime(2025, 3, 1),
                TongTien = 4000000m,
                GhiChu = "Tháng 3"
            };

            var result = await _controller.CreateHoaDonTong(model);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        // ─── EditHoaDonTong ─────────────────────────────────────────

        [Test]
        public async Task EditHoaDonTong_ReturnsBadRequest_WhenModelIsNull()
        {
            var result = await _controller.EditHoaDonTong(1, null);
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task EditHoaDonTong_ReturnsBadRequest_WhenIdInvalid()
        {
            var model = new HoaDonTongDTO { MaHopDong = 1, NgayXuat = DateTime.Now, TongTien = 100 };
            var result = await _controller.EditHoaDonTong(0, model);
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task EditHoaDonTong_ReturnsNotFound_WhenNotExist()
        {
            var model = new HoaDonTongDTO { MaHopDong = 1, NgayXuat = DateTime.Now, TongTien = 100 };
            var result = await _controller.EditHoaDonTong(999, model);
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task EditHoaDonTong_ReturnsOk_WhenFound()
        {
            _context.HoaDonTongs.Add(new HoaDonTong
            {
                MaHoaDon = 1, MaHopDong = 1, NgayXuat = new DateOnly(2025, 1, 1), TongTien = 1000000m
            });
            await _context.SaveChangesAsync();

            var model = new HoaDonTongDTO
            {
                MaHopDong = 1,
                NgayXuat = new DateTime(2025, 4, 1),
                TongTien = 2000000m,
                GhiChu = "Updated"
            };

            var result = await _controller.EditHoaDonTong(1, model);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        // ─── DeleteHoaDonTong ───────────────────────────────────────

        [Test]
        public async Task DeleteHoaDonTong_ReturnsBadRequest_WhenIdInvalid()
        {
            var result = await _controller.DeleteHoaDonTong(0);
            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task DeleteHoaDonTong_ReturnsNotFound_WhenNotExist()
        {
            var result = await _controller.DeleteHoaDonTong(999);
            Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task DeleteHoaDonTong_ReturnsOk_WhenFound()
        {
            _context.HoaDonTongs.Add(new HoaDonTong
            {
                MaHoaDon = 1, MaHopDong = 1, TongTien = 500000m
            });
            await _context.SaveChangesAsync();

            var result = await _controller.DeleteHoaDonTong(1);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }
    }
}
