using Microsoft.AspNetCore.Authorization; // Додано для авторизації
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScooterInfrastructure;

namespace ScooterInfrastructure.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")] // Обмеження доступу до всього контролера для Admin
    public class ChartsController : ControllerBase
    {
        private readonly ScootersContext _context;

        public ChartsController(ScootersContext context)
        {
            _context = context;
        }

        // Діаграма 1: Кількість самокатів за статусами
        [HttpGet("scootersByStatus")]
        public async Task<IActionResult> GetScootersByStatusAsync()
        {
            var data = await _context.Scooters
                .Include(s => s.Status)
                .GroupBy(s => s.Status.Name)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            return Ok(data);
        }

        // Діаграма 2: Кількість самокатів на станціях зарядки
        [HttpGet("scootersByStation")]
        public async Task<IActionResult> GetScootersByStationAsync()
        {
            var data = await _context.ChargingStations
                .Select(cs => new
                {
                    StationName = cs.Name,
                    Count = cs.CurrentScooterCount
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}