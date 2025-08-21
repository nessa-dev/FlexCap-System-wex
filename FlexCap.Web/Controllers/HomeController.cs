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
            ViewData["Title"] = "Home RH";
            return View();
        }

        public IActionResult Manager()
        {
            ViewData["Title"] = "Home Manager";
            return View();
        }

        public IActionResult Colaborador()
        {
            ViewData["Title"] = "Home Colaborador";
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }
}
