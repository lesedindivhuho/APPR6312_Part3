using GiftOfTheGivers.Helpers;
using Microsoft.AspNetCore.Mvc;
using Part1.Models;
using Part1.Services;

namespace Part1.Controllers
{
    public class DonorController : Controller
    {
        public static List<Donation> allDonations = new List<Donation>();
        public static List<Clothes> allClothes = new List<Clothes>();
        public static List<Food> allFood = new List<Food>();

        private static readonly object DonationLock = new();

        private readonly FunctionsClient _functions;

        public DonorController(FunctionsClient functions)
        {
            _functions = functions;
        }

        public IActionResult Donate() => View("Donation");

        [HttpPost]
        public async Task<IActionResult> DonateMoney(Donation d)
        {
            // Same rules the certificate function uses, so invalid donations are caught before calling it
            var errors = DonationValidator.Validate(
                d.DonationAmount,
                d.Currency,
                d.IsRecurring ? "recurring" : "once-off",
                d.DonorName,
                string.IsNullOrWhiteSpace(d.DonorName));

            if (errors.Count > 0)
            {
                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                return View("Donation");
            }

            lock (DonationLock)
            {
                d.Id = allDonations.Count + 1;
            }
            d.DonationDate = DateTime.Now;

            var certificate = await _functions.GenerateTaxCertificateAsync(d);
            if (certificate.Success)
            {
                d.CertificateNumber = certificate.CertificateNumber;
            }
            else
            {
                // Function unavailable or rejected the request, so issue a local reference instead
                d.CertificateNumber = $"TX-{d.Id}-{DateTime.Now.Year}";
                ViewBag.CertificateNote = certificate.ErrorMessage;
            }

            lock (DonationLock)
            {
                allDonations.Add(d);
            }
            return View("TaxCertificate", d);
        }

        [HttpPost]
        public IActionResult DonateClothes(Clothes c)
        {
            c.Id = allClothes.Count + 1;
            allClothes.Add(c);
            return View("ClothesConfirmation", c);
        }

        [HttpPost]
        public IActionResult DonateFood(Food f)
        {
            f.Id = allFood.Count + 1;
            allFood.Add(f);
            return View("FoodConfirmation", f);
        }

        public IActionResult TaxCertificate(Donation d) => View(d);
        public IActionResult ClothesConfirmation() => View();
        public IActionResult FoodConfirmation() => View();
    }
}