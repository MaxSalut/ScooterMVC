using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ScooterDomain.Model;
using ScooterInfrastructure;

namespace ScooterInfrastructure.Controllers
{
    public class RentalsController : Controller
    {
        private readonly ScootersContext _context;

        public RentalsController(ScootersContext context)
        {
            _context = context; 
        }
        
        // GET: Rentals
        public async Task<IActionResult> Index(int? id, string firstName, string lastName)
        {
            if (id == null)
            {
                return NotFound();
            }

            var scootersContext = _context.Rentals
                .Include(r => r.PaymentMethod)
                .Include(r => r.Rider)
                .Include(r => r.Scooter)
                .Include(r => r.Status)
                .Where(r => r.RiderId == id);

            ViewBag.RiderId = id; // Додаємо для передачі в Create
            ViewBag.FirstName = firstName;
            ViewBag.LastName = lastName;

            return View(await scootersContext.ToListAsync());
        }

        // GET: Rentals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rental = await _context.Rentals
                .Include(r => r.PaymentMethod)
                .Include(r => r.Rider)
                .Include(r => r.Scooter)
                .Include(r => r.Status)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rental == null)
            {
                return NotFound();
            }

            return View(rental);
        }

        // GET: Rentals/Create
        public IActionResult Create()
        {
            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods, "Id", "Name");
            ViewData["RiderId"] = new SelectList(_context.Riders, "Id", "FirstName");
            ViewData["ScooterId"] = new SelectList(_context.Scooters, "Id", "Model");
            ViewData["StatusId"] = new SelectList(_context.RentalStatuses, "Id", "Name");
            return View();
        }

        // POST: Rentals/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
[ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RiderId,ScooterId,StatusId,StartTime,EndTime,TotalCost,PaymentDate,Amount,PaymentMethodId,Id")] Rental rental)
        {
            if (rental == null)
            {
                return BadRequest("Модель оренди не може бути null.");
            }

            ModelState.Remove("PaymentMethod");
            ModelState.Remove("Rider");
            ModelState.Remove("Scooter");
            ModelState.Remove("Status");

            // Явна валідація моделі
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(rental);
            if (!Validator.TryValidateObject(rental, validationContext, validationResults, true))
            {
                foreach (var error in validationResults)
                {
                    ModelState.AddModelError(error.MemberNames.First(), error.ErrorMessage);
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(rental);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index), new { id = rental.RiderId, firstName = ViewBag.FirstName, lastName = ViewBag.LastName });
            }

            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods, "Id", "Name", rental.PaymentMethodId);
            ViewData["RiderId"] = new SelectList(_context.Riders, "Id", "FirstName", rental.RiderId);
            ViewData["ScooterId"] = new SelectList(_context.Scooters, "Id", "Model", rental.ScooterId);
            ViewData["StatusId"] = new SelectList(_context.RentalStatuses, "Id", "Name", rental.StatusId);
            return View(rental);
        }

        // GET: Rentals/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rental = await _context.Rentals.FindAsync(id);
            if (rental == null)
            {
                return NotFound();
            }
            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods, "Id", "Name", rental.PaymentMethodId);
            ViewData["RiderId"] = new SelectList(_context.Riders, "Id", "FirstName", rental.RiderId);
            ViewData["ScooterId"] = new SelectList(_context.Scooters, "Id", "Model", rental.ScooterId);
            ViewData["StatusId"] = new SelectList(_context.RentalStatuses, "Id", "Name", rental.StatusId);
            return View(rental);
        }

        // POST: Rentals/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RiderId,ScooterId,StatusId,StartTime,EndTime,TotalCost,PaymentDate,Amount,PaymentMethodId,Id")] Rental rental)
        {
            if (id != rental.Id)
            {
                return NotFound();
            }

            ModelState.Remove("PaymentMethod");
            ModelState.Remove("Rider");
            ModelState.Remove("Scooter");
            ModelState.Remove("Status");

            // Явна валідація моделі
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(rental);
            if (!Validator.TryValidateObject(rental, validationContext, validationResults, true))
            {
                foreach (var error in validationResults)
                {
                    ModelState.AddModelError(error.MemberNames.First(), error.ErrorMessage);
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rental);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RentalExists(rental.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index), new { id = rental.RiderId, firstName = ViewBag.FirstName, lastName = ViewBag.LastName });
            }

            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods, "Id", "Name", rental.PaymentMethodId);
            ViewData["RiderId"] = new SelectList(_context.Riders, "Id", "FirstName", rental.RiderId);
            ViewData["ScooterId"] = new SelectList(_context.Scooters, "Id", "Model", rental.ScooterId);
            ViewData["StatusId"] = new SelectList(_context.RentalStatuses, "Id", "Name", rental.StatusId);
            return View(rental);
        }

        // GET: Rentals/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rental = await _context.Rentals
                .Include(r => r.PaymentMethod)
                .Include(r => r.Rider)
                .Include(r => r.Scooter)
                .Include(r => r.Status)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rental == null)
            {
                return NotFound();
            }

            return View(rental);
        }

        // POST: Rentals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rental = await _context.Rentals.FindAsync(id);
            if (rental != null)
            {
                _context.Rentals.Remove(rental);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RentalExists(int id)
        {
            return _context.Rentals.Any(e => e.Id == id);
        }
    }
}
