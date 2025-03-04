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
    public class RidersController : Controller
    {
        private readonly ScootersContext _context;

        public RidersController(ScootersContext context)
        {
            _context = context;
        }

        // GET: Riders
        public async Task<IActionResult> Index()
        {
            return View(await _context.Riders.ToListAsync());
        }

        // GET: Riders/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rider = await _context.Riders
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rider == null)
            {
                return NotFound();
            }

            //return View(rider);
            return RedirectToAction("Index", "Rentals", new
            {
                id = rider.Id,
                firstName = rider.FirstName,
                lastName = rider.LastName
            });
        }

        // GET: Riders/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Riders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,PhoneNumber,RegistrationDate,AccountBalance,Id")] Rider rider)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rider);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(rider);
        }

        // GET: Riders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rider = await _context.Riders.FindAsync(id);
            if (rider == null)
            {
                return NotFound();
            }
            return View(rider);
        }

        // POST: Riders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FirstName,LastName,PhoneNumber,RegistrationDate,AccountBalance,Id")] Rider rider)
        {
            if (id != rider.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rider);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RiderExists(rider.Id))
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
            return View(rider);
        }

        // GET: Riders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rider = await _context.Riders
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rider == null)
            {
                return NotFound();
            }

            return View(rider);
        }

        // POST: Riders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rider = await _context.Riders.FindAsync(id);
            if (rider != null)
            {
                _context.Riders.Remove(rider);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Riders/Discounts/5
        public async Task<IActionResult> Discounts(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rider = await _context.Riders
                .Include(r => r.Discounts) // Завантажуємо знижки
                .FirstOrDefaultAsync(m => m.Id == id);

            if (rider == null)
            {
                return NotFound();
            }

            return View(rider);
        }

        private bool RiderExists(int id)
        {
            return _context.Riders.Any(e => e.Id == id);
        }

    }
}
