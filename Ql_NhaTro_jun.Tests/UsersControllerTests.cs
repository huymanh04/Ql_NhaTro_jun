using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public class UsersControllerTests
    {
        private QlNhatroContext _context;
        private UsersController _controller;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<QlNhatroContext>()
                .UseInMemoryDatabase($"UsersDb_{Guid.NewGuid()}")
                .Options;
            _context = new QlNhatroContext(options);
            _controller = new UsersController(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
            _controller?.Dispose();
        }

        private void SetupAuthenticatedUser(string userName, string vaiTro = "0")
        {
            _context.NguoiDungs.Add(new NguoiDung
            {
                MaNguoiDung = 1,
                SoDienThoai = userName,
                HoTen = "Test User",
                Email = "test@example.com",
                VaiTro = vaiTro
            });
            _context.SaveChanges();

            var claims = new[] { new Claim(ClaimTypes.Name, userName) };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        private void SetupUnauthenticatedUser()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        // ─── Index ──────────────────────────────────────────────────

        [Test]
        public void Index_ReturnsViewResult()
        {
            SetupUnauthenticatedUser();
            var result = _controller.Index();
            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        // ─── Login ──────────────────────────────────────────────────

        [Test]
        public async Task Login_ReturnsLoginView_WhenUserNotAuthenticated()
        {
            SetupUnauthenticatedUser();
            var result = await _controller.Login();

            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult!.ViewName, Is.Null.Or.EqualTo("Login"));
        }

        [Test]
        public async Task Login_RedirectsToHome_WhenUserAuthenticated()
        {
            SetupAuthenticatedUser("0912345678", "0");
            var result = await _controller.Login();

            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult!.ActionName, Is.EqualTo("index"));
            Assert.That(redirectResult.ControllerName, Is.EqualTo("Home"));
        }

        // ─── Register ───────────────────────────────────────────────

        [Test]
        public async Task Register_ReturnsRegisterView_WhenUserNotAuthenticated()
        {
            SetupUnauthenticatedUser();
            var result = await _controller.Register();

            Assert.That(result, Is.InstanceOf<ViewResult>());
            var viewResult = result as ViewResult;
            Assert.That(viewResult!.ViewName, Is.Null.Or.EqualTo("Register"));
        }

        [Test]
        public async Task Register_RedirectsToHome_WhenUserAuthenticated()
        {
            SetupAuthenticatedUser("0912345678", "0");
            var result = await _controller.Register();

            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult!.ActionName, Is.EqualTo("index"));
            Assert.That(redirectResult.ControllerName, Is.EqualTo("Home"));
        }

        // ─── Verycode (Email Verification) ──────────────────────────

        [Test]
        public void Verycode_ReturnsViewResult()
        {
            SetupUnauthenticatedUser();
            var result = _controller.Verycode();

            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        // ─── ForgotPassword ──────────────────────────────────────────

        [Test]
        public void ForgotPassword_ReturnsViewResult()
        {
            SetupUnauthenticatedUser();
            var result = _controller.ForgotPassword();

            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        // ─── Contract ────────────────────────────────────────────────

        [Test]
        public async Task Contract_ReturnsViewResult()
        {
            SetupUnauthenticatedUser();
            var result = await _controller.Contract();

            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        // ─── HoaDonTong (Invoice) ────────────────────────────────────

        [Test]
        public void HoaDonTong_ReturnsViewResult()
        {
            SetupUnauthenticatedUser();
            var result = _controller.HoaDonTong();

            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public void HoaDonTong_SetsUserIdInViewBag()
        {
            SetupUnauthenticatedUser();
            _controller.ControllerContext.HttpContext.Items["id"] = 123;

            var result = _controller.HoaDonTong();

            Assert.That(result, Is.InstanceOf<ViewResult>());
            Assert.That(_controller.ViewBag.UserId, Is.EqualTo(123));
        }

        // ─── Dashboard (Dashborad) ───────────────────────────────────

        [Test]
        public async Task Dashborad_RedirectsToLogin_WhenUserNotAuthenticated()
        {
            SetupUnauthenticatedUser();
            var result = await _controller.Dashborad();

            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult!.ActionName, Is.EqualTo("Login"));
            Assert.That(redirectResult.ControllerName, Is.EqualTo("Users"));
        }

        [Test]
        public async Task Dashborad_RedirectsToLogin_WhenUserNameIsNull()
        {
            var claims = new Claim[] { };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            var result = await _controller.Dashborad();

            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
        }

        [Test]
        public async Task Dashborad_RedirectsToLogin_WhenUserNotFound()
        {
            SetupUnauthenticatedUser();
            var claims = new[] { new Claim(ClaimTypes.Name, "nonexistent@example.com") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext.HttpContext = new DefaultHttpContext { User = principal };

            var result = await _controller.Dashborad();

            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult!.ActionName, Is.EqualTo("Login"));
        }

        [Test]
        public async Task Dashborad_ReturnsBadRequest_WhenUserRoleIsNotTenant()
        {
            SetupAuthenticatedUser("0912345678", "1"); // Manager role
            var result = await _controller.Dashborad();

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task Dashborad_ReturnsViewResult_WhenUserIsTenant()
        {
            SetupAuthenticatedUser("0912345678", "0"); // Tenant role
            var result = await _controller.Dashborad();

            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        [Test]
        public async Task Dashborad_ReturnsViewResult_WhenUserFoundByEmail()
        {
            SetupUnauthenticatedUser();
            _context.NguoiDungs.Add(new NguoiDung
            {
                MaNguoiDung = 2,
                SoDienThoai = "0987654321",
                HoTen = "Test User 2",
                Email = "test2@example.com",
                VaiTro = "0"
            });
            _context.SaveChanges();

            var claims = new[] { new Claim(ClaimTypes.Name, "test2@example.com") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext.HttpContext = new DefaultHttpContext { User = principal };

            var result = await _controller.Dashborad();

            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        // ─── Denbu (Complaint) ───────────────────────────────────────

        [Test]
        public void Denbu_RedirectsToLogin_WhenUserNotAuthenticated()
        {
            SetupUnauthenticatedUser();
            var result = _controller.Denbu();

            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult!.ActionName, Is.EqualTo("Login"));
            Assert.That(redirectResult.ControllerName, Is.EqualTo("Users"));
        }

        [Test]
        public void Denbu_ReturnsViewResult_WhenUserAuthenticated()
        {
            SetupAuthenticatedUser("0912345678", "0");
            var result = _controller.Denbu();

            Assert.That(result, Is.InstanceOf<ViewResult>());
        }

        // ─── Chatbot ─────────────────────────────────────────────────

        [Test]
        public void Chatbot_RedirectsToLogin_WhenUserNotAuthenticated()
        {
            SetupUnauthenticatedUser();
            var result = _controller.Chatbot();

            Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
            var redirectResult = result as RedirectToActionResult;
            Assert.That(redirectResult!.ActionName, Is.EqualTo("Login"));
        }

        [Test]
        public void Chatbot_ReturnsViewResult_WhenUserAuthenticated()
        {
            SetupAuthenticatedUser("0912345678", "0");
            var result = _controller.Chatbot();

            Assert.That(result, Is.InstanceOf<ViewResult>());
        }
    }
}
