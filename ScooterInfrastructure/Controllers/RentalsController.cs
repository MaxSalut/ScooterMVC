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
        public IActionResult Create(int riderId)
        {
            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods, "Id", "Name");
            ViewData["RiderId"] = new SelectList(_context.Riders, "Id", "FirstName", riderId);
            ViewData["ScooterId"] = new SelectList(_context.Scooters, "Id", "Model");
            ViewData["StatusId"] = new SelectList(_context.RentalStatuses, "Id", "Name");
            return View(new Rental { RiderId = riderId }); // Ініціалізуємо модель із RiderId
        }

        // POST: Rentals/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RiderId,ScooterId,StatusId,StartTime,EndTime,TotalCost,PaymentDate,Amount,PaymentMethodId,Id")] Rental rental)
        {
            // Додаткова перевірка наявності зв’язаних сутностей
            var riderExists = await _context.Riders.AnyAsync(r => r.Id == rental.RiderId);
            if (!riderExists)
            {
                ModelState.AddModelError("RiderId", "Обраного користувача не існує.");
            }

            var scooterExists = await _context.Scooters.AnyAsync(s => s.Id == rental.ScooterId);
            if (!scooterExists)
            {
                ModelState.AddModelError("ScooterId", "Обраного скутера не існує.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(rental);
                    await _context.SaveChangesAsync();
                    var rider = await _context.Riders.FindAsync(rental.RiderId);
                    return RedirectToAction(nameof(Index), new { id = rental.RiderId, firstName = rider.FirstName, lastName = rider.LastName });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Помилка: {ex.Message}");
                }
            }

            // Повторно ініціалізуємо ViewData
            ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods, "Id", "Name", rental.PaymentMethodId);
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

            if (!ModelState.IsValid)
            {
                // Виводимо всі помилки валідації в консоль
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Console.WriteLine($"Validation Error: {error.ErrorMessage}");
                }

                // Завантажуємо пов’язані об’єкти
                rental.Rider = await _context.Riders.FindAsync(rental.RiderId);
                rental.Scooter = await _context.Scooters.FindAsync(rental.ScooterId);
                rental.Status = await _context.RentalStatuses.FindAsync(rental.StatusId);
                if (rental.PaymentMethodId.HasValue)
                {
                    rental.PaymentMethod = await _context.PaymentMethods.FindAsync(rental.PaymentMethodId);
                }

                // Очищаємо стан валідації та повторюємо перевірку
                ModelState.Clear();
                TryValidateModel(rental);

                if (!ModelState.IsValid)
                {
                    ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods, "Id", "Name", rental.PaymentMethodId);
                    ViewData["RiderId"] = new SelectList(_context.Riders, "Id", "FirstName", rental.RiderId);
                    ViewData["ScooterId"] = new SelectList(_context.Scooters, "Id", "Model", rental.ScooterId);
                    ViewData["StatusId"] = new SelectList(_context.RentalStatuses, "Id", "Name", rental.StatusId);
                    return View(rental);
                }
            }

            // Додаємо додаткову перевірку перед збереженням
            if (!decimal.TryParse(rental.TotalCost.ToString(), out decimal totalCost) ||
                (rental.Amount.HasValue && !decimal.TryParse(rental.Amount.ToString(), out decimal amount)))
            {
                ModelState.AddModelError("TotalCost", "Загальна вартість має бути коректним числом.");
                if (rental.Amount.HasValue)
                {
                    ModelState.AddModelError("Amount", "Сума оплати має бути коректним числом.");
                }
                ViewData["PaymentMethodId"] = new SelectList(_context.PaymentMethods, "Id", "Name", rental.PaymentMethodId);
                ViewData["RiderId"] = new SelectList(_context.Riders, "Id", "FirstName", rental.RiderId);
                ViewData["ScooterId"] = new SelectList(_context.Scooters, "Id", "Model", rental.ScooterId);
                ViewData["StatusId"] = new SelectList(_context.RentalStatuses, "Id", "Name", rental.StatusId);
                return View(rental);
            }

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
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving changes: {ex.Message}");
                throw;
            }

            var rider = await _context.Riders.FindAsync(rental.RiderId);
            return RedirectToAction(nameof(Index), new { id = rental.RiderId, firstName = rider.FirstName, lastName = rider.LastName });
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
