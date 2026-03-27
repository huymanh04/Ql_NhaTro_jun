using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Ql_NhaTro_jun.Controllers;
using Ql_NhaTro_jun.Models;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ql_NhaTro_jun.Tests;

[TestFixture]
public class SimpleControllerSmokeTests
{
    [Test]
    public void ValuesController_BasicEndpoints_Work()
    {
        var controller = new ValuesController();

        var all = controller.Get();
        var one = controller.Get(1);

        Assert.That(all, Is.Not.Null);
        Assert.That(one, Is.EqualTo("value"));

        Assert.DoesNotThrow(() => controller.Post("x"));
        Assert.DoesNotThrow(() => controller.Put(1, "x"));
        Assert.DoesNotThrow(() => controller.Delete(1));
    }

    [Test]
    public void TinTucController_ChiTiet_ReturnsExpectedViewName()
    {
        var controller = new TinTucController();

        var result = controller.ChiTiet(3) as ViewResult;

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.ViewName, Is.EqualTo("ChiTiet3"));
        Assert.That(controller.ViewBag.BaiVietId, Is.EqualTo(3));
    }

    [Test]
    public void VitriController_Actions_ReturnView()
    {
        var controller = new vitri();

        Assert.That(controller.KhuVuc(), Is.InstanceOf<ViewResult>());
        Assert.That(controller.TinhThanh(), Is.InstanceOf<ViewResult>());
    }

    [Test]
    public void TypeRoomController_Actions_ReturnView()
    {
        var controller = new TypeRoom();

        Assert.That(controller.Index(), Is.InstanceOf<ViewResult>());
        Assert.That(controller.Create(), Is.InstanceOf<ViewResult>());
    }

    [Test]
    public void PhongtroController_Actions_ReturnView()
    {
        var controller = new Phongtro();

        Assert.That(controller.Index(), Is.InstanceOf<ViewResult>());
        Assert.That(controller.Detail(), Is.InstanceOf<ViewResult>());
    }

    [Test]
    public void ContractManagementController_Index_RedirectsForTenant()
    {
        var controller = new ContractManagementController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        controller.HttpContext.Items["role"] = "0";

        var result = controller.Index();

        Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
    }

    [Test]
    public void ContractManagementController_Index_ReturnsViewForManager()
    {
        var controller = new ContractManagementController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        controller.HttpContext.Items["role"] = "1";

        var result = controller.Index();

        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public void WebSettingsController_Index_RedirectsWhenNotAuthenticated()
    {
        var controller = new WebSettingsController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            }
        };

        var result = controller.Index();

        Assert.That(result, Is.InstanceOf<RedirectToActionResult>());
    }

    [Test]
    public void WebSettingsController_Index_ReturnsViewWhenAuthenticated()
    {
        var claims = new[] { new Claim(ClaimTypes.Name, "u") };
        var controller = new WebSettingsController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
                }
            }
        };

        var result = controller.Index();

        Assert.That(result, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public async Task RoomManagementController_Index_CoversCommonBranches()
    {
        var options = new DbContextOptionsBuilder<QlNhatroContext>()
            .UseInMemoryDatabase($"RoomMgmt_{Guid.NewGuid()}")
            .Options;

        using var context = new QlNhatroContext(options);
        var controller = new RoomManagementController(context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            }
        };

        var unauth = await controller.Index();
        Assert.That(unauth, Is.InstanceOf<RedirectToActionResult>());

        context.NguoiDungs.Add(new NguoiDung
        {
            MaNguoiDung = 11,
            SoDienThoai = "0900000001",
            Email = "tenant@test.com",
            VaiTro = "0",
            HoTen = "Tenant"
        });
        context.NguoiDungs.Add(new NguoiDung
        {
            MaNguoiDung = 12,
            SoDienThoai = "0900000002",
            Email = "manager@test.com",
            VaiTro = "1",
            HoTen = "Manager"
        });
        await context.SaveChangesAsync();

        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "tenant@test.com") }, "TestAuth"));
        var tenantResult = await controller.Index();
        Assert.That(tenantResult, Is.InstanceOf<RedirectToActionResult>());

        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(
            new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "manager@test.com") }, "TestAuth"));
        var managerResult = await controller.Index();
        Assert.That(managerResult, Is.InstanceOf<ViewResult>());
    }

    [Test]
    public async Task HomeController_BasicActions_AndChatbotUnauth_Work()
    {
        var options = new DbContextOptionsBuilder<QlNhatroContext>()
            .UseInMemoryDatabase($"HomeDb_{Guid.NewGuid()}")
            .Options;

        using var context = new QlNhatroContext(options);
        var logger = new Mock<ILogger<HomeController>>();

        var controller = new HomeController(logger.Object, context)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity())
                }
            }
        };

        Assert.That(await controller.Index(), Is.InstanceOf<ViewResult>());
        Assert.That(await controller.About(), Is.InstanceOf<ViewResult>());
        Assert.That(await controller.Contact(), Is.InstanceOf<ViewResult>());
        Assert.That(await controller.Chatbot(), Is.InstanceOf<RedirectToActionResult>());
        Assert.That(controller.Error(), Is.InstanceOf<ViewResult>());
    }
}
