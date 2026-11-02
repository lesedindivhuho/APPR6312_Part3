using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Part1.Models;

namespace Part1.Controllers
{
    public class RegisterController : Controller
    {
        public static List<Users> allUsers = new List<Users>();

        [HttpGet]
        public IActionResult Login() => View();

        [HttpGet]
        public IActionResult Dlogin() => View();

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public IActionResult RegisterUser(Users user)
        {
            if (allUsers.Any(u => u.Username == user.Username))
            {
                ViewBag.Error = "Username already exists!";
                return View("Register", user);
            }

            user.Id = allUsers.Count + 1;
            user.CreatedAt = DateTime.Now;
            allUsers.Add(user);

            TempData["Success"] = "Registration successful!";
            return user.role == "Donor" ? RedirectToAction("Dlogin") : RedirectToAction("Login");
        }

        [HttpPost]
        public IActionResult Login(Users user)
        {
            var found = allUsers.FirstOrDefault(u =>
                u.Username == user.Username &&
                u.Password == user.Password &&
                u.role == "Employee");

            if (found != null)
                return RedirectToAction("Dashboard", "Employee");

            ViewBag.Error = "Invalid credentials";
            return View();
        }

        [HttpPost]
        public IActionResult Dlogin(Users user)
        {
            var found = allUsers.FirstOrDefault(u =>
                u.Username == user.Username &&
                u.Password == user.Password &&
                u.role == "Donor");

            if (found != null)
                return RedirectToAction("Donate", "Donor");

            ViewBag.Error = "Invalid credentials";
            return View();
        }
    }
}