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
    public class RentalStatusController(ScootersContext context) : Controller
    {
        private readonly ScootersContext _context = context;

        // GET: RentalStatus
        public async Task<IActionResult> Index()
        {
            return View(await _context.RentalStatuses.ToListAsync());
        }

        // GET: RentalStatus/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rentalStatus = await _context.RentalStatuses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rentalStatus == null)
            {
                return NotFound();
            }

            return View(rentalStatus);
        }

        // GET: RentalStatus/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: RentalStatus/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Id")] RentalStatus rentalStatus)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rentalStatus);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(rentalStatus);
        }

        // GET: RentalStatus/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rentalStatus = await _context.RentalStatuses.FindAsync(id);
            if (rentalStatus == null)
            {
                return NotFound();
            }
            return View(rentalStatus);
        }

        // POST: RentalStatus/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,Id")] RentalStatus rentalStatus)
        {
            if (id != rentalStatus.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rentalStatus);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RentalStatusExists(rentalStatus.Id))
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
            return View(rentalStatus);
        }

        // GET: RentalStatus/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rentalStatus = await _context.RentalStatuses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rentalStatus == null)
            {
                return NotFound();
            }

            return View(rentalStatus);
        }

        // POST: RentalStatus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rentalStatus = await _context.RentalStatuses.FindAsync(id);
            if (rentalStatus != null)
            {
                _context.RentalStatuses.Remove(rentalStatus);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RentalStatusExists(int id)
        {
            return _context.RentalStatuses.Any(e => e.Id == id);
        }
    }
}
