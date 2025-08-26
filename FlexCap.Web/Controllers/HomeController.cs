using System.Diagnostics;
using FlexCap.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace FlexCap.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        //Page Home
        public IActionResult Rh()
        {
            ViewData["UserRole"] = "Home RH";
            ViewData["Profile"] = "Rh";
            return View();
        }

        public IActionResult Manager()
        {
            ViewData["UserRole"] = "Home Manager";
            ViewData["Profile"] = "Manager";
            return View();
        }

        public IActionResult Colaborador()
        {
            ViewData["UserRole"] = "Home Colaborador";
            ViewData["Profile"] = "Colaborador";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
