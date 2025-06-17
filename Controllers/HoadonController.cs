using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ql_NhaTro_jun.Models;

namespace Ql_NhaTro_jun.Controllers
{
    public class HoaDonController : Controller
    {
        private readonly ILogger<HoaDonController> _logger;
        private readonly QlNhatroContext _context;

        public HoaDonController(ILogger<HoaDonController> logger, QlNhatroContext context)
        {
            _logger = logger;
            _context = context;
        }

        // GET: HoadonController
        public ActionResult Index()
        {
            return View();
        }

        // GET: HoadonController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: HoadonController/Create
        public ActionResult ViewBill()
        {
            return View();
        }

        // POST: HoadonController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: HoadonController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: HoadonController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: HoadonController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: HoadonController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
