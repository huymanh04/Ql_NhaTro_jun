using Microsoft.AspNetCore.Mvc;

namespace Ql_NhaTro_jun.Controllers
{
    public class TinTucController : Controller
    {
        public IActionResult ChiTiet(int id)
        {
            ViewBag.BaiVietId = id;
            return View($"ChiTiet{id}");
        }
    }
} 