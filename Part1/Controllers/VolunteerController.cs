using Microsoft.AspNetCore.Mvc;
using Part1.Models;

namespace Part1.Controllers
{
    public class VolunteerController : Controller
    {
        public static List<Volunteer> allVolunteers = new List<Volunteer>();

        public IActionResult Volunteer() => View(new Volunteer());

        [HttpPost]
        public IActionResult Volunteer(Volunteer v)
        {
            v.Id = allVolunteers.Count + 1;
            v.ApplicationDate = DateTime.Now;
            v.Status = "Pending";
            allVolunteers.Add(v);
            return View("Confirmation", v);
        }

        public IActionResult ViewVolunteers() => View(allVolunteers);
        public IActionResult Confirmation() => View();
    }
}