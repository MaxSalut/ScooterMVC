using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Riders.ToListAsync());
        }

        // GET: Riders/Details/5
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rider = await _context.Riders
                .Include(r => r.Discounts) // Завантажуємо пов’язані знижки
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rider == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (rider.ApplicationUserId != userId)
                {
                    return Forbid(); // Доступ лише до власного профілю
                }
            }

            // Перенаправлення на сторінку історії оренд
            return RedirectToAction("Index", "Rentals", new
            {
                id = rider.Id,
                firstName = rider.FirstName,
                lastName = rider.LastName
            });
        }

        // GET: Riders/Profile
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Profile()
        {
            // Отримуємо RiderId із claims авторизованого користувача
            var riderIdClaim = User.FindFirstValue("RiderId");
            if (riderIdClaim == null || !int.TryParse(riderIdClaim, out int riderId))
            {
                return NotFound("RiderId не знайдено в claims користувача.");
            }

            var rider = await _context.Riders
                .Include(r => r.Discounts)
                .FirstOrDefaultAsync(r => r.Id == riderId);
            if (rider == null)
            {
                return NotFound("Користувача не знайдено.");
            }

            // Перенаправлення на дію Details для відображення профілю
            return RedirectToAction("Details", new { id = rider.Id });
        }

        // GET: Riders/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Riders/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,PhoneNumber,RegistrationDate,AccountBalance,Id")] Rider rider)
        {
            if (ModelState.IsValid)
            {
                // Перевірка унікальності номера телефону
                var existingRider = await _context.Riders
                    .FirstOrDefaultAsync(r => r.PhoneNumber == rider.PhoneNumber);
                if (existingRider != null)
                {
                    ModelState.AddModelError("PhoneNumber", "Цей номер телефону вже використовується.");
                    return View(rider);
                }

                _context.Add(rider);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(rider);
        }

        // GET: Riders/Edit/5
        [Authorize(Roles = "User,Admin")]
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

            if (!User.IsInRole("Admin"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (rider.ApplicationUserId != userId)
                {
                    return Forbid(); // Доступ лише до власного профілю
                }
            }

            return View(rider);
        }

        // POST: Riders/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User,Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("FirstName,LastName,PhoneNumber,RegistrationDate,AccountBalance,Id")] Rider rider)
        {
            if (id != rider.Id)
            {
                return NotFound();
            }

            if (!User.IsInRole("Admin"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var existingRider = await _context.Riders.FindAsync(id);
                if (existingRider == null || existingRider.ApplicationUserId != userId)
                {
                    return Forbid(); // Доступ лише до власного профілю
                }
            }

            if (ModelState.IsValid)
            {
                // Перевірка унікальності номера телефону
                var existingRider = await _context.Riders
                    .FirstOrDefaultAsync(r => r.PhoneNumber == rider.PhoneNumber && r.Id != rider.Id);
                if (existingRider != null)
                {
                    ModelState.AddModelError("PhoneNumber", "Цей номер телефону вже використовується.");
                    return View(rider);
                }

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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Discounts(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rider = await _context.Riders
                .Include(r => r.Discounts)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (rider == null)
            {
                return NotFound();
            }

            return View(rider);
        }

        // GET: Riders/ManageDiscounts/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageDiscounts(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rider = await _context.Riders
                .Include(r => r.Discounts)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (rider == null)
            {
                return NotFound();
            }

            var allDiscounts = await _context.Discounts.ToListAsync();
            var availableDiscounts = allDiscounts.Where(d => !rider.Discounts.Any(rd => rd.Id == d.Id)).ToList();

            ViewBag.AvailableDiscounts = new SelectList(availableDiscounts, "Id", "Name");
            return View(rider);
        }

        // POST: Riders/ManageDiscounts/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageDiscounts(int id, int discountId)
        {
            var rider = await _context.Riders
                .Include(r => r.Discounts)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (rider == null)
            {
                return NotFound();
            }

            var discount = await _context.Discounts.FindAsync(discountId);
            if (discount == null)
            {
                return NotFound();
            }

            if (!rider.Discounts.Any(d => d.Id == discountId))
            {
                rider.Discounts.Add(discount);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(ManageDiscounts), new { id });
        }

        // POST: Riders/RemoveDiscount/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveDiscount(int id, int discountId)
        {
            var rider = await _context.Riders
                .Include(r => r.Discounts)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (rider == null)
            {
                return NotFound();
            }

            var discount = rider.Discounts.FirstOrDefault(d => d.Id == discountId);
            if (discount == null)
            {
                return NotFound();
            }

            rider.Discounts.Remove(discount);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(ManageDiscounts), new { id });
        }

        private bool RiderExists(int id)
        {
            return _context.Riders.Any(e => e.Id == id);
        }
    }
}