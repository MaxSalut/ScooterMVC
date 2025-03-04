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
    public class ScooterStatusController : Controller
    {
        private readonly ScootersContext _context;

        public ScooterStatusController(ScootersContext context)
        {
            _context = context;
        }

        // GET: ScooterStatus
        public async Task<IActionResult> Index()
        {
            return View(await _context.ScooterStatuses.ToListAsync());
        }

        // GET: ScooterStatus/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var scooterStatus = await _context.ScooterStatuses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (scooterStatus == null)
            {
                return NotFound();
            }

            return View(scooterStatus);
        }

        // GET: ScooterStatus/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ScooterStatus/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Id")] ScooterStatus scooterStatus)
        {
            if (ModelState.IsValid)
            {
                _context.Add(scooterStatus);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(scooterStatus);
        }

        // GET: ScooterStatus/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var scooterStatus = await _context.ScooterStatuses.FindAsync(id);
            if (scooterStatus == null)
            {
                return NotFound();
            }
            return View(scooterStatus);
        }

        // POST: ScooterStatus/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,Id")] ScooterStatus scooterStatus)
        {
            if (id != scooterStatus.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(scooterStatus);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ScooterStatusExists(scooterStatus.Id))
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
            return View(scooterStatus);
        }

        // GET: ScooterStatus/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var scooterStatus = await _context.ScooterStatuses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (scooterStatus == null)
            {
                return NotFound();
            }

            return View(scooterStatus);
        }

        // POST: ScooterStatus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var scooterStatus = await _context.ScooterStatuses.FindAsync(id);
            if (scooterStatus != null)
            {
                _context.ScooterStatuses.Remove(scooterStatus);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ScooterStatusExists(int id)
        {
            return _context.ScooterStatuses.Any(e => e.Id == id);
        }
    }
}
