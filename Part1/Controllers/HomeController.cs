using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Part1.Models;

namespace Part1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // GET: Home/Index - Homepage where users select their role
        public IActionResult Index()
        {
            return View();
        }

        // GET: Home/Homepage - Display after successful login
        public IActionResult Homepage()
        {
            return View();
        }

        // GET: Home/Privacy - Privacy page
        public IActionResult Privacy()
        {
            return View();
        }

        // GET: Home/Error - Error handling
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}