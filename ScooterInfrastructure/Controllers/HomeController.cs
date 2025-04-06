using Microsoft.AspNetCore.Mvc;
using ScooterDomain.Model;
using ScooterInfrastructure;

namespace ScooterInfrastructure.Controllers
{
    public class HomeController : Controller
    {
        private readonly ScootersContext _context;

        public HomeController(ScootersContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.AvailableScooters = _context.Scooters.Count(s => s.Status.Name == "Доступний");
            ViewBag.ChargingStationsCount = _context.ChargingStations.Count();
            ViewBag.ActiveRentals = _context.Rentals.Count(r => r.StatusId == 1);

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}