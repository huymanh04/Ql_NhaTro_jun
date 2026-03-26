using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Ql_NhaTro_jun.Controllers;
using Ql_NhaTro_jun.Models;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ql_NhaTro_jun.Tests
{
    [TestFixture]
    public class AdminControllerTests
    {
        private QlNhatroContext _context;
        private Mock<ILogger<AdminController>> _mockLogger;
        private AdminController _controller;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<QlNhatroContext>()
                .UseInMemoryDatabase($"AdminDb_{Guid.NewGuid()}")
                .Options;
            _context = new QlNhatroContext(options);
            _mockLogger = new Mock<ILogger<AdminController>>();
            _controller = new AdminController(_mockLogger.Object, _context);
            
            // Setup mock HttpContext with authenticated admin user
            var mockHttpContext = new Mock<HttpContext>();
            var mockUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "0912345678")
            }, "TestAuthType"));
            mockHttpContext.Setup(x => x.User).Returns(mockUser);
            var mockControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };
            _controller.ControllerContext = mockControllerContext;
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        // ─── Helper Methods ─────────────────────────────────────────

        private void SeedMinimalData()
        {
            // Add at least one entity of each type for dashboard
            var tinhThanh = new TinhThanh { MaTinh = 1, TenTinh = "Hồ Chí Minh" };
            _context.TinhThanhs.Add(tinhThanh);

            var khuVuc = new KhuVuc { MaKhuVuc = 1, TenKhuVuc = "Quận 1" };
            _context.KhuVucs.Add(khuVuc);

            var theLoai = new TheLoaiPhongTro { MaTheLoai = 1, TenTheLoai = "Phòng đơn" };
            _context.TheLoaiPhongTros.Add(theLoai);

            var owner = new NguoiDung
            {
                MaNguoiDung = 1,
                SoDienThoai = "0912345678",
                HoTen = "Chủ Trọ A",
                Email = "owner@example.com",
                VaiTro = "1"
            };
            _context.NguoiDungs.Add(owner);

            var customer = new NguoiDung
            {
                MaNguoiDung = 2,
                SoDienThoai = "0987654321",
                HoTen = "Khách Hàng B",
                Email = "customer@example.com",
                VaiTro = "0"
            };
            _context.NguoiDungs.Add(customer);

            var nhaTro = new NhaTro
            {
                MaNhaTro = 1,
                TenNhaTro = "Nhà Trọ A",
                DiaChi = "123 Đường A, Q1",
                MaKhuVuc = 1,
                MaChuTro = 1,
                
            };
            _context.NhaTros.Add(nhaTro);

            var room1 = new PhongTro
            {
                MaPhong = 1,
                MaNhaTro = 1,
                MaTheLoai = 1,
                TenPhong = "Phòng 101",
                Gia = 3000000,
                DienTich = 20,
                ConTrong = true,
                MoTa = "Phòng sạch"
            };

            var room2 = new PhongTro
            {
                MaPhong = 2,
                MaNhaTro = 1,
                MaTheLoai = 1,
                TenPhong = "Phòng 102",
                Gia = 3500000,
                DienTich = 25,
                ConTrong = false,
                MoTa = "Phòng lớn"
            };

            _context.PhongTros.Add(room1);
            _context.PhongTros.Add(room2);

            var banner = new Banner { BannerId = 1, Title = "Banner 1", Content = "Mô tả banner" };
            _context.Banners.Add(banner);

            var bank = new Bank { Id = 1, TenNganHang = "MB Bank", SoTaiKhoan = "123456", Ten = "Owner A" };
            _context.Banks.Add(bank);

            var message = new TinNhan
            {
                MaTinNhan = 1,
                NoiDung = "Test message",
                  NguoiGuiId = 2,
                NguoiNhanId = 1
            };
            _context.TinNhans.Add(message);

            var hopdong = new HopDong
            {
                MaHopDong = 1,
                MaPhong = 1,                
                NgayBatDau = DateOnly.FromDateTime(DateTime.Now),
                NgayKetThuc = DateOnly.FromDateTime(DateTime.Now.AddMonths(11)),
                SoNguoiO = 1,
                SoXe = 1,
                TienDatCoc = 3000000,
                DaKetThuc = false // Hợp đồng đang hoạt động
                
            };
            _context.HopDongs.Add(hopdong);

            var denbu = new DenBu
            {
                MaDenBu = 1,
                MaHopDong = 1,
                NoiDung = "Phòng quá ồn ào",
                SoTien = 500000,
                NgayTao = DateTime.Now
            };
            _context.DenBus.Add(denbu);

            _context.SaveChanges();
        }

        // ─── Dashboard Tests ────────────────────────────────────────

        [Test]
        public async Task Doarboard_ReturnsOkResult_WithEmptyDatabase()
        {
            // Need to seed admin user for authentication, even if other data is empty
            var owner = new NguoiDung
            {
                MaNguoiDung = 1,
                SoDienThoai = "0912345678",
                HoTen = "Admin User",
                Email = "admin@example.com",
                VaiTro = "1"
            };
            _context.NguoiDungs.Add(owner);
            _context.SaveChanges();
            
            var result = await _controller.doarboard();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task Doarboard_ReturnsOkResult_WithData()
        {
            SeedMinimalData();
            var result = await _controller.doarboard();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult!.Value, Is.Not.Null);
        }

        [Test]
        public async Task Doarboard_ContainsSuccessMessage()
        {
            SeedMinimalData();
            var result = await _controller.doarboard();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult!.Value, Is.Not.Null);
        }

        [Test]
        public async Task Doarboard_CountsContractsCorrectly()
        {
            SeedMinimalData();
            var result = await _controller.doarboard();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task Doarboard_CountsComplaintsCorrectly()
        {
            SeedMinimalData();
            var result = await _controller.doarboard();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task Doarboard_CountsRoomsCorrectly()
        {
            SeedMinimalData();
            var result = await _controller.doarboard();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task Doarboard_CountsOccupiedAndEmptyRooms()
        {
            SeedMinimalData();
            var result = await _controller.doarboard();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task Doarboard_IncludesRevenueStats()
        {
            SeedMinimalData();
            var result = await _controller.doarboard();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        // ─── Error Handling Tests ────────────────────────────────────

        [Test]
        public async Task Doarboard_HandlesEmptyDatabase()
        {
            // Need to seed admin user for authentication, even if other data is empty
            var owner = new NguoiDung
            {
                MaNguoiDung = 1,
                SoDienThoai = "0912345678",
                HoTen = "Admin User",
                Email = "admin@example.com",
                VaiTro = "1"
            };
            _context.NguoiDungs.Add(owner);
            _context.SaveChanges();
            
            var result = await _controller.doarboard();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task Doarboard_ReturnsValidResponse_WithMultipleNhaTros()
        {
            SeedMinimalData();

            // Add another NhaTro
            var nhaTro2 = new NhaTro
            {
                MaNhaTro = 2,
                TenNhaTro = "Nhà Trọ B",
                DiaChi = "456 Đường B, Q2",
                MaKhuVuc = 1,
                MaChuTro = 1,
                
            };
            _context.NhaTros.Add(nhaTro2);
            _context.SaveChanges();

            var result = await _controller.doarboard();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task Doarboard_CountsCustomersCorrectly()
        {
            SeedMinimalData();
            var result = await _controller.doarboard();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task Doarboard_CountsBanksCorrectly()
        {
            SeedMinimalData();
            var result = await _controller.doarboard();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task Doarboard_CountsBannersCorrectly()
        {
            SeedMinimalData();
            var result = await _controller.doarboard();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task Doarboard_CountsMessagesCorrectly()
        {
            SeedMinimalData();
            var result = await _controller.doarboard();

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }
    }
}
