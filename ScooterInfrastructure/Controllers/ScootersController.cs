using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization; // Додано для авторизації
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ScooterDomain.Model;
using ScooterInfrastructure;

namespace ScooterInfrastructure.Controllers
{
    public class ScootersController : Controller
    {
        private readonly ScootersContext _context;

        public ScootersController(ScootersContext context)
        {
            _context = context;
        }

        // GET: Scooters
        [Authorize(Roles = "User,Admin")] // Доступ для User і Admin
        public async Task<IActionResult> Index()
        {
            var scootersQuery = _context.Scooters.AsQueryable();
            if (!User.IsInRole("Admin"))
            {
                scootersQuery = scootersQuery.Where(s => s.StatusId == 1);
            }
            var scootersContext = scootersQuery
                .Include(s => s.Station)
                .Include(s => s.Status);
            return View(await scootersContext.ToListAsync());
        }

        // GET: Scooters/Details/5
        [Authorize(Roles = "Admin")] // Лише для Admin
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var scooter = await _context.Scooters
                .Include(s => s.Station)
                .Include(s => s.Status)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (scooter == null)
            {
                return NotFound();
            }

            return View(scooter);
        }

        // GET: Scooters/Create
        [Authorize(Roles = "Admin")] // Лише для Admin
        public IActionResult Create()
        {
            ViewData["StationId"] = new SelectList(_context.ChargingStations, "Id", "Location");
            ViewData["StatusId"] = new SelectList(_context.ScooterStatuses, "Id", "Name");
            return View();
        }

        // POST: Scooters/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Лише для Admin
        public async Task<IActionResult> Create([Bind("Model,BatteryLevel,StatusId,CurrentLocation,StationId,Id")] Scooter scooter)
        {
            ModelState.Remove("Station");
            ModelState.Remove("Status");
            if (ModelState.IsValid)
            {
                var status = await _context.ScooterStatuses.FindAsync(scooter.StatusId);
                if (status == null)
                {
                    ModelState.AddModelError("StatusId", "Invalid StatusId. No corresponding ScooterStatus found.");
                }
                else
                {
                    scooter.Status = status;
                }

                if (scooter.StationId.HasValue)
                {
                    var station = await _context.ChargingStations.FindAsync(scooter.StationId);
                    if (station == null)
                    {
                        ModelState.AddModelError("StationId", "Invalid StationId. No corresponding ChargingStation found.");
                    }
                    else
                    {
                        scooter.Station = station;
                    }
                }

                if (ModelState.IsValid)
                {
                    _context.Add(scooter);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    Console.WriteLine($"Property: {state.Key}, Error: {error.ErrorMessage}");
                }
            }

            ViewData["StationId"] = new SelectList(_context.ChargingStations, "Id", "Location", scooter.StationId);
            ViewData["StatusId"] = new SelectList(_context.ScooterStatuses, "Id", "Name", scooter.StatusId);
            return View(scooter);
        }

        // GET: Scooters/Edit/5
        [Authorize(Roles = "Admin")] // Лише для Admin
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var scooter = await _context.Scooters.FindAsync(id);
            if (scooter == null)
            {
                return NotFound();
            }
            ViewData["StationId"] = new SelectList(_context.ChargingStations, "Id", "Location", scooter.StationId);
            ViewData["StatusId"] = new SelectList(_context.ScooterStatuses, "Id", "Name", scooter.StatusId);
            return View(scooter);
        }

        // POST: Scooters/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Лише для Admin
        public async Task<IActionResult> Edit(int id, [Bind("Model,BatteryLevel,StatusId,CurrentLocation,StationId,Id")] Scooter scooter)
        {
            if (id != scooter.Id)
            {
                return NotFound();
            }
            ModelState.Remove("Station");
            ModelState.Remove("Status");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(scooter);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ScooterExists(scooter.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["StationId"] = new SelectList(_context.ChargingStations, "Id", "Location", scooter.StationId);
            ViewData["StatusId"] = new SelectList(_context.ScooterStatuses, "Id", "Name", scooter.StatusId);
            return View(scooter);
        }

        // GET: Scooters/Delete/5
        [Authorize(Roles = "Admin")] // Лише для Admin
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var scooter = await _context.Scooters
                .Include(s => s.Station)
                .Include(s => s.Status)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (scooter == null)
            {
                return NotFound();
            }

            return View(scooter);
        }

        // POST: Scooters/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] // Лише для Admin
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var scooter = await _context.Scooters.FindAsync(id);
            if (scooter != null)
            {
                _context.Scooters.Remove(scooter);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ScooterExists(int id)
        {
            return _context.Scooters.Any(e => e.Id == id);
        }
    }
}