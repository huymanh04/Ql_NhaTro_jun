using Microsoft.AspNetCore.Mvc;

namespace Ql_NhaTro_jun.Controllers
{
    public class NewsController : Controller
    {
        // GET: /News
        public IActionResult Index()
        {
            // Nếu sau này lấy tin từ DB/API thì truyền model vào view
            return View();
        }

        // GET: /News/Detail/{id}
        public IActionResult Detail(int id)
        {
            // Lấy chi tiết tin từ DB/API theo id, truyền model vào view
            // Hiện tại trả về view mẫu
            return View();
        }
    }
} 