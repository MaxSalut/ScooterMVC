using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ScooterDomain.Model;
using ScooterInfrastructure;

namespace ScooterInfrastructure.Controllers
{
    public class ChargingStationsController : Controller
    {
        private readonly ScootersContext _context;

        public ChargingStationsController(ScootersContext context)
        {
            _context = context;
        }

        // GET: ChargingStations
        public async Task<IActionResult> Index()
        {
            return View(await _context.ChargingStations.ToListAsync());
        }

        // GET: ChargingStations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chargingStation = await _context.ChargingStations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (chargingStation == null)
            {
                return NotFound();
            }

            return View(chargingStation);
        }

        // GET: ChargingStations/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ChargingStations/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Location,ChargingSlots,CurrentScooterCount,Id")] ChargingStation chargingStation)
        {
            if (ModelState.IsValid)
            {
                _context.Add(chargingStation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(chargingStation);
        }

        // GET: ChargingStations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chargingStation = await _context.ChargingStations.FindAsync(id);
            if (chargingStation == null)
            {
                return NotFound();
            }
            return View(chargingStation);
        }

        // POST: ChargingStations/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,Location,ChargingSlots,CurrentScooterCount,Id")] ChargingStation chargingStation)
        {
            if (id != chargingStation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(chargingStation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChargingStationExists(chargingStation.Id))
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
            return View(chargingStation);
        }

        // GET: ChargingStations/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var chargingStation = await _context.ChargingStations
                .FirstOrDefaultAsync(m => m.Id == id);
            if (chargingStation == null)
            {
                return NotFound();
            }

            return View(chargingStation);
        }

        // POST: ChargingStations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var chargingStation = await _context.ChargingStations.FindAsync(id);
            if (chargingStation != null)
            {
                _context.ChargingStations.Remove(chargingStation);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ChargingStationExists(int id)
        {
            return _context.ChargingStations.Any(e => e.Id == id);
        }
    }
}
